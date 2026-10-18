// Captures the screenshots listed in manifest.json into out/<shot key>, one file per theme.
//
//   node shots.mjs --site http://127.0.0.1:5390 [--template osirion-blog=http://127.0.0.1:5391 ...]
//                  [--only <shot names>] [--channel msedge] [--probe]
//
// A "site" shot comes from the Bootstrap example run locally with the repository's content (Development, so its
// analytics are off); a "template:<name>" shot from a site created with `dotnet new <name>` and run locally. The theme
// is set with the site's own preference cookies, so the page renders dark or light from the first paint, as a visitor
// who picked it sees it. The announcement bar and the site's cookie banner are hidden; an embedded demo of the cookie
// banner is not. --probe prints how many elements each selector matches instead of capturing.
import { chromium } from 'playwright';
import { mkdir, readFile } from 'node:fs/promises';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const args = process.argv.slice(2);
const option = (name) => {
  const index = args.indexOf(name);
  return index >= 0 ? args[index + 1] : undefined;
};
const options = (name) => args.flatMap((value, index) => (args[index - 1] === name ? [value] : []));

const manifest = JSON.parse(await readFile(join(here, 'manifest.json'), 'utf8'));
const hosts = { site: option('--site') };
for (const pair of options('--template')) {
  const [name, url] = pair.split('=');
  hosts[`template:${name}`] = url;
}
const only = option('--only')?.split(',');
const probe = args.includes('--probe');
const { viewport, deviceScaleFactor, quality } = manifest.shot;

const shots = manifest.pages.flatMap((page) => page.shots.map((shot) => ({ ...shot, page: page.path })))
  .filter((shot) => !only || only.includes(shot.shot));

// Hides what is not the subject: the announcement bar and the site-wide cookie banner, but not a banner shown as a
// demo inside the page.
const hideChrome = `
  .osirion-announcement { display: none !important; }
  *, *::before, *::after { animation: none !important; transition: none !important; caret-color: transparent !important; }`;

const browser = await chromium.launch(option('--channel') ? { channel: option('--channel') } : {});
let failures = 0;
for (const shot of shots) {
  const base = hosts[shot.source];
  if (!base) {
    console.log(`skip ${shot.shot}: no address for ${shot.source}`);
    continue;
  }

  for (const theme of shot.themes) {
   for (const palette of shot.palettes ?? ['osirion']) {
    const context = await browser.newContext({
      viewport: { width: viewport[0], height: viewport[1] },
      deviceScaleFactor,
      reducedMotion: 'reduce',
      colorScheme: theme,
    });
    const host = new URL(base).hostname;
    await context.addCookies([
      { name: 'osirion-preferred-theme', value: theme, domain: host, path: '/' },
      { name: 'osirion-preferred-palette', value: palette, domain: host, path: '/' },
    ]);
    const tab = await context.newPage();
    await tab.goto(new URL(shot.path, base).href, { waitUntil: 'networkidle' });
    await tab.addStyleTag({ content: hideChrome });
    await tab.evaluate((keepConsent) => {
      for (const banner of document.querySelectorAll('.osirion-cookie-consent')) {
        if (!keepConsent && !banner.closest('.osirion-content-demo-preview')) banner.style.display = 'none';
      }
      document.activeElement?.blur();
    }, Boolean(shot.keepConsent));
    if (shot.hover) {
      await tab.hover(shot.hover);
    }
    await tab.evaluate(() => document.fonts.ready);
    await tab.waitForTimeout(300);

    const selector = shot.selector ?? 'body';
    const count = await tab.locator(selector).count();
    if (probe) {
      console.log(`${shot.shot} ${theme} ${palette} ${shot.path} ${selector}: ${count}`);
      await context.close();
      continue;
    }

    // A palette shot saves one tile per palette; optimize.py puts the tiles together in one image.
    const key = shot.key.replace('{theme}', shot.palettes ? `${theme}-${palette}` : theme);
    const target = join(here, 'out', key.replace(/\.webp$/, '.png'));
    await mkdir(dirname(target), { recursive: true });
    if (count === 0) {
      console.log(`FAIL ${key}: ${selector} matches nothing on ${shot.path}`);
      failures++;
    } else if (shot.selector && shot.maxHeight) {
      // A long element (a page's main column) is cut at maxHeight CSS pixels from its top.
      const box = await tab.locator(selector).first().boundingBox();
      await tab.screenshot({ path: target, fullPage: true, clip: { x: box.x, y: box.y, width: box.width, height: Math.min(box.height, shot.maxHeight) } });
      console.log(key);
    } else if (shot.selector) {
      await tab.locator(selector).first().screenshot({ path: target });
      console.log(key);
    } else if (shot.bottom) {
      // The bottom of the viewport, for something pinned there such as the cookie banner, with the page above it.
      await tab.screenshot({ path: target, clip: { x: 0, y: viewport[1] - shot.bottom, width: viewport[0], height: shot.bottom } });
      console.log(key);
    } else {
      await tab.screenshot({ path: target });
      console.log(key);
    }
    await context.close();
   }
  }
}
await browser.close();
if (!probe) console.log(`PNG captures in ${join(here, 'out')}; optimize.py turns them into WebP (quality ${quality}).`);
process.exit(failures ? 1 : 0);
