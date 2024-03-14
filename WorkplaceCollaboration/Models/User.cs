using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkplaceCollaboration.Models
{
    public class User : IdentityUser
    {
        // un user poate posta mai multe mesaje
        public virtual ICollection<Message>? Messages { get; set; }

        // un user poate aparea in unul sau mai multe canale
        public virtual ICollection<ChannelUser>? ChannelUsers { get; set; }

        // tabela asociativa un user poate modera mai multe canale
        public virtual ICollection<ChannelMod>? ChannelMods { get; set; }

        // un user poate creea mai multe canale
        public virtual ICollection<Channel>? ChannelsCreated { get; set; }

        // un user trimite mai multe cereri de solicitare
        public virtual ICollection<Invite>? Invites { get; set; }

        // atribute suplimentare adaugate pentru user
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        // variabila in care vom retine rolurile existente in baza de date
        // pentru popularea unui dropdown list
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

    }
}
