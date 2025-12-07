using ClinicManagement.Data;
using ClinicManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Services;



namespace ClinicManagement.Controllers
{
    [Authorize(Roles = "Secretary,Admin,Doctor")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly MedicalAiService _medicalAiService;

        public AppointmentController(ApplicationDbContext context, UserManager<IdentityUser> userManager, MedicalAiService medicalAiService)
        {

            _context = context;
            _userManager = userManager;
            _medicalAiService = medicalAiService;

        }

        public async Task<IActionResult> Index(string doctorId, string patientName)
        {
            ViewBag.Doctors = await _userManager.GetUsersInRoleAsync("Doctor");

            var appts = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                  .Include(a => a.Payment)  // REQUIRED


                .AsQueryable();

            if (!string.IsNullOrEmpty(doctorId))
            {
                appts = appts.Where(a => a.DoctorId == doctorId);
            }

            if (!string.IsNullOrWhiteSpace(patientName))
            {
                appts = appts.Where(a => a.Patient.Name.Contains(patientName));
            }

            return View(await appts.ToListAsync());
        }


        // GET — Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Patients = _context.Patients.ToList();
            ViewBag.Doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            return View();
        }

        // POST — Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            if (appointment.PatientId == null || appointment.DoctorId == null)
            {
                ModelState.AddModelError("", "Doctor and Patient are required");
            }

            
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            

            ViewBag.Patients = _context.Patients.ToList();
            ViewBag.Doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            return View(appointment);
        }

        // GET — Edit
        public async Task<IActionResult> Edit(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appt == null)
                return NotFound();

            return View(appt);
        }

        // POST — Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appt)
        {
            if (id != appt.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }

            // reload related objects
            appt.Patient = await _context.Patients.FindAsync(appt.PatientId);
            appt.Doctor = await _userManager.FindByIdAsync(appt.DoctorId);

            return View(appt);
        }

        // GET — Delete
        public async Task<IActionResult> Delete(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appt == null)
                return NotFound();

            return View(appt);
        }

        // POST — DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);

            if (appt != null)
            {
                _context.Appointments.Remove(appt);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Today()
        {
            var today = DateTime.Today;

            ViewBag.Doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            ViewBag.Patients = _context.Patients.ToList();

            var appts = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.Date.Date == today)
                .OrderBy(a => a.Date)
                .ToListAsync();

            return View(appts);
        }

        public async Task<IActionResult> MarkArrived(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            appt.Status = AppointmentStatus.Arrived;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MarkNoShow(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            appt.Status = AppointmentStatus.NoShow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MarkCancelled(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            appt.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Reschedule(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appt == null)
                return NotFound();

            return View(appt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime newDate)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null)
                return NotFound();

            if (newDate < DateTime.Now)
            {
                ModelState.AddModelError("", "❗ Cannot schedule in the past.");
                return View(appt);
            }

            appt.Date = newDate;
            appt.Status = AppointmentStatus.Scheduled;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> MyAppointments()
        {
            var doctorId = _userManager.GetUserId(User);

            var appts = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .OrderBy(a => a.Date)
                .ToListAsync();

            return View(appts);
        }
        [HttpGet]  // <-- REQUIRED
        public async Task<IActionResult> AddNotesDr(int id)
        {
            var doctorId = _userManager.GetUserId(User);

            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);

            if (appt == null)
                return Unauthorized();

            return View("AddNotesDr", appt); // explicitly load view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNotesDr(int id, string DoctorNotes, List<IFormFile> files)
        {
            var doctorId = _userManager.GetUserId(User);

            var appt = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);

            if (appt == null)
                return Unauthorized();

            // Save doctor notes
            appt.DoctorNotes = DoctorNotes ?? "";
            appt.Status = AppointmentStatus.Completed;
            await _context.SaveChangesAsync();

            // ============ SAVE UPLOADED FILES ============
            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Create uploads folder if missing
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                        if (!Directory.Exists(uploadPath))
                            Directory.CreateDirectory(uploadPath);

                        // Generate unique filename
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        // Save file physically to server
                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Save file record in DB
                        _context.MedicalFiles.Add(new MedicalFile()
                        {
                            AppointmentId = appt.Id,
                            FilePath = "/uploads/" + fileName,
                            FileName = file.FileName,
                            UploadedAt = DateTime.Now
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }
            

            return RedirectToAction(nameof(MyAppointments));
        }

        public async Task<IActionResult> GenerateAISummary(int id)
        {
            var doctorId = _userManager.GetUserId(User);

            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);

            if (appt == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(appt.DoctorNotes))
            {
                TempData["Error"] = "Cannot generate AI summary — doctor notes are empty.";
                return RedirectToAction(nameof(MyAppointments));
            }

            string summary = "";

            try
            {
                // AI SUMMARY GENERATION
                summary = await _medicalAiService.GeneratePatientSummary(
                    appt.Patient.Name,
                    appt.Patient.Age,
                    appt.DoctorNotes ?? "",
                    appt.Patient.Diagnosis ?? ""
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 AI ERROR: " + ex.Message);
                Console.WriteLine("🔥 STACK TRACE: " + ex.StackTrace);

                TempData["Error"] = "AI summary request failed — system exception occurred.";
                return RedirectToAction(nameof(MyAppointments));
            }

            // If OpenAI returns nothing
            if (string.IsNullOrWhiteSpace(summary))
            {
                TempData["Error"] = "AI summary failed to generate. Try again later.";
                return RedirectToAction(nameof(MyAppointments));
            }

            // SAVE TO DB
            appt.AISummary = summary;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                Console.WriteLine("🔥 DATABASE SAVE ERROR: " + dbEx.Message);
                TempData["Error"] = "Database error while saving AI summary.";
                return RedirectToAction(nameof(MyAppointments));
            }

            TempData["Success"] = "AI summary successfully generated!";
            return RedirectToAction(nameof(MyAppointments));
        }


        public async Task<IActionResult> ViewAISummary(int id)
        {
            var doctorId = _userManager.GetUserId(User);

            var appt = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);

            if (appt == null)
                return Unauthorized();

            return View(appt);
        }


        public async Task<IActionResult> PaymentStatusList()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Payment)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return View(appointments);
        }
        ///

        public async Task<IActionResult> AiChat(int id)
        {
            var doctorId = _userManager.GetUserId(User);

            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);

            if (appt == null)
                return Unauthorized();

            return View(appt); // Send appointment info to the Razor page
        }

        public async Task<IActionResult> SendAiMessage(int id, string message)
        {
            var doctorId = _userManager.GetUserId(User);

            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);

            if (appt == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Message cannot be empty.");

            try
            {
                string response = await _medicalAiService.ChatWithAI(
                    appt.Patient.Name,
                    appt.Patient.Age,
                    appt.DoctorNotes ?? "",
                    appt.Patient.Diagnosis ?? "",
                    message // 👈 Prompt typed by the doctor
                );

                return Json(new { success = true, reply = response });
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 AI CHAT ERROR: " + ex.Message);
                return Json(new { success = false, reply = "AI Chat failed. Try again later." });
            }
        }



    }

}
