using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public partial class AppointmentService
    {
        public int AppointmentId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public virtual Appointment Appointment { get; set; }
        public virtual MedicalService MedicalService { get; set; }
    }
}