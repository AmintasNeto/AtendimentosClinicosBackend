using BaseLibrary.Entities;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController(IAppointment appointmentInterface) : ControllerBase
    {
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("Invalid request sent");
            return Ok(await appointmentInterface.DeleteByID(id));
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(Appointment model)
        {
            if (model is null) return BadRequest("Bad request made");
            return Ok(await appointmentInterface.Insert(model));
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(Appointment model)
        {
            if (model is null) return BadRequest("Bad request made");
            return Ok(await appointmentInterface.Update(model));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllByUser(string token)
        {
            if (token == null || token.Trim().Equals("")) return BadRequest("Invalid token"); 
            return Ok(await appointmentInterface.GetAllByUser(token));
        }

        [HttpGet("all-openned")]
        public async Task<IActionResult> GetAllOpenned()
        {
            return Ok(await appointmentInterface.GetAllOpenned());
        }

        [HttpPut("schedule")]
        public async Task<IActionResult> Schedule(Appointment appointment)
        {
            return Ok(await appointmentInterface.Schedule(appointment));
        }

        [HttpDelete("cancel-schedule")]
        public async Task<IActionResult> CancelSchedule(int id)
        {
            if (id <= 0) return BadRequest("Invalid request sent");
            return Ok(await appointmentInterface.CancelSchedule(id));
        }
    }
}
