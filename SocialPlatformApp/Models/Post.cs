using System.ComponentModel.DataAnnotations;
using SocialPlatformApp.Models;

namespace SocialPlatformApp.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Descrierea postarii este obligatorie")]
        public string Description { get; set; }

        public string? Media { get; set; }

        public string? MediaType { get; set; }

        public string? UserId { get; set; }

        public int? GroupId { get; set; }

        //o postare este postata de catre un singur user
        public virtual ApplicationUser? User { get; set; }

        // o postare poate fi postata in cadrul profilului personal
        public virtual Profile? Profile { get; set; }

        // o postare poate fi postata in cadrul unui grup
        public virtual Group? Group { get; set; }

        // un articol poate avea o colectie de comentarii
        public virtual ICollection<Comment>? Comments { get; set; }
    }
}
