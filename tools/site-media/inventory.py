"""Builds the draft image manifest for getosirion.com from the content folder.

Every published, indexable page gets an Open Graph card. Screenshots are listed only where they show something a live
demo on the page cannot, as decided for Phase 7 (2026-10-18): the templates, the themes, the guides that end in a
running site, and the blog posts about a visible feature. The manifest is the input of the card and screenshot
scripts; run this again after adding pages, then review the screenshot entries by hand.

Usage: python tools/site-media/inventory.py   (from the repository root)
"""
import json
import os
import re

CONTENT = os.path.join('content', 'en')
MEDIA_HOST = 'https://media.getosirion.com'

# Screenshots that teach something a live demo cannot. Keyed by the page's site path; each entry is a shot spec.
SHOTS = {
    '/templates': [
        {'shot': 'osirion-blog', 'source': 'template:osirion-blog', 'path': '/', 'alt': 'A blog created with dotnet new osirion-blog: the newest posts on the home page'},
        {'shot': 'osirion-docs', 'source': 'template:osirion-docs', 'path': '/docs', 'alt': 'A documentation site created with dotnet new osirion-docs: side navigation, a page and its table of contents'},
        {'shot': 'osirion-site', 'source': 'template:osirion-site', 'path': '/', 'alt': 'A product site created with dotnet new osirion-site: a hero, three steps and the latest news'},
    ],
    '/docs/getting-started': [
        {'shot': 'first-blog', 'source': 'template:osirion-blog', 'path': '/', 'alt': 'The blog the getting started commands create, running on localhost'},
    ],
    '/themes': [
        # The page lists the palettes in a table; the shot shows what they do: a documentation page in each one. The home
        # page hero would not show it, because its buttons are Bootstrap buttons that keep their compiled color.
        {'shot': 'palettes', 'source': 'site', 'path': '/docs/getting-started', 'palettes': ['osirion', 'commit', 'editor', 'studio', 'studio-blue', 'contrast'], 'themes': ['dark', 'light'], 'alt': 'The getting started page of getosirion.com in the six palettes: Osirion, Commit, Editor, Studio, Studio Blue and Contrast'},
    ],
    '/blog/modern-hero-sections-blazor': [
        {'shot': 'hero-variants', 'source': 'site', 'path': '/components/hero-section', 'selector': '.osirion-content-demo-preview', 'alt': 'HeroSection with a side image, as the post builds it'},
    ],
    '/blog/navigation-components': [
        {'shot': 'menu', 'source': 'site', 'path': '/components/menu', 'selector': '.osirion-content-demo-preview', 'hover': '.osirion-content-demo-preview .osirion-menu-item-has-submenu > a.osirion-menu-item', 'alt': 'An Osirion menu with the Docs dropdown open'},
    ],
    '/blog/gdpr-cookie-consent-blazor': [
        # The component page has no live demo of the banner; the site's own banner is the real thing.
        {'shot': 'consent-banner', 'source': 'site', 'path': '/', 'bottom': 360, 'keepConsent': True, 'alt': 'The cookie consent banner at the bottom of getosirion.com, with Decline, Customize and Accept All'},
    ],
    '/blog/osirion-blazor-4-0-components': [
        {'shot': 'gallery', 'source': 'site', 'path': '/components', 'selector': 'main', 'maxHeight': 900, 'alt': 'The component gallery on getosirion.com'},
    ],
    '/blog/github-cms-with-blazor': [
        {'shot': 'blog-from-github', 'source': 'site', 'path': '/blog', 'selector': 'main', 'maxHeight': 900, 'alt': 'A blog list rendered from Markdown files in a GitHub repository'},
    ],
}


def front_matter(text):
    match = re.match(r'---\r?\n(.*?)\r?\n---', text, re.S)
    if not match:
        return {}
    data = {}
    current = None
    for line in match.group(1).splitlines():
        top = re.match(r'^([a-z_]+):\s*(.*)$', line)
        nested = re.match(r'^  ([a-z_]+):\s*(.*)$', line)
        if top:
            current = top.group(1)
            data[current] = top.group(2).strip().strip('\'"')
        elif nested and current in ('seo_properties', 'custom_fields'):
            data.setdefault(current + '.' + nested.group(1), nested.group(2).strip().strip('\'"'))
    return data


def site_path(relative, data):
    permalink = data.get('permalink')
    if permalink:
        return '/' + permalink.strip('/') if permalink.strip('/') else '/'
    folder, name = os.path.split(relative[:-3])
    slug = data.get('slug') or name
    return '/' + '/'.join(part for part in folder.replace(os.sep, '/').split('/') + [slug] if part)


def section(path, data):
    if path == '/':
        return 'Osirion.Blazor'
    if path.startswith('/components'):
        return 'Components'
    if path.startswith('/docs/packages'):
        return 'Packages'
    if path.startswith('/docs'):
        return 'Guides' if path != '/docs/getting-started' else 'Getting started'
    if path.startswith('/blog'):
        return 'Blog'
    return 'Osirion.Blazor'


entries = []
for directory, _, files in os.walk(CONTENT):
    for name in sorted(files):
        if not name.endswith('.md'):
            continue
        full = os.path.join(directory, name)
        relative = os.path.relpath(full, CONTENT)
        data = front_matter(open(full, encoding='utf-8-sig').read())
        if data.get('published', 'true') == 'false' or 'noindex' in data.get('seo_properties.robots', ''):
            continue
        path = site_path(relative, data)
        slug = 'home' if path == '/' else path.strip('/').replace('/', '-')
        entries.append({
            'file': relative.replace(os.sep, '/'),
            'path': path,
            'section': section(path, data),
            'title': data.get('title', ''),
            'card': {
                'key': f'og/{slug}.jpg',
                'url': f'{MEDIA_HOST}/og/{slug}.jpg',
                'heading': data.get('seo_properties.title') or data.get('title', ''),
                'theme': 'dark',
            },
            'featured_image_now': data.get('featured_image', ''),
            'shots': [
                {
                    **shot,
                    'themes': shot.get('themes', ['dark']),
                    'key': f"shots/{slug}/{shot['shot']}-{{theme}}.webp",
                }
                for shot in SHOTS.get(path, [])
            ],
        })

# Pages the site renders without a content file of their own.
entries.append({
    'file': 'CHANGELOG.md',
    'path': '/changelog',
    'section': 'Osirion.Blazor',
    'title': 'Changelog',
    'card': {'key': 'og/changelog.jpg', 'url': f'{MEDIA_HOST}/og/changelog.jpg', 'heading': 'Changelog: every Osirion.Blazor release', 'theme': 'dark'},
    'featured_image_now': '',
    'shots': [],
})

entries.sort(key=lambda entry: (entry['section'], entry['path']))
missing = sorted(set(SHOTS) - {entry['path'] for entry in entries})
manifest = {
    'mediaHost': MEDIA_HOST,
    'card': {'width': 1200, 'height': 630, 'format': 'jpeg', 'quality': 85},
    'shot': {'viewport': [1280, 800], 'deviceScaleFactor': 2, 'format': 'webp', 'quality': 80, 'maxWidth': 1600},
    'pages': entries,
}
os.makedirs(os.path.join('tools', 'site-media'), exist_ok=True)
with open(os.path.join('tools', 'site-media', 'manifest.json'), 'w', encoding='utf-8', newline='\n') as handle:
    json.dump(manifest, handle, indent=2, ensure_ascii=False)
    handle.write('\n')

shots = sum(len(entry['shots']) * len(entry['shots'][0]['themes']) if entry['shots'] else 0 for entry in entries)
print(f"{len(entries)} pages, {len(entries)} cards, {sum(len(e['shots']) for e in entries)} shot specs, unknown shot pages: {missing}")
for name, count in sorted({e['section']: 0 for e in entries}.items()):
    print(' ', name, sum(1 for e in entries if e['section'] == name))
