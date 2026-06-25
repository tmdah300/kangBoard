# board-front

このテンプレートは、Vite での Vue 3 開発を始める際の参考になるはずです。

## 推奨される IDE の設定

[VS Code](https://code.visualstudio.com/) ＋ [Vue (公式)](https://marketplace.visualstudio.com/items?itemName=Vue.volar) （Vetur は無効にしてください）。

## 推奨されるブラウザの設定

- Chromiumベースのブラウザ（Chrome、Edge、Braveなど）：
  - [Vue.js デベロッパーツール](https://chromewebstore.google.com/detail/vuejs-devtools/nhdogjmejiglipccpnnnanhbledajbpd)
  - [Chrome DevToolsでカスタムオブジェクトフォーマッタを有効にする](http://bit.ly/object-formatters)
- Firefox：
  - [Vue.js デベロッパーツール](https://addons.mozilla.org/en-US/firefox/addon/vue-js-devtools/)
  - [Firefox DevToolsでカスタムオブジェクトフォーマッタを有効にする](https://fxdx.dev/firefox-devtools-custom-object-formatters/)

## TSにおける`.vue`インポートの型サポート

TypeScriptはデフォルトでは`.vue`インポートの型情報を処理できないため、型チェックには`tsc` CLIの代わりに`vue-tsc`を使用します。エディタでは、TypeScript言語サービスに`.vue`の型を認識させるために[Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar)が必要です。

## 設定のカスタマイズ

[Vite 設定リファレンス](https://vite.dev/config/)を参照してください。

## プロジェクトのセットアップ

```sh
npm install
```

### 開発用のコンパイルとホットリロード

```sh
npm run dev
```

### 本番環境用の型チェック、コンパイル、およびミニファイ

```sh
npm run build
```
