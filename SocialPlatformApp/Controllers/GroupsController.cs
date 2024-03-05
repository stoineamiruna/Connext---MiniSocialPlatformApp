using SocialPlatformApp.Data;
using SocialPlatformApp.Models;
using Microsoft.AspNetCore.Mvc;
using SocialPlatformApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;



namespace SocialPlatformApp.Controllers
{
    public class GroupsController : Controller
    {

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public GroupsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

          

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }

        // lista tututor grupurilor + user-ul care le-a creat
        public IActionResult Index()
        {
            SetAccessRights();
            var groups = db.Groups.Include("User"); //.Include("UserGroup")

            var usergroups = db.UserGroups.Include("User").Include("Group");

            ViewBag.Groups = groups;
            ViewBag.UserGroups = usergroups;
            ViewBag.Alaturat = false;

            ViewBag.AreProfil = db.Profiles.Any(prf => prf.UserId == _userManager.GetUserId(User));
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            return View();
        }


        //afisarea unui singur grup in functie de id-ul sau
        public IActionResult Show(int id)
{

    SetAccessRights();
    Group group = db.Groups.Include("User") 
                       .Where(group => group.Id == id)
                       .First();
    ViewBag.Group = group;
    ViewBag.User = group.User;
    var postsWithAutor = from post in db.Posts
                           .Where(pstt => pstt.GroupId == id)
                           .Select(pst => new
                           {
                                Post = pst,
                                AutorFirstName = db.Profiles.FirstOrDefault(prf => prf.UserId == pst.UserId).FirstName,
                                AutorLastName = db.Profiles.FirstOrDefault(prf => prf.UserId == pst.UserId).LastName,
                                AutorId = db.Profiles.FirstOrDefault(prf => prf.UserId == pst.UserId).Id
                            })
                         orderby post.Post.Date descending
                         select post;

    

    ViewBag.PostsWithAutor = postsWithAutor;
    ViewBag.UserInGroup = db.UserGroups.Any(ug => ug.UserId == _userManager.GetUserId(User) && ug.GroupId == id);
    if (TempData.ContainsKey("message"))
    {
        ViewBag.Message = TempData["message"];
        ViewBag.Alert = TempData["messageType"];
    }
    return View();
}

        

        // formularul in care se vor completa datele unei grup nou
        public IActionResult New()
        {
            Group group = new Group();

            return View(group);
        }


        // Se adauga grupul in baza de date

        [HttpPost]
        public IActionResult New(Group group)
        {
            

            group.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {

                db.Groups.Add(group);
                
                TempData["message"] = "Grupul a fost creat cu succes";
                TempData["messageType"] = "alert-success";
                db.SaveChanges();
                return RedirectToAction("Index");
                 
            }

            else
            {
                return View(group);
            }

        }


        // Se editeaza un grup existent in baza de date 
        // HttpGet implicit
        // Se afiseaza formularul impreuna cu datele aferente grupului din baza de date

        [HttpGet]
        public IActionResult Edit(int id)
        {

            Group group = db.Groups.Include("User")
                                        .Where(group => group.Id == id)
                                        .First();

            ViewBag.Group = group;
            return View(group);
        }

        // Se adauga grupul modificat in baza de date
        [HttpPost]
        public IActionResult Edit(int id, Group requestGroup)
        {
            Group group = db.Groups.Find(id);

            if (ModelState.IsValid)
            {

                group.Name = requestGroup.Name;
                group.Description = requestGroup.Description;
               
                TempData["message"] = "Grupul a fost modificat cu succes";
                TempData["messageType"] = "alert-success";

                db.SaveChanges();


                return RedirectToAction("Index");
            }

            else
            {
                return View(group);

            }
        }

        // Se sterge un grup din baza de date 
        //Utilizatorii pot sterge doar grupurile create de ei
        //Adminul poate sterge orice grup

        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Group group = db.Groups
                                   .Where(group => group.Id == id).First();


            if (group.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                var comms = db.Comments.Where(c => c.Post.GroupId == id);
                db.Comments.RemoveRange(comms);

                var posts = db.Posts.Where(p => p.GroupId == id);
                db.Posts.RemoveRange(posts);

                var usergroups = db.UserGroups.Where(ug => ug.GroupId == id);
                db.UserGroups.RemoveRange(usergroups);

                db.Groups.Remove(group);
                TempData["message"] = "Grupul a fost șters cu succes";
                TempData["messageType"] = "alert-success";
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else return RedirectToAction("Index");

        }

        

        [Authorize(Roles = "User,Admin")]
        public IActionResult JoinGroup(int groupId)
        {
            // Verifică dacă utilizatorul nu este deja membru al grupului
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!db.UserGroups.Any(ug => ug.UserId == userId && ug.GroupId == groupId))
            {
                // Adaugă o nouă înregistrare în tabela UserGroups
                var userGroup = new UserGroup { UserId = userId, GroupId = groupId };
                TempData["message"] = "Bun venit în grup! Te-ai alăturat grupului ";
                TempData["messageType"] = "alert-success";
                db.UserGroups.Add(userGroup);
                db.SaveChanges();
            }

            // Redirectează înapoi la pagina cu lista de grupuri sau altundeva, după caz
            return RedirectToAction("Index");
        }

    }
}
