using CareConnect.Models;
using Connect.Models;
using Microsoft.AspNetCore.Mvc;

namespace Connect.Repositories.Nurse
{
    public interface INurseRepository
    {
        Task RecordVitalsAsync(RecordVitalsViewModel vitals);
        Task TreatPatientAsync(TreatPatientViewModel treatment);
        Task AdministerMedicationAsync(MedicationDispenseViewModel medication);
        Task<IEnumerable<DoctorInstructionViewModel>> GetDoctorInstructionsAsync(int patientFileId);
        Task<IEnumerable<PatientFile>> GetPatientsAsync();
        Task<Patient> GetPatientByFileIdAsync(int patientFileId);
        Task<PatientFile> GetPatientByIdAsync(int patientFileId);

    }
}
