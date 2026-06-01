using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public partial class Doctor
    {
        public Doctor()
        {
            Appointments = new HashSet<Appointment>();
            DoctorReviews = new HashSet<DoctorReview>();
            Schedules = new HashSet<Schedule>();
        }

        public int Id { get; set; }
        public int UserAccountId { get; set; }
        public int SpecialtyId { get; set; }
        public string LicenseNumber { get; set; }
        public int ExperienceYears { get; set; }
        public decimal ConsultationFee { get; set; }
        public string Biography { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual UserAccount UserAccount { get; set; }
        public virtual Specialty Specialty { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<DoctorReview> DoctorReviews { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
        public object FeaturedDoctor { get; internal set; }
    }
}