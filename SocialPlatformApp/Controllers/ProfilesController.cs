using Microsoft.AspNetCore.Mvc;
using SocialPlatformApp.Data;
using SocialPlatformApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace SocialPlatformApp.Controllers
{
    public class ProfilesController : Controller
    {

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProfilesController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }

        // lista tututor profilelor si search bar

        public IActionResult Index()
{
    SetAccessRights();
    var profiles = db.Profiles.Include("User");

    //CAUTARE

    var search = "";

    if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
    {
        search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

        // Cautare profil (dupa nume si prenume)
        profiles = db.Profiles
            .Where(at => at.FirstName.Contains(search) || at.LastName.Contains(search))
            .Include("User");
    }

    ViewBag.Profiles = profiles;
    ViewBag.SearchString = search;
    if (TempData.ContainsKey("message"))
    {
        ViewBag.Message = TempData["message"];
        ViewBag.Alert = TempData["messageType"];
    }
    return View();
}


        //afisarea unui singur profil in functie de id-ul sau
        public IActionResult Show(int? id)
{
    SetAccessRights();
    if (id == null)
    {
        if (!User.Identity.IsAuthenticated)
        {
            // Dacă utilizatorul nu este autentificat, direcționează-l către pagina de înregistrare
            return Redirect("/Identity/Account/Login");
        }

        ApplicationUser currentUser = _userManager.GetUserAsync(User).Result;

        Profile currentProfile = db.Profiles.Include("User")
                              .Where(u => u.UserId == currentUser.Id)
                              .FirstOrDefault();

        if (currentProfile == null)
        {
            return RedirectToAction("New");
        }
        else
        {
            id = currentProfile.Id;
        }
    }
    Profile profile = db.Profiles.Include("User").Include("User.Posts")
                          .Where(p => p.Id == id)
                          .FirstOrDefault();
    var posts = from post in profile.User.Posts
                where post.GroupId == null
                orderby post.Date descending
                select post;
    ViewBag.Posts = posts;
    ViewBag.Profile = profile;
    ViewBag.User = profile.User;

    // Obține expeditorul
    string expeditor = _userManager.GetUserId(User);

    // Obține destinatarul
    string destinatar = profile.UserId;

    Request cerere_existenta_primita = db.Requests.FirstOrDefault(r =>
            (r.UserId1 == destinatar && r.UserId2 == expeditor));
    Request cerere_existenta_trimisa = db.Requests.FirstOrDefault(r =>
            (r.UserId1 == expeditor && r.UserId2 == destinatar));
    ViewBag.RequestSent = cerere_existenta_trimisa;
    ViewBag.RequestReceived = cerere_existenta_primita;
    if (TempData.ContainsKey("message"))
    {
        ViewBag.Message = TempData["message"];
        ViewBag.Alert = TempData["messageType"];
    }
    return View();
}
        // formularul in care se vor completa datele unei profil nou
        public IActionResult New()
        {
            Profile profile = new Profile();

            return View(profile);
        }


        // Se adauga profilul in baza de date

        [HttpPost]
        public IActionResult New(Profile profile)
        {
            
            profile.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {

                db.Profiles.Add(profile);
                
                TempData["message"] = "Profilul a fost creat cu succes";
                TempData["messageType"] = "alert-success"; 
                db.SaveChanges();
                return RedirectToAction("Show");
            }

            else
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
                return View(profile);
            }

        }

        // Se editeaza un profil existent in baza de date 
        // HttpGet implicit
        // Se afiseaza formularul impreuna cu datele aferente profilului din baza de date

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Profile profile = db.Profiles.Include("User")
                                        .Where(profile => profile.UserId == _userManager.GetUserId(User))
                                        .First();
            profile.UserId = _userManager.GetUserId(User);
            ViewBag.Profile = profile;
            return View(profile);
        }

        // Se adauga profilul modificat in baza de date
        [HttpPost]
        public IActionResult Edit(int id, Profile requestProfile)
        {
            Profile profile = db.Profiles.Find(id);

            if (ModelState.IsValid)
            {

                profile.FirstName = requestProfile.FirstName;
                profile.LastName = requestProfile.LastName;
                profile.Bio = requestProfile.Bio;
                profile.PublicProfile= requestProfile.PublicProfile;
                TempData["message"] = "Profilul a fost modificat cu succes";
                TempData["messageType"] = "alert-success";
                db.SaveChanges();
                return RedirectToAction("Show");
            }
            else
            {
                return View(requestProfile);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(string id)
        {
            Profile profile = db.Profiles
                                   .Where(profile => profile.UserId == id).First();


            if (profile.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                var comments = db.Comments.Where(c => c.UserId == id);
                db.Comments.RemoveRange(comments);

                var posts = db.Posts.Where(p => p.UserId == id);
                db.Posts.RemoveRange(posts);

                var usergroups = db.UserGroups.Where(ug => ug.UserId == id || ug.Group.UserId == id);
                db.UserGroups.RemoveRange(usergroups);

                var groups = db.Groups.Where(g => g.UserId == id);
                db.Groups.RemoveRange(groups);

                var requests = db.Requests.Where(r => r.UserId1 == id || r.UserId2 == id);
                db.Requests.RemoveRange(requests);

                db.Profiles.Remove(profile);
                TempData["message"] = "Profilul a fost șters cu succes";
                TempData["messageType"] = "alert-success";
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else return RedirectToAction("Index");

        }

    }
}

