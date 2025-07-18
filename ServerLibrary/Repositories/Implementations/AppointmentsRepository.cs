using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class AppointmentsRepository(AppDbContext appDbContext) : IAppointment
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

        public async Task<List<Appointment>> GetAllOpenned()
        {
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var currentHour = DateTime.Now.TimeOfDay;

            return await appDbContext
                .Appointments.AsNoTracking()
                .Where(_ => !_.IsCanceled 
                    && (_.AppointmentDate > currentDate || (_.AppointmentDate == currentDate 
                        && _.AppointmentStartTime >= currentHour))
                    && _.PatientId == null)
                .Include(d => d.Doctor)
                .Include(p => p.Patient)
                .Select(_ => new Appointment
                {
                    Id = _.Id,
                    AppointmentDate = _.AppointmentDate,
                    AppointmentStartTime = _.AppointmentStartTime,
                    AppointmentEndTime = _.AppointmentEndTime,
                    IsCanceled = _.IsCanceled,
                    DoctorId = _.DoctorId,
                    Doctor = new ApplicationUser { Id = _.Doctor!.Id, Fullname = _.Doctor!.Fullname },
                    PatientId = _.PatientId,
                    Patient = _.Patient != null
                        ? new ApplicationUser { Id = _.Patient!.Id, Fullname = _.Patient!.Fullname }
                        : null,
                })
                .OrderByDescending(_ => _.AppointmentDate)
                .ThenByDescending(_ => _.AppointmentStartTime)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAllByUser(string token)
        {
            var tokenInfo = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.Token.Equals(token));
            if (tokenInfo is null) return new List<Appointment>();

            return await appDbContext
                .Appointments.AsNoTracking()
                .Where(_ => _.DoctorId == tokenInfo!.UserId || _.PatientId == tokenInfo!.UserId)
                .Include(d => d.Doctor)
                .Include(p => p.Patient)
                .Select(_ => new Appointment
                {
                    Id = _.Id,
                    AppointmentDate = _.AppointmentDate,
                    AppointmentStartTime = _.AppointmentStartTime,
                    AppointmentEndTime = _.AppointmentEndTime,
                    IsCanceled = _.IsCanceled,
                    DoctorId = _.DoctorId,
                    Doctor = new ApplicationUser { Id = _.Doctor!.Id, Fullname = _.Doctor!.Fullname },
                    PatientId = _.PatientId,
                    Patient = _.Patient != null 
                        ? new ApplicationUser { Id = _.Patient!.Id, Fullname = _.Patient!.Fullname }
                        : null,
                })
                .OrderByDescending(_ => _.AppointmentDate)
                .ThenByDescending(_ => _.AppointmentStartTime)
                .ToListAsync();
        }
        
        public async Task<GeneralResponse> Insert(Appointment item)
        {
            if (await CheckAppoitmentTimeConflict(item)) return new(false, "O horário da consulta conflita com o de outra já exstente!");
            appDbContext.Appointments.Add(item);
            await Commit();
            return Success();
        }

        private async Task<bool> CheckAppoitmentTimeConflict(Appointment item) // Return false if it able to register
            => await appDbContext.Appointments.AnyAsync(_ => _.DoctorId == item.DoctorId
                    && _.Id != item.Id
                    && _.AppointmentDate == item.AppointmentDate
                    && !_.IsCanceled
                    && ((_.AppointmentStartTime < item.AppointmentStartTime && item.AppointmentStartTime < _.AppointmentEndTime)
                    || (_.AppointmentStartTime < item.AppointmentEndTime && item.AppointmentEndTime < _.AppointmentEndTime))
                );

        public async Task<GeneralResponse> Update(Appointment item)
        {
            if (await CheckAppoitmentTimeConflict(item)) return new(false, "O horário da consulta conflita com o de outra já exstente!");
            appDbContext.Appointments.Update(item);

            await Commit();
            return Success();
        }

        public async Task<GeneralResponse> Schedule(Appointment item)
        {
            var appointment = await appDbContext.Appointments.FindAsync(item.Id);
            if (appointment is null) return NotFound();

            if (appointment.PatientId != null) return new(false, "A consulta já foi agendada para outra pessoa!");

            appointment.PatientId = item.PatientId;

            appDbContext.Appointments.Update(appointment);

            await Commit();
            return Success();
        }

        public async Task<GeneralResponse> CancelSchedule(int id)
        {
            var appointment = await appDbContext.Appointments.FindAsync(id);
            if (appointment is null) return NotFound();

            appointment.PatientId = null;

            appDbContext.Appointments.Update(appointment);

            await Commit();
            return Success();
        }

        private static GeneralResponse NotFound() => new(false, "Desculpe, consulta não encontrada");
        private static GeneralResponse Success() => new(true, "Processo concluído!");
        private async Task Commit() => await appDbContext.SaveChangesAsync();
    }
}
