import sharp from 'sharp';
import { copyFileSync } from 'fs';

const SOURCE = './public/icons/logo.png';

const sizes = [72, 96, 128, 144, 152, 192, 384, 512];

for (const size of sizes) {
    await sharp(SOURCE)
        .resize(size, size, { fit: 'contain', background: { r: 24, g: 24, b: 27, alpha: 1 } })
        .png()
        .toFile(`./public/icons/icon-${size}x${size}.png`);

    console.log(`✓ icon-${size}x${size}.png`);
}

// favicon.png (32x32)
await sharp(SOURCE)
    .resize(32, 32, { fit: 'contain', background: { r: 24, g: 24, b: 27, alpha: 1 } })
    .png()
    .toFile('./public/favicon.png');

console.log('✓ favicon.png');
console.log('\nFeito! Substitua o favicon.ico pelo favicon.png no index.html se quiser.');
