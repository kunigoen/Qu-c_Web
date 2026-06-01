using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public partial class ClinicRoom
    {
        public ClinicRoom()
        {
            Schedules = new HashSet<Schedule>();
        }

        public int Id { get; set; }
        public string RoomCode { get; set; }
        public string RoomName { get; set; }
        public int? FloorNumber { get; set; }
        public string Status { get; set; }

        public virtual ICollection<Schedule> Schedules { get; set; }
    }
}