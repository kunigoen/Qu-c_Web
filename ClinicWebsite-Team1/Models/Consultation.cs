using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public partial class Consultation
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string Diagnosis { get; set; }
        public string Prescription { get; set; }
        public string DoctorNotes { get; set; }
        public DateTime ConsultationDate { get; set; }

        public virtual Appointment Appointment { get; set; }
    }
}