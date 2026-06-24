using BoardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DBのテーブルに対応するプロパティ
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // PostとCommentの1対多リレーションを明示的に設定
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)           // CommentはPostを1つ持つ
                .WithMany(p => p.Comments)      // PostはCommentを複数持つ
                .HasForeignKey(c => c.PostId)   // 外部キーはPostId
                .OnDelete(DeleteBehavior.Cascade); // Post削除時にCommentも削除
        }
    }
}
