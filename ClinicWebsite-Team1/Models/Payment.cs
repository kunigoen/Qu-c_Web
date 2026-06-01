using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicWebsite_Team1.Models
{
    public partial class Payment
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionCode { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Status { get; set; }

        public virtual Appointment Appointment { get; set; }
    }
}