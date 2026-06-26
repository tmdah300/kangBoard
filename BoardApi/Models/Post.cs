using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BoardApi.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ViewCount { get; set; } = 0;

        public int LikeCount { get; set; } = 0;

        public bool DelFlag { get; set; } = false;

        public int? UserId { get; set; }
        public User User { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}
