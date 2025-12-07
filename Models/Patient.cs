using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Models
{
    public class Patient
    {

        public int Id { get; set; }

        [Required]
        [Display(Name = "Patient Name")]
        public string Name { get; set; }

        [Required]
        [Range(0, 120)]
        public int Age { get; set; }

        [Required]
        public string Diagnosis { get; set; }

        [Display(Name = "Visit Date")]
        [DataType(DataType.Date)]
        public DateTime DateOfVisit { get; set; }

        [Display(Name = "Next Visit")]
        [DataType(DataType.Date)]
        public DateTime? DateOfRevisit { get; set; }

        // Foreign Key to Doctor User
        public string DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public IdentityUser Doctor { get; set; }
    
    }
}
