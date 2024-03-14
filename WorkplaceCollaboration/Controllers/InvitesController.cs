using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using WorkplaceCollaboration.Data;
using WorkplaceCollaboration.Models;

namespace WorkplaceCollaboration.Controllers
{
    [Authorize]
    public class InvitesController : Controller
    {

        private readonly ApplicationDbContext db;

        private readonly UserManager<User> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public InvitesController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            if (User.IsInRole("Moderator"))
            {
                var servers = from invite in db.Channels
                               .Where(b => b.CreatorId == _userManager.GetUserId(User))
                              select invite;

                ViewBag.Servers = servers;

                return View();
            }
            else
            if (User.IsInRole("Admin"))
            {
                var servers = from invite in db.Channels
                              .Where(b => b.CreatorId == b.Creator.Id)
                              select invite;

                ViewBag.Servers = servers;

                return View();
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi asupra colectiei";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Channels");
            }

        }
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Show(int id)
        {
            //check if user is mod or creator, then show the channel if so
            var checkCreator = db.Channels.Where(c => c.Id == id).FirstOrDefault();


            if (User.IsInRole("Moderator") && checkCreator.CreatorId == _userManager.GetUserId(User))
            {
                var invites = db.Invites
                                  .Include("Channel")
                                  .Include("User")
                .Where(b => b.ChannelId == id);

                if (invites == null)
                {
                    TempData["message"] = "Nu aveti cereri de alaturare!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Channels");
                }
                ViewBag.EsteAdmin = false;

                ViewBag.invites = invites;

                return View();
            }

            else
            if (User.IsInRole("Admin"))
            {
                var invites = db.Invites
                                  .Include("Channel")
                                  .Include("User")
                                  .Where(b => b.ChannelId == id);


                if (invites == null)
                {
                    TempData["message"] = "Resursa cautata nu poate fi gasita";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Channels");
                }

                ViewBag.EsteAdmin = true;
                ViewBag.invites = invites;

                return View();
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Channels");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        public ActionResult Accept(int id)
        {
            Invite invite = db.Invites.Include("User")
                                      .Include("Channel")
                                         .Where(inv => inv.Id == id)
                                         .First();

            if (invite.Channel.CreatorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                //trebuie modificat cu ChannelUser, insa nu-mi merge merge
                ChannelMod chu = new ChannelMod();
                chu.ChannelId = invite.ChannelId;
                chu.UserId  = invite.UserId;
                chu.User = invite.User;
                chu.Channel = invite.Channel;
                db.ChannelMods.Add(chu);
                db.Invites.Remove(invite);
                db.SaveChanges();
                TempData["message"] = "Utilizatorul a fost acceptat!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa acceptati utilizatorii canalului acesta";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        public ActionResult Reject(int id)
        {
            Invite invite = db.Invites.Include("User")
                                      .Include("Channel")
                                         .Where(inv => inv.Id == id)
                                         .First();

            if (invite.Channel.CreatorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Invites.Remove(invite);
                db.SaveChanges();
                TempData["message"] = "Utilizatorul a fost respins!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa acceptati utilizatorii canalului acesta";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
    }
}
