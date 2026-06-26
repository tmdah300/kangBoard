using System.ComponentModel.DataAnnotations;

namespace BoardApi.DTOs
{
    public class CreatePostRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
