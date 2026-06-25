# フロントエンド構成ガイド

Vue3 + TypeScript + Vite で構成された匿名掲示板フロントエンドの構造説明。

---

## ディレクトリ構成

```
board-front/src/
├── api/                  # バックエンドとの通信層
│   ├── client.ts         #   Axiosインスタンス（全APIの土台）
│   ├── posts.ts          #   投稿API
│   ├── comments.ts       #   コメントAPI
│   └── auth.ts           #   認証API
│
├── types/                # TypeScript型定義
│   ├── post.ts           #   Post型・CreatePostRequest型
│   ├── comment.ts        #   Comment型・CreateCommentRequest型
│   └── auth.ts           #   LoginRequest・RegisterRequest・AuthResponse型
│
├── composables/          # 再利用可能なロジック（Vue3 Composition API）
│   └── useAuth.ts        #   認証状態の管理（ログイン・ログアウト・登録）
│
├── router/               # 画面遷移の定義
│   └── index.ts          #   URLとViewコンポーネントのマッピング
│
├── views/                # ページ単位のコンポーネント
│   ├── HomeView.vue      #   / → 投稿一覧
│   ├── PostDetailView.vue #  /posts/:id → 投稿詳細＋コメント
│   ├── PostNewView.vue   #   /posts/new → 新規投稿フォーム
│   ├── LoginView.vue     #   /login → ログイン
│   └── RegisterView.vue  #   /register → 新規登録
│
├── components/           # 再利用可能なUI部品置き場
├── assets/               # CSSファイル・画像など静的ファイル
│   ├── main.css          #   グローバルスタイル
│   └── base.css          #   CSS変数・リセット
│
├── App.vue               # アプリケーションのルートコンポーネント（ヘッダー・レイアウト）
└── main.ts               # エントリーポイント（Vueアプリの起動）
```

---

## データの流れ（全体像）

```
ユーザー操作
     ↓
views/ (ページコンポーネント)
     ↓ api/を呼ぶ
api/ (HTTP通信)
     ↓ client.tsを経由
client.ts (Axiosインスタンス・JWTトークン自動付与)
     ↓ HTTP
ASP.NET Core バックエンド (localhost:5000/api)

composables/ ─── ログイン状態など複数Viewで共有する状態を管理
types/       ─── api/とviews/の間をつなぐ型定義
router/      ─── URLと表示するviewsを対応付け
```

---

## 各フォルダの詳細

---

### `api/` — バックエンドとの通信層

**役割:** HTTP通信の処理をまとめた場所。ViewからHTTPリクエストの詳細を隠蔽する。

#### `client.ts` — Axiosインスタンス（全APIの土台）

```ts
// api/client.ts
const apiClient = axios.create({
  baseURL: 'http://localhost:5000/api',
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})
```

- `axios.create()` でインスタンスを1つ作り、他のファイルが全てこれを使う
- `interceptors.request` はリクエスト送信の直前に割り込む処理（ミドルウェアのようなもの）
- localStorageからJWTトークンを取得して `Authorization: Bearer <token>` ヘッダーを自動で付与
- これにより各APIファイルで毎回トークン処理を書く必要がない

#### `posts.ts` — 投稿API

```ts
// api/posts.ts
export const getPosts = () => apiClient.get<Post[]>('/posts')
export const getPost = (id: number) => apiClient.get<Post>(`/posts/${id}`)
export const createPost = (data: CreatePostRequest) => apiClient.post<Post>('/posts', data)
export const likePost = (id: number) => apiClient.post<{ likeCount: number }>(`/posts/${id}/like`)
export const deletePost = (id: number) => apiClient.delete(`/posts/${id}`)
```

- `<Post[]>` のジェネリクスでレスポンス型を指定しているため、`res.data` が自動的に `Post[]` 型になる
- 関数名（`getPosts`, `createPost`等）はView側で直接importして呼び出す

#### `comments.ts` — コメントAPI

```ts
// api/comments.ts
export const getComments = (postId: number) =>
  apiClient.get<Comment[]>(`/posts/${postId}/comments`)

export const createComment = (data: CreateCommentRequest) =>
  apiClient.post<Comment>(`/posts/${data.postId}/comments`, data)

export const deleteComment = (postId: number, commentId: number) =>
  apiClient.delete(`/posts/${postId}/comments/${commentId}`)
```

- コメントは投稿に紐づくため、URLに `postId` が含まれる（`/posts/{postId}/comments`）

#### `auth.ts` — 認証API

```ts
// api/auth.ts
export const login = (data: LoginRequest) =>
  apiClient.post<AuthResponse>('/auth/login', data)

export const register = (data: RegisterRequest) =>
  apiClient.post<AuthResponse>('/auth/register', data)
```

- ログインと新規登録のみ（認証APIは2つだけ）
- レスポンスは共通で `AuthResponse`（`token`と`username`を含む）

---

### `types/` — TypeScript型定義

**役割:** APIのリクエスト・レスポンスのデータ形状を型として定義する。C#のモデルクラスとTypeScriptの型を対応させる。

#### `post.ts`

```ts
// APIから返ってくるPostの型（C#のPostモデルと対応）
export interface Post {
  id: number
  title: string
  content: string
  createdAt: string  // C#のDateTimeはJSONにするとstring
  viewCount: number
  likeCount: number
}

// 投稿作成時に送信するデータの型
export interface CreatePostRequest {
  title: string
  content: string
}
```

- `Post`: バックエンドから取得するデータの型（`id`・`createdAt`などはサーバーが生成）
- `CreatePostRequest`: ユーザーが入力してPOSTするデータの型（`id`などは含まない）
- **C#との対応:** C#の `DateTime` はJSONシリアライズすると `string` になる点に注意

#### `comment.ts`

```ts
export interface Comment {
  id: number
  postId: number
  content: string
  createdAt: string
}

export interface CreateCommentRequest {
  postId: number
  content: string
}
```

#### `auth.ts`

```ts
export interface LoginRequest {
  username: string
  password: string
}

export interface RegisterRequest {
  username: string
  password: string  // バックエンドでBCryptハッシュ化される
}

export interface AuthResponse {
  token: string    // JWTトークン（7日間有効）
  username: string
}
```

---

### `composables/` — 再利用可能なロジック

**役割:** 複数のViewで共有したいリアクティブな状態とロジックをまとめる。Vue3のComposition APIの考え方。

#### `useAuth.ts` — 認証状態の管理

```ts
// composables/useAuth.ts
const token = ref<string | null>(localStorage.getItem('token'))
const username = ref<string | null>(localStorage.getItem('username'))

export function useAuth() {
  const isLoggedIn = computed(() => !!token.value)

  const login = async (usernameInput: string, password: string) => {
    const res = await loginApi({ username: usernameInput, password })
    token.value = res.data.token
    username.value = res.data.username
    localStorage.setItem('token', res.data.token)
    localStorage.setItem('username', res.data.username)
  }

  const logout = () => {
    token.value = null
    username.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('username')
  }

  return { token, username, isLoggedIn, login, register, logout }
}
```

**重要なポイント:**

- `token`と`username`は**関数の外**（モジュールスコープ）に置かれているため、どのViewから`useAuth()`を呼び出しても**同じ`ref`オブジェクトを共有**する
- `computed(() => !!token.value)` — `token`がnullでなければ`true`になるリアクティブな値。`token`が変わると自動で再計算される
- `localStorage` との二重管理: `ref`はVueのリアクティブシステム用（画面の再描画）、`localStorage`はページリロード後も維持するため

**使い方（View側）:**
```ts
// どのViewでも同じように呼び出せる
const { isLoggedIn, username, logout } = useAuth()
```

---

### `router/` — 画面遷移の定義

**役割:** URLと表示するViewコンポーネントを対応付ける。

```ts
// router/index.ts
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/',            name: 'home',        component: HomeView },
    { path: '/posts/new',   name: 'post-new',    component: () => import('../views/PostNewView.vue') },
    { path: '/posts/:id',   name: 'post-detail', component: () => import('../views/PostDetailView.vue') },
    { path: '/login',       name: 'login',       component: () => import('../views/LoginView.vue') },
    { path: '/register',    name: 'register',    component: () => import('../views/RegisterView.vue') },
  ],
})
```

**ポイント:**

- `HomeView` は直接import（最初に表示されるため即時ロードが必要）
- 他のViewは `() => import(...)` の遅延ロード（Lazy Load）→ 初期ページ表示を速くする
- `:id` はURL パラメータ。`/posts/42` にアクセスすると `route.params.id` が `"42"` になる
- `createWebHistory` — URLに `#` が付かないHTML5モードを使用

**View側でのルーター使用:**
```ts
const route = useRoute()   // 現在のURL情報（params.id など）
const router = useRouter() // プログラムでのページ遷移（router.push('/')）

const postId = Number(route.params.id)  // /posts/42 → 42
```

---

### `views/` — ページ単位のコンポーネント

**役割:** 各URLに対応した画面。`router/`から呼び出される最大単位のコンポーネント。

各Viewは `<script setup>` + `<template>` + `<style scoped>` の3セクション構成。

#### `HomeView.vue` — 投稿一覧（`/`）

```
やること:
  1. マウント時にgetPosts()で一覧取得
  2. ローディング・エラー・空状態を表示
  3. 各投稿をリスト表示（タイトル・日時・閲覧数・いいね数）
  4. ログイン時のみ削除ボタンを表示
```

```ts
onMounted(async () => {
  try {
    const res = await getPosts()
    posts.value = res.data
  } catch {
    error.value = 'データの取得に失敗しました'
  } finally {
    loading.value = false
  }
})
```

`onMounted` — コンポーネントがDOMに追加された直後に実行されるライフサイクルフック。

#### `PostDetailView.vue` — 投稿詳細（`/posts/:id`）

```
やること:
  1. マウント時にgetPost()とgetComments()を並列取得
  2. 投稿本文・いいねボタン・削除ボタンを表示
  3. コメント一覧を表示
  4. コメント投稿フォームを表示（ログイン不要）
```

```ts
// 投稿とコメントを並列で取得（Promise.all で同時に実行）
const [postRes, commentsRes] = await Promise.all([
  getPost(postId),
  getComments(postId),
])
```

`Promise.all` を使うことで、投稿取得とコメント取得を同時に実行し、両方完了したら処理を進める。

#### `PostNewView.vue` — 新規投稿（`/posts/new`）

```
やること:
  1. タイトル・内容の入力フォーム
  2. バリデーション（空欄チェック）
  3. createPost()でPOST → 成功したら一覧へ遷移
```

```ts
const validate = () => {
  if (!title.value.trim()) errors.value.title = 'タイトルを入力してください'
  if (!content.value.trim()) errors.value.content = '内容を入力してください'
  return !errors.value.title && !errors.value.content
}
```

#### `LoginView.vue` — ログイン（`/login`）

```
やること:
  1. ユーザー名・パスワードの入力フォーム
  2. useAuth()のlogin()を呼ぶ
  3. 成功したら一覧へ遷移、失敗したらエラーメッセージ表示
```

```ts
const { login } = useAuth()

const handleSubmit = async () => {
  try {
    await login(username.value.trim(), password.value)
    router.push('/')
  } catch (e: any) {
    error.value = e.response?.data?.message ?? 'ログインに失敗しました'
  }
}
```

`e.response?.data?.message` — バックエンドがエラーメッセージをJSON bodyで返してきた場合はそれを表示、なければデフォルトメッセージ。`?.` はオプショナルチェーン（nullならundefined）。

---

### `components/` — UI部品置き場

**役割:** Viewより細かい、再利用可能なUIパーツを置く場所。

現状はViteの初期ファイルのみが残っている。今後ここに追加するとよいもの:

| コンポーネント案 | 切り出すタイミング |
|---|---|
| `PostCard.vue` | 投稿一覧の各アイテムを部品化したいとき |
| `CommentItem.vue` | コメントの表示を別コンポーネントにしたいとき |
| `LoadingSpinner.vue` | ローディング表示を統一したいとき |
| `ErrorMessage.vue` | エラー表示を統一したいとき |

---

### `assets/` — 静的ファイル

**役割:** CSSや画像などの静的ファイル置き場。`main.ts` でグローバルimportする。

```ts
// main.ts
import './assets/main.css'
import './assets/base.css'
```

- `base.css` — CSS変数（`--color-background`等）とブラウザデフォルトのリセット
- `main.css` — グローバルに適用するスタイル
- 各Vueコンポーネントの `<style scoped>` はそのコンポーネント専用のスタイルであり、他のコンポーネントには影響しない

---

### `App.vue` — ルートコンポーネント

**役割:** 全ページ共通のレイアウト（ヘッダー・サイドバー）を定義し、中央に `<RouterView />` でページコンポーネントを表示。

```
┌─────────────────────────────────────────┐
│  ヘッダー（ロゴ・ナビゲーション）           │
├────────────────────────┬────────────────┤
│                        │                │
│  <RouterView />        │  サイドバー    │
│  (HomeView etc.)       │  （900px以上）  │
│                        │                │
└────────────────────────┴────────────────┘
```

```ts
// App.vue
const { isLoggedIn, username, logout } = useAuth()
```

- ヘッダーのナビゲーション（ログイン/ログアウト表示）は `useAuth()` の `isLoggedIn` を参照
- サイドバーは `@media (min-width: 900px)` のCSS条件で表示・非表示を切り替え
- ヘッダーの「新規投稿」ボタンは900px以上では非表示（サイドバー側のボタンを使う）

---

## Vue3 Composition API の基本概念（このプロジェクトで使っているもの）

| 関数 | 役割 | 使用例 |
|------|------|--------|
| `ref<T>(初期値)` | リアクティブな変数を作る。`.value` でアクセス | `const loading = ref(true)` |
| `computed(() => 式)` | 他のrefから自動計算される値 | `const isLoggedIn = computed(() => !!token.value)` |
| `onMounted(fn)` | DOMが準備された後に実行 | API呼び出しでデータ取得 |
| `useRoute()` | 現在のURLの情報を取得 | `route.params.id` |
| `useRouter()` | プログラム的なページ遷移 | `router.push('/')` |
| `v-model` | input要素とrefを双方向バインド | `<input v-model="title">` |
| `v-if` / `v-else-if` | 条件によって要素を表示・非表示 | `<div v-if="loading">` |
| `v-for` | 配列をリスト表示 | `<li v-for="post in posts" :key="post.id">` |
| `@click.prevent` | クリックイベント（preventDefaultあり） | フォームのsubmitをJS側で処理 |

---

## 各レイヤーの依存関係

```
App.vue
  └── composables/useAuth  (ログイン状態・ヘッダー表示制御)

router/index.ts
  └── views/* を遅延ロード

views/HomeView.vue
  ├── api/posts (getPosts, deletePost)
  ├── composables/useAuth (isLoggedIn)
  └── types/post (Post型)

views/PostDetailView.vue
  ├── api/posts (getPost, likePost, deletePost)
  ├── api/comments (getComments, createComment, deleteComment)
  ├── composables/useAuth (isLoggedIn)
  ├── types/post (Post型)
  └── types/comment (Comment型)

views/PostNewView.vue
  ├── api/posts (createPost)
  └── (useAuthは不使用 — router guardで保護予定)

views/LoginView.vue, RegisterView.vue
  └── composables/useAuth (login, register)

api/posts.ts, api/comments.ts, api/auth.ts
  └── api/client.ts (Axiosインスタンス)

api/client.ts
  └── localStorage (JWTトークン読み取り)
```

---

## 新機能を追加するときの手順例

**例: 投稿検索機能を追加する場合**

1. **`types/post.ts`** に `SearchPostRequest` 型を追加
2. **`api/posts.ts`** に `searchPosts(query: string)` 関数を追加
3. **`views/HomeView.vue`** に検索フォームと検索結果の表示を追加
4. 必要なら **`components/SearchBar.vue`** として部品化

この順番（型定義 → API関数 → View）で作ると、型エラーがコンパイル時に検出できる。
