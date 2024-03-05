using SocialPlatformApp.Data;
using SocialPlatformApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace SocialPlatformApp.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RequestsController(ApplicationDbContext context,
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

        [HttpPost]
        public IActionResult New(string userId2, string status)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            // Obține expeditorul
            string expeditor = _userManager.GetUserId(User);

            // Obține destinatarul
            string destinatar = userId2;

            if (!User.Identity.IsAuthenticated)
            {
                // Dacă utilizatorul nu este autentificat, direcționează-l către pagina de înregistrare
                return Redirect("/Identity/Account/Login");
            }

            Profile prf = db.Profiles.Include("User")
                                      .Where(u => u.UserId == destinatar)
                                      .FirstOrDefault();
            if (prf == null)
            {
                return Redirect("/Profiles/New");
            }

            ApplicationUser usr_expeditor = db.ApplicationUsers.Where(u => u.Id == expeditor)
                                      .FirstOrDefault();
            try
            {

                Request newRequest = new Request();
                newRequest.UserId1 = expeditor;
                newRequest.UserId2 = destinatar;
                newRequest.Status = status;
                newRequest.Date = DateTime.Now;
                newRequest.User = usr_expeditor;

                db.Requests.Add(newRequest);
                if(status=="Acceptata")
                {
                    TempData["message"] = "Acum îl urmărești!";
                    TempData["messageType"] = "alert-success";
                }
                else
                {
                    TempData["message"] = "Cererea de urmărire a fost trimisă!";
                    TempData["messageType"] = "alert-success";
                }

                db.SaveChanges();
                return Redirect("/Profiles/Show/" + prf.Id);
            }

            catch (Exception)
            {
                return Redirect("/Profiles/Show/" + prf.Id);
            }

        }

        [HttpPost]
        public IActionResult Accept(int id)
        {
            //SetAccessRights();
            Request request = db.Requests.Find(id);
            request.Status = "Acceptata";
            TempData["message"] = "Ai un urmăritor nou";
            TempData["messageType"] = "alert-success";
            db.SaveChanges();
            var refererUrl = Request.Headers["Referer"].ToString();           
            return Redirect(refererUrl);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            //SetAccessRights();
            Request req = db.Requests.Find(id);
            Profile prf = db.Profiles.Include("User")
                                      .Where(u => u.UserId == req.UserId2)
                                      .FirstOrDefault();
            string status = req.Status;
            db.Requests.Remove(req);
            if(status=="Acceptata")
            {
                TempData["message"] = "Nu îl mai urmărești";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Cerere de urmărire ștearsă";
                TempData["messageType"] = "alert-success";
            }
            
            db.SaveChanges();
            var refererUrl = Request.Headers["Referer"].ToString();
            return Redirect(refererUrl);
        }
        
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var requestsWithSender = db.Requests
                                        .Where(r => r.UserId2 == userId && r.Status == "Neacceptata")
                                        .Select(r => new
                                        {
                                            Request = r,
                                            SenderFirstName = db.Profiles.FirstOrDefault(p => p.UserId == r.UserId1).FirstName,
                                            SenderLastName = db.Profiles.FirstOrDefault(p => p.UserId == r.UserId1).LastName
                                        })
                                        .ToList();

            ViewBag.RequestsWithSender = requestsWithSender; // Ensure this matches with the view
            ViewBag.Count = requestsWithSender.Count;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            return View();
        }
    }
}
