using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentStartTime { get; set; }
        public TimeSpan AppointmentEndTime { get; set; }
        public bool IsCanceled { get; set; } = false;

        public ApplicationUser? Doctor { get; set; }
        public int DoctorId { get; set; }

        public ApplicationUser? Patient { get; set; }
        public int? PatientId { get; set; }
    }
}
