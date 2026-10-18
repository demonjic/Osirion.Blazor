// Renders the 1200 x 630 Open Graph card of every page in manifest.json into out/<card key>.
//
//   cd tools/site-media && npm ci && node cards.mjs [--only home,components-tabs] [--channel msedge]
//
// The card is HTML rendered by a real browser, so type, wrapping and the mark look as they do on the site. The colors
// are the site's dark theme (examples/Osirion.Blazor.Example.Bootstrap/wwwroot/app.scss): the near-black page, the
// Blazor purple scale for the accent. Without --channel the browser Playwright installs is used (npx playwright
// install chromium); --channel msedge uses the installed Edge instead.
import { chromium } from 'playwright';
import { mkdir, readFile } from 'node:fs/promises';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const root = join(here, '..', '..');
const args = process.argv.slice(2);
const option = (name) => {
  const index = args.indexOf(name);
  return index >= 0 ? args[index + 1] : undefined;
};

const manifest = JSON.parse(await readFile(join(here, 'manifest.json'), 'utf8'));
const only = option('--only')?.split(',');
const pages = manifest.pages.filter((page) => !only || only.includes(page.card.key.replace(/^og\/|\.jpg$/g, '')));
const mark = (await readFile(join(root, 'examples', 'Osirion.Blazor.Example.Bootstrap', 'wwwroot', 'favicon.png'))).toString('base64');

const escape = (text) => text.replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[c]);

const capitalize = (text) => text.charAt(0).toUpperCase() + text.slice(1);

// "OsirionTabs: CSS-only tabs for Blazor" reads best as a title and a line under it.
function split(heading) {
  // The brand is on the card already, so a title-tag suffix such as " | Osirion.Blazor" is dropped.
  heading = heading.replace(/\s+\|\s+Osirion[ .]Blazor\s*$/i, '');
  const colon = heading.indexOf(': ');
  return colon > 0 && colon < heading.length - 2
    ? { title: heading.slice(0, colon), subtitle: capitalize(heading.slice(colon + 2)) }
    : { title: heading, subtitle: '' };
}

function html(page) {
  const { title, subtitle } = split(page.card.heading);
  // Long titles step down so every card keeps its title on at most three lines.
  const size = title.length > 52 ? 54 : title.length > 34 ? 64 : 76;
  return `<!doctype html><html><head><meta charset="utf-8"><style>
    * { box-sizing: border-box; margin: 0; }
    html, body { width: 1200px; height: 630px; }
    body {
      font-family: "Segoe UI", system-ui, -apple-system, Roboto, "Helvetica Neue", Arial, sans-serif;
      color: #e8e8f2;
      background:
        radial-gradient(circle at 92% 108%, rgba(115, 115, 196, 0.55) 0, rgba(74, 74, 150, 0.22) 30%, transparent 58%),
        radial-gradient(circle at 0% 0%, rgba(44, 44, 90, 0.7) 0, transparent 45%),
        #020617;
      padding: 64px 72px;
      display: flex; flex-direction: column; justify-content: space-between;
      position: relative; overflow: hidden;
    }
    body::before {
      content: ""; position: absolute; inset: 0;
      background-image: linear-gradient(rgba(144, 144, 214, 0.07) 1px, transparent 1px),
                        linear-gradient(90deg, rgba(144, 144, 214, 0.07) 1px, transparent 1px);
      background-size: 48px 48px;
      mask-image: linear-gradient(115deg, transparent 35%, #000 100%);
    }
    header, main, footer { position: relative; }
    header { display: flex; align-items: center; gap: 18px; }
    header img { width: 64px; height: 64px; }
    .brand { font-size: 30px; font-weight: 600; letter-spacing: -0.01em; color: #f4f4fb; }
    .section {
      margin-left: auto; font-size: 22px; font-weight: 600; letter-spacing: 0.08em; text-transform: uppercase;
      color: #b4b4e6; border: 2px solid rgba(180, 180, 230, 0.45); border-radius: 999px; padding: 8px 22px;
    }
    h1 {
      font-size: ${size}px; line-height: 1.14; padding-bottom: 0.08em; font-weight: 700; letter-spacing: -0.02em; color: #ffffff;
      display: -webkit-box; -webkit-line-clamp: 3; -webkit-box-orient: vertical; overflow: hidden; max-width: 1020px;
    }
    .subtitle {
      margin-top: 22px; font-size: 34px; line-height: 1.25; color: #c9c9e6; max-width: 980px;
      display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
    }
    footer { display: flex; align-items: center; gap: 16px; font-size: 26px; color: #9090d6; }
    footer .bar { width: 56px; height: 6px; border-radius: 3px; background: linear-gradient(90deg, #7373c4, #b4b4e6); }
  </style></head><body>
    <header>
      <img src="data:image/png;base64,${mark}" alt="">
      <span class="brand">Osirion.Blazor</span>
      ${page.section === 'Osirion.Blazor' ? '' : `<span class="section">${escape(page.section)}</span>`}
    </header>
    <main>
      <h1>${escape(title)}</h1>
      ${subtitle ? `<p class="subtitle">${escape(subtitle)}</p>` : ''}
    </main>
    <footer><span class="bar"></span><span>getosirion.com${page.path === '/' ? '' : escape(page.path)}</span></footer>
  </body></html>`;
}

const browser = await chromium.launch(option('--channel') ? { channel: option('--channel') } : {});
const context = await browser.newContext({ viewport: { width: 1200, height: 630 }, deviceScaleFactor: 1 });
const tab = await context.newPage();
for (const page of pages) {
  const target = join(here, 'out', page.card.key);
  await mkdir(dirname(target), { recursive: true });
  await tab.setContent(html(page), { waitUntil: 'load' });
  await tab.screenshot({ path: target, type: 'jpeg', quality: manifest.card.quality });
  console.log(page.card.key);
}
await browser.close();
console.log(`${pages.length} card(s) in ${join(here, 'out')}`);
