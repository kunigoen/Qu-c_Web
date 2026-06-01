using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public partial class Appointment
    {
        public Appointment()
        {
            AppointmentServices = new HashSet<AppointmentService>();
        }

        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int ScheduleId { get; set; }
        public int StatusId { get; set; }
        public string Symptoms { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Patient Patient { get; set; }
        public virtual Doctor Doctor { get; set; }
        public virtual Schedule Schedule { get; set; }
        public virtual AppointmentStatus AppointmentStatus { get; set; }

        public virtual Consultation Consultation { get; set; }
        public virtual Payment Payment { get; set; }
        public virtual DoctorReview DoctorReview { get; set; }

        public virtual ICollection<AppointmentService> AppointmentServices { get; set; }
    }
}