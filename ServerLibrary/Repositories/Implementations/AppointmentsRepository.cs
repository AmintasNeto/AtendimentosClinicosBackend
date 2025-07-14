using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class AppointmentsRepository(AppDbContext appDbContext) : IGenerictRepositoryInterface<Appointment>, IAppointmentInterface
    {
        public async Task<GeneralResponse> DeleteByID(int id)
        {
            var appointment = await appDbContext.Appointments.FindAsync(id);
            if (appointment is null) return NotFound();

            appointment.IsCanceled = true;

            appDbContext.Appointments.Update(appointment);
            await Commit();
            return Success();
        }

        public async Task<List<Appointment>> GetAll() => await appDbContext
            .Appointments.AsNoTracking()
            .Include(d => d.Doctor)
            .Include(d => d.Patient)
            .ToListAsync();

        public async Task<List<Appointment>> GetAllByUser(string token)
        {
            var tokenInfo = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.Token.Equals(token));
            if (tokenInfo is null) return new List<Appointment>();

            var userRole = await appDbContext.UserRoles.FirstOrDefaultAsync(_ => _.UserId == tokenInfo!.UserId);
            var systemRole = await appDbContext.SystemRoles.FindAsync(userRole!.RoleId);

            bool isDoctor = systemRole!.Name!.Equals(Constants.Doctor);

            return await appDbContext
                .Appointments.AsNoTracking()
                .Where(_ => (isDoctor && _.DoctorId == tokenInfo!.UserId) || _.PatientId == tokenInfo!.UserId)
                .Include(d => d.Doctor)
                .Include(d => d.Patient)
                .ToListAsync();
        }

        public async Task<Appointment> GetById(int id) => await appDbContext.Appointments.FindAsync(id) ?? new Appointment();
        
        public async Task<GeneralResponse> Insert(Appointment item)
        {
            if (!await CheckAppoitmentTimeConflict(item)) return new(false, "Appointment time conflict!");
            appDbContext.Appointments.Add(item);
            await Commit();
            return Success();
        }

        private async Task<bool> CheckAppoitmentTimeConflict(Appointment item) // Return false if it able to register
            => await appDbContext.Appointments.AnyAsync(_ => _.DoctorId == item.DoctorId 
                && _.AppointmentDate == item.AppointmentDate
                && (item.AppointmentStartTime < _.AppointmentEndTime || item.AppointmentEndTime > _.AppointmentStartTime)
            );

        public async Task<GeneralResponse> Update(Appointment item)
        {
            var appointment = await appDbContext.Appointments.FindAsync(item.Id);
            if (appointment is null) return NotFound();

            appDbContext.Appointments.Update(item);

            await Commit();
            return Success();
        }

        private static GeneralResponse NotFound() => new(false, "Sorry, Appointment not found");
        private static GeneralResponse Success() => new(true, "Process completed");
        private async Task Commit() => await appDbContext.SaveChangesAsync();
    }
}
