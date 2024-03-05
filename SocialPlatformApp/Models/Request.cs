using System.ComponentModel.DataAnnotations;

namespace SocialPlatformApp.Models
{
    public class Request
    {
        [Key]
        public int Id { get; set; }
        
        //de la cine
        public string UserId1 { get; set; }
        
        //catre cine
        public string UserId2 { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Now;

        public string Status { get; set; }

        //o cerere este trimisa catre user
        public virtual ApplicationUser User { get; set; }

    }
}
