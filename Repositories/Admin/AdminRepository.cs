using CareConnect.Models;
using Connect.Data;
using Connect.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

public class AdminRepository : IAdminRepository
{
    private readonly CareConnectDbContext _context;
    private readonly IDbConnection _connection;

    public AdminRepository(CareConnectDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task<(int AvailableDoctors, int AvailableBeds, int AvailablePatients, int AvailableNurses)> GetDashboardDataAsync()
    {
        using (var connection = _connection)
        {
            var result = await connection.QueryMultipleAsync("sp_GetDashboardData", commandType: CommandType.StoredProcedure);
            var availableDoctors = await result.ReadFirstOrDefaultAsync<int>();
            var availableBeds = await result.ReadFirstOrDefaultAsync<int>();
            var availablePatients = await result.ReadFirstOrDefaultAsync<int>();
            var availableNurses = await result.ReadFirstOrDefaultAsync<int>();

            return (AvailableDoctors: availableDoctors,
                    AvailableBeds: availableBeds,
                    AvailablePatients: availablePatients,
                    AvailableNurses: availableNurses);
        }
    }


    public async Task CreateAsync(User user)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
           
            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

         
            if (user.WardId.HasValue)
            {
                var userWard = new UserWard
                {
                    UserId = user.UserId,
                    WardId = user.WardId.Value
                };
                await _context.UserWards.AddAsync(userWard);

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw; 
        }
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        using (var connection = _connection)
        {
            return await connection.QueryAsync<User>("sp_GetAllUsers", commandType: CommandType.StoredProcedure);
        }
    }

    public async Task<User> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task UpdateAsync(User user)
    {
        _context.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            using (var connection = _connection)
            {
                await connection.ExecuteAsync("sp_Delete_User", new { ID = id }, commandType: CommandType.StoredProcedure);
                return true;
            }
        }
        catch (Exception ex)
        {
            // Log exception details here
            return false;
        }
    }


    public async Task<IEnumerable<EmployeeListViewModel>> GetEmployeeListAsync(string GetEmployees)
    {
        var users = await _connection.QueryAsync<User>("sp_GetEmployeeList",
            new { GetEmployees },
            commandType: CommandType.StoredProcedure);

        var employeeList = users.Select(user => new EmployeeListViewModel
        {

            Name = user.Name,
            Surname = user.Surname,
            Title = user.Title,
            Status = user.Status,
            Role = user.Role,
            Email = user.Email
        });

        return employeeList;
    }

    public async Task<bool> CreateScheduleAsync(ShiftAssignment assignment)
    {
        try
        {
            using (var connection = _connection)
            {
                var affectedRows = await connection.ExecuteAsync("AssignShift", new
                {
                    assignment.UserId,
                    assignment.Date,
                    assignment.Shift
                }, commandType: CommandType.StoredProcedure);

                return affectedRows > 0;
            }
        }
        catch (Exception ex)
        {

            return false;
        }
    }

    public async Task<List<ShiftAssignment>> GetUserShiftsAsync(int userId)
    {
        using (var connection = _connection)
        {
            return (await connection.QueryAsync<ShiftAssignment>("GetUserShifts", new { UserId = userId }, commandType: CommandType.StoredProcedure)).ToList();
        }
    }

    public async Task<List<ShiftAssignment>> GetShiftsByDateAsync(DateTime date)
    {
        using (var connection = _connection)
        {
            return (await connection.QueryAsync<ShiftAssignment>("GetShiftsByDate", new { Date = date }, commandType: CommandType.StoredProcedure)).ToList();
        }
    }

    public async Task<Allergy> GetAllergyByIdAsync(int id)
    {
        return await _connection.QueryFirstOrDefaultAsync<Allergy>(
            "sp_GetAllergyById", new { AllergyId = id }, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateAllergyAsync(Allergy allergy)
    {
        await _connection.ExecuteAsync("sp_UpdateAllergy",
            new
            {
                allergy.AllergyId,
                allergy.Name,
                allergy.Severity
            }, commandType: CommandType.StoredProcedure);
    }

    public async Task<Condidtion> GetConditionByIdAsync(int id)
    {
        return await _connection.QueryFirstOrDefaultAsync<Condidtion>(
            "sp_GetConditionById", new { ConditionId = id }, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateConditionAsync(Condidtion condition)
    {
        await _connection.ExecuteAsync("sp_UpdateCondition",
            new
            {
                condition.ConditionId,
                condition.Name,
                condition.Description
            }, commandType: CommandType.StoredProcedure);
    }

    public async Task<Medication> GetMedicationByIdAsync(int id)
    {
        return await _connection.QueryFirstOrDefaultAsync<Medication>(
            "sp_GetMedicationById", new { MedicationId = id }, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateMedicationAsync(Medication medication)
    {
        await _connection.ExecuteAsync("sp_UpdateMedication",
            new
            {
                medication.MedicationId,
                medication.Name,
                medication.Description
            }, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ConsumableViewModel>> GetAllConsumablesAsync(string GetConsumables)
    {
        return await _connection.QueryAsync<ConsumableViewModel>("sp_GetAllConsumables"
            ,new { GetConsumables},commandType: CommandType.StoredProcedure);
    }

    public async Task<ConsumableViewModel> GetConsumableByIdAsync(int id)
    {
        using (var connection = _connection)
        {
            return await connection.QueryFirstOrDefaultAsync<ConsumableViewModel>("sp_GetConsumableById",new { ConsumableId = id },commandType: CommandType.StoredProcedure);
        }
    }
    public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
    {
        return await _connection.QueryAsync<Supplier>("sp_GetAllSuppliers", commandType: CommandType.StoredProcedure);
    }
    public async Task<int> AddConsumableAsync(Consumable consumable)
    {
        try
        {
            return await _connection.ExecuteScalarAsync<int>("sp_AddConsumable",
                new
                {
                    consumable.Name,
                    consumable.Type,
                    consumable.QuantityAvailable,
                    consumable.SupplierId
                },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error adding consumable: {ex.Message}");
            throw; // Re-throw the exception to be handled by the caller
        }
    }

    public async Task UpdateConsumableAsync(Consumable consumable)
    {
        using (var connection = _connection)
        {
            await connection.ExecuteAsync("sp_UpdateConsumable",
                new
                {
                    consumable.ConsumableId,
                    consumable.Name,
                    consumable.Type,
                    consumable.QuantityAvailable,
                    consumable.SupplierId
                },
                commandType: CommandType.StoredProcedure);
        }
    }

    public async Task DeleteConsumableAsync(int id)
    {
        using (var connection = _connection)
        {
            await connection.ExecuteAsync("sp_DeleteConsumable",
                new { ConsumableId = id },
                commandType: CommandType.StoredProcedure);
        }
    }

    public async Task<IEnumerable<MedicationViewModel>> GetAllMedicationsAsync(string getMedication)
    {
        using (var connection = _connection)
        {
            return await connection.QueryAsync<MedicationViewModel>(
                "sp_GetAllMedications",
                new { GetMedication = getMedication },
                commandType: CommandType.StoredProcedure
            );
        }
    }

    public async Task<int> AddMedicationAsync(Medication medication)
    {
        using (var connection = _connection)
        {
            return await connection.ExecuteScalarAsync<int>("sp_AddMedication",
                new
                {
                    medication.Name,
                    medication.Description
                },
                commandType: CommandType.StoredProcedure);
        }
    }
  
    public async Task SoftDeleteMedicationAsync(int id)
    {
        using (var connection = _connection)
        {
            await connection.ExecuteAsync("sp_SoftDeleteMedication",
                new { MedicationId = id },
                commandType: CommandType.StoredProcedure);
        }
    }

    public async Task<int> AddConditionAsync(Condidtion condidtion)
    {
        try
        {
            return await _connection.ExecuteScalarAsync<int>("sp_AddCondition",
                new
                {
                    condidtion.Name,
                    condidtion.Description
                  
                },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error adding Condition: {ex.Message}");
            throw; // Re-throw the exception to be handled by the caller
        }
    }
   
    public async Task<IEnumerable<ConditionViewModel>> GetAllConditionsAsync(string getCondition)
    {
        return await _connection.QueryAsync<ConditionViewModel>(
            "sp_GetAllConditions",
            new { GetCondition = getCondition },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<int> AddAllergyAsync(Allergy allergy)
    {
        try
        {
            return await _connection.ExecuteScalarAsync<int>("sp_AddAllergy",
                new
                {
                    allergy.Name,
                    allergy.Severity
                },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error adding Allergy: {ex.Message}");
            throw; // Re-throw the exception to be handled by the caller
        }
    }

    public async Task<IEnumerable<AllergyViewModel>> GetAllAllergiesAsync(string getAllergy)
    {
        return await _connection.QueryAsync<AllergyViewModel>(
            "sp_GetAllAllergies",
            new { GetAllergy = getAllergy },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task DeleteAllergyAsync(int id)
    {
        using (var connection = _connection)
        {
            await connection.ExecuteAsync(
                "sp_DeleteAllergy",
                new { AllergyId = id },
                commandType: CommandType.StoredProcedure
            );
        }
    }

    // Room Management Methods
    public async Task<IEnumerable<Room>> GetAllRoomsAsync()
    {
        try
        {
            return await _connection.QueryAsync<Room>(
                "sp_GetAllRooms",
                commandType: CommandType.StoredProcedure
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all rooms: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<RoomViewModel>> GetAllRoomDetailsAsync()
    {
        try
        {
            return await _connection.QueryAsync<RoomViewModel>(
                "sp_GetAllRoomDetails",
                commandType: CommandType.StoredProcedure
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting room details: {ex.Message}");
            throw;
        }
    }

    public async Task<Room> GetRoomByIdAsync(int id)
    {
        try
        {
            return await _connection.QueryFirstOrDefaultAsync<Room>(
                "sp_GetRoomById",
                new { RoomId = id },
                commandType: CommandType.StoredProcedure
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting room by id: {ex.Message}");
            throw;
        }
    }

    public async Task<int> AddRoomAsync(Room room)
    {
        try
        {
            return await _connection.ExecuteScalarAsync<int>(
                "sp_InsertRoom",
                new
                {
                    room.RoomNumber,
                    room.Capacity,
                    room.AvailabilityStatus,
                    room.WardId
                },
                commandType: CommandType.StoredProcedure
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding room: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateRoomAsync(Room room)
    {
        try
        {
            await _connection.ExecuteAsync(
                "sp_UpdateRoom",
                new
                {
                    room.RoomId,
                    room.RoomNumber,
                    room.Capacity,
                    room.AvailabilityStatus,
                    room.WardId
                },
                commandType: CommandType.StoredProcedure
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating room: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteRoomAsync(int id)
    {
        try
        {
            await _connection.ExecuteAsync(
                "sp_DeleteRoom",
                new { RoomId = id },
                commandType: CommandType.StoredProcedure
            );
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting room: {ex.Message}");
            return false;
        }
    }

    // Bed Management Methods
    public async Task<IEnumerable<BedViewModel>> GetBedsByRoomAsync(int roomId)
    {
        try
        {
            var bedDictionary = new Dictionary<int, BedViewModel>();

            await _connection.QueryAsync<BedViewModel, AdminPatientFileViewModel, BedViewModel>(
                "sp_GetBedsByRoom",
                (bed, patientFile) =>
                {
                    if (!bedDictionary.TryGetValue(bed.BedId, out var bedEntry))
                    {
                        bedEntry = bed;
                        if (patientFile?.PatientFileId != null)
                        {
                            // Only set the patient file if it exists
                            bedEntry.CurrentPatientFile = patientFile;
                        }
                        bedDictionary.Add(bed.BedId, bedEntry);
                    }
                    return bedEntry;
                },
                new { RoomId = roomId },
                splitOn: "PatientFileId",
                commandType: CommandType.StoredProcedure
            );

            return bedDictionary.Values;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting beds for room {roomId}: {ex.Message}");
            throw;
        }
    }
    public async Task<IEnumerable<BedViewModel>> GetAllBedsAsync()
    {
        try
        {
            var bedDictionary = new Dictionary<int, BedViewModel>();

            var beds = await _connection.QueryAsync<BedViewModel, AdminPatientFileViewModel, BedViewModel>(
                "sp_GetAllBeds",
                (bed, patientFile) =>
                {
                    if (!bedDictionary.TryGetValue(bed.BedId, out var bedEntry))
                    {
                        bedEntry = bed;
                        bedEntry.CurrentPatientFile = patientFile;
                        bedDictionary.Add(bed.BedId, bedEntry);
                    }
                    return bedEntry;
                },
                splitOn: "PatientFileId",
                commandType: CommandType.StoredProcedure
            );

            return bedDictionary.Values;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all beds: {ex.Message}");
            throw;
        }
    }

    public async Task<int> AddBedAsync(Bed bed)
    {
        try
        {
            return await _connection.ExecuteScalarAsync<int>(
                "sp_InsertBed",
                new
                {
                    bed.Status,
                    bed.RoomId
                },
                commandType: CommandType.StoredProcedure
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding bed: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateBedAsync(Bed bed)
    {
        try
        {
            await _connection.ExecuteAsync(
                "sp_UpdateBed",
                new
                {
                    bed.BedId,
                    bed.Status,
                    bed.RoomId
                },
                commandType: CommandType.StoredProcedure
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating bed: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteBedAsync(int id)
    {
        try
        {
            await _connection.ExecuteAsync(
                "sp_DeleteBed",
                new { BedId = id },
                commandType: CommandType.StoredProcedure
            );
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting bed: {ex.Message}");
            return false;
        }
    }


    public async Task<CareConnectInfor> GetInfoAsync()
    {
        using (var connection = _connection)
        {
            return await connection.QuerySingleOrDefaultAsync<CareConnectInfor>(
                "sp_GetCareConnectInfo",
                commandType: CommandType.StoredProcedure
            );
        }
    }

    public async Task<int> CreateInfoAsync(CareConnectInfor info)
    {
        using (var connection = _connection)
        {
            return await connection.ExecuteScalarAsync<int>(
                "sp_CreateCareConnectInfo",
                new
                {
                    info.CompanyName,
                    info.Phone,
                    info.Email,
                    info.WorkingHours,
                    info.AboutTitle,
                    info.AboutDescription,
                    info.ServiceTitle,
                    info.ServiceDescription
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }

    public async Task<bool> UpdateInfoAsync(CareConnectInfor info)
    {
        using (var connection = _connection)
        {
            var affected = await connection.ExecuteAsync(
                "sp_UpdateCareConnectInfo",
                new
                {
                    info.Id,
                    info.CompanyName,
                    info.Phone,
                    info.Email,
                    info.WorkingHours,
                    info.AboutTitle,
                    info.AboutDescription,
                    info.ServiceTitle,
                    info.ServiceDescription
                },
                commandType: CommandType.StoredProcedure
            );
            return affected > 0;
        }
    }

    public async Task<bool> DeleteInfoAsync(int id)
    {
        using (var connection = _connection)
        {
            var affected = await connection.ExecuteAsync(
                "sp_DeleteCareConnectInfo",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
            return affected > 0;
        }
    }
}




