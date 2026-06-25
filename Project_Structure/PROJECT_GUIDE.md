# プロジェクト構造ガイド

> 「DBの構造・フロントエンド・バックエンドの基本概念は分かる、Vue.jsは初めて」な人向けの解説

---

## 1. このプロジェクトは何か

**匿名掲示板**のWebアプリ。技術的には2つの独立したアプリで構成されている。

```
Board_geachu/
├── BoardApi/       ← バックエンド（C# / ASP.NET Core）
└── board-front/    ← フロントエンド（Vue.js / TypeScript）
```

ブラウザ（Vue.js）が API（ASP.NET Core）に HTTP リクエストを投げて、
API が DB（SQL Server Express）と通信する、よくある3層構造。

```
ブラウザ ←→ Vue.js（:5173）  ←→  ASP.NET Core API（:5000）  ←→  SQL Server Express
  画面表示        HTTP/JSON              C#コード                    データ永続化
```

---

## 2. データベース構造

DB名: `BoardDb`（SQL Server Express の `.\SQLEXPRESS` に存在）

### テーブル一覧

**Users テーブル**
```
Id          int          主キー（自動採番）
Username    nvarchar(50) ユーザー名（必須）
PasswordHash nvarchar    BCryptハッシュ化されたパスワード（平文は保存しない）
CreatedAt   datetime     作成日時
```

**Posts テーブル**
```
Id          int           主キー（自動採番）
Title       nvarchar(200) タイトル（必須）
Content     nvarchar(max) 本文（必須）
CreatedAt   datetime      作成日時
ViewCount   int           閲覧数（デフォルト:0）
LikeCount   int           いいね数（デフォルト:0）
DelFlag     bit           削除フラグ（0=表示、1=非表示）
```

**Comments テーブル**
```
Id          int           主キー（自動採番）
PostId      int           外部キー → Posts.Id
Content     nvarchar(max) コメント内容（必須）
CreatedAt   datetime      作成日時
DelFlag     bit           削除フラグ（0=表示、1=非表示）
```

### テーブルの関係

```
Posts (1) ────< Comments (多)
   PostIdで紐付け。Postを削除したらCommentも一緒に消える（Cascade Delete）
```

> **論理削除について**  
> `DELETE`文でレコードを消すのではなく、`DelFlag = true` にセットして「見えないようにする」方式。
> データが残るのでリカバリーしやすい。

---

## 3. バックエンド（BoardApi）

### ディレクトリ構成

```
BoardApi/
├── Models/             ← DBのテーブルに対応するC#クラス
│   ├── User.cs
│   ├── Post.cs
│   └── Comment.cs
├── DTOs/               ← APIのリクエスト/レスポンスの型定義
│   ├── LoginRequest.cs
│   ├── RegisterRequest.cs
│   └── AuthResponse.cs
├── Data/
│   └── AppDbContext.cs ← EF Core の設定（DBとの橋渡し役）
├── Controllers/        ← URLのルーティングと処理
│   ├── AuthController.cs   （/api/auth/...）
│   └── PostsController.cs  （/api/posts/...）
├── Migrations/         ← DBのスキーマ変更履歴（自動生成）
├── Startup.cs          ← アプリの初期設定（DI・CORS・JWT等）
├── Program.cs          ← アプリの起動エントリーポイント
└── appsettings.json    ← 設定ファイル（接続文字列・JWTキー等）
```

### 役割の説明

**Models/** — DBのテーブル1つ = C#クラス1つ  
EF Core（Entity Framework Core）が「このクラス = このテーブル」と解釈して SQL を自動生成してくれる。

**DTOs/**（Data Transfer Object）— API の入出力専用の型  
例えば `RegisterRequest.cs` には `Username` と `Password` だけ定義されている。
Model（`User.cs`）には `PasswordHash` があるが、API 経由で直接返したくないため分離している。

**AppDbContext.cs** — DB との接続窓口  
```
DbSet<Post> Posts    → Posts テーブルを操作するためのオブジェクト
DbSet<Comment> Comments
DbSet<User> Users
```

**Controllers/** — URL に対して「何をするか」を定義  
`[HttpGet]`, `[HttpPost]`, `[Authorize]` といった属性（アノテーション）でルートと認証を制御する。

### APIエンドポイント一覧

| メソッド | URL | 認証必須 | 説明 |
|---------|-----|---------|------|
| POST | /api/auth/register | × | ユーザー登録 |
| POST | /api/auth/login | × | ログイン → JWTトークン発行 |
| GET | /api/posts | × | 投稿一覧（DelFlag=false のみ） |
| GET | /api/posts/{id} | × | 投稿詳細（ViewCount++) |
| POST | /api/posts | ○ | 新規投稿 |
| DELETE | /api/posts/{id} | ○ | 投稿の論理削除 |
| GET | /api/posts/{id}/comments | × | コメント一覧 |
| POST | /api/posts/{id}/comments | × | コメント投稿（匿名OK） |
| DELETE | /api/posts/{id}/comments/{cId} | ○ | コメントの論理削除 |
| POST | /api/posts/{id}/like | ○ | いいね（LikeCount++) |

### 認証の仕組み（JWT）

1. ログイン → API が **JWTトークン**（長い文字列）を返す
2. フロントエンドが `localStorage` に保存
3. 以降のリクエストで `Authorization: Bearer {トークン}` ヘッダーを付ける
4. API 側で `[Authorize]` 属性がついているエンドポイントはトークンを検証する
5. トークンは **7日間有効**、パスワードは **BCrypt** でハッシュ化して保存

---

## 4. フロントエンド（board-front）

### ディレクトリ構成

```
board-front/src/
├── main.ts             ← アプリの起動エントリーポイント
├── App.vue             ← 全ページ共通の外枠（ヘッダー・レイアウト）
├── router/
│   └── index.ts        ← URL と画面の対応表
├── api/                ← バックエンドへのHTTPリクエスト処理
│   ├── client.ts       （Axiosの共通設定）
│   ├── auth.ts         （ログイン・登録API）
│   ├── posts.ts        （投稿関連API）
│   └── comments.ts     （コメント関連API）
├── composables/        ← 画面をまたいで使う共通ロジック
│   └── useAuth.ts      （ログイン状態の管理）
├── types/              ← TypeScriptの型定義
│   ├── post.ts
│   ├── comment.ts
│   └── auth.ts
└── views/              ← 各ページの画面コンポーネント
    ├── HomeView.vue        （/ : 投稿一覧）
    ├── PostDetailView.vue  （/posts/:id : 投稿詳細+コメント）
    ├── PostNewView.vue     （/posts/new : 新規投稿フォーム）
    ├── LoginView.vue       （/login : ログイン）
    └── RegisterView.vue    （/register : 新規登録）
```

### Vue.js の基本概念（初めての人向け）

#### `.vue` ファイルとは何か
1つのファイルに「HTML + JavaScript + CSS」が全部入っている。これを**単一ファイルコンポーネント（SFC）**と呼ぶ。

```vue
<template>
  <!-- ここに HTML を書く -->
  <h1>{{ title }}</h1>
  <button @click="doSomething">クリック</button>
</template>

<script setup lang="ts">
// ここに TypeScript を書く
import { ref } from 'vue'
const title = ref('こんにちは')  // リアクティブな変数
function doSomething() { ... }
</script>

<style scoped>
/* ここに CSS を書く（scoped = このコンポーネント内だけに適用） */
</style>
```

#### よく使う Vue の機能

| 記法 | 意味 | 例 |
|------|------|----|
| `{{ 変数 }}` | 変数の値を画面に表示 | `{{ post.title }}` |
| `v-for` | 配列をループして繰り返し表示 | `v-for="post in posts"` |
| `v-if` | 条件が true の時だけ表示 | `v-if="isLoggedIn"` |
| `@click` | クリックイベント | `@click="deletePost(id)"` |
| `v-model` | フォームの入力と変数を双方向バインド | `v-model="inputText"` |
| `ref()` | リアクティブな変数（値が変わると画面も更新） | `const count = ref(0)` |
| `computed()` | 他の変数から計算する読み取り専用変数 | `const isLoggedIn = computed(...)` |
| `onMounted()` | ページが表示された時に実行する処理 | データの初期取得 |

#### `<script setup>` とは
Vue 3 の新しい記法。`setup()` 関数を明示的に書かずに済む簡略版。
ここで定義した変数・関数は、自動的に `<template>` 内で使える。

#### Router（ルーター）とは
URL と「どの .vue ファイルを表示するか」を対応付けるもの。
```
/              → HomeView.vue
/posts/new     → PostNewView.vue
/posts/123     → PostDetailView.vue（id=123）
/login         → LoginView.vue
/register      → RegisterView.vue
```
`<RouterLink to="/posts/new">` でページ遷移、`useRouter().push('/')` でコード内から遷移。

#### Composable（コンポーザブル）とは
複数の画面で使いまわすロジックをまとめたもの。`use〇〇` という名前にするのが慣習。
```
useAuth.ts = ログイン状態の管理
  ・isLoggedIn  → ログインしているか
  ・login()     → ログイン処理
  ・logout()    → ログアウト処理
  ・username    → 現在のユーザー名
```
どの画面でも `const { isLoggedIn, logout } = useAuth()` と書けば使える。

#### Axios とは
HTTP リクエストを送るためのライブラリ。`fetch()` の使いやすい版。
```typescript
// こういうコードでバックエンドの API を呼ぶ
const response = await axios.get('http://localhost:5000/api/posts')
const posts = response.data  // 受け取ったJSONデータ
```
`client.ts` で Axios インスタンスを共通設定している（BaseURL・認証ヘッダーの自動付与）。

### 画面ごとの説明

**App.vue（共通レイアウト）**  
全ページで表示されるヘッダーを定義している。ログイン状態によってナビゲーションのリンクが変わる。
`<RouterView />` という部分に、現在の URL に対応する画面が差し込まれる。

```
┌────────────────────────────────────┐
│ ヘッダー（ナビゲーション）[App.vue] │
├────────────────┬───────────────────┤
│                │                   │
│  <RouterView>  │   サイドバー      │
│  （各ページの  │  （900px以上で    │
│   コンテンツ） │   表示）          │
│                │                   │
└────────────────┴───────────────────┘
```

**HomeView.vue（投稿一覧）**  
- ページ表示時（`onMounted`）に `getPosts()` でAPI呼び出し
- 投稿を新着順にリスト表示
- ログイン中なら削除ボタンを表示（`v-if="isLoggedIn"`）
- タイトルをクリックで `/posts/{id}` へ遷移

**PostDetailView.vue（投稿詳細）**  
- URLの `id` パラメータで投稿を1件取得
- いいね・削除・コメント投稿機能を含む
- コメント一覧は古い順に表示

**PostNewView.vue（新規投稿）**  
- タイトル・本文フォーム
- 投稿成功後はホーム（`/`）にリダイレクト

**LoginView.vue / RegisterView.vue**  
- フォームの入力 → API 呼び出し → JWT トークンを localStorage に保存
- 成功後はホームへリダイレクト

---

## 5. フロントエンドとバックエンドの繋がり

### データの流れ例：投稿一覧を表示するまで

```
① ブラウザが / にアクセス
      ↓
② Vue Router が HomeView.vue を表示
      ↓
③ onMounted() が発火
      ↓
④ api/posts.ts の getPosts() を呼ぶ
      ↓
⑤ Axios が GET http://localhost:5000/api/posts を送信
      ↓
⑥ PostsController.cs の [HttpGet] メソッドが受け取る
      ↓
⑦ AppDbContext 経由で SELECT * FROM Posts WHERE DelFlag=0 を実行
      ↓
⑧ JSON でレスポンスを返す
      ↓
⑨ Vue の posts 変数（ref）が更新される
      ↓
⑩ v-for で画面に投稿リストが描画される
```

### 認証が必要な操作（例：いいね）

```
① ログイン済み → localStorage に token が存在
      ↓
② api/client.ts の Axios が全リクエストに Authorization: Bearer {token} を付加
      ↓
③ POST /api/posts/{id}/like を送信
      ↓
④ PostsController の [Authorize] がトークンを検証
      ↓
⑤ 有効なら LikeCount++ して保存
      ↓
⑥ 更新後の LikeCount を JSON で返す
      ↓
⑦ 画面の likeCount 変数が更新され、表示も変わる
```

---

## 6. 開発サーバーの起動方法

**バックエンド（BoardApi）**
```bash
cd BoardApi
dotnet run
# → http://localhost:5000 で起動
```

**フロントエンド（board-front）**
```bash
# Git Bash で実行（PowerShellだとスクリプトポリシーエラーが出る）
cd board-front
npm run dev
# → http://localhost:5173 で起動
```

> VS Code のタスク（`folderOpen`）で両方自動起動するよう設定済み。

---

## 7. 未実装の機能

| 機能 | 説明 |
|------|------|
| 投稿のオーナー管理 | 現在 Posts に UserId がない。自分の投稿だけ削除できる仕組みが未完成 |
| コメントのログイン必須化 | 現在コメントは誰でも投稿できる |
| ページネーション | 全件取得してしまっている |
| 投稿検索 | 未実装 |
| グローバルエラーハンドラー | API エラー時の共通処理が未実装 |
