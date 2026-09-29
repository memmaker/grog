#!/usr/bin/env python3
"""Writes the in-page guide (dist/help.html) for the Grog web build: the game's own
help text (src/Grog/Constants.cs HelpText) plus the browser notes written here."""
import ast, html, os, re, sys
src = open(os.path.join(os.path.dirname(__file__), '..', 'src', 'Grog', 'Constants.cs'), encoding='utf-8').read()
body = re.search(r'HelpText\s*=\s*new string\[\]\s*\{(.*?)\};', src, re.S).group(1)
lines = [ast.literal_eval(s) for s in re.findall(r'"(?:[^"\\]|\\.)*"', body)]
e = html.escape
out = ['<h2>Grog</h2><p>Grog (2018) by Thomas Biskup, the author of ADOM: a small, fast roguelike. '
       'Descend as deep as you dare and live to tell. Original: <a href="https://www.roguelike.games/grog/" target="_blank">roguelike.games/grog</a>.</p>',
       '<div class="box key"><h3>Keys to remember</h3><dl>'
       '<dt><kbd>h</kbd> <kbd>?</kbd></dt><dd>the game\'s own help</dd>'
       '<dt><kbd>g</kbd></dt><dd>explore automatically (stops on danger or news)</dd>'
       '<dt><kbd>&lt;</kbd> <kbd>&gt;</kbd></dt><dd>walk to known stairs; press again on them to take them</dd>'
       '<dt><kbd>Enter</kbd></dt><dd>menu of all commands (click an entry, or arrows + Enter)</dd>'
       '<dt><kbd>i</kbd></dt><dd>inventory: letter = main action, Enter or a click = item menu</dd>'
       '<dt><kbd>Q</kbd></dt><dd>save and quit</dd></dl></div>']
sec = None
for l in lines:
    m = re.match(r'--- (.*) ---', l)
    if m:
        if sec: out.append('</pre>')
        sec = m.group(1); out.append('<h3>%s</h3><pre>' % e(sec)); continue
    out.append(e(l.lstrip('|')[1:] if l.startswith('| ') else l.lstrip('|')))
if sec: out.append('</pre>')
out.append('<h2>Playing in the browser</h2><ul>'
           '<li>Your games live in this browser (IndexedDB). <b>Q</b> saves and quits; loading a save deletes it, as in the original. '
           'The game also saves itself automatically whenever it waits for a command: closing the tab loses nothing, and the next visit continues the game.</li>'
           '<li>Audio ▾ turns the synthesized sound effects on (off by default; Grog has no music).</li>'
           '<li>File ▾ → Export save downloads all game files (saves, options, high scores) as one file; Import save loads them back.</li>'
           '<li>Windows ▾: one window (the whole game screen) or several (map, messages, status, inventory, visible monsters). '
           'Drag the gaps to resize, A−/A+ on a title bar sets that window\'s text size. The layout is kept in this browser.</li>'
           '<li>---more--- prompts never wait here; every message goes to the Messages window.</li></ul>')
# Tips, new-player guide and saving from the desktop Docs (build-docs.py + guides.py, entry grog.html), when present
try:
    import importlib.util, sys
    DOCS = os.path.expanduser('~/Desktop/Games/Roguelikes/Docs')
    sys.path.insert(0, DOCS)
    spec = importlib.util.spec_from_file_location('build_docs', os.path.join(DOCS, 'build-docs.py'))
    docs = importlib.util.module_from_spec(spec); spec.loader.exec_module(docs)
    from guides import GUIDES, SAVING
    game = next(g for g in docs.GAMES if g['file'] == 'grog.html')
    extra = ['<h2>Saving</h2>' + SAVING['grog.html']]
    extra += ['<h2>%s</h2>%s' % (e(t), h) for t, h in GUIDES['grog.html']]
    extra += ['<h2>%s</h2>%s' % (e(t), h) for t, h in game['info'] if t in ('Tips', 'Credits')]
    out[2:2] = extra
except Exception as x:
    print('make-help: no Docs entry (%s), game text only' % x, file=sys.stderr)
print('\n'.join(out))
