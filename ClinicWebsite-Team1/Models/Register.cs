using ClinicWebsite_Team1.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

    namespace ClinicWebsite_Team1.Models
    {
        public class Register
        {
        [Required]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Full Name must contain letters only")]
        public string FullName { get; set; }

        public string Gender { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string PhoneNumber { get; set; }

            [Required]
            [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
            [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
                ErrorMessage = "Password must contain letters and numbers")]
            public string Password { get; set; }

            [Required]
            [Compare("Password", ErrorMessage = "Passwords do not match")]
            public string ConfirmPassword { get; set; }
        }
    }
