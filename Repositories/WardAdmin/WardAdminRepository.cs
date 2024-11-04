using CareConnect.Models;
using Connect.Data;
using Connect.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Connect.Repositories.WardAdmin
{
    public class WardAdminRepository : IWardAdminRepository
    {
        private readonly CareConnectDbContext _context;
        private readonly IDbConnection _connection;
        private readonly ILogger<AdminRepository> _logger;

        public WardAdminRepository(CareConnectDbContext context, IConfiguration configuration, ILogger<AdminRepository> logger)
        {
            _context = context;
            _connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            _logger = logger;
        }

        public async Task<WardDashboardData> GetWardDashboardDataAsync(int wardId)
        {
            try
            {
                _logger.LogInformation($"Fetching ward dashboard data for wardId: {wardId}");

                using var multi = await _connection.QueryMultipleAsync("sp_GetWardDashboardData",
                    new { WardId = wardId },
                    commandType: CommandType.StoredProcedure);

                var bedData = await multi.ReadFirstOrDefaultAsync<(int TotalBeds, int AvailableBeds)>();
                var roomData = await multi.ReadFirstOrDefaultAsync<(int TotalRooms, int AvailableRooms)>();
                var totalDoctors = await multi.ReadFirstOrDefaultAsync<int>();
                var totalPatientFiles = await multi.ReadFirstOrDefaultAsync<int>();
                var dailyAdmissions = (await multi.ReadAsync<DailyAdmission>()).ToList();
                var bedOccupancyRates = (await multi.ReadAsync<BedOccupancyRate>()).ToList();

                var dashboardData = new WardDashboardData
                {
                    TotalBeds = bedData.TotalBeds,
                    AvailableBeds = bedData.AvailableBeds,
                    TotalRooms = roomData.TotalRooms,
                    AvailableRooms = roomData.AvailableRooms,
                    TotalDoctors = totalDoctors,
                    TotalPatientFiles = totalPatientFiles,
                    DailyAdmissions = dailyAdmissions,
                    BedOccupancyRates = bedOccupancyRates
                };

                _logger.LogInformation($"Dashboard data retrieved successfully: " +
                    $"TotalBeds={dashboardData.TotalBeds}, " +
                    $"AvailableBeds={dashboardData.AvailableBeds}, " +
                    $"TotalDoctors={dashboardData.TotalDoctors}, " +
                    $"TotalPatientFiles={dashboardData.TotalPatientFiles}");

                return dashboardData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching ward dashboard data for wardId: {wardId}");
                throw;
            }
        }



        public async Task AdmitPatientAsync(Patient patient)
        {
            try
            {
                await _connection.ExecuteAsync(
                    "sp_AdmitPatient",
                    new
                    {
                        patient.UserId,
                        patient.PatientType,
                        patient.ConditionId,
                        patient.MedicationId,
                        patient.AllergyId
                    },
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error admitting patient: {ex.Message}");
                throw;
            }
        }


        public async Task<IEnumerable<PatientListViewModel>> GetAllPatientsAsync(string searchPatient, int? wardId)
        {
            _logger.LogInformation($"Calling GetAllPatientsAsync with searchPatient: {searchPatient}, wardId: {wardId}");
            var patients = await _connection.QueryAsync<PatientListViewModel>(
            "sp_GetAllPatients",
            new { SearchPatient = searchPatient, WardId = wardId },
            commandType: CommandType.StoredProcedure
);

            _logger.LogInformation($"GetAllPatientsAsync returned {patients.Count()} patients");
            return patients;
        }
        public async Task<bool> UpdatePatientAsync(EditPatientViewModel model)
        {
            _logger.LogInformation($"Updating patient with PatientFileId: {model.PatientFileId}");

            var parameters = new
            {
                PatientFileId = model.PatientFileId,
                Name = model.Name,
                Surname = model.Surname,
                ConditionId = model.ConditionId,
                ProfilePhoto = model.ProfilePhoto,
                IsActive = model.IsActive
            };

            var result = await _connection.ExecuteAsync(
                "sp_EditPatient",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            _logger.LogInformation($"UpdatePatientAsync result: {result} rows affected");
            return result > 0; 
        }

        public async Task<EditPatientViewModel> GetPatientByPatientFileIdAsync(int patientFileId)
        {
            _logger.LogInformation($"Fetching patient with PatientFileId: {patientFileId}");

            var parameters = new { PatientFileId = patientFileId };

            var patient = await _connection.QueryFirstOrDefaultAsync<EditPatientViewModel>(
                "sp_GetPatientByPatientFileId",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (patient != null)
            {
                _logger.LogInformation($"Fetched patient: {patient.Name} {patient.Surname}");
            }
            else
            {
                _logger.LogWarning($"No patient found with PatientFileId: {patientFileId}");
            }

            return patient;
        }
        public async Task<IEnumerable<ConditionViewModel>> GetAllConditionsAsync()
        {
            _logger.LogInformation("Fetching all conditions.");

            var conditions = await _connection.QueryAsync<ConditionViewModel>(
                "sp_GetAllConditions",
                commandType: CommandType.StoredProcedure
            );

            _logger.LogInformation($"GetAllConditionsAsync returned {conditions.Count()} conditions.");
            return conditions;
        }
        public async Task<PatientFileDoctorAssignmentDetailsViewModel> GetPatientFileDetailsAsync(int patientFileId)
        {
            try
            {
                _logger.LogInformation($"Getting patient file details for PatientFileId: {patientFileId}");

                var patientFile = await _connection.QueryFirstOrDefaultAsync<PatientFileDoctorAssignmentDetailsViewModel>(
                    "sp_GetWardPatientFileDetailsforAssignment",
                    new { PatientFileId = patientFileId },
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation($"Retrieved patient file details for PatientFileId: {patientFileId}");
                return patientFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting patient file details for PatientFileId: {patientFileId}");
                throw;
            }
        }

        public async Task<IEnumerable<WardDoctorViewModel>> GetAvailableDoctorsForWardAsync(int wardId)
        {
            try
            {
                _logger.LogInformation($"Getting available doctors for WardId: {wardId}");

                var doctors = await _connection.QueryAsync<WardDoctorViewModel>(
                    "sp_GetAvailableDoctorsInWard",
                    new { WardId = wardId },
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation($"Retrieved {doctors.Count()} available doctors for WardId: {wardId}");
                return doctors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting available doctors for WardId: {wardId}");
                throw;
            }
        }

        public async Task AssignDoctorToPatientFileAsync(int patientFileId, int userId)
        {
            try
            {
                _logger.LogInformation($"Assigning doctor (UserId: {userId}) to PatientFileId: {patientFileId}");

                await _connection.ExecuteAsync(
                    "sp_AssignDoctorToPatientFile",
                    new { PatientFileId = patientFileId, UserId = userId },
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation($"Successfully assigned doctor (UserId: {userId}) to PatientFileId: {patientFileId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning doctor (UserId: {userId}) to PatientFileId: {patientFileId}");
                throw;
            }
        }
        public async Task<User> SearchPatientAsync(int userId)
        {
            return await _connection.QueryFirstOrDefaultAsync<User>(
                "SearchPatient",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure
            );
        }

        public IEnumerable<Condidtion> GetConditions()
        {
            return _connection.Query<Condidtion>("sp_LoadCondition", commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Medication> GetMedications()
        {
            return _connection.Query<Medication>("sp_LoadMedication", commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Allergy> GetAllergies()
        {
            return _connection.Query<Allergy>("sp_LoadAllergies", commandType: CommandType.StoredProcedure);
        }


        public async Task<bool> AssignRoomAndBedAsync(AssignRoomAndBedRequest request)
        {
            _logger.LogInformation($"AssignRoomAndBedAsync started for UserId: {request.UserId}");

            var patientExists = await _connection.QueryFirstOrDefaultAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Patients WHERE UserId = @UserId) THEN 1 ELSE 0 END",
                new { request.UserId });

            if (!patientExists)
            {
                _logger.LogWarning($"User with ID {request.UserId} does not exist or is not a patient.");
                throw new InvalidOperationException($"User with ID {request.UserId} does not exist or is not a patient.");
            }

            var parameters = new DynamicParameters(new
            {
                request.UserId,
                request.AdmissionDate,
                request.WardId,
                request.RoomNumber,
                request.BedId
            });
            parameters.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            try
            {
                _logger.LogInformation("Executing sp_AssignRoomAndBed stored procedure");
                await _connection.ExecuteAsync(
                    "sp_AssignRoomAndBed",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                var result = parameters.Get<int>("@Result");
                _logger.LogInformation($"sp_AssignRoomAndBed execution completed. Result: {result}");
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing sp_AssignRoomAndBed");
                throw;
            }
        }

        public async Task<PatientDetailsViewModel> GetPatientDetailsAsync(int userId)
        {
           return await _connection.QueryFirstOrDefaultAsync<PatientDetailsViewModel>(
           "sp_GetPatientDetails",
           new {userId },
           commandType: CommandType.StoredProcedure);
        }
        public async Task<IEnumerable<WardViewModel>> GetWardsAsync()
        {
            return await _connection.QueryAsync<WardViewModel>(
                "sp_GetWards",
                commandType: CommandType.StoredProcedure);
        }
        public async Task<IEnumerable<RoomViewModel>> GetRoomsByWardIdAsync(int wardId)
        {
            try
            {
                _logger.LogInformation($"GetRoomsByWardIdAsync called with wardId: {wardId}");
                var rooms = await _connection.QueryAsync<RoomViewModel>(
                    "sp_GetRoomsByWardId",
                    new { WardId = wardId },
                    commandType: CommandType.StoredProcedure);
                _logger.LogInformation($"GetRoomsByWardIdAsync returned {rooms.Count()} rooms");
                return rooms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetRoomsByWardIdAsync for wardId: {wardId}");
                throw;
            }
        }

        public async Task<IEnumerable<BedViewModel>> GetAvailableBedsByRoomIdAsync(int roomId)
        {
            try
            {
                _logger.LogInformation($"GetAvailableBedsByRoomIdAsync called with roomId: {roomId}");
                var beds = await _connection.QueryAsync<BedViewModel>(
                    "sp_GetAvailableBedsByRoomId",
                    new { RoomId = roomId },
                    commandType: CommandType.StoredProcedure);
                _logger.LogInformation($"GetAvailableBedsByRoomIdAsync returned {beds.Count()} beds");
                return beds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetAvailableBedsByRoomIdAsync for roomId: {roomId}");
                throw;
            }
        }
        public async Task<IEnumerable<PatientFileViewModel>> GetPatientsByRoomAsync(int roomId)
        {
            return await _connection.QueryAsync<PatientFileViewModel>("sp_GetPatientsByRoomId",
                new { RoomId = roomId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PatientFileViewModel> GetPatientByFileIdAsync(int patientFileId)
        {
            return await _connection.QueryFirstOrDefaultAsync<PatientFileViewModel>("sp_GetPatientByFileId",
                new { PatientFileId = patientFileId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PatientUpdateResult> UpdatePatientMovementStatusAsync(int patientFileId, string status)
        {
            return await _connection.QueryFirstOrDefaultAsync<PatientUpdateResult>("sp_UpdatePatientMovementStatus",
                new { PatientFileId = patientFileId, Status = status },
                commandType: CommandType.StoredProcedure);
        }

      

       
        public async Task<DischargeReport> GetDischargeReportAsync(int reportId)
        {
            try
            {
                _logger.LogInformation("Retrieving discharge report for ReportId: {ReportId}", reportId);

                var report = await _connection.QueryFirstOrDefaultAsync<DischargeReport>(
                    "sp_GetDischargeReport",
                    new { DischargeReportId = reportId },
                    commandType: CommandType.StoredProcedure
                );

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving discharge report for ReportId: {ReportId}", reportId);
                throw;
            }
        }
        public async Task<int> CreateDischargeReportAsync(DischargeReport dischargeReport)
        {
            try
            {
                _logger.LogInformation("Creating discharge report for PatientFileId: {PatientFileId}",
                    dischargeReport.PatientFileId);

                // Changed from QuerySingleAsync to QueryFirstOrDefaultAsync
                var result = await _connection.QueryFirstOrDefaultAsync<int>(
                    "sp_CreateDischargeReport",
                    new
                    {
                        dischargeReport.PatientFileId,
                        dischargeReport.DischargeDate,
                        dischargeReport.TotalBill,
                        dischargeReport.DischargeSummary,
                        dischargeReport.TreatmentProvided,
                        dischargeReport.MedicationPrescribed,
                        dischargeReport.DoctorFee,
                        dischargeReport.MedicationFee,
                        dischargeReport.RoomFee,
                        dischargeReport.OtherCosts,
                        dischargeReport.LengthOfStay,
                        dischargeReport.VitalSignsAtDischarge
                    },
                    commandType: CommandType.StoredProcedure
                );

                // Check if we got a valid result
                if (result == 0)
                {
                    throw new InvalidOperationException("Failed to create discharge report - no ID returned");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating discharge report for PatientFileId: {PatientFileId}",
                    dischargeReport.PatientFileId);
                throw;
            }
        }

        public async Task<bool> DischargePatientAsync(int userId, DateTime? dischargeDate, decimal total)
        {
            try
            {
                _logger.LogInformation("Attempting to discharge patient. UserId: {UserId}, DischargeDate: {DischargeDate}, Total: {Total}",
                    userId, dischargeDate, total);

                var result = await _connection.ExecuteAsync(
                    "sp_DischargePatient",
                    new
                    {
                        UserId = userId,
                        DischargeDate = dischargeDate,
                        Total = total
                    },
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation("Discharge result: {Result}", result);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing discharge stored procedure for UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<PatientDischargeViewModel> SearchPatientForDischargeAsync(int patientFileId)
        {
            try
            {
                var patient = await _connection.QueryFirstOrDefaultAsync<PatientDischargeViewModel>(
                    "sp_SearchPatientForDischarge",
                    new { PatientFileId = patientFileId },
                    commandType: CommandType.StoredProcedure
                );

                if (patient != null)
                {
                    // Calculate the length of stay
                    patient.LengthOfStay = (DateTime.Now - patient.AdmissionDate).Days;

                    // Format the room and bed information
                    if (!string.IsNullOrEmpty(patient.WardName))
                    {
                        patient.WardName = $"{patient.WardName} - Room {patient.RoomNumber}";
                        patient.BedNumber = $"Bed {patient.BedId}";
                        patient.BedStatus = string.IsNullOrEmpty(patient.BedStatus) ? "Not Assigned" : patient.BedStatus;
                    }
                    else
                    {
                        patient.WardName = "Available";
                        patient.BedNumber = "Available";
                        patient.BedStatus = "Available";
                    }
                }

                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for patient discharge details. PatientFileId: {PatientFileId}", patientFileId);
                throw;
            }
        }


        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = await _connection.QuerySingleOrDefaultAsync<User>("sp_GetUserById",new { UserId = userId },commandType: CommandType.StoredProcedure);
            return user;
        }
        public async Task<PatientDischargeViewModel> GetPatientDischargeDetailsAsync(int patientFileId)
        {
            var dischargeDetails = await _connection.QueryAsync<PatientDischargeViewModel>(
                "sp_GetPatientDischargeDetails",
                new { PatientFileId = patientFileId },
                commandType: CommandType.StoredProcedure
            );
            return dischargeDetails.FirstOrDefault();
        }
        public async Task<PatientFile> GetPatientFileByUserIdAsync(int userId)
        {
            var patientFile = await _connection.QuerySingleOrDefaultAsync<PatientFile>(
                "sp_GetPatientFileByUserId",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure
            );
            return patientFile;
        }

        public async Task<DischargeReportViewModel> GetDischargeReportDetailsAsync(int patientFileId)
        {
            try
            {
                var report = await _connection.QueryFirstOrDefaultAsync<DischargeReportViewModel>(
                    "sp_GetDischargeReportDetails",
                    new { PatientFileId = patientFileId },
                    commandType: CommandType.StoredProcedure
                );

                if (report == null)
                {
                    throw new ($"Discharge report not found for PatientFileId: {patientFileId}");
                }

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving discharge report details for PatientFileId: {PatientFileId}", patientFileId);
                throw;
            }
        }


    }
}

