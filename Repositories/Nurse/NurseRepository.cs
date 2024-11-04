using CareConnect.Models;
using Connect.Data;
using Connect.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using CareConnect.Models;

namespace Connect.Repositories.Nurse
{
    public class NurseRepository : INurseRepository
    {

        private readonly CareConnectDbContext _context;
        private readonly IDbConnection _connection;

        public NurseRepository(CareConnectDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        public async Task<Patient> GetPatientByFileIdAsync(int patientFileId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@PatientFileId", patientFileId);

                var result = await _connection.QueryFirstOrDefaultAsync<Patient>(
                    "sp_GetPatientByFileIdNurse",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving patient: {ex.Message}", ex);
            }
        }

        public async Task RecordVitalsAsync(RecordVitalsViewModel vitals)
        {
            try
            {
                await _connection.ExecuteAsync("sp_RecordPatientVitals", new
                {
                    vitals.PatientId,
                    vitals.BloodPressure,
                    vitals.Temperature,
                    vitals.SugarLevel,
                    vitals.RecordedAt
                }, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error recording vitals: {ex.Message}", ex);
            }
        }

        // Treat Patient
        public async Task TreatPatientAsync(TreatPatientViewModel treatment)
        {
            try
            {
                await _connection.ExecuteAsync("usp_TreatPatient", new
                {
                    treatment.PatientFileId,
                    treatment.TreatmentType,
                    treatment.Notes,
                    treatment.TreatedAt
                }, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error treating patient: {ex.Message}", ex);
            }
        }

        // Administer Medication
        public async Task AdministerMedicationAsync(MedicationDispenseViewModel medication)
        {
            try
            {
                await _connection.ExecuteAsync("usp_AdministerMedication", new
                {
                    medication.PatientFileId,
                    medication.MedicationName,
                    medication.Dosage,
                    medication.AdministeredAt
                }, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error administering medication: {ex.Message}", ex);
            }
        }

        // Fetch patients assigned to the nurse
        public async Task<IEnumerable<PatientFile>> GetPatientsAsync()
        {
            try
            {
                var patients = await _connection.QueryAsync<PatientFile>("usp_GetPatientsAssignedToNurse", commandType: CommandType.StoredProcedure);
                return patients;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching patients: {ex.Message}", ex);
            }
        }

       
        public async Task<PatientFile> GetPatientByIdAsync(int patientFileId)
        {
            try
            {
                var patientFile = await _connection.QueryFirstOrDefaultAsync<PatientFile>("usp_GetPatientById", new {patientFileId }, commandType: CommandType.StoredProcedure);
                return patientFile;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching patient: {ex.Message}", ex);
            }
        }

        // View Doctor Instructions
        public async Task<IEnumerable<DoctorInstructionViewModel>> GetDoctorInstructionsAsync(int patientFileId)
        {
            try
            {
                var instructions = await _connection.QueryAsync<DoctorInstructionViewModel>(
                    "usp_GetDoctorInstructions",
                    new { patientFileId = patientFileId },
                    commandType: CommandType.StoredProcedure
                );

                return instructions;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving doctor instructions: {ex.Message}", ex);
            }
        }
    }
}
