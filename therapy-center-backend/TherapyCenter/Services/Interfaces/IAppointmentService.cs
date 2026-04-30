using TherapyCenter.DTO_s.Appointment;
using AppointmentEntity = TherapyCenter.Entities.Appointment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TherapyCenter.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentEntity> BookAsync(BookAppointmentRequest request);
        Task<AppointmentEntity> UpdateStatusAsync(int appointmentId, UpdateAppointmentStatusRequest request);
        Task<IEnumerable<AppointmentEntity>> GetByPatientAsync(int patientId);
        Task<IEnumerable<AppointmentEntity>> GetByDoctorAsync(int doctorId);
        Task<IEnumerable<AppointmentEntity>> GetByDateAsync(DateOnly date);
        Task<AppointmentEntity?> GetByIdAsync(int appointmentId);
    }
}