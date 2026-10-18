"""Turns the PNG captures in out/ into the WebP files manifest.json names, and lists every output file.

- A screenshot becomes WebP at the manifest's quality, at most maxWidth pixels wide.
- A palette shot is captured as one tile per palette; the tiles become one grid image per theme, each tile labeled
  with its palette name, in the manifest's palette order.
- out/media.json lists every card and screenshot with its key, width, height, bytes and SHA-256, for the upload and
  for the content edits (width and height go into the Markdown so the page reserves the space).

Usage: python tools/site-media/optimize.py   (from the repository root, after cards.mjs and shots.mjs)
"""
import hashlib
import json
import os

from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, 'out')
manifest = json.load(open(os.path.join(HERE, 'manifest.json'), encoding='utf-8'))
quality = manifest['shot']['quality']
max_width = manifest['shot']['maxWidth']

PALETTE_NAMES = {
    'osirion': 'Osirion', 'commit': 'Commit', 'editor': 'Editor', 'studio': 'Studio',
    'studio-blue': 'Studio Blue', 'contrast': 'Contrast',
}


def font(size):
    for name in ('segoeuib.ttf', 'arialbd.ttf', 'DejaVuSans-Bold.ttf'):
        for folder in (r'C:\Windows\Fonts', '/usr/share/fonts/truetype/dejavu'):
            path = os.path.join(folder, name)
            if os.path.exists(path):
                return ImageFont.truetype(path, size)
    return ImageFont.load_default()


def save_webp(image, key):
    if image.width > max_width:
        image = image.resize((max_width, round(image.height * max_width / image.width)), Image.LANCZOS)
    target = os.path.join(OUT, key)
    os.makedirs(os.path.dirname(target), exist_ok=True)
    image.convert('RGB').save(target, 'WEBP', quality=quality, method=6)
    return target


def palette_grid(tiles, theme):
    """Three columns of labeled tiles; the labels sit on a band in the page's own background color."""
    columns, tile_width, gap, band = 3, 520, 16, 44
    rows = (len(tiles) + columns - 1) // columns
    first = Image.open(tiles[0][1])
    tile_height = round(first.height * tile_width / first.width)
    background = (2, 6, 23) if theme == 'dark' else (243, 244, 246)
    ink = (232, 232, 242) if theme == 'dark' else (17, 24, 39)
    grid = Image.new('RGB', (columns * tile_width + (columns + 1) * gap, rows * (tile_height + band) + (rows + 1) * gap), background)
    draw = ImageDraw.Draw(grid)
    label = font(22)
    for index, (palette, path) in enumerate(tiles):
        x = gap + (index % columns) * (tile_width + gap)
        y = gap + (index // columns) * (tile_height + band + gap)
        grid.paste(Image.open(path).convert('RGB').resize((tile_width, tile_height), Image.LANCZOS), (x, y))
        draw.text((x + 4, y + tile_height + 10), PALETTE_NAMES.get(palette, palette), font=label, fill=ink)
    return grid


written = []
for page in manifest['pages']:
    for shot in page['shots']:
        for theme in shot['themes']:
            key = shot['key'].replace('{theme}', theme)
            if 'palettes' in shot:
                tiles = [(p, os.path.join(OUT, key.replace('.webp', f'-{p}.png'))) for p in shot['palettes']]
                tiles = [(p, path) for p, path in tiles if os.path.exists(path)]
                if not tiles:
                    continue
                written.append((key, save_webp(palette_grid(tiles, theme), key), shot['alt'], page['path']))
                continue
            capture = os.path.join(OUT, key.replace('.webp', '.png'))
            if os.path.exists(capture):
                written.append((key, save_webp(Image.open(capture), key), shot['alt'], page['path']))
    card = os.path.join(OUT, page['card']['key'])
    if os.path.exists(card):
        written.append((page['card']['key'], card, page['card']['heading'], page['path']))

index = []
for key, path, text, page_path in written:
    with Image.open(path) as image:
        width, height = image.size
    data = open(path, 'rb').read()
    index.append({'key': key, 'page': page_path, 'width': width, 'height': height, 'bytes': len(data),
                  'sha256': hashlib.sha256(data).hexdigest(), 'alt': text})
with open(os.path.join(OUT, 'media.json'), 'w', encoding='utf-8', newline='\n') as handle:
    json.dump(index, handle, indent=2, ensure_ascii=False)
    handle.write('\n')

shots = [entry for entry in index if entry['key'].startswith('shots/')]
cards = [entry for entry in index if entry['key'].startswith('og/')]
print(f"{len(cards)} cards ({sum(e['bytes'] for e in cards) // 1024} KB), {len(shots)} screenshots ({sum(e['bytes'] for e in shots) // 1024} KB)")
for entry in shots:
    print(f"  {entry['key']} {entry['width']}x{entry['height']} {entry['bytes'] // 1024} KB")
