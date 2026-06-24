using System;
using System.ComponentModel.DataAnnotations;

namespace BoardApi.Models
{
    public class Comment
    {
        public int Id { get; set; }

        // 外部キー：どのPostに属するかを示す
        public int PostId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool DelFlag { get; set; } = false;

        public Post Post { get; set; }
    }
}
