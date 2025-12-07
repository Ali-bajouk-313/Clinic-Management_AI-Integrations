namespace ClinicManagement.Models
{
    public class AdminStatsVM
    {
        public int TotalUsers { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; } 

        public Dictionary<string, int> PatientsPerMonth { get; set; }
    }
}
