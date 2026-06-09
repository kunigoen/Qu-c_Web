using ClinicWebsite_Team1.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace ClinicWebsite_Team1.Controllers
{
    public class DoctorController : Controller
    {
        ClinicWebsiteDataContext db =
            new ClinicWebsiteDataContext(
                System.Configuration.ConfigurationManager
                    .ConnectionStrings["ClinicWebsiteConnectionString"]
                    .ConnectionString);

        private doctor GetCurrentDoctor()
        {
            return db.doctors.FirstOrDefault();
        }

        private ActionResult CheckDoctorAccess(out doctor currentDoctor)
        {
            currentDoctor = GetCurrentDoctor();
            return null;
        }

        // 1. Doctor Dashboard
        public ActionResult Dashboard()
        {
            doctor currentDoctor;
            var accessResult = CheckDoctorAccess(out currentDoctor);
            if (accessResult != null) return accessResult;

            int doctorId = currentDoctor.id;
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            int totalAppointments = db.appointments.Count(x => x.doctor_id == doctorId);

            int todayAppointments = db.appointments.Count(x =>
                x.doctor_id == doctorId &&
                x.schedule.work_date >= today &&
                x.schedule.work_date < tomorrow
            );

            int patientCount = db.appointments
                .Where(x => x.doctor_id == doctorId)
                .Select(x => x.patient_id)
                .Distinct()
                .Count();

            var recentActivities = db.appointments
                .Where(x => x.doctor_id == doctorId)
                .OrderByDescending(x => x.created_at)
                .Take(5)
                .ToList();

            ViewBag.Doctor = currentDoctor;
            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.TodayAppointments = todayAppointments;
            ViewBag.PatientCount = patientCount;
            ViewBag.RecentActivities = recentActivities;

            return View();
        }

        // 2. My Schedule
        public ActionResult Schedule()
        {
            doctor currentDoctor;
            var accessResult = CheckDoctorAccess(out currentDoctor);
            if (accessResult != null) return accessResult;

            int doctorId = currentDoctor.id;

            var schedules = db.schedules
                .Where(x => x.doctor_id == doctorId)
                .OrderByDescending(x => x.work_date)
                .ThenBy(x => x.start_time)
                .ToList();

            ViewBag.Doctor = currentDoctor;
            ViewBag.Rooms = db.clinic_rooms
                .OrderBy(x => x.room_code)
                .ToList();

            return View(schedules);
        }

        // Doctor - Add Schedule
        [HttpPost]
        public ActionResult AddSchedule(
            int roomId,
            DateTime workDate,
            TimeSpan startTime,
            TimeSpan endTime,
            int maxPatients,
            string status)
        {
            doctor currentDoctor;
            var accessResult = CheckDoctorAccess(out currentDoctor);
            if (accessResult != null) return accessResult;

            if (endTime <= startTime)
            {
                TempData["Error"] = "End time must be later than start time.";
                return RedirectToAction("Schedule");
            }

            bool isOverlap = db.schedules.Any(x =>
                x.doctor_id == currentDoctor.id &&
                x.work_date == workDate &&
                x.status != "Closed" &&
                (
                    (startTime >= x.start_time && startTime < x.end_time) ||
                    (endTime > x.start_time && endTime <= x.end_time) ||
                    (startTime <= x.start_time && endTime >= x.end_time)
                )
            );

            if (isOverlap)
            {
                TempData["Error"] = "This schedule overlaps with an existing schedule.";
                return RedirectToAction("Schedule");
            }

            var schedule = new schedule
            {
                doctor_id = currentDoctor.id,
                room_id = roomId,
                work_date = workDate,
                start_time = startTime,
                end_time = endTime,
                max_patients = maxPatients,
                booked_patients = 0,
                status = string.IsNullOrWhiteSpace(status) ? "Available" : status,
                created_at = DateTime.Now
            };

            db.schedules.InsertOnSubmit(schedule);
            db.SubmitChanges();

            TempData["Success"] = "Schedule added successfully.";
            return RedirectToAction("Schedule");
        }

        // Doctor - Edit Schedule
        [HttpPost]
        public ActionResult EditSchedule(
            int id,
            int roomId,
            DateTime workDate,
            TimeSpan startTime,
            TimeSpan endTime,
            int maxPatients,
            string status)
        {
            doctor currentDoctor;
            var accessResult = CheckDoctorAccess(out currentDoctor);
            if (accessResult != null) return accessResult;

            var schedule = db.schedules.FirstOrDefault(x =>
                x.id == id &&
                x.doctor_id == currentDoctor.id
            );

            if (schedule == null)
            {
                return HttpNotFound();
            }

            if (endTime <= startTime)
            {
                TempData["Error"] = "End time must be later than start time.";
                return RedirectToAction("Schedule");
            }

            int bookedPatients = schedule.booked_patients ?? 0;

            if (maxPatients < bookedPatients)
            {
                TempData["Error"] = "Max patients cannot be less than booked patients.";
                return RedirectToAction("Schedule");
            }

            bool isOverlap = db.schedules.Any(x =>
                x.id != id &&
                x.doctor_id == currentDoctor.id &&
                x.work_date == workDate &&
                x.status != "Closed" &&
                (
                    (startTime >= x.start_time && startTime < x.end_time) ||
                    (endTime > x.start_time && endTime <= x.end_time) ||
                    (startTime <= x.start_time && endTime >= x.end_time)
                )
            );

            if (isOverlap)
            {
                TempData["Error"] = "This schedule overlaps with an existing schedule.";
                return RedirectToAction("Schedule");
            }

            schedule.room_id = roomId;
            schedule.work_date = workDate;
            schedule.start_time = startTime;
            schedule.end_time = endTime;
            schedule.max_patients = maxPatients;
            schedule.status = status;

            if (bookedPatients >= maxPatients)
            {
                schedule.status = "Full";
            }

            db.SubmitChanges();

            TempData["Success"] = "Schedule updated successfully.";
            return RedirectToAction("Schedule");
        }

        // Doctor - Delete Schedule
        [HttpPost]
        public ActionResult DeleteSchedule(int id)
        {
            doctor currentDoctor;
            var accessResult = CheckDoctorAccess(out currentDoctor);
            if (accessResult != null) return accessResult;

            var schedule = db.schedules.FirstOrDefault(x =>
                x.id == id &&
                x.doctor_id == currentDoctor.id
            );

            if (schedule == null)
            {
                return HttpNotFound();
            }

            bool hasAppointments = db.appointments.Any(x => x.schedule_id == id);

            if (hasAppointments)
            {
                schedule.status = "Closed";
                db.SubmitChanges();

                TempData["Success"] = "Schedule has appointments, so it was closed instead of deleted.";
                return RedirectToAction("Schedule");
            }

            db.schedules.DeleteOnSubmit(schedule);
            db.SubmitChanges();

            TempData["Success"] = "Schedule deleted successfully.";
            return RedirectToAction("Schedule");
        }

        // Doctor - Update Availability Status
        [HttpPost]
        public ActionResult UpdateScheduleStatus(int id, string status)
        {
            doctor currentDoctor;
            var accessResult = CheckDoctorAccess(out currentDoctor);
            if (accessResult != null) return accessResult;

            var schedule = db.schedules.FirstOrDefault(x =>
                x.id == id &&
                x.doctor_id == currentDoctor.id
            );

            if (schedule == null)
            {
                return HttpNotFound();
            }

            int bookedPatients = schedule.booked_patients ?? 0;

            if (status == "Available" && bookedPatients >= schedule.max_patients)
            {
                TempData["Error"] = "Cannot set this schedule to Available because it is already full.";
                return RedirectToAction("Schedule");
            }

            schedule.status = status;
            db.SubmitChanges();

            TempData["Success"] = "Schedule status updated successfully.";
            return RedirectToAction("Schedule");
        }

        // 3. View Appointments
        public ActionResult Appointments(string status, DateTime? date)
        {
            doctor currentDoctor;
            var accessResult = CheckDoctorAccess(out currentDoctor);
            if (accessResult != null) return accessResult;

            int doctorId = currentDoctor.id;

            var appointments = db.appointments
                .Where(x => x.doctor_id == doctorId);

            if (!string.IsNullOrEmpty(status))
            {
                appointments = appointments.Where(x => x.appointment_status.name == status);
            }

            if (date.HasValue)
            {
                DateTime selectedDate = date.Value.Date;
                DateTime nextDate = selectedDate.AddDays(1);

                appointments = appointments.Where(x =>
                    x.schedule.work_date >= selectedDate &&
                    x.schedule.work_date < nextDate
                );
            }

            ViewBag.StatusList = db.appointment_status.ToList();
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedDate = date;

            var result = appointments
                .OrderByDescending(x => x.created_at)
                .ToList();

            return View(result);
        }

        // 4. Update Appointment Status
        [HttpPost]
        public ActionResult UpdateAppointmentStatus(int appointmentId, int statusId)
        {
            doctor currentDoctor;
            var accessResult = CheckDoctorAccess(out currentDoctor);
            if (accessResult != null) return accessResult;

            var appointment = db.appointments.FirstOrDefault(x =>
                x.id == appointmentId &&
                x.doctor_id == currentDoctor.id
            );

            if (appointment == null)
            {
                return HttpNotFound();
            }

            appointment.status_id = statusId;
            appointment.updated_at = DateTime.Now;

            db.SubmitChanges();

            TempData["Success"] = "Appointment status updated successfully.";

            return RedirectToAction("Appointments");
        }

        // 5. View Patient Information
        public ActionResult PatientDetails(int id)
        {
            doctor currentDoctor;
            var accessResult = CheckDoctorAccess(out currentDoctor);
            if (accessResult != null) return accessResult;

            var patient = db.patients.FirstOrDefault(x => x.id == id);

            if (patient == null)
            {
                return HttpNotFound();
            }

            bool hasAppointmentWithDoctor = db.appointments.Any(x =>
                x.patient_id == id &&
                x.doctor_id == currentDoctor.id
            );

            if (!hasAppointmentWithDoctor)
            {
                return RedirectToAction("Appointments");
            }

            var appointmentHistory = db.appointments
                .Where(x => x.patient_id == id && x.doctor_id == currentDoctor.id)
                .OrderByDescending(x => x.created_at)
                .ToList();

            ViewBag.AppointmentHistory = appointmentHistory;

            return View(patient);
        }

        // 6. View Reviews
        public ActionResult Reviews()
        {
            doctor currentDoctor;
            var accessResult = CheckDoctorAccess(out currentDoctor);
            if (accessResult != null) return accessResult;

            int doctorId = currentDoctor.id;

            var reviews = db.doctor_reviews
                .Where(x => x.doctor_id == doctorId)
                .OrderByDescending(x => x.review_date)
                .ToList();

            int totalReviews = reviews.Count;
            double averageRating = totalReviews > 0
                ? reviews.Average(x => x.rating ?? 0)
                : 0;

            ViewBag.TotalReviews = totalReviews;
            ViewBag.AverageRating = averageRating;

            return View(reviews);
        }
    }
}