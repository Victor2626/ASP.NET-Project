using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;
using WorkplaceCollaboration.Data;
using WorkplaceCollaboration.Models;

namespace WorkplaceCollaboration.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext db;

        public MessagesController(ApplicationDbContext context)
        {
            db = context;
        }

        [HttpPost]
        public IActionResult New(Message mess)
        {
            mess.Date = DateTime.Now;

            try
            {
                db.Messages.Add(mess);
                db.SaveChanges();
                return Redirect("/Channels/Show/" + mess.ChannelId);
            }

            catch (Exception)
            {
                return Redirect("/Channels/Show/" + mess.ChannelId);
            }

        }

        // Stergerea unui mesaj asociat unui canal din baza de date
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Message mess = db.Messages.Find(id);
            db.Messages.Remove(mess);
            db.SaveChanges();
            return Redirect("/Channels/Show/" + mess.ChannelId);
        }

        // In acest moment vom implementa editarea intr-o pagina View separata
        // Se editeaza un mesaj existent

        public IActionResult Edit(int id)
        {
            Message mess = db.Messages.Find(id);
            ViewBag.Message = mess;
            return View();
        }

        [HttpPost]
        public IActionResult Edit(int id, Message requestMessage)
        {
            Message mess = db.Messages.Find(id);
            try
            {

                mess.Content = requestMessage.Content;

                db.SaveChanges();

                return Redirect("/Channels/Show/" + mess.ChannelId);
            }
            catch (Exception e)
            {
                return Redirect("/Channels/Show/" + mess.ChannelId);
            }

        }
    }
}
