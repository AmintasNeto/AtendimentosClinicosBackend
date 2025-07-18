using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary.Entities
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public DateOnly AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentStartTime { get; set; }

        [Required]
        public TimeSpan AppointmentEndTime { get; set; }

        [Required]
        public bool IsCanceled { get; set; } = false;

        public ApplicationUser? Doctor { get; set; }
        [Required]
        public int DoctorId { get; set; }

        public ApplicationUser? Patient { get; set; }
        public int? PatientId { get; set; }
    }
}
