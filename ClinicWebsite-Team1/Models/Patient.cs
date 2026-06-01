using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public partial class Patient
    {
        public Patient()
        {
            Appointments = new HashSet<Appointment>();
            DoctorReviews = new HashSet<DoctorReview>();
        }

        public int Id { get; set; }
        public int UserAccountId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string EmergencyContact { get; set; }
        public string BloodType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual UserAccount UserAccount { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<DoctorReview> DoctorReviews { get; set; }
    }
}