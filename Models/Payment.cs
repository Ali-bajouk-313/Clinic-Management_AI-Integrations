namespace ClinicManagement.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public string PaymentMethod { get; set; } // e.g. "Stripe"
        public string Status { get; set; } // e.g. "Paid"

        

    }
}
