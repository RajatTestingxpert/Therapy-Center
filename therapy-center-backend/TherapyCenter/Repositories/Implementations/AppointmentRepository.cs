using TherapyCenter.Data;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TherapyCenter.Repositories.Implementations
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;

        public AppointmentRepository(AppDbContext context) => _context = context;

        public async Task<Appointment?> GetByIdAsync(int appointmentId)
            => await _context.Appointments
                             .Include(a => a.Patient)
                             .Include(a => a.Doctor).ThenInclude(d => d.User)
                             .Include(a => a.Therapy)
                             .Include(a => a.Finding)
                             .Include(a => a.Payment)
                             .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId)
            => await _context.Appointments
                             .Include(a => a.Doctor).ThenInclude(d => d.User)
                             .Include(a => a.Therapy)
                             .Include(a => a.Finding)
                             .Where(a => a.PatientId == patientId)
                             .OrderByDescending(a => a.AppointmentDate)
                             .ToListAsync();

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId)
            => await _context.Appointments
                             .Include(a => a.Patient)
                             .Include(a => a.Therapy)
                             .Where(a => a.DoctorId == doctorId)
                             .OrderBy(a => a.AppointmentDate)
                             .ThenBy(a => a.StartTime)
                             .ToListAsync();

        public async Task<IEnumerable<Appointment>> GetByDateAsync(DateOnly date)
            => await _context.Appointments
                             .Include(a => a.Patient)
                             .Include(a => a.Doctor).ThenInclude(d => d.User)
                             .Include(a => a.Therapy)
                             .Where(a => a.AppointmentDate == date)
                             .ToListAsync();

        public async Task<Appointment> CreateAsync(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<Appointment> UpdateAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }
    }
}