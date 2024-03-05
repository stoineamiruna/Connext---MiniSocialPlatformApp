using System.ComponentModel.DataAnnotations;

namespace SocialPlatformApp.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele grupului este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele grupului nu poate avea mai mult de 100 de caractere")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Descrierea grupului este obligatorie")]
        public string Description { get; set; }


        public string? UserId { get; set; }   //FK - utilizatorul care a creat grupul

        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<UserGroup>? UserGroups { get; set; }

        public virtual ICollection<Post>? Posts { get; set; }

    }
}
