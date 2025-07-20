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
        private static bool IsOpen(Appointment appointment)
        {
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var currentHour = DateTime.Now.TimeOfDay;

            return (!appointment.IsCanceled
                && appointment.AppointmentDate > currentDate
                || (appointment.AppointmentDate == currentDate
                    && appointment.AppointmentStartTime >= currentHour)
            );
        }

        public async Task<GeneralResponse> DeleteByID(int id)
        {
            var appointment = await appDbContext.Appointments.FindAsync(id);
            if (appointment is null) return NotFound();

            if(!IsOpen(appointment)) return new(false, "Não foi possível realizar o cancelamento!");

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
                    .Where(appointment => !appointment.IsCanceled
                        && appointment.AppointmentDate > currentDate
                        || (appointment.AppointmentDate == currentDate
                            && appointment.AppointmentStartTime >= currentHour)
                        && appointment.PatientId == null)
                    .Include(d => d.Doctor)
                    .Include(p => p.Patient)
                    .Select(appointment => new Appointment
                    {
                        Id = appointment.Id,
                        AppointmentDate = appointment.AppointmentDate,
                        AppointmentStartTime = appointment.AppointmentStartTime,
                        AppointmentEndTime = appointment.AppointmentEndTime,
                        IsCanceled = appointment.IsCanceled,
                        DoctorId = appointment.DoctorId,
                        Doctor = new ApplicationUser { Id = appointment.Doctor!.Id, Fullname = appointment.Doctor!.Fullname },
                        PatientId = appointment.PatientId,
                        Patient = appointment.Patient != null
                            ? new ApplicationUser { Id = appointment.Patient!.Id, Fullname = appointment.Patient!.Fullname }
                            : null,
                    })
                    .OrderByDescending(appointment => appointment.AppointmentDate)
                    .ThenByDescending(appointment => appointment.AppointmentStartTime)
                    .ToListAsync();
        }

        public async Task<List<Appointment>> GetAllByUser(string token)
        {
            var tokenInfo = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.Token.Equals(token));
            if (tokenInfo is null) return new List<Appointment>();

            return await appDbContext
                .Appointments.AsNoTracking()
                .Where(appointment => appointment.DoctorId == tokenInfo!.UserId || appointment.PatientId == tokenInfo!.UserId)
                .Include(d => d.Doctor)
                .Include(p => p.Patient)
                .Select(appointment => new Appointment
                {
                    Id = appointment.Id,
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentStartTime = appointment.AppointmentStartTime,
                    AppointmentEndTime = appointment.AppointmentEndTime,
                    IsCanceled = appointment.IsCanceled,
                    DoctorId = appointment.DoctorId,
                    Doctor = new ApplicationUser { Id = appointment.Doctor!.Id, Fullname = appointment.Doctor!.Fullname },
                    PatientId = appointment.PatientId,
                    Patient = appointment.Patient != null 
                        ? new ApplicationUser { Id = appointment.Patient!.Id, Fullname = appointment.Patient!.Fullname }
                        : null,
                })
                .OrderByDescending(appointment => appointment.AppointmentDate)
                .ThenByDescending(appointment => appointment.AppointmentStartTime)
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
            => await appDbContext.Appointments.AnyAsync(appointment => appointment.DoctorId == item.DoctorId
                    && appointment.Id != item.Id
                    && appointment.AppointmentDate == item.AppointmentDate
                    && !appointment.IsCanceled
                    && ((appointment.AppointmentStartTime < item.AppointmentStartTime && item.AppointmentStartTime < appointment.AppointmentEndTime)
                    || (appointment.AppointmentStartTime < item.AppointmentEndTime && item.AppointmentEndTime < appointment.AppointmentEndTime))
                );

        public async Task<GeneralResponse> Update(Appointment item)
        {
            if (await CheckAppoitmentTimeConflict(item)) return new(false, "O horário da consulta conflita com o de outra já exstente!");

            if(!IsOpen(item)) return new(false, "Apenas consultas em aberto ou agendadas podem ser editadas!");

            appDbContext.Appointments.Update(item);

            await Commit();
            return Success();
        }

        public async Task<GeneralResponse> Schedule(Appointment item)
        {
            var appointment = await appDbContext.Appointments.FindAsync(item.Id);
            if (appointment is null) return NotFound();

            if (appointment.PatientId != null) return new(false, "A consulta já foi agendada para outra pessoa!");

            if (!IsOpen(item)) return new(false, "Apenas consultas em aberto ou agendadas podem ser agendadas!");

            appointment.PatientId = item.PatientId;

            appDbContext.Appointments.Update(appointment);

            await Commit();
            return Success();
        }

        public async Task<GeneralResponse> CancelSchedule(int id)
        {
            var appointment = await appDbContext.Appointments.FindAsync(id);
            if (appointment is null) return NotFound();

            if (!IsOpen(appointment)) return new(false, "Não foi possível realizar o cancelamento!");

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
