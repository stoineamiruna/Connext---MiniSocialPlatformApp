using SocialPlatformApp.Models;
using System.ComponentModel.DataAnnotations;

namespace SocialPlatformApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul comentariului este obligatoriu")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        //un comentariu apartine unei postari
        public int? PostId { get; set; }

        //un comentariu e postat de un utilizator - FK
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Post? Post { get; set; }
    }

}
