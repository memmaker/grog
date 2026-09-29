/*
 * Grog in the browser. The game (C#, .NET browser-wasm) runs in worker.js and
 * hands over its finished 80x26 cell buffer (char, fg, bg per cell; colours
 * from the game's ConsoleColor palette in src/Grog.Kernel.GCurses/Term.cs).
 * This page only blits cells and forwards keys. Every file the game writes
 * (saves, options, defaults, high scores, ghosts, revenge monsters) is mirrored
 * to IndexedDB '/grog/files'. Stage 5: rvip-wm windows from the game's info (Term.Info).
 */
const FONT = '"DejaVu Sans Mono", Menlo, Consolas, "Liberation Mono", monospace';
const DB = '/grog/files';
const SLOT = 48, NSLOT = 64;
const $ = id => document.getElementById(id);
const dpr = Math.max(1, Math.min(3, window.devicePixelRatio || 1));

let worker, ring, db, ended = false, wm = null, L = { wm: null }, saveT = 0;
const isGame = k => !k.startsWith('web-');
async function gameFiles() { return Object.keys(await allFiles()).filter(isGame); }
/* Export / Import / New game (rvip-app.js): every game file in one bundle */
const app = RvipApp({
	name: 'grog',
	save: async () => { const f = await gameFiles(); return f.length ? f : null; },
	read: name => getFile(name),
	clear: async () => { for (const f of await gameFiles()) await delFile(f); },
	put: (f, data) => putFile(f.name || f, data).then(() => {}), // a string result = refuse (rvip-app)
	noSave: 'No game files yet. Save first (Q).'
});
let scr = null, COLS = 80, ROWS = 26, cur = { x: 0, y: 0, vis: false }, dirty = true, ctx, info = {}, rects = {}, cellH = 16;
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
	if (e.target && (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA')) return;
	if (['Shift', 'Control', 'Alt', 'Meta', 'CapsLock', 'NumLock', 'Dead'].includes(e.key)) return;
	if (e.metaKey || ['F5', 'F11', 'F12'].includes(e.code)) return;
	e.preventDefault();
	sendKey(e.code, e.key, (e.shiftKey ? 's' : '') + (e.ctrlKey ? 'c' : '') + (e.altKey ? 'a' : ''));
}

/* ---------- drawing ----------
 * The game (Term.Info, DungeonLevel.RvipInfo) sends: main (the map screen is up), hero [x,row],
 * map [first row, rows], status lines, inv [[text, equipped]], vis [names], log, prompt, fg/bg.
 * Multi-window: map rows on the canvas, the rest in text windows; a whole-screen view
 * (lists, menus, help) on the map canvas, as in one window.
 * Fixed-size levels: the cell size fits the rows shown into the Map window (no scroll, no A-/A+). */
const hex = v => '#' + (v & 0xffffff).toString(16).padStart(6, '0');
const one = () => wm && wm.mode() === 'single';
function draw() {
	requestAnimationFrame(draw);
	if (!dirty || !scr || !wm) return;
	dirty = false;
	const face = faceOf(L.mapFace), c = $('map').querySelector('canvas');
	const whole = one() || !info.main || !info.map;
	const r0 = whole ? 0 : info.map[0], R = whole ? ROWS : info.map[1];
	ctx.font = '100px ' + face;
	const mw = ctx.measureText('M').width / 100, vb = c.parentNode;
	const p = Math.max(4, Math.floor(Math.min(vb.clientWidth / COLS / mw, vb.clientHeight / R / 1.2)) - 1);
	ctx.font = p + 'px ' + face;
	const w = Math.ceil(ctx.measureText('M').width), h = cellH = Math.ceil(p * 1.2);
	if (c.width !== Math.round(COLS * w * dpr) || c.height !== Math.round(R * h * dpr)) { c.width = Math.round(COLS * w * dpr); c.height = Math.round(R * h * dpr); }
	c.style.width = COLS * w + 'px'; c.style.height = R * h + 'px';
	const g = c.getContext('2d');
	g.setTransform(dpr, 0, 0, dpr, 0, 0); g.font = p + 'px ' + face; g.textBaseline = 'middle'; g.textAlign = 'center';
	for (let r = 0; r < R; r++) for (let k = 0; k < COLS; k++) {
		const i = ((r0 + r) * COLS + k) * 3, x = k * w, y = r * h;
		g.fillStyle = hex(scr[i + 2]); g.fillRect(x, y, w, h);
		if (scr[i] > 32) { g.fillStyle = hex(scr[i + 1]); g.fillText(String.fromCharCode(scr[i]), x + w / 2, y + h / 2 + 1); }
	}
	const cy = cur.y - r0;
	if (cur.vis && cy >= 0 && cy < R) { g.fillStyle = hex(scr[(cur.y * COLS + cur.x) * 3 + 1]); g.fillRect(cur.x * w, cy * h + h - 3, w, 2); }
	if (info.hero && (info.main || !whole)) RvipWM.center(c, (info.hero[0] + 0.5) * w, (info.hero[1] - r0 + 0.5) * h, COLS * w, R * h);
	else RvipWM.center(c, 0, 0, COLS * w, R * h);
	c._r0 = r0;
}
/* mouse: a click on a row of a menu or item list sends that row; the game picks the entry */
function clickRow(e, el, r0) {
	if (!info.click || !app.running) return;
	const b = el.getBoundingClientRect(), h = cellH;
	const row = Math.floor((e.clientY - b.top) / h) + r0;
	if (row >= 0 && row < ROWS) sendKey('RvipRow', String(row), '');
}
function update(i) {
	RvipWM.prompt.text(i.main && !one() ? i.prompt || '' : '');
	RvipWM.prompt.wait(i.atCmd);
	if (i.fg !== undefined) ['msgb', 'stat', 'inv', 'vis'].forEach(id => { $(id).style.color = hex(i.fg); $(id).style.background = hex(i.bg); });
	if (i.log) RvipWM.setLog($('log'), i.log);
	if (i.status) { const t = i.status.join('\n'); if ($('stat').textContent !== t) $('stat').textContent = t; }
	if (i.inv) {
		const el = $('inv'), k = JSON.stringify(i.inv);
		if (el._k !== k) {
			el._k = k; el.textContent = '';
			if (!i.inv.length) { const d = document.createElement('div'); d.className = 'wm-vh'; d.textContent = 'empty'; el.appendChild(d); }
			i.inv.forEach(([t, eq]) => { const d = document.createElement('div'); d.textContent = t; if (eq) d.className = 'inv-eq'; el.appendChild(d); });
		}
	}
	if (i.vis) RvipWM.visible($('vis'), i.vis.join('\n'));
}
function saveLayout() { clearTimeout(saveT); saveT = setTimeout(() => putFile('web-layout.json', new TextEncoder().encode(JSON.stringify(L))), 300); }
async function makeWM() {
	try { const d = await getFile('web-layout.json'); if (d) { const s = JSON.parse(new TextDecoder().decode(d)); L = { wm: s.wm, sound: !!s.sound, face: s.face || '', mapFace: s.mapFace || '' }; } } catch (_) { }
	loadFace(L.face); loadFace(L.mapFace);
	wm = RvipWM({
		area: $('game'), menu: $('btn-layout'),
		wins: [{ id: 'map', title: 'Map' }, { id: 'msg', title: 'Messages' }, { id: 'stat', title: 'Status' },
			{ id: 'inv', title: 'Inventory' }, { id: 'vis', title: 'Visible' }],
		multi: { d: 'h', r: 0.7, a: { d: 'v', r: 0.8, a: 'map', b: 'stat' }, b: { d: 'v', r: 0.4, a: 'msg', b: { d: 'v', r: 0.6, a: 'inv', b: 'vis' } } },
		single: 'map', state: L.wm,
		save: st => { L.wm = st; saveLayout(); },
		layout: r => { rects = r; dirty = true; update(info); renderMapSel(); },
		noFont: 'map',
		onReset: () => { L.wm = wm.state(); saveLayout(); dirty = true; }
	});
	wm.apply();
	renderMapSel();
}
/* fonts: the top-bar choice (L.face) for every text window and the whole-screen view,
   the map's own (L.mapFace, a select on its title bar) for the map canvas */
const faceOf = n => n ? '"' + n + '", ' + FONT : FONT;
function faces() {
	['msgb', 'stat', 'inv', 'vis'].forEach(id => { $(id).style.fontFamily = L.face ? faceOf(L.face) : ''; });
	dirty = true;
}
function loadFace(n) {
	if (!n) { faces(); return; }
	const ff = new FontFace(n, 'url(../fonts/' + n + '.woff)');
	ff.load().then(() => { document.fonts.add(ff); faces(); }).catch(() => app.status('Could not load the font ' + n + '.', true));
}
const mapSel = document.createElement('select');
mapSel.title = 'Map font'; mapSel.innerHTML = '<option value="">Default font</option>';
mapSel.addEventListener('pointerdown', e => e.stopPropagation()); /* not a window drag */
mapSel.addEventListener('mousedown', e => e.stopPropagation());
function renderMapSel() {
	const bs = document.querySelector('#t-map .wm-btns');
	if (bs && mapSel.parentNode !== bs) bs.insertBefore(mapSel, bs.firstChild);
	mapSel.value = L.mapFace || '';
}

/* ---------- worker ---------- */
function onMessage(e) {
	const m = e.data;
	switch (m.t) {
	case 'screen':
		scr = m.cells; COLS = m.cols; ROWS = m.rows; cur = { x: m.cx, y: m.cy, vis: m.vis }; dirty = true;
		if (m.info) { try { info = JSON.parse(m.info); update(info); } catch (err) { console.error('info', err); } }
		if (!app.running && !ended) { app.running = true; app.status(''); $('game').hidden = false; wm.apply(); }
		break;
	case 'beacon': // RVIP 12: the game builds the report, the page only sends it
		if (window.RvipWM && RvipWM.report) RvipWM.report(m.q); else fetch('/roguelikes/beacon?' + m.q, { keepalive: true, mode: 'no-cors' }).catch(function () {});
		break;
	case 'sound': if (L.sound) RVIPSound.play([m.name], 0.6); break;
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
	get info() { return info; },
	get cur() { return cur; },
};

async function main() {
	ctx = document.createElement('canvas').getContext('2d');
	$('btn-restart').onclick = () => location.reload();
	RvipWM.dropdown($('btn-file'), $('menu-file'));
	RvipWM.dropdown($('btn-audio'), $('menu-audio'));
	fetch('fonts.json').then(r => r.json()).then(list => {
		[[$('sel-font'), 'face'], [mapSel, 'mapFace']].forEach(([sel, k]) => {
			list.forEach(n => { const o = document.createElement('option'); o.value = n; o.textContent = n.replace(/^Web(Plus|437)_/, '').replace(/_/g, ' '); sel.appendChild(o); });
			sel.value = L[k] || '';
		});
	}).catch(() => { });
	[[$('sel-font'), 'face'], [mapSel, 'mapFace']].forEach(([sel, k]) => {
		sel.onchange = function () { L[k] = this.value; saveLayout(); loadFace(this.value); this.blur(); };
	});
	$('chk-sound').onchange = function () { L.sound = this.checked; saveLayout(); this.blur(); };
	document.querySelectorAll('#bar button').forEach(b => b.addEventListener('mousedown', e => e.preventDefault()));
	$('map').addEventListener('click', e => { const c = $('map').querySelector('canvas'); clickRow(e, c, c._r0 || 0); });
	if (!await isolate()) { app.status('This browser cannot run the game here (no cross-origin isolation / SharedArrayBuffer).', true); return; }
	db = await openDB();
	await makeWM();
	$('chk-sound').checked = !!L.sound;
	const files = await allFiles();
	ring = new Int32Array(new SharedArrayBuffer(4 * (2 + NSLOT * SLOT)));
	worker = new Worker('worker.js', { type: 'module' });
	worker.onmessage = onMessage;
	worker.onerror = e => app.crashed(e);
	worker.postMessage({ t: 'init', ring: ring.buffer, files });
	window.addEventListener('keydown', onKey);
	window.addEventListener('resize', () => { wm.apply(); dirty = true; });
	window.addEventListener('beforeunload', e => { if (app.running) { e.preventDefault(); e.returnValue = ''; } });
	requestAnimationFrame(draw);
}
main();
