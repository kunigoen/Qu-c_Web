using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public class ResetPassword
    {
        [Required]
        public string Token { get; set; }
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
                ErrorMessage = "Password must contain letters and numbers")]
        public string NewPassword { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}