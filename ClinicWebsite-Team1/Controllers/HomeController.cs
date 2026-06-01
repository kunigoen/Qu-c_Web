using ClinicWebsite_Team1.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

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
        public ActionResult Details(int? id)
        {
            if (id == null)
                return RedirectToAction("About");

            var doctor = db.doctors.FirstOrDefault(x => x.id == id);

            if (doctor == null)
                return HttpNotFound();

            var schedules = db.schedules
                              .Where(s => s.doctor_id == id)
                              .OrderBy(s => s.work_date)
                              .ToList();

            var reviews = db.doctor_reviews
                            .Where(r => r.doctor_id == id)
                            .OrderByDescending(r => r.review_date)
                            .ToList();

            ViewBag.Schedules = schedules ?? new List<schedule>();
            ViewBag.Reviews = reviews ?? new List<doctor_review>();

            return View(doctor);
        }
        public ActionResult DoctorDetail(int id)
        {
            var doctor = db.doctors
                           .Include("specialty")
                           .Include("user_account")
                           .FirstOrDefault(x => x.id == id);

            var schedules = db.schedules
                              .Where(s => s.doctor_id == id)
                              .ToList();

            var reviews = db.doctor_reviews
                             .Where(r => r.doctor_id == id)
                             .ToList();

            ViewBag.Schedules = schedules;
            ViewBag.Reviews = reviews;

            return View(doctor);
        }
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        
        public ActionResult Register(Register model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existEmail = db.user_accounts.FirstOrDefault(x => x.email == model.Email);
            if (existEmail != null)
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(model);
            }

            var existPhone = db.user_accounts.FirstOrDefault(x => x.phone_number == model.PhoneNumber);
            if (existPhone != null)
            {
                ModelState.AddModelError("PhoneNumber", "Phone number already exists");
                return View(model);
            }

            var user = new user_account
            {
                full_name = model.FullName,
                email = model.Email,
                phone_number = model.PhoneNumber,
                password_hash = model.Password,
                role = "Patient",
                is_active = true,
                created_at = DateTime.Now
            };

            db.user_accounts.InsertOnSubmit(user);
            db.SubmitChanges();

            return RedirectToAction("Login");
        }
        public ActionResult Login()
        {
            return View();
        }
        
       [HttpPost]
        public ActionResult Login(Login model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.user_accounts
                .FirstOrDefault(x => x.email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("Email", "Email does not exist");
                return View(model);
            }

            if (user.password_hash != model.Password)
            {
                ModelState.AddModelError("Password", "Incorrect password");
                return View(model);
            }
            Session["UserName"] = user.full_name;
            Session["UserId"] = user.id;
            Session["Role"] = user.role;


            return RedirectToAction("Index", "Home");
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        
        public ActionResult ForgotPassword(string email)
        {
            var user = db.user_accounts.FirstOrDefault(x => x.email == email);

            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại";
                return View();
            }

            // tạo token
            var token = new Random().Next(100000, 999999).ToString();

            user.reset_token = token;
            user.reset_expire = DateTime.Now.AddMinutes(15);

            db.SubmitChanges();

            SendEmail(user.email, token);

            TempData["email"] = email;

            return RedirectToAction("VerifyCode");
        }
        public ActionResult ResetPassword(string token)
        {
            var user = db.user_accounts
                .FirstOrDefault(x => x.reset_token == token &&
                                     x.reset_expire > DateTime.Now);

            if (user == null)
                return Content("Token hết hạn hoặc không hợp lệ");

            return View(new ResetPassword { Token = token });
        }
        public ActionResult VerifyCode()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerifyCode(string code)
        {
            var email = TempData["email"]?.ToString();

            var user = db.user_accounts
                .FirstOrDefault(x => x.email == email);

            if (user == null)
            {
                ViewBag.Error = "Session expired";
                return View();
            }

            if (user.reset_token != code || user.reset_expire < DateTime.Now)
            {
                ViewBag.Error = "Invalid or expired code";
                TempData["email"] = email;
                return View();
            }

            TempData["email"] = email;
            return RedirectToAction("ResetPassword");
        }
        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string newPassword, string confirmPassword)
        {
            var email = TempData["email"]?.ToString();

            var user = db.user_accounts.FirstOrDefault(x => x.email == email);

            if (user == null)
                return RedirectToAction("Login");

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Password not match";
                return View();
            }

            user.password_hash = newPassword;
            user.reset_token = null;
            user.reset_expire = null;

            db.SubmitChanges();

            return RedirectToAction("Login");
        }


        void SendEmail(string toEmail, string token)
    {
            var mail = new MailMessage();
            mail.To.Add(toEmail);
            mail.Subject = "Your OTP Code";
            mail.Body = $"Your verification code is: {token}";
            mail.IsBodyHtml = false;

            mail.From = new MailAddress("tranduythanh0705@gmail.com");

        var smtp = new SmtpClient("smtp.gmail.com", 587);
        smtp.Credentials = new System.Net.NetworkCredential(
            "tranduythanh0705@gmail.com",
            "yfyb bdsa wzbw bjjw"
        );

        smtp.EnableSsl = true;
        smtp.Send(mail);
    }
    public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}