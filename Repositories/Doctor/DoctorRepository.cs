using CareConnect.Models;
using Connect.Data;
using Connect.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Data;
using System.Data.Common;

namespace Connect.Repositories.Doctor
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<DoctorRepository> _logger;

        public DoctorRepository(IDbConnection connection, ILogger<DoctorRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<DoctorDashboardViewModel> GetDashboardAsync(int? doctorUserId)
        {
            try
            {
                if (!doctorUserId.HasValue)
                {
                    _logger.LogWarning("GetDashboardAsync called with null doctorUserId");
                    return new DoctorDashboardViewModel();
                }

                var parameters = new DynamicParameters();
                parameters.Add("@DoctorUserId", doctorUserId.Value);

                using var multi = await _connection.QueryMultipleAsync(
                    "sp_GetDoctorDashboardData",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return new DoctorDashboardViewModel
                {
                    PatientsToSeeToday = await multi.ReadFirstOrDefaultAsync<int>(),
                    PendingTasks = await multi.ReadFirstOrDefaultAsync<int>(),
                    SucessTasks = await multi.ReadFirstOrDefaultAsync<int>(),
                    PrescriptionsToday = await multi.ReadFirstOrDefaultAsync<int>(),
                    AppointmentsToday = await multi.ReadFirstOrDefaultAsync<int>(),
                    UpcomingAppointments = (await multi.ReadAsync<AppointmentViewModel>()).ToList(),
                    PendingInformation = (await multi.ReadAsync<PendingTaskViewModel>()).ToList(),
                    TotalPatients = await multi.ReadFirstOrDefaultAsync<int>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDashboardAsync for doctorUserId: {DoctorUserId}", doctorUserId);
                throw;
            }
        }
        public async Task<IEnumerable<DoctorPatientViewModel>> GetDoctorPatientsAsync(string searchTerm, int? doctorUserId)
        {
            return await _connection.QueryAsync<DoctorPatientViewModel>(
                "sp_GetDoctorPatients",
                new { DoctorUserId = doctorUserId, SearchTerm = searchTerm },
                commandType: CommandType.StoredProcedure
            );
        }


        public async Task<IEnumerable<DoctorPatientViewModel>> GetAllPatientsAsync(string searchTerm, int? doctorUserId)
        {
            return await _connection.QueryAsync<DoctorPatientViewModel>("sp_GetAllPatientsDoctor",
                new { DoctorUserId = doctorUserId, SearchTerm = searchTerm }, commandType: CommandType.StoredProcedure);

        }
        public async Task<DoctorPatientViewModel> GetByIdAsync(int id)
        {
            return await _connection.QueryFirstOrDefaultAsync<DoctorPatientViewModel>("sp_GetPatientbyID", new { UserId = id }, commandType: CommandType.StoredProcedure);
        }
        // Schedules
        public async Task<bool> GetAppointmentByDateTime(DateTime date, DateTime time, int? excludeAppointmentId = null)
        {
            const string sql = @"
                        SELECT 1
                        FROM Appointments
                        WHERE CAST(Date AS DATE) = CAST(@Date AS DATE)
                          AND CAST(Time AS TIME) = CAST(@Time AS TIME)
                          AND IsDeleted = 0
                          AND (@ExcludeAppointmentId IS NULL OR AppointmentId != @ExcludeAppointmentId)";

            var result = await _connection.QueryFirstOrDefaultAsync<int?>(sql,
                new
                {
                    Date = date.Date, // Ensure we're only comparing the date part
                    Time = time.TimeOfDay, // Ensure we're only comparing the time part
                    ExcludeAppointmentId = excludeAppointmentId
                });

            return result.HasValue;
        }
        public async Task<AppointmentViewModel> GetSchedulePatientFileDetails(int PatientFileid)
        {
            return await _connection.QueryFirstOrDefaultAsync<AppointmentViewModel>("sp_GetAppointmentPatientFileDetails", new { PatientFileId = PatientFileid }, commandType: CommandType.StoredProcedure);
        }
        //public async Task<IEnumerable<AppointmentViewModel>> GetAllScheduleAsync(int doctorUserId)
        //{
        //    var parameters = new DynamicParameters();
        //    parameters.Add("@DoctorUserId", doctorUserId);

        //    return await _connection.QueryAsync<AppointmentViewModel>("sp_GetAllSchedule", parameters, commandType: CommandType.StoredProcedure);
        //}
        public async Task<IEnumerable<AppointmentViewModel>> GetAllScheduleAsync(string searchTerm, int? doctorUserId)
        {
            try { 
            return await _connection.QueryAsync<AppointmentViewModel>("sp_GetAllSchedule",
                new { DoctorUserId = doctorUserId, SearchTerm = searchTerm }, commandType: CommandType.StoredProcedure);
             }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllScheduleAsync");
                return new List<AppointmentViewModel>();
            }
        }
        public async Task<bool> UpdateScheduleStatusAsync(int appointmentId, string status)
        {
            var affectedRows = await _connection.ExecuteAsync("sp_UpdateScheduleStatus",
                new { AppointmentId = appointmentId, Status = status }, commandType: CommandType.StoredProcedure);

            return affectedRows > 0;  // Return true if the update was successful
        }

        public async Task<AppointmentViewModel> GetScheduleByIdAsync(int id)
        {
            return await _connection.QueryFirstOrDefaultAsync<AppointmentViewModel>("sp_GetScheduleById", new { ScheduleId = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> AddScheduleAsync(AppointmentViewModel appointment)
        {
            try
            {
                return await _connection.ExecuteScalarAsync<int>("sp_AddSchedule",
                    new
                    {
                        appointment.PatientFileId,
                        appointment.Date,
                        appointment.Time,
                        appointment.Status,
                        appointment.Reason,
                        appointment.UserId


                    },
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding schedule: {ex.Message}");
                throw;
            }
        }
        public async Task UpdateScheduleAsync(AppointmentViewModel appointment)
        {
            await _connection.ExecuteAsync("sp_UpdateSchedule",
                new
                {
                    appointment.AppointmentId,
                    appointment.PatientFileId,
                    appointment.Date,
                    appointment.Time,
                    appointment.Status,
                    appointment.Reason
                },
                commandType: CommandType.StoredProcedure);
        }
        public async Task<AppointmentViewModel> GetPatientFilee(int patientFileId)
        {
            return await _connection.QueryFirstOrDefaultAsync<AppointmentViewModel>(
               "sp_GetPatientFileDetailsforAssigning", new { PatientFileId = patientFileId },
               commandType: CommandType.StoredProcedure);
        }
        public async Task<AppointmentViewModel> GetPatientFiledetails(int patientFileId)//For Editing the PatientFile
        {
            return await _connection.QueryFirstOrDefaultAsync<AppointmentViewModel>(
               "sp_GetAppointmentPatientFileDetails", new { PatientFileId = patientFileId },
               commandType: CommandType.StoredProcedure);
        }
        public async Task SoftDeleteScheduleAsync(int id)
        {
            await _connection.ExecuteAsync("sp_SoftDeleteSchedule",
                new { ScheduleId = id },
                commandType: CommandType.StoredProcedure);
        }
        public async Task<List<AppointmentViewModel>> GetAppointmentByDateAsync(DateTime date)
        {
            return (await _connection.QueryAsync<AppointmentViewModel>("sp_GetScheduleByDate", new { Date = date }, commandType: CommandType.StoredProcedure)).ToList();
        }
        // Instructions
        public async Task<IEnumerable<InstructionViewModel>> GetAllInstructionAsync(int? doctorUserId)
        {
            return await _connection.QueryAsync<InstructionViewModel>(
                "sp_GetAllInstructions", new { DoctorUserId = doctorUserId },
                commandType: CommandType.StoredProcedure);
        }
        public async Task<InstructionViewModel> GetSchedulePatientFileDetailsAssign(int PatientFileId)
        {
            return await _connection.QueryFirstOrDefaultAsync<InstructionViewModel>(
             "GetInstructionPatientFileDetailsforAssigning", new { PatientFileId = PatientFileId },
             commandType: CommandType.StoredProcedure);
        }
        public async Task<InstructionViewModel> GetPatientFileDetail(int patientFileId)
        {
            return await _connection.QueryFirstOrDefaultAsync<InstructionViewModel>(
               "sp_GetInstructionPatientFileDetails", new { PatientFileId = patientFileId },
               commandType: CommandType.StoredProcedure);
        }
        public async Task<InstructionViewModel> GetInstructionByIdAsync(int id)
        {
            return await _connection.QueryFirstOrDefaultAsync<InstructionViewModel>(
                "sp_GetInstructionById",
                new { InstructionId = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> AddInstructionAsync(InstructionViewModel instruction)
        {
            try
            {
                // Get the doctor's UserId
                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                    "GetDoctorName",
                    commandType: CommandType.StoredProcedure
                );

                if (result == null || result.UserId == 0)
                {
                    throw new Exception("Doctor not found.");
                }

                // Add the instruction
                return await _connection.ExecuteScalarAsync<int>(
                    "sp_AddInstruction",
                    new
                    {
                        instruction.PatientFileId,
                        instruction.Date,
                        instruction.Details,
                        instruction.Importance,
                        UserId = result.UserId
                    },
                    commandType: CommandType.StoredProcedure
                );
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error adding instruction: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<InstructionViewModel>> GetAllDoctorsNameAsync()
        {
            try
            {
                return await _connection.QueryAsync<InstructionViewModel>(
                    "GetDoctorName",
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching doctors: {ex.Message}");
                throw;
            }
        }
        public async Task UpdateInstructionAsync(InstructionViewModel instruction)
        {
            await _connection.ExecuteAsync(
                "sp_UpdateInstruction",
                new
                {
                    instruction.InstructionId,
                    instruction.PatientFileId,
                    instruction.Date,
                    instruction.Details,
                    instruction.Importance
                },
                commandType: CommandType.StoredProcedure);
        }

        //public async Task SoftDeleteInstructionAsync(int id)
        //{
        //    await _connection.ExecuteAsync(
        //        "sp_SoftDeleteInstruction",
        //        new { InstructionId = id },
        //        commandType: CommandType.StoredProcedure);
        //}
        public async Task SoftDeleteInstructionAsync(int id)
        {
            try
            {
                var result = await _connection.ExecuteAsync(
                    "sp_SoftDeleteInstruction",
                    new { InstructionId = id },
                    commandType: CommandType.StoredProcedure);

                if (result == 0)
                {
                    throw new Exception($"No instruction found with ID {id}");
                }
            }
            catch (Exception ex)
            {
                // Log the error
                throw;
            }
        }
        // Prescriptions

        public async Task<PrescriptionViewModel> GetPrescriptionByIdAsync(int id)
        {
            return await _connection.QueryFirstOrDefaultAsync<PrescriptionViewModel>("sp_GetPrescriptionById", new { PrescriptionId = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> AddPrescriptionAsync(PrescriptionViewModel prescription)
        {
            return await _connection.ExecuteScalarAsync<int>("sp_AddPrescription",
                new
                {

                    prescription.Dosage,
                    prescription.Date,
                    prescription.PatientFileId,
                    prescription.Medication,
                    prescription.UserId
                    //   prescription.Name

                },
                commandType: CommandType.StoredProcedure);
        }
        public async Task<PrescriptionViewModel> GetPatientFileDetailAssign(int patientFileId)
        {
            return await _connection.QueryFirstOrDefaultAsync<PrescriptionViewModel>(
               "sp_GetPatientFileDetailsAssign", new { PatientFileId = patientFileId },
               commandType: CommandType.StoredProcedure);
        }
        public async Task<PrescriptionViewModel> GetPatientFileDetails(int patientFileId)
        {
            return await _connection.QueryFirstOrDefaultAsync<PrescriptionViewModel>(
                "sp_GetPatientFileDetails", new { PatientFileId = patientFileId },
                commandType: CommandType.StoredProcedure);
        }
        public async Task<IEnumerable<PrescriptionViewModel>> GetAllPrescriptionsAsync(int? doctorUserId)
        {
            return await _connection.QueryAsync<PrescriptionViewModel>(
                "sp_GetAllPrescription", new { DoctorUserId = doctorUserId },
                commandType: CommandType.StoredProcedure);
        }
        public async Task UpdatePrescriptionAsync(PrescriptionViewModel prescription)
        {
            await _connection.ExecuteAsync("sp_UpdatePrescription",
                new
                {
                    prescription.Name,
                    prescription.Medication,
                    prescription.Date,
                    prescription.Dosage,
                    prescription.PatientFileId,

                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task SoftDeletePrescriptionAsync(int id)
        {
            await _connection.ExecuteAsync("sp_SoftDeletePrescription",
                new { PrescriptionId = id },
                commandType: CommandType.StoredProcedure);
        }
        public async Task<IEnumerable<Medication>> GetAllMedicationsAsync()
        {
            var query = "SELECT  Name FROM Medication";
            return await _connection.QueryAsync<Medication>(query);
        }

        public async Task<IEnumerable<AppointmentViewModel>> GetAllScheduleStatus()
        {
            var query = "SELECT Status FROM Appointments";
            return await _connection.QueryAsync<AppointmentViewModel>(query);
        }
        public async Task UpdateAppointmentStatusAsync(int patientFileId, string status)
        {
            var query = "UPDATE Appointments SET Status = @Status WHERE AppointmentId = @AppointmentId";

            await _connection.ExecuteAsync(query, new { Status = status, AppointmentId = patientFileId });
        }
        public async Task<IEnumerable<PatientReportViewModel>> GetPatientPrescriptionDetailsAsync(int doctorUserId, string searchTerm = null)
        {
            var parameters = new
            {
                DoctorUserId = doctorUserId,
                SearchTerm = searchTerm
            };

            return await _connection.QueryAsync<PatientReportViewModel>(
                "sp_GetPatientREPORTDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}

    
    
