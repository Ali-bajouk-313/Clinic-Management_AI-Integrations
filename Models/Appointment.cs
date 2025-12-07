using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }

        public string? DoctorId { get; set; }
        public IdentityUser? Doctor { get; set; }

        public DateTime Date { get; set; }

        public string? Notes { get; set; }
        public string? DoctorNotes { get; set; } // medical notes

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        public List<MedicalFile> MedicalFiles { get; set; } = new List<MedicalFile>();

        public Payment? Payment { get; set; } // navigation

        public string? AISummary { get; set; } //AI


    }
}
