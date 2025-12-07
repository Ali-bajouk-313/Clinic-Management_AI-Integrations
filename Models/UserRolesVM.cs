namespace ClinicManagement.Models
{
    public class UserRolesVM
    {

        public string UserId { get; set; }
        public string Email { get; set; }

        public List<string> Roles { get; set; }
        public string SelectedRole { get; set; }
    }
}
