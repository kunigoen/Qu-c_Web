using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public partial class Specialty
    {
        public Specialty()
        {
            Doctors = new HashSet<Doctor>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public virtual ICollection<Doctor> Doctors { get; set; }
    }
}