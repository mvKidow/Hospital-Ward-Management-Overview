using CareConnect.Models;
using Connect.Models;
using System.Collections.Generic;

namespace Connect.Repositories.Doctor
{
    public interface IDoctorRepository
    {
        // Dashboard
        Task<DoctorDashboardViewModel> GetDashboardAsync(int? doctorUserId = null);

        // Patients
        Task<IEnumerable<DoctorPatientViewModel>> GetDoctorPatientsAsync(string searchTerm, int? doctorUserId = null);
        //Task<IEnumerable<PatientListViewModelDoctor>> GetAllPatientsAsync(string searchTerm, int? doctorUserId);
        Task<IEnumerable<DoctorPatientViewModel>> GetAllPatientsAsync(string searchTerm, int? doctorUserId);
        //Task<PatientListViewModelDoctor> GetByIdAsync(int id);
        Task<DoctorPatientViewModel> GetByIdAsync(int id);
        // Schedules
        Task<IEnumerable<AppointmentViewModel>> GetAllScheduleAsync(string searchTerm, int? doctorUserId);
        Task<bool> UpdateScheduleStatusAsync(int appointmentId, string status);
        Task<AppointmentViewModel> GetScheduleByIdAsync(int id);
        Task<AppointmentViewModel> GetSchedulePatientFileDetails(int PatientFileid);
        Task<int> AddScheduleAsync(AppointmentViewModel appointment);
        Task UpdateScheduleAsync(AppointmentViewModel appointment);
        Task SoftDeleteScheduleAsync(int id);  // Updated method for soft delete
        Task<List<AppointmentViewModel>> GetAppointmentByDateAsync(DateTime date);

        // Instructions
        Task<IEnumerable<InstructionViewModel>> GetAllInstructionAsync(int? doctorUserId);
        Task<InstructionViewModel> GetInstructionByIdAsync(int id);
        Task<int> AddInstructionAsync(InstructionViewModel instruction);
        Task UpdateInstructionAsync(InstructionViewModel instruction);
        Task SoftDeleteInstructionAsync(int id);

        // Prescriptions
        Task<IEnumerable<PrescriptionViewModel>> GetAllPrescriptionsAsync(int? doctorUserId);
        Task<PrescriptionViewModel> GetPrescriptionByIdAsync(int id);
        Task<int> AddPrescriptionAsync(PrescriptionViewModel prescription);

        Task UpdatePrescriptionAsync(PrescriptionViewModel prescription);
        Task SoftDeletePrescriptionAsync(int id);  // Updated method for soft delete
        Task<PrescriptionViewModel> GetPatientFileDetails(int patientFileId);
        Task<PrescriptionViewModel> GetPatientFileDetailAssign(int patientFileId);
        Task<InstructionViewModel> GetPatientFileDetail(int patientFileId);
        Task<InstructionViewModel> GetSchedulePatientFileDetailsAssign(int PatientFileId);
        Task<AppointmentViewModel> GetPatientFilee(int patientFileId);
        Task<AppointmentViewModel> GetPatientFiledetails(int patientFileId);
        Task<IEnumerable<Medication>> GetAllMedicationsAsync();
        Task<IEnumerable<AppointmentViewModel>> GetAllScheduleStatus();
        Task UpdateAppointmentStatusAsync(int patientFileId, string status);
        Task<IEnumerable<InstructionViewModel>> GetAllDoctorsNameAsync();
        Task<bool> GetAppointmentByDateTime(DateTime date, DateTime time, int? excludeAppointmentId = null);
    }

}
