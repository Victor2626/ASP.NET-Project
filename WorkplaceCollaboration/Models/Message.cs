using System.ComponentModel.DataAnnotations;

namespace WorkplaceCollaboration.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul mesajului este obligatoriu")]
        public string Content { get; set; }
        public DateTime Date { get; set; }

        // un comentariu apartine unui canal
        public int? ChannelId { get; set; }

        // un mesaj este postat de catre un user
        public string? UserId { get; set; }

        // userul care a transmis mesajul
        public virtual User? User { get; set; }

        // canalul din care face parte mesajul
        public virtual Channel? Channel { get; set; }
    }
}
