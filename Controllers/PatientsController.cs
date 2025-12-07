using ClinicManagement.Data;
using ClinicManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicManagement.Controllers
{
    [Authorize]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Display only patients belonging to logged in Doctor
        public async Task<IActionResult> Index()
        {
            var doctorId = _userManager.GetUserId(User);
            var patients = await _context.Patients
                .Where(p => p.DoctorId == doctorId)
                .ToListAsync();

            return View(patients);
        }

        // GET: Create patient
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Patient patient)
        {
                patient.DoctorId = _userManager.GetUserId(User);
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
            return View(patient);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var doctorId = _userManager.GetUserId(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctorId);

            if (patient == null) return NotFound();
            return View(patient);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Patient patient)
        {
            if (id != patient.Id) return BadRequest();
            var doctorId = _userManager.GetUserId(User);

            
            
                patient.DoctorId = doctorId; // ensure no tampering
                _context.Update(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
            return View(patient);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int id)
        {
            var doctorId = _userManager.GetUserId(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctorId);

            if (patient == null) return NotFound();
            return View(patient);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> History(int id)
        {
            var doctorId = _userManager.GetUserId(User);

            // Ensure doctor only views their own patient
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctorId);

            if (patient == null)
                return Unauthorized();

            var history = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.MedicalFiles)     // 👈 REQUIRED!!!
                .Where(a => a.PatientId == id)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return View(history);
        }

        public async Task<IActionResult> PrintPatientReport(int id)
        {
            // Get patient object
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            // Get appointment history
            var history = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.MedicalFiles)
                .Where(a => a.PatientId == id)
                .OrderBy(a => a.Date)
                .ToListAsync();

            // PASS SAFE DATA TO VIEW
            ViewBag.PatientName = patient.Name;
            ViewBag.PatientId = id;

            return View("PrintPatientReport", history);
        }

        public async Task<IActionResult> Details(int id)
        {
            var doctorId = _userManager.GetUserId(User);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctorId);

            if (patient == null)
                return NotFound();   // or Unauthorized()

            return View(patient);
        }


    }
}
