import assert from 'node:assert/strict';
import { readFile, writeFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import sharp from 'sharp';

const root = new URL('../../src/Linkubator.Web/wwwroot/', import.meta.url);
const source = await readFile(new URL('brand/symbol.svg', root), 'utf8');
const wordmark = await readFile(new URL('wordmark.svg', import.meta.url), 'utf8');
const wordmarkBox = wordmark.match(/viewBox="0 0 ([\d.]+) ([\d.]+)"/);
assert.ok(wordmarkBox, 'The outlined wordmark must have a viewBox.');
const inner = (svg) => svg.replace(/^[\s\S]*?<svg[^>]*>/, '').replace(/<\/svg>\s*$/, '').replace(/<title>[\s\S]*?<\/title>/g, '').trim();
const icon = inner(source);
const letters = inner(wordmark);
const textScale = 38 / Number(wordmarkBox[2]);
const textWidth = Number(wordmarkBox[1]) * textScale;
const logoWidth = Math.ceil(80 + textWidth + 18);
const frameSizes = [16, 32, 48, 64, 256];
const pngs = [
  ['favicon-16x16.png', 16],
  ['favicon-32x32.png', 32],
  ['brand/icon-192.png', 192],
  ['brand/icon-512.png', 512],
  ['apple-touch-icon.png', 180],
];
const variants = [
  ['logo-dark.svg', '#f3f4f6', '#5fffa6', icon],
  ['logo-light.svg', '#070b0d', '#19723f', icon],
  ['logo-monochrome-dark.svg', '#ffffff', '#ffffff', icon.replace('#5fffa6', '#ffffff')],
  ['logo-monochrome-light.svg', '#070b0d', '#070b0d', icon.replace('#070b0d', '#ffffff').replace('#5fffa6', '#070b0d')],
];
const svgDocument = (content, width = 64) =>
  `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="64" viewBox="0 0 ${width} 64"><title>Linkubator</title>${content}</svg>\n`;
const rasterize = (svg, size) => sharp(Buffer.from(svg), { density: 384 })
  .resize(size, size, { kernel: 'lanczos3' }).png().toBuffer();

if (!process.argv.includes('--check')) {
  for (const [name, textColor, dotColor, mark] of variants) {
    const logo = svgDocument(
      `${mark}<g fill="${textColor}" transform="translate(80 13) scale(${textScale.toFixed(6)})">${letters}</g><circle cx="${(80 + textWidth + 10).toFixed(3)}" cy="48" r="3.5" fill="${dotColor}"/>`,
      logoWidth);
    await writeFile(new URL(`brand/${name}`, root), logo);
    await writeFile(new URL(`brand/${name.replace('.svg', '.png')}`, root),
      await sharp(Buffer.from(logo), { density: 384 }).resize(logoWidth * 4, 256).png().toBuffer());
  }
  for (const [name, color] of [['symbol-black.svg', '#070b0d'], ['symbol-white.svg', '#ffffff']]) {
    await writeFile(new URL(`brand/${name}`, root), svgDocument(
      icon.replace(/<rect[^>]*\/>/, '').replace('#070b0d', color)));
  }
  await writeFile(new URL('favicon.svg', root), source);
  for (const [name, size] of pngs) {
    const svg = name === 'apple-touch-icon.png' ? source.replace('rx="17"', 'rx="0"') : source;
    await writeFile(new URL(name, root), await rasterize(svg, size));
  }
  const frames = await Promise.all(frameSizes.map(size => rasterize(source, size)));
  const header = Buffer.alloc(6 + frames.length * 16);
  header.writeUInt16LE(1, 2);
  header.writeUInt16LE(frames.length, 4);
  let offset = header.length;
  frames.forEach((frame, index) => {
    const entry = 6 + index * 16;
    header[entry] = frameSizes[index] === 256 ? 0 : frameSizes[index];
    header[entry + 1] = header[entry];
    header.writeUInt16LE(1, entry + 4);
    header.writeUInt16LE(32, entry + 6);
    header.writeUInt32LE(frame.length, entry + 8);
    header.writeUInt32LE(offset, entry + 12);
    offset += frame.length;
  });
  await writeFile(new URL('favicon.ico', root), Buffer.concat([header, ...frames]));
}

for (const [name, size] of pngs) {
  const buffer = await readFile(new URL(name, root));
  const metadata = await sharp(buffer).metadata();
  assert.equal(metadata.format, 'png', `${name}: format`);
  assert.equal(metadata.width, size, `${name}: width`);
  assert.equal(metadata.height, size, `${name}: height`);
  if (name === 'apple-touch-icon.png') {
    const { channels } = metadata;
    const pixels = await sharp(buffer).ensureAlpha().raw().toBuffer();
    assert.ok(channels >= 3);
    assert.equal(pixels[3], 255, 'The Apple icon must have an opaque square background.');
  }
}
assert.equal(await readFile(new URL('favicon.svg', root), 'utf8'), source);
const ico = await readFile(new URL('favicon.ico', root));
assert.equal(ico.readUInt16LE(0), 0);
assert.equal(ico.readUInt16LE(2), 1);
assert.equal(ico.readUInt16LE(4), frameSizes.length);
for (const [index, size] of frameSizes.entries()) {
  const entry = 6 + index * 16;
  assert.equal(ico[entry] || 256, size);
  assert.equal(ico[entry + 1] || 256, size);
  const length = ico.readUInt32LE(entry + 8);
  const offset = ico.readUInt32LE(entry + 12);
  const metadata = await sharp(ico.subarray(offset, offset + length)).metadata();
  assert.equal(metadata.format, 'png');
  assert.equal(metadata.width, size);
  assert.equal(metadata.height, size);
}
for (const [name] of variants) {
  const svg = await readFile(new URL(`brand/${name}`, root), 'utf8');
  assert.ok(!/<text\b|<image\b|href=/.test(svg), `${name} must be self-contained vector paths.`);
  const metadata = await sharp(Buffer.from(svg)).metadata();
  assert.equal(metadata.width, logoWidth);
  assert.equal(metadata.height, 64);
  const png = await sharp(await readFile(new URL(`brand/${name.replace('.svg', '.png')}`, root))).metadata();
  assert.equal(png.format, 'png');
  assert.equal(png.width, logoWidth * 4);
  assert.equal(png.height, 256);
}
console.log(`Brand assets verified: ${pngs.length + variants.length} PNGs, ${frameSizes.length} ICO frames, SVG icon and 4 outlined logos.`);
console.log(fileURLToPath(new URL('brand/', root)));
