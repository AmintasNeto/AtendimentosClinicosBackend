using BaseLibrary.Entities;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController(IGenerictRepositoryInterface<Appointment> generictRepositoryInterface,
            IAppointmentInterface appointmentInterface
        ) 
        : GenericController<Appointment>(generictRepositoryInterface)
    {
        [HttpGet]
        public async Task<IActionResult> GetAllByUser(string refreshToken)
        {
            if (refreshToken == null || refreshToken.Trim().Equals("")) return BadRequest("Invalid token"); 
            return Ok(await appointmentInterface.GetAllByUser(refreshToken));
        }
    }
}
