using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public partial class AppointmentStatus
    {
        public AppointmentStatus()
        {
            Appointments = new HashSet<Appointment>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}