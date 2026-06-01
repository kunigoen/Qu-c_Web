using ClinicWebsite_Team1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace ClinicWebsite_Team1.Controllers
{
    public class HomeController : Controller
    {
        // Khai báo DbContext ở đây
        private ClinicWebsiteDataContext db = new ClinicWebsiteDataContext();

        public ActionResult Index()
        {
            var doctors = db.doctors
                            .OrderBy(x => x.id)
                            .ToList();
            var specialties = db.specialties
                                .OrderBy(x => x.id)
                                .ToList();
            var service =  db.medical_services
                            .OrderBy(x => x.id)
                            .ToList();
            ViewBag.Services = service;
            ViewBag.Specialties = specialties;
            return View(doctors);
        }


        public ActionResult About()
        {
            var doctors = db.doctors
                  .Include("specialty")
                  .Include("user_account")
                  .ToList();

            return View(doctors);

            
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}