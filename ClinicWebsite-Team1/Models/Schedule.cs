using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public partial class Schedule
    {
        public Schedule()
        {
            Appointments = new HashSet<Appointment>();
        }

        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int RoomId { get; set; }
        public DateTime WorkDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxPatients { get; set; }
        public int BookedPatients { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Doctor Doctor { get; set; }
        public virtual ClinicRoom ClinicRoom { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}