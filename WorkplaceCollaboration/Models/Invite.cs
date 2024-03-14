using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkplaceCollaboration.Models
{
    public class Invite
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? ChannelId { get; set; }
        public virtual User? User { get; set; }
        public virtual Channel? Channel { get; set; }
        public DateTime Date { get; set; }
    }
}