using System.ComponentModel.DataAnnotations.Schema;

namespace SocialPlatformApp.Models
{
    public class UserGroup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? UserId { get; set; }
        public int? GroupId { get; set; }  //pana aici avem cheia primara compusa

        public virtual ApplicationUser? User { get; set; }
        public virtual Group? Group { get; set; }
    }
}
