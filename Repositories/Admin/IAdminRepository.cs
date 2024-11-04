using CareConnect.Models;
using Connect.Models;

public interface IAdminRepository
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> GetByIdAsync(int id);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<EmployeeListViewModel>> GetEmployeeListAsync(string GetEmployees);
    Task<(int AvailableDoctors, int AvailableBeds, int AvailablePatients, int AvailableNurses)> GetDashboardDataAsync();
    Task<bool> CreateScheduleAsync(ShiftAssignment assignment);
    Task<List<ShiftAssignment>> GetUserShiftsAsync(int userId);
    Task<List<ShiftAssignment>> GetShiftsByDateAsync(DateTime date);
    Task<IEnumerable<ConsumableViewModel>> GetAllConsumablesAsync(string GetConsumables);
    Task<ConsumableViewModel> GetConsumableByIdAsync(int id);
    Task<IEnumerable<Supplier>> GetAllSuppliersAsync();
    Task<int> AddConsumableAsync(Consumable consumable);
    Task UpdateConsumableAsync(Consumable consumable);
    Task DeleteConsumableAsync(int id);
    Task<IEnumerable<MedicationViewModel>> GetAllMedicationsAsync(string getMedication);
    Task<Medication> GetMedicationByIdAsync(int id);
    Task<int> AddMedicationAsync(Medication medication);
    Task UpdateMedicationAsync(Medication medication);
    Task SoftDeleteMedicationAsync(int id);
    Task<int>AddConditionAsync(Condidtion condidtion);
    Task UpdateConditionAsync(Condidtion condition);
    Task<IEnumerable<ConditionViewModel>> GetAllConditionsAsync(string GetCondition);
    Task<Condidtion> GetConditionByIdAsync(int conditionId);
    Task<int> AddAllergyAsync(Allergy allergy);
    Task<IEnumerable<AllergyViewModel>> GetAllAllergiesAsync(string getAllergy);
    Task<Allergy> GetAllergyByIdAsync(int allergyId);
    Task UpdateAllergyAsync(Allergy allergy);

    Task<IEnumerable<Room>> GetAllRoomsAsync();
    Task<IEnumerable<RoomViewModel>> GetAllRoomDetailsAsync();
    Task<Room> GetRoomByIdAsync(int id);
    Task<int> AddRoomAsync(Room room);
    Task UpdateRoomAsync(Room room);
    Task<bool> DeleteRoomAsync(int id);
    Task<IEnumerable<BedViewModel>> GetAllBedsAsync();
    Task<int> AddBedAsync(Bed bed);
    Task UpdateBedAsync(Bed bed);
    Task<bool> DeleteBedAsync(int id);
    Task<IEnumerable<BedViewModel>> GetBedsByRoomAsync(int roomId);




    Task<CareConnectInfor> GetInfoAsync();
    Task<int> CreateInfoAsync(CareConnectInfor info);
    Task<bool> UpdateInfoAsync(CareConnectInfor info);
    Task<bool> DeleteInfoAsync(int id);


}