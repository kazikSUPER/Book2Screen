import js from '@eslint/js';
import pluginVue from 'eslint-plugin-vue';
import vueTsEslintConfig from '@vue/eslint-config-typescript';
import skipFormatting from '@vue/eslint-config-prettier/skip-formatting';

export default [
  { files: ['**/*.{ts,mts,tsx,vue}'] },
  { ignores: ['**/dist/**', '**/node_modules/**'] },

  js.configs.recommended,
  ...pluginVue.configs['flat/essential'],
  ...vueTsEslintConfig(),
  skipFormatting,

  {
    rules: {
      'id-denylist': ['error', 'data', 'info', 'temp'],
      'vue/multi-word-component-names': 'off',
    },
  },
];
