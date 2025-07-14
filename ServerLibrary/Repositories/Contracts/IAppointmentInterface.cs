using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibrary.Entities;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IAppointmentInterface
    {
        Task<List<Appointment>> GetAllByUser(string token);
    }
}
