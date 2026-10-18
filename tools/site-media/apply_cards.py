"""Points every page's front matter at its Open Graph card on media.getosirion.com.

- `seo_properties.og_image_url` becomes the card (added, with `seo_properties` if missing), so link previews show it.
- A front matter value that names the shared placeholder (`/assets/images/blazor-cover.png`), such as
  `featured_image` or `seo_properties.image`, becomes the card too, so lists and covers show the page's own card.
- A real featured image (the home page hero) is kept.

Idempotent: running it again changes nothing. Usage: python tools/site-media/apply_cards.py (from the repository root)
"""
import json
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.join(HERE, '..', '..')
PLACEHOLDER = '/assets/images/blazor-cover.png'
manifest = json.load(open(os.path.join(HERE, 'manifest.json'), encoding='utf-8'))

changed = 0
for page in manifest['pages']:
    if page['file'] == 'CHANGELOG.md':
        # The changelog page sets its card in code (Changelog.razor.cs); the file's front matter is not read.
        continue
    path = os.path.join(ROOT, 'content', 'en', page['file'])
    raw = open(path, 'rb').read()
    bom = raw.startswith(b'\xef\xbb\xbf')
    text = raw.decode('utf-8-sig')
    nl = '\r\n' if '\r\n' in text else '\n'
    match = re.match(r'---\r?\n(.*?)\r?\n---', text, re.S)
    if not match:
        print('no front matter:', page['file'])
        continue
    card = page['card']['url']
    lines = match.group(1).split(nl)

    # The placeholder, quoted or not, wherever it is a value
    lines = [re.sub(r"""^(\s*[a-z_]+:\s*)['"]?""" + re.escape(PLACEHOLDER) + r"""['"]?\s*(#.*)?$""", lambda m: f"{m.group(1)}'{card}'", line) for line in lines]

    if not any(re.match(r'^featured_image:', line) for line in lines):
        lines.append(f"featured_image: '{card}'")

    seo = next((i for i, line in enumerate(lines) if re.match(r'^seo_properties:\s*$', line)), None)
    if seo is None:
        lines += ['seo_properties:', f"  og_image_url: '{card}'"]
    else:
        end = seo + 1
        while end < len(lines) and (lines[end].startswith('  ') or not lines[end].strip()):
            end += 1
        existing = next((i for i in range(seo + 1, end) if re.match(r'^\s+og_image_url:', lines[i])), None)
        if existing is None:
            lines.insert(seo + 1, f"  og_image_url: '{card}'")
        else:
            lines[existing] = f"  og_image_url: '{card}'"

    updated = text[:match.start(1)] + nl.join(lines) + text[match.end(1):]
    if updated != text:
        open(path, 'wb').write((b'\xef\xbb\xbf' if bom else b'') + updated.encode('utf-8'))
        changed += 1
print(f'{changed} file(s) changed of {len(manifest["pages"])}')
