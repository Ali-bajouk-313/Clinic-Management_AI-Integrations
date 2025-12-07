using ClinicManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admin can manage roles
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // Display All Roles
        public IActionResult Index()
        {
            var roles = _roleManager.Roles
                .Select(r => new RoleVM { Id = r.Id, Name = r.Name })
                .ToList();

            return View(roles);
        }

        // Show Create Role Form
        public IActionResult Create()
        {
            return View();
        }

        // Handle Create Request
        [HttpPost]
        public async Task<IActionResult> Create(RoleVM model)
        {
            
                var roleExists = await _roleManager.RoleExistsAsync(model.Name);

                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Name));
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Role already exists");
            

            return View(model);
        }

        // GET: Role/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            return View(new RoleVM { Id = role.Id, Name = role.Name });
        }

        // POST: Role/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            // Optional: Prevent deleting Admin
            if (role.Name == "Admin")
            {
                ModelState.AddModelError("", "The Admin role cannot be deleted.");
                return View(new RoleVM { Id = role.Id, Name = role.Name });
            }

            await _roleManager.DeleteAsync(role);

            return RedirectToAction(nameof(Index));
        }

    }
}
