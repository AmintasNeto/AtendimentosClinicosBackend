using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibrary.Entities;
using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IAppointment
    {
        Task<GeneralResponse> Insert(Appointment item);
        Task<GeneralResponse> Update(Appointment item);
        Task<GeneralResponse> DeleteByID(int id);
        Task<List<Appointment>> GetAllByUser(string token);
        Task<List<Appointment>> GetAllOpenned();
        Task<GeneralResponse> Schedule(Appointment item);
        Task<GeneralResponse> CancelSchedule(int id);
    }
}
