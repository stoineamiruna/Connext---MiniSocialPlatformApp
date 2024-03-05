using SocialPlatformApp.Data;
using SocialPlatformApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;



namespace SocialPlatformApp.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CommentsController(ApplicationDbContext context,
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


        // Adaugarea unui comentariu asociat unei postari in baza de date
        [HttpPost]
        public IActionResult New(Comment comm)
        {
            comm.Date = DateTime.Now;
            if (!User.Identity.IsAuthenticated)
            {
                // Dacă utilizatorul nu este autentificat, direcționează-l către pagina de înregistrare
                return Redirect("/Identity/Account/Login");
            }
            Profile profile = db.Profiles.Where(profile => profile.UserId == _userManager.GetUserId(User)).FirstOrDefault();
            if (profile == null)
                return Redirect("/Profiles/New");
            comm.UserId = _userManager.GetUserId(User);
            try
            {
                db.Comments.Add(comm);
                db.SaveChanges();
                return Redirect("/Posts/Show/" + comm.PostId);
            }

            catch (Exception)
            {
                return Redirect("/Posts/Show/" + comm.PostId);
            }

        }

        // Stergerea unui comentariu asociat unei postari din baza de date
        [HttpPost]
        public IActionResult Delete(int id)
        {
            SetAccessRights();
            Comment comm = db.Comments.Find(id);
            db.Comments.Remove(comm);
            db.SaveChanges();
            return Redirect("/Posts/Show/" + comm.PostId);
        }

     

        public IActionResult Edit(int id)
        {
            SetAccessRights();
            Comment comm = db.Comments.Find(id);
            ViewBag.Comment = comm;
            return View();
        }

        [HttpPost]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments.Find(id);
            try
            {

                comm.Content = requestComment.Content;

                db.SaveChanges();

                return Redirect("/Posts/Show/" + comm.PostId);
            }
            catch (Exception e)
            {
                return Redirect("/Posts/Show/" + comm.PostId);
            }

        }
    }
}
