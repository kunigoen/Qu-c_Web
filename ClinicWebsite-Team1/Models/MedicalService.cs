using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public partial class MedicalService
    {
        public MedicalService()
        {
            AppointmentServices = new HashSet<AppointmentService>();
        }

        public int Id { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int? DurationMinutes { get; set; }
        public bool Status { get; set; }

        public virtual ICollection<AppointmentService> AppointmentServices { get; set; }
    }
}