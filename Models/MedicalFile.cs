namespace ClinicManagement.Models
{
    public class MedicalFile
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        public string FilePath { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
