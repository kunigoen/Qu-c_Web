using ClinicWebsite_Team1.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClinicWebsite_Team1.Controllers
{
    public class PatientController : Controller
    {
        ClinicWebsiteDataContext db =
            new ClinicWebsiteDataContext(
                System.Configuration.ConfigurationManager
                    .ConnectionStrings["ClinicWebsiteConnectionString"]
                    .ConnectionString);

        // 1. My Profile - View Profile
        public ActionResult Profile()
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            return View(currentPatient);
        }

        // 1. My Profile - Update Profile
        [HttpPost]
        public ActionResult UpdateProfile(
            string fullName,
            string phoneNumber,
            DateTime? dateOfBirth,
            string gender,
            string address,
            string emergencyContact,
            string bloodType)
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            var user = currentPatient.user_account;

            user.full_name = fullName;
            user.phone_number = phoneNumber;
            user.updated_at = DateTime.Now;

            currentPatient.phone_number = phoneNumber;
            currentPatient.date_of_birth = dateOfBirth;
            currentPatient.gender = gender;
            currentPatient.address = address;
            currentPatient.emergency_contact = emergencyContact;
            currentPatient.blood_type = bloodType;
            currentPatient.updated_at = DateTime.Now;

            db.SubmitChanges();

            Session["UserName"] = fullName;

            TempData["Success"] = "Profile updated successfully.";

            return RedirectToAction("Profile");
        }

        // 1. My Profile - Change Password
        [HttpPost]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            var user = currentPatient.user_account;

            if (user.password_hash != currentPassword)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("Profile");
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New password and confirm password do not match.";
                return RedirectToAction("Profile");
            }

            user.password_hash = newPassword;
            user.updated_at = DateTime.Now;

            db.SubmitChanges();

            TempData["Success"] = "Password changed successfully.";

            return RedirectToAction("Profile");
        }

        // 1. My Profile - Upload Avatar
        [HttpPost]
        public ActionResult UploadAvatar(HttpPostedFileBase avatarFile)
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            if (avatarFile == null || avatarFile.ContentLength == 0)
            {
                TempData["Error"] = "Please select an image.";
                return RedirectToAction("Profile");
            }

            string extension = Path.GetExtension(avatarFile.FileName);

            if (extension == null)
            {
                TempData["Error"] = "Invalid file.";
                return RedirectToAction("Profile");
            }

            extension = extension.ToLower();

            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                TempData["Error"] = "Only JPG, JPEG, and PNG files are allowed.";
                return RedirectToAction("Profile");
            }

            string fileName = "patient_" + currentPatient.id + "_" + DateTime.Now.Ticks + extension;
            string folderPath = Server.MapPath("~/Content/images/avatars/");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fullPath = Path.Combine(folderPath, fileName);
            avatarFile.SaveAs(fullPath);

            currentPatient.user_account.avatar_url = "~/Content/images/avatars/" + fileName;
            currentPatient.user_account.updated_at = DateTime.Now;

            db.SubmitChanges();

            TempData["Success"] = "Avatar uploaded successfully.";

            return RedirectToAction("Profile");
        }

        // 2. Book Appointment - GET
        public ActionResult BookAppointment(int? specialtyId, int? doctorId)
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.Specialties = db.specialties
                .OrderBy(x => x.name)
                .ToList();

            ViewBag.SelectedSpecialtyId = specialtyId;
            ViewBag.SelectedDoctorId = doctorId;

            var doctorsQuery = db.doctors
                .Where(x => x.status == "Active");

            if (specialtyId.HasValue)
            {
                doctorsQuery = doctorsQuery.Where(x => x.specialty_id == specialtyId.Value);
            }

            ViewBag.Doctors = doctorsQuery
                .OrderBy(x => x.user_account.full_name)
                .ToList();

            if (doctorId.HasValue)
            {
                var selectedDoctor = db.doctors.FirstOrDefault(x => x.id == doctorId.Value);
                ViewBag.SelectedDoctor = selectedDoctor;

                ViewBag.Schedules = db.schedules
                    .Where(x => x.doctor_id == doctorId.Value
                             && x.work_date >= DateTime.Today)
                    .OrderBy(x => x.work_date)
                    .ThenBy(x => x.start_time)
                    .ToList();
            }
            else
            {
                ViewBag.SelectedDoctor = null;
                ViewBag.Schedules = new List<schedule>();
            }

            return View();
        }

        // 2. Book Appointment - POST
        [HttpPost]
        public ActionResult BookAppointment(int doctorId, int scheduleId, string symptoms, string note)
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            var schedule = db.schedules.FirstOrDefault(x =>
                x.id == scheduleId &&
                x.doctor_id == doctorId
            );

            if (schedule == null)
            {
                TempData["Error"] = "Selected schedule does not exist.";
                return RedirectToAction("BookAppointment", new { doctorId = doctorId });
            }

            int booked = schedule.booked_patients ?? 0;
            int available = schedule.max_patients - booked;

            if (schedule.status != "Available" || available <= 0)
            {
                TempData["Error"] = "This schedule is busy or full. Please select another schedule.";
                return RedirectToAction("BookAppointment", new { doctorId = doctorId });
            }

            var pendingStatus = db.appointment_status.FirstOrDefault(x => x.name == "Pending");

            if (pendingStatus == null)
            {
                TempData["Error"] = "Appointment status data is missing.";
                return RedirectToAction("BookAppointment", new { doctorId = doctorId });
            }

            var appointment = new appointment
            {
                patient_id = currentPatient.id,
                doctor_id = doctorId,
                schedule_id = scheduleId,
                status_id = pendingStatus.id,
                symptoms = symptoms,
                note = note,
                created_at = DateTime.Now
            };

            db.appointments.InsertOnSubmit(appointment);

            schedule.booked_patients = booked + 1;

            if (schedule.booked_patients >= schedule.max_patients)
            {
                schedule.status = "Full";
            }

            db.SubmitChanges();

            TempData["Success"] = "Appointment booked successfully.";

            return RedirectToAction("MyAppointments");
        }

        // 3. My Appointments
        public ActionResult MyAppointments()
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            var appointments = db.appointments
                                 .Where(x => x.patient_id == currentPatient.id)
                                 .OrderByDescending(x => x.created_at)
                                 .ToList();

            return View(appointments);
        }

        // 3. Appointment Details
        public ActionResult AppointmentDetails(int id)
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            var appointment = db.appointments.FirstOrDefault(x =>
                x.id == id &&
                x.patient_id == currentPatient.id
            );

            if (appointment == null)
            {
                return HttpNotFound();
            }

            return View(appointment);
        }

        // 4. Cancel Appointment - GET
        public ActionResult CancelAppointment(int id)
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            var appointment = db.appointments.FirstOrDefault(x =>
                x.id == id &&
                x.patient_id == currentPatient.id
            );

            if (appointment == null)
            {
                return HttpNotFound();
            }

            return View(appointment);
        }

        // 4. Cancel Appointment - POST
        [HttpPost]
        public ActionResult CancelAppointment(int appointmentId, string reason)
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            var appointment = db.appointments.FirstOrDefault(x =>
                x.id == appointmentId &&
                x.patient_id == currentPatient.id
            );

            if (appointment == null)
            {
                return HttpNotFound();
            }

            var cancelledStatus = db.appointment_status.FirstOrDefault(x => x.name == "Cancelled");

            if (cancelledStatus == null)
            {
                TempData["Error"] = "Cancelled status does not exist.";
                return RedirectToAction("MyAppointments");
            }

            appointment.status_id = cancelledStatus.id;
            appointment.note = "Cancel reason: " + reason;
            appointment.updated_at = DateTime.Now;

            var schedule = appointment.schedule;

            if (schedule != null && schedule.booked_patients.HasValue && schedule.booked_patients > 0)
            {
                schedule.booked_patients = schedule.booked_patients - 1;

                if (schedule.status == "Full")
                {
                    schedule.status = "Available";
                }
            }

            db.SubmitChanges();

            TempData["Success"] = "Appointment cancelled successfully.";

            return RedirectToAction("MyAppointments");
        }

        // 5. Payment - View payment information
        public ActionResult Payment(int id)
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            var appointment = db.appointments.FirstOrDefault(x =>
                x.id == id &&
                x.patient_id == currentPatient.id
            );

            if (appointment == null)
            {
                return HttpNotFound();
            }

            var payment = db.payments.FirstOrDefault(x => x.appointment_id == id);

            if (payment == null)
            {
                payment = new payment
                {
                    appointment_id = appointment.id,
                    amount = appointment.doctor.consultation_fee,
                    payment_method = "Not selected",
                    transaction_code = "PENDING-" + DateTime.Now.Ticks,
                    payment_date = null,
                    status = "Pending"
                };

                db.payments.InsertOnSubmit(payment);
                db.SubmitChanges();
            }

            var transactionHistory = db.payments
                .Where(x => x.appointment.patient_id == currentPatient.id)
                .OrderByDescending(x => x.payment_date)
                .ToList();

            ViewBag.TransactionHistory = transactionHistory;

            return View(payment);
        }

        // 5. Payment - Confirm online payment
        [HttpPost]
        public ActionResult ConfirmPayment(
            int paymentId,
            string paymentMethod,
            string accountName,
            string accountNumber,
            string bankName,
            string walletPhone,
            string paypalEmail)
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            var payment = db.payments.FirstOrDefault(x =>
                x.id == paymentId &&
                x.appointment.patient_id == currentPatient.id
            );

            if (payment == null)
            {
                return HttpNotFound();
            }

            payment.payment_method = paymentMethod;
            payment.status = "Paid";
            payment.payment_date = DateTime.Now;

            string prefix = "TXN";

            if (paymentMethod == "VNPay")
            {
                prefix = "VNPAY";
            }
            else if (paymentMethod == "MoMo")
            {
                prefix = "MOMO";
            }
            else if (paymentMethod == "PayPal")
            {
                prefix = "PAYPAL";
            }
            else if (paymentMethod == "Bank Transfer")
            {
                prefix = "BANK";
            }

            payment.transaction_code = prefix + "-" + DateTime.Now.Ticks;

            db.SubmitChanges();

            TempData["Success"] = "Payment completed successfully. Transaction code: " + payment.transaction_code;

            return RedirectToAction("Payment", new { id = payment.appointment_id });
        }

        // 6. Doctor Review - GET
        public ActionResult ReviewDoctor(int id)
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            var appointment = db.appointments.FirstOrDefault(x =>
                x.id == id &&
                x.patient_id == currentPatient.id
            );

            if (appointment == null)
            {
                return HttpNotFound();
            }

            if (appointment.appointment_status.name != "Completed")
            {
                TempData["Error"] = "You can only review completed appointments.";
                return RedirectToAction("MyAppointments");
            }

            var existedReview = db.doctor_reviews.FirstOrDefault(x => x.appointment_id == id);

            ViewBag.Appointment = appointment;
            ViewBag.ExistedReview = existedReview;

            return View();
        }

        // 6. Doctor Review - POST
        [HttpPost]
        public ActionResult SubmitReview(int appointmentId, int rating, string comment)
        {
            patient currentPatient;
            var accessResult = CheckPatientAccess(out currentPatient);

            if (accessResult != null)
            {
                return accessResult;
            }

            var appointment = db.appointments.FirstOrDefault(x =>
                x.id == appointmentId &&
                x.patient_id == currentPatient.id
            );

            if (appointment == null)
            {
                return HttpNotFound();
            }

            var existedReview = db.doctor_reviews.FirstOrDefault(x => x.appointment_id == appointmentId);

            if (existedReview != null)
            {
                existedReview.rating = rating;
                existedReview.comment = comment;
                existedReview.review_date = DateTime.Now;
            }
            else
            {
                var review = new doctor_review
                {
                    appointment_id = appointment.id,
                    doctor_id = appointment.doctor_id,
                    patient_id = currentPatient.id,
                    rating = rating,
                    comment = comment,
                    review_date = DateTime.Now
                };

                db.doctor_reviews.InsertOnSubmit(review);
            }

            db.SubmitChanges();

            TempData["Success"] = "Review submitted successfully.";

            return RedirectToAction("MyAppointments");
        }
        private patient GetCurrentPatient()
        {
            return db.patients.FirstOrDefault();
        }

        private ActionResult CheckPatientAccess(out patient currentPatient)
        {
            currentPatient = GetCurrentPatient();
            return null;
        }
    }

}