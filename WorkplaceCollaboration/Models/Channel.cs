using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WorkplaceCollaboration.Models
{
    public class Channel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul canalului este obligatoriu!")]
        [StringLength(100, ErrorMessage = "Titlul canalului nu poate avea mai mult de 100 de caractere!")]
        [MinLength(5, ErrorMessage = "Titlul canalului trebuie sa aiba minim 5 caractere!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Descrierea canalului este obligatoriu")]
        public string Description { get; set; }

        // data crearii canalului (poate fi util idk)
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Categoria este obligatorie")]
        // un canal are asociat o categorie
        public int? CategoryId { get; set; }

        // un canal este creat de catre un user
        public string? CreatorId { get; set; }

        // userul care a creat canalul
        public virtual User? Creator { get; set; }

        // tabela asociativa userii care au drept de moderare
        public virtual ICollection<ChannelMod>? ChannelMods { get; set; }

        // userii care fac parte din canal
        public virtual ICollection<ChannelUser>? ChannelUsers { get; set; }

        public virtual ICollection<Invite>? Invites { get; set; }

        // categoria canalului
        public virtual Category? Category { get; set; }

        // un canal poate avea o colectie de mesaje
        public virtual ICollection<Message>? Messages { get; set; }

        // select list item pentru maparea categoriei
        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }
    }
}
