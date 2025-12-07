using ClinicManagement.Data;
using ClinicManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Admin Dashboard
        public IActionResult Index()
        {
            return View();
        }

        // View all users
        public IActionResult ManageUsers()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // View patients grouped by doctor
        public async Task<IActionResult> PatientsByDoctor()
        {
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");

            var data = new Dictionary<IdentityUser, List<Patient>>();

            foreach (var doctor in doctors)
            {
                var patients = await _context.Patients
                    .Where(p => p.DoctorId == doctor.Id)
                    .ToListAsync();

                data.Add(doctor, patients);
            }

            return View(data);
        }

        public async Task<IActionResult> Statistics()
        {
            var model = new AdminStatsVM();

            // Total Users
            model.TotalUsers = _userManager.Users.Count();

            // Total Doctors
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            model.TotalDoctors = doctors.Count;

            // Total Patients
            model.TotalPatients = _context.Patients.Count();

            // Total Appointments (if you add appointment table later)
            model.TotalAppointments = _context.Appointments.Count();


            // Patients per month (current year)
            model.PatientsPerMonth = _context.Patients
                .GroupBy(p => p.DateOfVisit.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToDictionary(g => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Month), g => g.Count);

            return View(model);
        }


    }
}
