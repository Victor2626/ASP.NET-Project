using System.ComponentModel.DataAnnotations.Schema;

namespace WorkplaceCollaboration.Models
{
    public class ChannelMod
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? ChannelId { get; set; }
        public string? UserId { get; set; }
        public virtual Channel? Channel { get; set; }
        public virtual User? User { get; set; }
    }
}
