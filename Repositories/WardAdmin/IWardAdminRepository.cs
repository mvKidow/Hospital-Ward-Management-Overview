using CareConnect.Models;
using Connect.Models;

namespace Connect.Repositories.WardAdmin
{
    public interface IWardAdminRepository
    {

   
        Task<WardDashboardData> GetWardDashboardDataAsync(int wardId);

        Task<IEnumerable<WardDoctorViewModel>> GetAvailableDoctorsForWardAsync(int wardId);
        Task AssignDoctorToPatientFileAsync(int patientFileId, int userId);
        Task<PatientFileDoctorAssignmentDetailsViewModel> GetPatientFileDetailsAsync(int patientFileId);
        Task<User> SearchPatientAsync(int userId);
        Task<PatientDetailsViewModel> GetPatientDetailsAsync(int userId);
        Task<bool> AssignRoomAndBedAsync(AssignRoomAndBedRequest request);
        Task<IEnumerable<PatientListViewModel>> GetAllPatientsAsync(string searchPatient, int? wardId = null);
        Task<IEnumerable<PatientFileViewModel>> GetPatientsByRoomAsync(int roomId);
        Task<PatientFileViewModel> GetPatientByFileIdAsync(int patientFileId);
        Task<PatientUpdateResult> UpdatePatientMovementStatusAsync(int patientFileId, string status);
        Task<PatientDischargeViewModel> GetPatientDischargeDetailsAsync(int userId);
        Task<bool> DischargePatientAsync(int userId, DateTime? dischargeDate, decimal total);
        Task<User> GetUserByIdAsync(int userId);
        Task<PatientFile> GetPatientFileByUserIdAsync(int userId);
        Task AdmitPatientAsync(Patient patient);
        IEnumerable<Condidtion> GetConditions();
        IEnumerable<Medication> GetMedications();
        IEnumerable<Allergy> GetAllergies();
        Task<IEnumerable<RoomViewModel>> GetRoomsByWardIdAsync(int wardId);
        Task<IEnumerable<BedViewModel>> GetAvailableBedsByRoomIdAsync(int roomId);
        Task<IEnumerable<WardViewModel>> GetWardsAsync();
        Task<PatientDischargeViewModel> SearchPatientForDischargeAsync(int patientFileId);
        Task<int> CreateDischargeReportAsync(DischargeReport dischargeReport);

        Task<DischargeReport> GetDischargeReportAsync(int reportId);
        Task<DischargeReportViewModel> GetDischargeReportDetailsAsync(int patientFileId);
        Task<EditPatientViewModel> GetPatientByPatientFileIdAsync(int patientFileId);
        Task<IEnumerable<ConditionViewModel>> GetAllConditionsAsync();


    }

}

