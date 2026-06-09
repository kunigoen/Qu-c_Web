using ClinicWebsite_Team1.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClinicWebsite_Team1.Controllers
{
    public class AdminController : Controller
    {
        ClinicWebsiteDataContext db =
            new ClinicWebsiteDataContext(
                System.Configuration.ConfigurationManager
                    .ConnectionStrings["ClinicWebsiteConnectionString"]
                    .ConnectionString);

        public ActionResult Dashboard()
        {
            if (Session["Role"] == null ||
            Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.TotalDoctors = db.doctors.Count();
            ViewBag.TotalPatients = db.patients.Count();
            ViewBag.TotalAppointments = db.appointments.Count();
            ViewBag.TotalSchedules = db.schedules.Count();

            ViewBag.TotalRevenue =
                db.payments.Sum(x => (decimal?)x.amount) ?? 0;

            return View();
        }
        //======== MANAGE DOCTOR =======
        public ActionResult Doctors()
        {
            var doctors = db.doctors.ToList();

            return View(doctors);
        }
        public ActionResult CreateDoctor()
        {
            ViewBag.Specialties = db.specialties.ToList();

            return View();
        }
        [HttpPost]
        public ActionResult AddDoctor(AddDoctorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Specialties = db.specialties.ToList();
                return View(model);
            }

            string imagePath = "~/Content/images/default-doctor.jpg";

            if (model.ImageFile != null)
            {
                string fileName = System.IO.Path.GetFileName(model.ImageFile.FileName);

                string savePath = Server.MapPath("~/Content/images/" + fileName);

                model.ImageFile.SaveAs(savePath);

                imagePath = "~/Content/images/" + fileName;
            }

            // Tạo UserAccount
            user_account user = new user_account
            {
                full_name = model.FullName,
                email = model.Email,
                password_hash = model.Password,
                phone_number = model.PhoneNumber,
                role = "Doctor",
                is_active = true,
                created_at = DateTime.Now
            };

            db.user_accounts.InsertOnSubmit(user);
            db.SubmitChanges();

            // Tạo Doctor
            doctor doctor = new doctor
            {
                user_account_id = user.id,
                specialty_id = model.SpecialtyId,
                license_number = model.LicenseNumber,
                experience_years = model.ExperienceYears,
                consultation_fee = model.ConsultationFee,
                image_url = imagePath,
                phone_number = model.PhoneNumber,
                status = model.Status,
                created_at = DateTime.Now
            };

            db.doctors.InsertOnSubmit(doctor);
            db.SubmitChanges();

            return RedirectToAction("Doctors");
        }
        public ActionResult EditDoctor(int id)
        {
            var doctor = db.doctors.FirstOrDefault(x => x.id == id);

            if (doctor == null)
                return HttpNotFound();

            ViewBag.Specialties = db.specialties.ToList();

            return View(doctor);
        }
        [HttpPost]
        public ActionResult EditDoctor(
            doctor model,
            string FullName,
            string Email,
            string PhoneNumber,
            HttpPostedFileBase ImageFile)
        {
            var doctor = db.doctors.FirstOrDefault(x => x.id == model.id);

            if (doctor == null)
                return HttpNotFound();

            var user = db.user_accounts
                         .FirstOrDefault(x => x.id == doctor.user_account_id);

            if (user != null)
            {
                user.full_name = FullName;
                user.email = Email;
                user.phone_number = PhoneNumber;
            }

            doctor.specialty_id = model.specialty_id;
            doctor.license_number = model.license_number;
            doctor.experience_years = model.experience_years;
            doctor.consultation_fee = model.consultation_fee;
            doctor.status = model.status;

            doctor.phone_number = PhoneNumber;

            // Upload ảnh mới
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                string fileName =
                    System.IO.Path.GetFileName(ImageFile.FileName);

                string savePath =
                    Server.MapPath("~/Content/images/" + fileName);

                ImageFile.SaveAs(savePath);

                doctor.image_url = "~/Content/images/" + fileName;
            }

            db.SubmitChanges();

            return RedirectToAction("Doctors");
        }
        public ActionResult DeleteDoctor(int id)
        {
            var schedules = db.schedules
                      .Where(s => s.doctor_id == id)
                      .ToList();

            foreach (var schedule in schedules)
            {
                var appointments = db.appointments
                                     .Where(a => a.schedule_id == schedule.id)
                                     .ToList();

                foreach (var appointment in appointments)
                {
                    db.appointment_services.DeleteAllOnSubmit(
                        db.appointment_services
                          .Where(x => x.appointment_id == appointment.id));

                    db.consultations.DeleteAllOnSubmit(
                        db.consultations
                          .Where(x => x.appointment_id == appointment.id));

                    db.payments.DeleteAllOnSubmit(
                        db.payments
                          .Where(x => x.appointment_id == appointment.id));

                    db.doctor_reviews.DeleteAllOnSubmit(
                        db.doctor_reviews
                          .Where(x => x.appointment_id == appointment.id));
                }

                db.SubmitChanges();

                db.appointments.DeleteAllOnSubmit(appointments);
                db.SubmitChanges();
            }

            db.schedules.DeleteAllOnSubmit(schedules);
            db.SubmitChanges();

            var doctor = db.doctors.FirstOrDefault(x => x.id == id);

            if (doctor != null)
            {
                db.doctors.DeleteOnSubmit(doctor);
                db.SubmitChanges();
            }

            return RedirectToAction("Doctors");
        }
        //===============SPECIALTIES===============
        public ActionResult Specialties()
        {
            return View(db.specialties.ToList());
        }
        public ActionResult CreateSpecialty()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateSpecialty(
            specialty model,
            HttpPostedFileBase ImageFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            string imagePath = "~/Content/images/default-specialty.jpg";

            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                string fileName =
                    System.IO.Path.GetFileName(ImageFile.FileName);

                string savePath =
                    Server.MapPath("~/Content/images/" + fileName);

                ImageFile.SaveAs(savePath);

                imagePath = "~/Content/images/" + fileName;
            }

            model.image_url = imagePath;

            db.specialties.InsertOnSubmit(model);
            db.SubmitChanges();

            return RedirectToAction("Specialties");
        }
        public ActionResult EditSpecialty(int id)
        {
            var specialty =
                db.specialties.FirstOrDefault(x => x.id == id);

            if (specialty == null)
                return HttpNotFound();

            return View(specialty);
        }
        [HttpPost]
        public ActionResult EditSpecialty(specialty model, HttpPostedFileBase ImageFile)
        {
            var data = db.specialties.FirstOrDefault(x => x.id == model.id);

            if (data != null)
            {
                data.name = model.name;
                data.description = model.description;

                if (ImageFile != null)
                {
                    string fileName = System.IO.Path.GetFileName(ImageFile.FileName);
                    string path = Server.MapPath("~/Content/images/" + fileName);

                    ImageFile.SaveAs(path);

                    // CHỈ LƯU TÊN FILE
                    data.image_url = fileName;
                }

                db.SubmitChanges();
            }

            return RedirectToAction("Specialties");
        }
        public ActionResult DeleteSpecialty(int id)
        {
            var specialty =
                db.specialties.FirstOrDefault(x => x.id == id);

            if (specialty != null)
            {
                db.specialties.DeleteOnSubmit(specialty);
                db.SubmitChanges();
            }

            return RedirectToAction("Specialties");
        }
        //===================Schedules================

        public ActionResult Schedules()
        {
            var schedules = db.schedules
            .OrderByDescending(x => x.id)
            .ToList();

            return View(schedules);
        }
        public ActionResult CreateSchedule()
        {
            ViewBag.Doctors = db.doctors.ToList();
            ViewBag.Rooms = db.clinic_rooms.ToList();

            return View();
        }
        [HttpPost]
        public ActionResult CreateSchedule(schedule model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Doctors = db.doctors.ToList();
                ViewBag.Rooms = db.clinic_rooms.ToList();
                return View(model);
            }

            // 🚫 CHẶN NGÀY QUÁ KHỨ
            if (model.work_date < DateTime.Today)
            {
                ModelState.AddModelError("", "Không được chọn ngày trong quá khứ!");
                ViewBag.Doctors = db.doctors.ToList();
                ViewBag.Rooms = db.clinic_rooms.ToList();
                return View(model);
            }

            db.schedules.InsertOnSubmit(model);
            db.SubmitChanges();

            return RedirectToAction("Schedules");
        }
        public ActionResult EditSchedule(int id)
        {
            var schedule = db.schedules.FirstOrDefault(x => x.id == id);

            ViewBag.Doctors = db.doctors.ToList();
            ViewBag.Rooms = db.clinic_rooms.ToList();

            return View(schedule);
        }
        [HttpPost]
        public ActionResult EditSchedule(schedule model)
        {
            var schedule = db.schedules.FirstOrDefault(x => x.id == model.id);

            if (schedule == null)
                return RedirectToAction("Schedules");

            // update
            schedule.doctor_id = model.doctor_id;
            schedule.room_id = model.room_id;
            schedule.work_date = model.work_date;
            schedule.start_time = model.start_time;
            schedule.end_time = model.end_time;
            schedule.max_patients = model.max_patients;
            schedule.status = model.status;

            db.SubmitChanges();

            return RedirectToAction("Schedules");
        }
        public ActionResult DeleteSchedule(int id)
        {
            var schedule = db.schedules.FirstOrDefault(x => x.id == id);

            if (schedule != null)
            {
                var appointments =
                    db.appointments.Where(a => a.schedule_id == id);

                db.appointments.DeleteAllOnSubmit(appointments);

                db.schedules.DeleteOnSubmit(schedule);

                db.SubmitChanges();
            }

            return RedirectToAction("Schedules");
        }
        public ActionResult Patients()
        {
            var patients = db.patients
            .Select(p => new PatientViewModel
            {
                Id = p.id,
                FullName = p.user_account.full_name,
                Email = p.user_account.email,
                Phone = p.phone_number,
                Gender = p.gender,
                Address = p.address,
                IsActive = p.user_account.is_active ?? false
            })
        .ToList();

            return View(patients);
        }
        public ActionResult EditPatient(int id)
        {
            var patient = db.patients
                .FirstOrDefault(x => x.id == id);

            if (patient == null)
                return RedirectToAction("Patients");

            return View(patient);
        }
        [HttpPost]
        public ActionResult EditPatient(patient model)
        {
            var patient = db.patients
                .FirstOrDefault(x => x.id == model.id);

            if (patient != null)
            {
                patient.phone_number = model.phone_number;
                patient.address = model.address;
                patient.gender = model.gender;
                patient.blood_type = model.blood_type;
                patient.emergency_contact = model.emergency_contact;
                patient.updated_at = DateTime.Now;

                db.SubmitChanges();
            }

            return RedirectToAction("Patients");
        }
        public ActionResult TogglePatientStatus(int id)
        {
            var patient = db.patients
                .FirstOrDefault(x => x.id == id);

            if (patient != null)
            {
                var user = db.user_accounts
                    .FirstOrDefault(x => x.id == patient.user_account_id);

                if (user != null)
                {
                    user.is_active = !(user.is_active ?? false);

                    db.SubmitChanges();
                }
            }

            return RedirectToAction("Patients");
        }

        public ActionResult Appointments()
        {
            var data = db.appointments
                .Include("patient.user_account")
                .Include("doctor.user_account")
                .Include("schedule")
                .Include("appointment_status")
                .OrderByDescending(x => x.id)
                .ToList();

            return View(data);
        }
        public ActionResult UpdateAppointmentStatus(int id, int statusId)
        {
            var appt = db.appointments.FirstOrDefault(x => x.id == id);

            if (appt != null)
            {
                appt.status_id = statusId;
                appt.updated_at = DateTime.Now;

                db.SubmitChanges(); 
            }

            return RedirectToAction("Appointments");
        }
        public ActionResult DeleteAppointment(int id)
        {
            var appt = db.appointments.FirstOrDefault(x => x.id == id);

            if (appt != null)
            {
                db.appointments.DeleteOnSubmit(appt);
                db.SubmitChanges();
            }

            return RedirectToAction("Appointments");
        }
        public ActionResult Payments()
        {
            return View(db.payments.ToList());
        }

        public ActionResult Reviews()
        {
            return View(db.doctor_reviews.ToList());
        }
    }
}