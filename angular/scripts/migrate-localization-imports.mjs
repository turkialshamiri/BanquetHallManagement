import fs from 'node:fs';
import path from 'node:path';

const root = path.resolve('src/app');
const pipeImport =
  "import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';";
const serviceImport =
  "import { AppLocalizationService } from 'src/app/core/services/app-localization.service';";

const skip = new Set([
  path.normalize('src/app/core/pipes/app-localization.pipe.ts'),
  path.normalize('src/app/core/services/app-localization.service.ts'),
  path.normalize('src/app/core/services/language-direction.service.ts'),
]);

function walk(dir, files = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      walk(full, files);
    } else if (entry.name.endsWith('.ts')) {
      files.push(full);
    }
  }
  return files;
}

for (const file of walk(root)) {
  const rel = path.relative(process.cwd(), file).replaceAll('\\', '/');
  if (skip.has(rel)) {
    continue;
  }

  let content = fs.readFileSync(file, 'utf8');
  if (!content.includes('LocalizationPipe') && !content.includes('LocalizationService')) {
    continue;
  }

  content = content.replaceAll('LocalizationPipe', 'AppLocalizationPipe');
  content = content.replaceAll('inject(LocalizationService)', 'inject(AppLocalizationService)');
  content = content.replaceAll('l10n: LocalizationService', 'l10n: AppLocalizationService');

  content = content.replace(
    /import\s*\{([^}]*)\}\s*from\s*'@abp\/ng\.core';/g,
    (match, imports) => {
      const parts = imports
        .split(',')
        .map((p) => p.trim())
        .filter(Boolean)
        .filter((p) => p !== 'AppLocalizationPipe' && p !== 'AppLocalizationService');
      if (parts.length === 0) {
        return '';
      }
      return `import { ${parts.join(', ')} } from '@abp/ng.core';`;
    }
  );

  if (content.includes('AppLocalizationPipe') && !content.includes(pipeImport)) {
    const abpImport = content.match(/import\s*\{[^}]+\}\s*from\s*'@abp\/ng\.core';/);
    if (abpImport) {
      content = content.replace(abpImport[0], `${abpImport[0]}\n${pipeImport}`);
    } else {
      content = `${pipeImport}\n${content}`;
    }
  }

  if (content.includes('AppLocalizationService') && !content.includes(serviceImport)) {
    content = content.replace(pipeImport, `${pipeImport}\n${serviceImport}`);
    if (!content.includes(serviceImport)) {
      content = `${serviceImport}\n${content}`;
    }
  }

  content = content.replace(/\n{3,}/g, '\n\n');
  fs.writeFileSync(file, content);
}

console.log('Localization imports migrated.');
