#!/usr/bin/env python3
"""Local test server: http://127.0.0.1:<port>/grog/ = web/dist, /rvip-*.js from
~/Games/rvip-tools/web (as ../rvip-wm.js on the server), /fonts/ from the index.
Sends COOP/COEP itself (the browser pane runs no service worker on http)."""
import http.server, os, sys
HERE = os.path.dirname(os.path.abspath(__file__))
HOME = os.path.expanduser('~/Games')
class H(http.server.SimpleHTTPRequestHandler):
    def translate_path(self, path):
        p = path.split('?')[0].split('#')[0]
        if p.startswith('/grog/'): return os.path.join(HERE, 'dist', p[6:])
        if p.startswith('/rvip-'): return os.path.join(HOME, 'rvip-tools/web', p[1:])
        if p.startswith('/fonts/'): return os.path.join(HOME, 'roguelikes-index', p[1:])
        return '/nonexistent'
    def end_headers(self):
        self.send_header('Cross-Origin-Opener-Policy', 'same-origin')
        self.send_header('Cross-Origin-Embedder-Policy', 'require-corp')
        self.send_header('Cross-Origin-Resource-Policy', 'same-origin')
        self.send_header('Cache-Control', 'no-store')
        super().end_headers()
H.extensions_map['.wasm'] = 'application/wasm'
http.server.ThreadingHTTPServer(('127.0.0.1', int(sys.argv[1]) if len(sys.argv) > 1 else 8431), H).serve_forever()
