# 投稿の所有権検証 — 実装解説

## 問題：なぜ所有権検証が必要だったか

実装前は `DELETE /api/posts/{id}` に `[Authorize]` が付いているだけだった。
「ログインしていること」は確認できても「自分の投稿かどうか」は確認していなかった。

```
// 実装前
[HttpDelete("{id}")]
[Authorize]                         // ← ログイン済みなら誰でも削除できた
public async Task<IActionResult> DeletePost(int id)
{
    var post = await _context.Posts.FindAsync(id);
    if (post == null || post.DelFlag) return NotFound();

    post.DelFlag = true;            // ← 投稿者を確認せず即削除
    await _context.SaveChangesAsync();
    return NoContent();
}
```

つまり、ログインしているユーザーなら **他人の投稿でも削除できる** 状態だった。

---

## 解決策の全体像

所有権を検証するには「この投稿を誰が作ったか」をデータとして持つ必要がある。
そのために以下の3段階で実装した。

```
[投稿作成時]  JWTからUserId取得 → PostにUserId保存
     ↓
[DBに保存]    Posts テーブルに UserId カラムが存在する
     ↓
[削除時]      JWTからUserId取得 → PostのUserIdと比較 → 一致しなければ拒否
```

---

## 変更ファイルと役割

| レイヤー | ファイル | 役割 |
|---------|---------|------|
| モデル | `Models/Post.cs` | `UserId` カラムを追加 |
| DB設定 | `Data/AppDbContext.cs` | Post と User のリレーションを定義 |
| DTO | `DTOs/CreatePostRequest.cs` | 投稿作成リクエストの入力型 |
| DTO | `DTOs/AuthResponse.cs` | ログイン応答に `UserId` を追加 |
| API | `Controllers/PostsController.cs` | 作成時に UserId 記録、削除時に検証 |
| API | `Controllers/AuthController.cs` | ログイン応答に `UserId` を含める |
| 型定義 | `types/auth.ts` | フロント側の応答型に `userId` 追加 |
| 型定義 | `types/post.ts` | フロント側の Post 型に `userId` 追加 |
| 状態管理 | `composables/useAuth.ts` | `userId` を localStorage に保存 |
| 画面 | `views/PostDetailView.vue` | 自分の投稿のみ削除ボタンを表示 |

---

## バックエンド実装の詳細

### 1. Post モデルに UserId を追加 (`Models/Post.cs`)

```csharp
public int? UserId { get; set; }   // nullable: 既存データを壊さないため
public User User { get; set; }     // ナビゲーションプロパティ
```

**`int?` にした理由**  
マイグレーション前に作成済みの投稿はどのユーザーのものか不明なため、
`null` を許容することで既存データをそのまま維持できる。

---

### 2. リレーション設定 (`Data/AppDbContext.cs`)

```csharp
modelBuilder.Entity<Post>()
    .HasOne(p => p.User)       // Post は User を1人持つ
    .WithMany()                // User は Post を複数持つ（User側にナビは不要）
    .HasForeignKey(p => p.UserId)
    .OnDelete(DeleteBehavior.SetNull);  // Userが削除されたらUserIdをnullにする
```

**`OnDelete(DeleteBehavior.SetNull)` にした理由**  
`Cascade` にするとユーザーを削除したとき投稿も一緒に消えてしまう。
掲示板では「ユーザー退会後も投稿内容は残す」設計が一般的なため `SetNull` を選んだ。

---

### 3. DTO で過剰投稿を防ぐ (`DTOs/CreatePostRequest.cs`)

```csharp
public class CreatePostRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }
}
```

**なぜ DTO を新たに作ったか**  
実装前はコントローラーが `Post` モデルを直接受け取っていた。
そのため悪意のあるユーザーが以下のようなリクエストを送ると、
`UserId` を上書きして他人に成りすますことができた。

```json
// 悪意のあるリクエスト例（実装前）
POST /api/posts
{
  "title": "...",
  "content": "...",
  "userId": 999       ← 他人のIDを指定できた
}
```

`CreatePostRequest` は `title` と `content` しか持たないため、
クライアントから `userId` を受け取る経路が存在しない。

---

### 4. 投稿作成：JWT から UserId を取得 (`PostsController.cs`)

```csharp
[HttpPost]
[Authorize]
public async Task<ActionResult<Post>> CreatePost(CreatePostRequest dto)
{
    // JWTのClaimsからUserIdを取り出す
    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdStr, out int userId))
        return Unauthorized();

    var post = new Post
    {
        Title = dto.Title,
        Content = dto.Content,
        UserId = userId,        // ← クライアントではなくJWTから取得したIDを使う
    };

    _context.Posts.Add(post);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
}
```

`User.FindFirst(ClaimTypes.NameIdentifier)` は ASP.NET Core が  
JWT を検証・復号した後にセットするClaimsから値を読む。  
クライアントが JWT を改ざんしても署名検証で弾かれるため、  
**ここで取得した UserId は信頼できる値**。

---

### 5. 投稿削除：所有権を検証 (`PostsController.cs`)

```csharp
[HttpDelete("{id}")]
[Authorize]
public async Task<IActionResult> DeletePost(int id)
{
    var post = await _context.Posts.FindAsync(id);
    if (post == null || post.DelFlag) return NotFound();

    // JWTからリクエスト送信者のUserIdを取得
    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdStr, out int userId))
        return Unauthorized();

    // 所有権チェック
    if (post.UserId != null && post.UserId != userId)
        return Forbid();    // ← 403 Forbidden を返す

    post.DelFlag = true;
    await _context.SaveChangesAsync();
    return NoContent();
}
```

**`post.UserId != null` という条件がある理由**  
既存の古い投稿（`UserId = null`）はどのユーザーが作ったか分からない。  
`null` チェックを入れることで、古い投稿は誰でも削除できる後方互換性を保った。  
新規の投稿にはすべて `UserId` が入るため、今後はこの分岐に入らない。

**HTTP ステータスコードの使い分け**

| 状況 | レスポンス | 意味 |
|------|-----------|------|
| 投稿が存在しない | `404 Not Found` | リソースが見つからない |
| JWT に UserId がない（異常） | `401 Unauthorized` | 認証情報が不正 |
| 他人の投稿を削除しようとした | `403 Forbidden` | 認証済みだが権限がない |
| 削除成功 | `204 No Content` | 処理完了、返すデータなし |

`401` と `403` の違いが重要。  
- `401`：「あなたが誰か分からない」→ ログインし直してください  
- `403`：「あなたが誰かは分かるが、この操作は許可されていない」

---

### 6. ログイン応答に UserId を追加 (`AuthController.cs` / `AuthResponse.cs`)

```csharp
// AuthResponse.cs
public class AuthResponse
{
    public string Token { get; set; }
    public string Username { get; set; }
    public int UserId { get; set; }    // ← 追加
}

// AuthController.cs（Register / Login 両方）
return Ok(new AuthResponse
{
    Token = GenerateToken(user),
    Username = user.Username,
    UserId = user.Id,              // ← 追加
});
```

フロントエンドが「ログイン中のユーザーのID」を知るための入口。  
JWT にも UserId は入っているが、フロント側で JWT をデコードするのは  
セキュリティ上推奨されないため、ログイン時に明示的に返す。

---

## フロントエンド実装の詳細

### 7. userId を保存・管理 (`composables/useAuth.ts`)

```typescript
// モジュールスコープ（全コンポーネントで共有される）
const storedUserId = localStorage.getItem('userId')
const userId = ref<number | null>(storedUserId ? Number(storedUserId) : null)

// ログイン・登録時
userId.value = res.data.userId
localStorage.setItem('userId', String(res.data.userId))

// ログアウト時
userId.value = null
localStorage.removeItem('userId')

// 公開
return { token, username, userId, isLoggedIn, login, register, logout }
```

`token` や `username` と同じパターンで `userId` を管理する。  
モジュールのトップレベルで定義することで、  
どのコンポーネントから `useAuth()` を呼んでも **同じ ref インスタンスを共有**できる。

---

### 8. 削除ボタンの表示制御 (`views/PostDetailView.vue`)

```typescript
const { isLoggedIn, userId } = useAuth()

// 自分の投稿かどうかを算出
const isOwnPost = computed(() =>
  post.value?.userId != null && post.value.userId === userId.value
)
```

```html
<!-- 実装前：ログイン済みなら誰でも削除ボタンが見えた -->
<button v-if="isLoggedIn" @click="handleDeletePost">🗑 削除</button>

<!-- 実装後：自分の投稿のみ削除ボタンが見える -->
<button v-if="isOwnPost" @click="handleDeletePost">🗑 削除</button>
```

**フロントの制御はあくまで「見た目」の話**  
削除ボタンを非表示にしても、APIに直接 `DELETE` リクエストを送れば  
バックエンドの検証が走る前の状態では削除できてしまう。  
フロントの制御はUXのため、本質的な防御はバックエンドの `Forbid()` が担っている。

---

## データの流れまとめ

```
【投稿作成】

ユーザーがフォームを送信
    ↓
POST /api/posts  { title, content }          ← UserIdは含まれない
    ↓
[PostsController.CreatePost]
 ├─ [Authorize] で JWT を検証
 ├─ JWT の Claims から UserId を取得
 └─ Post { Title, Content, UserId } を DB に保存


【削除】

ユーザーが削除ボタンをクリック（isOwnPost = true の場合のみ表示）
    ↓
DELETE /api/posts/{id}  + Authorization: Bearer <JWT>
    ↓
[PostsController.DeletePost]
 ├─ [Authorize] で JWT を検証
 ├─ DB から Post を取得
 ├─ JWT の Claims から UserId を取得
 ├─ post.UserId と比較
 │   ├─ 一致 → DelFlag = true → 204 No Content
 │   └─ 不一致 → 403 Forbidden
 └─ （フロントは 403 を受け取ったら alert を表示）
```

---

## この実装の限界（今後の課題）

- **管理者が削除できない**  
  管理者ロール（Admin）を実装すれば `UserId` に関係なく削除できる仕組みが必要。

- **既存データの扱い**  
  `UserId = null` の古い投稿は誰でも削除できる。  
  本番運用するなら手動で所有者を割り当てるか、  
  `null` の投稿は削除不可にするポリシーを検討すること。

- **フロントの userId は localStorage に平文保存**  
  現時点では XSS 対策が不十分。HttpOnly Cookie に移行すると  
  JavaScript から userId を読めなくなるため、  
  別の仕組みで「自分の投稿かどうか」を判定する必要が生じる  
  （例：APIが `isOwner: true` を返す）。
```
