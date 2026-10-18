"""Places the screenshots in their pages: after a given anchor line, with the size and alt text from out/media.json.

Each placement names the page, the line the images follow (the first line, outside code blocks, that starts with
the anchor), an optional lead sentence and the screenshot keys. The image is written as Markdown with its width and
height ({width=.. height=..}), so the page reserves its space before it loads. Pages that change get last_modified
set to the given date. Idempotent: an image already on the page is not added again.

Usage: python tools/site-media/apply_shots.py   (from the repository root)
"""
import json
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
CONTENT = os.path.join(HERE, '..', '..', 'content', 'en')
EDIT_DATE = '2026-10-18'
media = {entry['key']: entry for entry in json.load(open(os.path.join(HERE, 'out', 'media.json'), encoding='utf-8'))}
host = json.load(open(os.path.join(HERE, 'manifest.json'), encoding='utf-8'))['mediaHost']

PLACEMENTS = [
    ('templates.md', '| `osirion-site` |', 'What each template creates, run straight after `dotnet new`:',
     ['shots/templates/osirion-blog-dark.webp', 'shots/templates/osirion-docs-dark.webp', 'shots/templates/osirion-site-dark.webp']),
    ('docs/getting-started.md', '35 seconds, most of it the first restore and build.', None,
     ['shots/docs-getting-started/first-blog-dark.webp']),
    ('themes.md', '[ThemePicker](/components/theme-picker) is the list at the top of this page.',
     'The getting started page in each palette, in the dark theme and then the light one:',
     ['shots/themes/palettes-dark.webp', 'shots/themes/palettes-light.webp']),
    ('blog/modern-hero-sections-blazor.md', '             SecondaryButtonUrl="https://github.com/demonjic/Osirion.Blazor" />', None,
     ['shots/blog-modern-hero-sections-blazor/hero-variants-dark.webp']),
    ('blog/navigation-components.md', '`NavLinkMatch` comes from `Microsoft.AspNetCore.Components.Routing`', None,
     ['shots/blog-navigation-components/menu-dark.webp']),
    ('blog/osirion-blazor-4-0-components.md', 'need no script at all.', 'Every one of them has a page with a live demo, all listed in the component gallery:',
     ['shots/blog-osirion-blazor-4-0-components/gallery-dark.webp']),
    ('blog/github-cms-with-blazor.md', 'title, description, canonical URL and structured data into the head.', 'The blog list, as getosirion.com renders it:',
     ['shots/blog-github-cms-with-blazor/blog-from-github-dark.webp']),
]


def image_markdown(key):
    entry = media[key]
    return f"![{entry['alt']}]({host}/{key}){{width={entry['width']} height={entry['height']}}}"


for file, anchor, lead, keys in PLACEMENTS:
    path = os.path.join(CONTENT, file)
    raw = open(path, 'rb').read()
    bom = raw.startswith(b'\xef\xbb\xbf')
    text = raw.decode('utf-8-sig')
    nl = '\r\n' if '\r\n' in text else '\n'
    keys = [key for key in keys if f'{host}/{key}' not in text]
    if not keys:
        continue
    lines = text.split(nl)
    fence, line_index, front = False, None, 0
    for index, line in enumerate(lines):
        if line.strip() == '---' and front < 2:
            front += 1
            continue
        if front < 2:
            continue
        if line.lstrip().startswith('```'):
            # The hero anchor is the last line of a code sample, so the block after the fence is the place.
            if fence and lines[index - 1].startswith(anchor):
                line_index = index
                break
            fence = not fence
            continue
        if not fence and line.startswith(anchor):
            line_index = index
            break
    assert line_index is not None, (file, anchor)
    # Skip to the end of the paragraph or table the anchor is in.
    while line_index + 1 < len(lines) and lines[line_index + 1].strip() and not lines[line_index].lstrip().startswith('```'):
        line_index += 1
    block = [''] + ([lead, ''] if lead else []) + [part for key in keys for part in (image_markdown(key), '')]
    lines[line_index + 1:line_index + 1] = block[:-1] if lines[line_index + 1:line_index + 2] == [''] else block
    text = nl.join(lines)
    text = re.sub(r'(?m)^last_modified:.*$', f'last_modified: {EDIT_DATE}', text, count=1)
    open(path, 'wb').write((b'\xef\xbb\xbf' if bom else b'') + text.encode('utf-8'))
    print(f'{file}: {len(keys)} image(s)')
