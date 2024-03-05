using SocialPlatformApp.Data;
using SocialPlatformApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Identity;

namespace SocialPlatformApp.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public PostsController(ApplicationDbContext context,
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

        public IActionResult Index()
        {
            SetAccessRights();
            var posts = db.Posts.Include("User");
            var postsWithAutor = from post in db.Posts
                                        .Select(pst => new
                                        {
                                            Post = pst,
                                            AutorFirstName = db.Profiles.FirstOrDefault(prf => prf.UserId == pst.UserId).FirstName,
                                            AutorLastName = db.Profiles.FirstOrDefault(prf => prf.UserId == pst.UserId).LastName,
                                            AutorId = db.Profiles.FirstOrDefault(prf => prf.UserId == pst.UserId).Id,
                                            profil_public = db.Profiles.FirstOrDefault(prf => prf.UserId == pst.UserId).PublicProfile,
                                            cerere_trimisa = db.Requests.FirstOrDefault(r => (r.UserId1 == _userManager.GetUserId(User) && r.UserId2 == pst.UserId))
                                        })
                                 orderby post.Post.Date descending
                                 select post;
 
                                         
            ViewBag.PostsWithAutor = postsWithAutor;
            ViewBag.AreProfil = db.Profiles.Any(prf => prf.UserId == _userManager.GetUserId(User));
           
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            return View();
        }

        public ActionResult Show(int id)
        {
            SetAccessRights();
            Post post = db.Posts.Include("User")
                        .Include("Comments")
                        .Where(post => post.Id == id)
                        .First();
            var postsWithComments = post.Comments
                                        .Select(comm => new
                                        {
                                            Comment = comm,
                                            AutorFirstName = db.Profiles.FirstOrDefault(prf => prf.UserId == comm.UserId).FirstName,
                                            AutorLastName = db.Profiles.FirstOrDefault(prf => prf.UserId == comm.UserId).LastName
                                            //AutorId = db.Profiles.FirstOrDefault(prf => prf.UserId == comm.UserId).Id
                                        })
                                        .ToList();
            ViewBag.PostsWithComments = postsWithComments;

            ViewBag.Post = post;
            Profile profile=db.Profiles.Where(profile => profile.UserId == post.UserId).FirstOrDefault();
            ViewBag.Profile = profile;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            return View();
        }

        // formularul in care se vor completa datele unei postari noi
        public IActionResult New()
        {
            Post postare = new Post();
            if (!User.Identity.IsAuthenticated)
            {
                // Dacă utilizatorul nu este autentificat, direcționează-l către pagina de înregistrare
                return Redirect("/Identity/Account/Login");
            }
            Profile profile= db.Profiles.Where(profile => profile.UserId == _userManager.GetUserId(User)).FirstOrDefault();
            if (profile == null)
                return Redirect("/Profiles/New");

            Console.BackgroundColor = ConsoleColor.Green;
            return View();
        }

        // Se adauga postarea in baza de date
        [HttpPost]
        public IActionResult New(Post postare)
        {
            postare.UserId = _userManager.GetUserId(User);
            try
            {
                Console.BackgroundColor = ConsoleColor.Green;
                postare.Date = DateTime.Now;

                db.Posts.Add(postare);

                TempData["message"] = "Postarea a fost adăugată cu succes";
                TempData["messageType"] = "alert-success";

                db.SaveChanges();

                var refererUrl = Request.Headers["Referer"].ToString();
                return Redirect(refererUrl);
            }
            catch (Exception)
            {
                TempData["message"] = "Descrierea este obligatorie!";
                TempData["messageType"] = "alert-danger";
                var refererUrl = Request.Headers["Referer"].ToString();
                return Redirect(refererUrl);
            }

        }


        public IActionResult Edit(int id)
        {
            Post post = db.Posts.Find(id);

            ViewBag.Post = post;

            return View(post);
        }

        [HttpPost]
        public ActionResult Edit(int id, Post requestPost)
        {
            Post post = db.Posts.Find(id);

            if (ModelState.IsValid)
            {
                post.Description = requestPost.Description;
                post.Media = requestPost.Media;

                db.SaveChanges();
                TempData["message"] = "Postarea a fost editată cu succes";
                TempData["messageType"] = "alert-success";

                if(post.GroupId==null)
                    return Redirect("/Posts/Index");
                else
                    return Redirect("/Groups/Show/"+post.GroupId);


            }
            else 
            {
                return View(post);
            }
        }

        public ActionResult Delete(int id)
        {
            Post post = db.Posts.Find(id);

            var comments = db.Comments.Where(c => c.PostId == id);
            db.Comments.RemoveRange(comments);

            db.Posts.Remove(post);
            TempData["message"] = "Postarea a fost ștearsă cu succes";
            TempData["messageType"] = "alert-success";
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
