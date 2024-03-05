using System.ComponentModel.DataAnnotations;

namespace SocialPlatformApp.Models
{
    public class Profile
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required(ErrorMessage = "Prenumele este obligatoriu")]
        [StringLength(100, ErrorMessage = "Prenumele nu poate avea mai mult de 100 de caractere")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Numele de familie este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele de familie nu poate avea mai mult de 100 de caractere")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Bibliografia este obligatorie")]
        public string Bio { get; set; }

        public bool PublicProfile { get; set; }

        // un profil apartine unui singur user
        public virtual ApplicationUser? User { get; set; }

        // un profil poate avea mai multe postari
        public virtual ICollection<Post>? Posts { get; set; }

        // un profil poate avea mai multe cereri de prietenie
        public virtual ICollection<Request>? Requests { get; set; }

    }
}
