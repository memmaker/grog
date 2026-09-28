/*
 * Grog in the browser. The game (C#, .NET browser-wasm) runs in worker.js and
 * hands over its finished 80x26 cell buffer (char, fg, bg per cell; colours
 * from the game's ConsoleColor palette in src/Grog.Kernel.GCurses/Term.cs).
 * This page only blits cells and forwards keys. Every file the game writes
 * (saves, options, defaults, high scores, ghosts, revenge monsters) is mirrored
 * to IndexedDB '/grog/files'. Stage 1: the whole screen in the Map window.
 */
const FONT = '"DejaVu Sans Mono", Menlo, Consolas, "Liberation Mono", monospace';
const DB = '/grog/files';
const SLOT = 48, NSLOT = 64;
const $ = id => document.getElementById(id);
const dpr = Math.max(1, Math.min(3, window.devicePixelRatio || 1));

let worker, ring, db, ended = false, wm = null, L = { px: 16, wm: null }, saveT = 0;
const app = RvipApp({ name: 'grog', save: () => null, read: () => null, clear: () => {}, put: () => {} });
let scr = null, COLS = 80, ROWS = 26, cur = { x: 0, y: 0, vis: false }, dirty = true, ctx;

/* ---------- cross-origin isolation (SharedArrayBuffer) ---------- */
async function isolate() {
	if (window.crossOriginIsolated) return true;
	if (!('serviceWorker' in navigator)) return false;
	if (sessionStorage.getItem('grog-coi')) { sessionStorage.removeItem('grog-coi'); return false; }
	sessionStorage.setItem('grog-coi', '1');
	await navigator.serviceWorker.register('coi-sw.js');
	await navigator.serviceWorker.ready;
	location.reload();
	return new Promise(() => {});
}

/* ---------- IndexedDB mirror of the game's files ---------- */
function openDB() {
	return new Promise((ok, bad) => {
		const r = indexedDB.open(DB, 1);
		r.onupgradeneeded = () => r.result.createObjectStore('files');
		r.onsuccess = () => ok(r.result);
		r.onerror = () => bad(r.error);
	});
}
function tx(mode, f) {
	return new Promise((ok, bad) => {
		const t = db.transaction('files', mode), out = f(t.objectStore('files'));
		t.oncomplete = () => ok(out && out.result);
		t.onerror = () => bad(t.error);
	});
}
async function allFiles() {
	const out = {};
	await new Promise((ok, bad) => {
		const t = db.transaction('files', 'readonly'), c = t.objectStore('files').openCursor();
		c.onsuccess = () => { const k = c.result; if (k) { out[k.key] = new Uint8Array(k.value); k.continue(); } };
		t.oncomplete = ok; t.onerror = () => bad(t.error);
	});
	return out;
}
const putFile = (name, data) => tx('readwrite', s => s.put(data, name));
const delFile = name => tx('readwrite', s => s.delete(name));
const getFile = name => tx('readonly', s => s.get(name));

/* ---------- keys ---------- */
function sendKey(code, key, mods) {
	const str = code + '\t' + key + '\t' + mods, w = Atomics.load(ring, 0);
	if (w - Atomics.load(ring, 1) >= NSLOT) return;
	const s = 2 + (w % NSLOT) * SLOT, n = Math.min(str.length, SLOT - 1);
	ring[s] = n;
	for (let i = 0; i < n; i++) ring[s + 1 + i] = str.charCodeAt(i);
	Atomics.store(ring, 0, w + 1);
	Atomics.notify(ring, 0);
}
function onKey(e) {
	if (!app.running) return;
	if (['Shift', 'Control', 'Alt', 'Meta', 'CapsLock', 'NumLock', 'Dead'].includes(e.key)) return;
	if (e.metaKey || ['F5', 'F11', 'F12'].includes(e.code)) return;
	e.preventDefault();
	sendKey(e.code, e.key, (e.shiftKey ? 's' : '') + (e.ctrlKey ? 'c' : '') + (e.altKey ? 'a' : ''));
}

/* ---------- drawing: cell size from the Map window's A-/A+, never from the window size ---------- */
const hex = v => '#' + (v & 0xffffff).toString(16).padStart(6, '0');
function draw() {
	requestAnimationFrame(draw);
	if (!dirty || !scr || !wm) return;
	dirty = false;
	const p = L.px, c = $('map').querySelector('canvas');
	ctx.font = p + 'px ' + FONT;
	const w = Math.ceil(ctx.measureText('M').width), h = Math.ceil(p * 1.2);
	if (c.width !== Math.round(COLS * w * dpr) || c.height !== Math.round(ROWS * h * dpr)) { c.width = Math.round(COLS * w * dpr); c.height = Math.round(ROWS * h * dpr); }
	c.style.width = COLS * w + 'px'; c.style.height = ROWS * h + 'px';
	const g = c.getContext('2d');
	g.setTransform(dpr, 0, 0, dpr, 0, 0); g.font = p + 'px ' + FONT; g.textBaseline = 'middle'; g.textAlign = 'center';
	for (let r = 0; r < ROWS; r++) for (let k = 0; k < COLS; k++) {
		const i = (r * COLS + k) * 3, x = k * w, y = r * h;
		g.fillStyle = hex(scr[i + 2]); g.fillRect(x, y, w, h);
		if (scr[i] > 32) { g.fillStyle = hex(scr[i + 1]); g.fillText(String.fromCharCode(scr[i]), x + w / 2, y + h / 2 + 1); }
	}
	if (cur.vis) { g.fillStyle = hex(scr[(cur.y * COLS + cur.x) * 3 + 1]); g.fillRect(cur.x * w, cur.y * h + h - 3, w, 2); }
	RvipWM.center(c, 0, 0, COLS * w, ROWS * h);
}
function saveLayout() { clearTimeout(saveT); saveT = setTimeout(() => putFile('web-layout.json', new TextEncoder().encode(JSON.stringify(L))), 300); }
async function makeWM() {
	try { const d = await getFile('web-layout.json'); if (d) { const s = JSON.parse(new TextDecoder().decode(d)); L = { px: s.px >= 8 && s.px <= 48 ? s.px : 16, wm: s.wm }; } } catch (_) { }
	wm = RvipWM({
		area: $('game'), menu: $('btn-layout'),
		wins: [{ id: 'map', title: 'Map' }],
		multi: 'map', single: 'map', state: L.wm,
		save: st => { L.wm = st; saveLayout(); },
		layout: () => { dirty = true; },
		zoom: { map: (p, d) => { L.px = Math.max(8, Math.min(48, L.px + d)); saveLayout(); dirty = true; } },
		onReset: () => { L.px = 16; L.wm = wm.state(); saveLayout(); dirty = true; }
	});
	wm.apply();
}

/* ---------- worker ---------- */
function onMessage(e) {
	const m = e.data;
	switch (m.t) {
	case 'screen':
		scr = m.cells; COLS = m.cols; ROWS = m.rows; cur = { x: m.cx, y: m.cy, vis: m.vis }; dirty = true;
		if (!app.running && !ended) { app.running = true; app.status(''); $('game').hidden = false; wm.apply(); }
		break;
	case 'store': putFile(m.name, m.data); break;
	case 'delete': delFile(m.name); break;
	case 'quit': case 'exit': if (!ended) { ended = true; app.running = false; setTimeout(() => { $('overlay').hidden = false; }, 300); } break;
	case 'crash': app.crashed(new Error(m.msg.split('\n')[0])); console.error(m.msg); break;
	}
}
/* hooks for tests */
window.grog = {
	text() { if (!scr) return ''; let s = ''; for (let r = 0; r < ROWS; r++) { for (let c = 0; c < COLS; c++) { const k = scr[(r * COLS + c) * 3]; s += k < 32 ? ' ' : String.fromCharCode(k); } s += '\n'; } return s; },
	key: sendKey,
	get running() { return app.running; },
};

async function main() {
	ctx = document.createElement('canvas').getContext('2d');
	$('btn-restart').onclick = () => location.reload();
	if (!await isolate()) { app.status('This browser cannot run the game here (no cross-origin isolation / SharedArrayBuffer).', true); return; }
	db = await openDB();
	await makeWM();
	const files = await allFiles();
	ring = new Int32Array(new SharedArrayBuffer(4 * (2 + NSLOT * SLOT)));
	worker = new Worker('worker.js', { type: 'module' });
	worker.onmessage = onMessage;
	worker.onerror = e => app.crashed(e);
	worker.postMessage({ t: 'init', ring: ring.buffer, files });
	window.addEventListener('keydown', onKey);
	requestAnimationFrame(draw);
}
main();
