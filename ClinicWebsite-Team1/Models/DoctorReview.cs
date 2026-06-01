using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public partial class DoctorReview
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public int? Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; }

        public virtual Appointment Appointment { get; set; }
        public virtual Doctor Doctor { get; set; }
        public virtual Patient Patient { get; set; }
    }
}