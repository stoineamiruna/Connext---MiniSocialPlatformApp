using Microsoft.AspNetCore.Identity;

namespace SocialPlatformApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // un user posteza mai multe comments
        public virtual ICollection<Comment>? Comments { get; set; }

        // un user publica mai multe postari
        public virtual ICollection<Post>? Posts { get; set; }

        //// un user poate trimite mai multe requests
        public virtual ICollection<Request>? Requests { get; set; }

        /// un user poate avea un singur profil
        public virtual Profile? Profile { get; set; }


        // relatie many to many
        public virtual ICollection<UserGroup>? UserGroups { get; set; }

        // un user poate crea mai multe grupuri
        public virtual ICollection<Group>? Groups { get; set; }



    }
}
