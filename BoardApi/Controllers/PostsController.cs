using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardApi.Data;
using BoardApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PostsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/posts?page=1&pageSize=30
        [HttpGet]
        public async Task<IActionResult> GetPosts(int page = 1, int pageSize = 30)
        {
            if (pageSize != 10 && pageSize != 30 && pageSize != 50 && pageSize != 100)
                pageSize = 30;
            if (page < 1) page = 1;

            var query = _context.Posts.Where(p => !p.DelFlag).OrderByDescending(p => p.CreatedAt);
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new { items, totalCount, page, pageSize });
        }

        // GET /api/posts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null || post.DelFlag) return NotFound();

            post.ViewCount++;
            await _context.SaveChangesAsync();

            return post;
        }

        // POST /api/posts
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Post>> CreatePost(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }

        // DELETE /api/posts/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null || post.DelFlag) return NotFound();

            post.DelFlag = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET /api/posts/{id}/comments
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments(int id)
        {
            if (!await _context.Posts.AnyAsync(p => p.Id == id && !p.DelFlag))
                return NotFound();

            return await _context.Comments
                .Where(c => c.PostId == id && !c.DelFlag)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        // POST /api/posts/{id}/comments
        [HttpPost("{id}/comments")]
        public async Task<ActionResult<Comment>> CreateComment(int id, Comment comment)
        {
            if (!await _context.Posts.AnyAsync(p => p.Id == id && !p.DelFlag))
                return NotFound();

            comment.PostId = id;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetComments), new { id }, comment);
        }

        // DELETE /api/posts/{id}/comments/{commentId}
        [HttpDelete("{id}/comments/{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id, int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null || comment.DelFlag || comment.PostId != id) return NotFound();

            comment.DelFlag = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST /api/posts/{id}/like
        [HttpPost("{id}/like")]
        [Authorize]
        public async Task<IActionResult> LikePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null || post.DelFlag) return NotFound();

            post.LikeCount++;
            await _context.SaveChangesAsync();
            return Ok(new { likeCount = post.LikeCount });
        }
    }
}
