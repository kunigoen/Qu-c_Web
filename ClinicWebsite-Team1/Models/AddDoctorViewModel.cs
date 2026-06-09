using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public class AddDoctorViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        public int SpecialtyId { get; set; }

        [Required]
        public string LicenseNumber { get; set; }

        public int ExperienceYears { get; set; }

        public decimal ConsultationFee { get; set; }

        public string Status { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }
    }
}