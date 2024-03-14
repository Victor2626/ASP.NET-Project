using Ganss.Xss;
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
    public class ChannelsController : Controller
    {

        private readonly ApplicationDbContext db;

        private readonly UserManager<User> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public ChannelsController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        // Se afiseaza lista tuturor canalelor impreuna cu categoria 
        // din care fac parte
        // Pentru fiecare articol se afiseaza si userul care a postat articolul respectiv
        // HttpGet implicit
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Index()
        {
            // Alegem sa afisam 3 articole pe pagina
            int _perPage = 3;
            var channels = db.Channels.Include("Category")
            .Include("Creator").OrderBy(a => a.Date);
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
                ViewBag.Alert = TempData["messageType"];
            }

            var search = "";
            // MOTOR DE CAUTARE
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {

                // eliminam spatiile libere
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                // Cautare in canal (Title si Description)
                List<int> channelIds = db.Channels.Where
                (
                    at => at.Title.Contains(search)
                    || at.Description.Contains(search)
                ).Select(a => a.Id).ToList();

                // Lista articolelor care contin cuvantul cautat
                // fie in articol -> Title si Content
                // fie in comentarii -> Content
                channels = db.Channels.Where(channel =>
                channelIds.Contains(channel.Id))
                .Include("Category")
                .Include("Creator")
                .OrderBy(a => a.Date);
            }

            ViewBag.SearchString = search;

            // Fiind un numar variabil de canale, verificam de
            //fiecare data utilizand
            // metoda Count()
            int totalItems = channels.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;
            // Se calculeaza offsetul in functie de numarul
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedChannels = channels.Skip(offset).Take(_perPage);

            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            // Trimitem canalele cu ajutorul unui ViewBag
            ViewBag.Channels = paginatedChannels;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Channels/Index/?search="
                + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Channels/Index/?page";
            }

            return View();
        }

        // Se afiseaza un singur articol in functie de id-ul sau 
        // impreuna cu categoria din care face parte
        // In plus sunt preluate si toate comentariile asociate unui articol
        // Se afiseaza si userul care a postat articolul respectiv
        // HttpGet implicit

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Show(int id)
        {
            Channel channel = db.Channels.Include("Category")
                                         .Include("Creator")
                                         .Include("Messages")
                                         .Include("Messages.User")
                                         .Where(ch => ch.Id == id)
                                         .First();


            SetAccessRights(id);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(channel);
        }


        // Adaugarea unui mesaj asociat unui canal in baza de date
        // Toate rolurile pot adauga mesaje in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Show([FromForm] Message message)
        {
            message.Date = DateTime.Now;
            message.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Messages.Add(message);
                db.SaveChanges();
                return Redirect("/Channels/Show/" + message.ChannelId);
            }

            else
            {
                Channel channel = db.Channels.Include("Category")
                                             .Include("Creator")
                                             .Include("Comments")
                                             .Include("Comments.User")
                                             .Where(ch => ch.Id == message.ChannelId)
                                             .First();
                
                SetAccessRights(channel.Id);

                return View(channel);
            }
        }

        // Se creeaza canalul
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult New()
        {
            Channel channel = new Channel();

            // Se preia lista de categorii cu ajutorul metodei GetAllCategories()
            channel.Categ = GetAllCategories();

            return View(channel);
        }

        // Se adauga canalul in baza de date
        [Authorize(Roles = "User,Moderator,Admin")]
        [HttpPost]
        public IActionResult New(Channel channel)
        {
            var sanitizer = new HtmlSanitizer();

            channel.Date = DateTime.Now;

            

            // preluam id-ul utilizatorului care creeaza canalul
            channel.CreatorId = _userManager.GetUserId(User);

            //nu merge ChannelUser
            /*ChannelUser chu = new ChannelUser();
            chu.UserId = _userManager.GetUserId(User);
            chu.User = db.Users.Where(u => u.Id == _userManager.GetUserId(User)).FirstOrDefault();
            chu.Channel = channel;
            chu.ChannelId = channel.Id;*/

             ChannelMod chm = new ChannelMod();
            chm.UserId = _userManager.GetUserId(User);
            chm.User = db.Users.Where(u => u.Id == _userManager.GetUserId(User)).FirstOrDefault();
            chm.Channel = channel;
            chm.ChannelId = channel.Id;

            if (ModelState.IsValid)
            {
                channel.Description = sanitizer.Sanitize(channel.Description);
                channel.Description = (channel.Description);
                db.Channels.Add(channel);
                //db.ChannelUsers.Add(chu);
                db.ChannelMods.Add(chm);
                db.SaveChanges();
                TempData["message"] = "Canalul a fost creat!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                channel.Categ = GetAllCategories();
                return View(channel);
            }
        }

        [Authorize(Roles = "Moderator,Admin")]
        public IActionResult Edit(int id)
        {

            Channel channel = db.Channels.Include("Category")
                                        .Where(ch => ch.Id == id)
                                        .First();

            channel.Categ = GetAllCategories();

            if (channel.CreatorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(channel);
            }

            else
            {
                TempData["message"] = "Nu puteti face modificari asupra unui canal care nu va apartine!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

        }

        // Se adauga canalul modificat in baza de date
        // Verificam rolul utilizatorilor care au dreptul sa editeze (Moderator sau Admin)
        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        public IActionResult Edit(int id, Channel requestChannel)
        {
            var sanitizer = new HtmlSanitizer();

            Channel channel = db.Channels.Find(id);


            if (ModelState.IsValid)
            {
                if (channel.CreatorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    channel.Title = requestChannel.Title;

                    requestChannel.Description = sanitizer.Sanitize(requestChannel.Description);

                    channel.Description = requestChannel.Description;
                    channel.CategoryId = requestChannel.CategoryId;
                    TempData["message"] = "Canalul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu puteti face modificari asupra unui canal care nu va apartine!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestChannel.Categ = GetAllCategories();
                return View(requestChannel);
            }
        }



        // Se sterge un canal din baza de date
        // Utilizatorii cu rolul de Moderator sau Admin pot sterge canale
        // Moderatorii pot sterge doar canalele publicate de ei
        // Adminii pot sterge orice canal din baza de date

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        public ActionResult Delete(int id)
        {
            Channel channel = db.Channels.Include("Messages")
                                         .Where(ch => ch.Id == id)
                                         .First();

            if (channel.CreatorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Channels.Remove(channel);
                db.SaveChanges();
                TempData["message"] = "Canalul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un canal care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        public ActionResult Join(int id)
        {
            Channel channel = db.Channels.Where(ch => ch.Id == id)
                                         .First();

            User user = db.Users.Where(u => u.Id == _userManager.GetUserId(User)).First();

            Invite inv = db.Invites.Where(inv => inv.ChannelId == id && inv.UserId == _userManager.GetUserId(User)).First();

            if (inv == null)
            {
                inv.ChannelId = id;
                inv.UserId = _userManager.GetUserId(User);
                inv.Channel = channel;
                inv.User = user;
                db.SaveChanges();
                TempData["message"] = "Cererea de alaturare a fost trimisa";
                TempData["messageType"] = "alert-success";
                //check if it should return a post view of show and the id
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Deja ai trimis o invitatie!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

        }


        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights(int id)
        {
            ViewBag.AfisareButoane = false;

            ChannelMod chm = db.ChannelMods.Where(cm => cm.UserId == _userManager.GetUserId(User) && cm.ChannelId == id).FirstOrDefault();

            if (chm is not null)
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);

            //nu merge ChannelUser
            /*ChannelUser chu = db.ChannelUsers.Where(cm => cm.UserId == _userManager.GetUserId(User) && cm.ChannelId == id).FirstOrDefault();
            if (chu is not null)
            {
                ViewBag.AscundeInvitatie = true;
            }*/
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName.ToString();

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }

        public IActionResult AddInvite([FromForm] Invite invite)
        {
            // Daca modelul este valid
            if (ModelState.IsValid)
            {
                // Verificam daca avem deja s-a trimis o invitatie canalului la care incercam sa ne alaturam
                if (db.Invites
                    .Where(inv => inv.ChannelId == invite.ChannelId)
                    .Where(inv => inv.UserId == _userManager.GetUserId(User))
                    .Count() > 0)
                {
                    TempData["message"] = "Deja ai trimis o cerere de alaturare acestui canal!";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {
                    // Adaugam userul
                    invite.Date= DateTime.Now;
                    invite.UserId = _userManager.GetUserId(User);
                    invite.User = db.Users.Where(u => u.Id == _userManager.GetUserId(User)).First();
                    db.Invites.Add(invite);
                    // Salvam modificarile
                    db.SaveChanges();

                    // Adaugam un mesaj de succes
                    TempData["message"] = "Cererea de alaturare a fost trimisa cu success";
                    TempData["messageType"] = "alert-success";
                }

            }
            else
            {
                TempData["message"] = "Nu s-a putut adauga articolul in colectie";
                TempData["messageType"] = "alert-danger";
            }

            // Ne intoarcem la pagina articolului
            return Redirect("/Channels/Show/" + invite.ChannelId);
        }
    }
}
