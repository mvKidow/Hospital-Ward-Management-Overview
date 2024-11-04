using CareConnect.Models;
using Connect.Models;
using Connect.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;


namespace Connect.Controllers
{


    public class AdminController : BaseController
    {
        private readonly IAdminRepository _adminRepository;
        public AdminController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await _adminRepository.GetDashboardDataAsync();
                var viewModel = new AdminDashboardViewModel
                {
                    AvailableDoctors = dashboardData.AvailableDoctors,
                    AvailableBeds = dashboardData.AvailableBeds,
                    AvailablePatients = dashboardData.AvailablePatients,
                    AvailableNurses = dashboardData.AvailableNurses
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard. Please try again.";
                return View(new AdminDashboardViewModel());
            }
        }

        public IActionResult CreateMember()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMember(CreateMemberViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new User
                    {
                        Name = model.Name,
                        Surname = model.Surname,
                        Email = model.Email,
                        Password = model.Password,
                        Title = model.Title,
                        Phone = model.Phone,
                        Role = model.Role,
                        Status = model.Status,
                        WardId = model.WardId
                    };

                    if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await model.ProfilePhoto.CopyToAsync(memoryStream);
                            user.ProfilePhoto = memoryStream.ToArray();
                        }
                    }

                    await _adminRepository.CreateAsync(user);
                    TempData["SuccessMessage"] = "Member added successfully!";
                    return RedirectToAction("CreateMember");
                }
                catch (DbUpdateException ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while adding the member: {ex.InnerException?.Message ?? ex.Message}";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                }
            }
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> ViewEmployees(int page = 1, string getEmployees = null)
        {
            try
            {
                var employees = await _adminRepository.GetEmployeeListAsync(getEmployees);
                var pageSize = 5;

                var paginatedEmployees = employees
                .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling(employees.Count() / (double)pageSize);
                ViewBag.TotalItems = employees.Count();

                return View(paginatedEmployees);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading employees. Please try again.";
                return View(new List<EmployeeListViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ViewEmployees(string getEmployees, int page = 1)
        {
            try
            {
                var employees = await _adminRepository.GetEmployeeListAsync(getEmployees);
                if (employees == null || !employees.Any())
                {
                    TempData["msg"] = "Member Not Found";
                    employees = await _adminRepository.GetEmployeeListAsync(null);
                }
                var pageSize = 5;
                var paginatedEmployees = employees
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling(employees.Count() / (double)pageSize);
                ViewBag.TotalItems = employees.Count();

                return View(paginatedEmployees);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while searching for employees. Please try again.";
                return View(new List<EmployeeListViewModel>());
            }
        }

        public IActionResult CreateSchedule()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateSchedule(ShiftAssignment model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _adminRepository.CreateScheduleAsync(model);
                    TempData["SuccessMessage"] = "Schedule created successfully!";
                    return RedirectToAction("CreateSchedule");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while creating the schedule. Please try again.";
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult UserShifts(int userId)
        {
            try
            {
                var shifts = _adminRepository.GetUserShiftsAsync(userId);
                return View(shifts);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading user shifts. Please try again.";
                return View(new List<ShiftAssignment>());
            }
        }

        [HttpGet]
        public IActionResult ShiftsByDate(DateTime date)
        {
            try
            {
                var shifts = _adminRepository.GetShiftsByDateAsync(date);
                return View(shifts);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading shifts for the selected date. Please try again.";
                return View(new List<ShiftAssignment>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllConsumables(int page = 1, string getConsumable = null)
        {
            try
            {
                var consumables = await _adminRepository.GetAllConsumablesAsync(getConsumable);
                var pageSize = 5;

                var paginatedConsumables = consumables
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling(consumables.Count() / (double)pageSize);
                ViewBag.TotalItems = consumables.Count();

                return View(paginatedConsumables);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading consumables. Please try again.";
                return View(new List<Consumable>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ViewAllConsumables(string getConsumable, int page = 1)
        {
            try
            {
                var consumables = await _adminRepository.GetAllConsumablesAsync(getConsumable);
                if (consumables == null || !consumables.Any())
                {
                    TempData["msg"] = "Consumable Not Found";
                    consumables = await _adminRepository.GetAllConsumablesAsync(null);
                }

                var pageSize = 5;

                var paginatedConsumables = consumables
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling(consumables.Count() / (double)pageSize);
                ViewBag.TotalItems = consumables.Count();

                return View(paginatedConsumables);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while searching for consumables. Please try again.";
                return View(new List<Consumable>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNewConsumable()
        {
            try
            {
                var suppliers = await _adminRepository.GetAllSuppliersAsync();
                ViewBag.Suppliers = new SelectList(suppliers, "SupplierId", "SupplierName");
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading suppliers. Please try again.";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewConsumable(Consumable consumable)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _adminRepository.AddConsumableAsync(consumable);
                    TempData["SuccessMessage"] = "Consumable added successfully!";
                    return RedirectToAction(nameof(ViewAllConsumables));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while adding the consumable. Please try again.";
                }
            }
            return View(consumable);
        }

        [HttpGet]
        public async Task<IActionResult> EditConsumable(int id)
        {
            try
            {
                var consumable = await _adminRepository.GetConsumableByIdAsync(id);
                if (consumable == null)
                {
                    return NotFound();
                }
                return View(consumable);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the consumable. Please try again.";
                return RedirectToAction(nameof(ViewAllConsumables));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConsumable(int id, Consumable consumable)
        {
            if (id != consumable.ConsumableId)
            {
                TempData["ErrorMessage"] = "Invalid consumable ID.";
                return RedirectToAction(nameof(ViewAllConsumables));
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _adminRepository.UpdateConsumableAsync(consumable);
                    TempData["SuccessMessage"] = "Consumable updated successfully!";
                    return RedirectToAction(nameof(ViewAllConsumables));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the consumable. Please try again.";
                }
            }
            return View(consumable);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConsumable(int id)
        {
            try
            {
                await _adminRepository.DeleteConsumableAsync(id);
                TempData["SuccessMessage"] = "Consumable deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the consumable. Please try again.";
            }
            return RedirectToAction(nameof(ViewAllConsumables));
        }

        [HttpGet]
        public async Task<IActionResult> ViewMedication(int page = 1, string getMedication = null)
        {
            try
            {
                var medications = await _adminRepository.GetAllMedicationsAsync(getMedication);

                if (medications == null)
                {
                    medications = new List<MedicationViewModel>();
                }

                var pageSize = 4;
                var paginatedMedications = medications
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling(medications.Count() / (double)pageSize);
                ViewBag.TotalItems = medications.Count();
                return View(paginatedMedications);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading medications. Please try again.";
                return View(new List<Medication>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ViewMedication(string getMedication, int page = 1)
        {
            try
            {
                var medications = await _adminRepository.GetAllMedicationsAsync(getMedication);
                if (medications == null || !medications.Any())
                {
                    TempData["msg"] = "Medication Not Found";
                    medications = await _adminRepository.GetAllMedicationsAsync(null);
                }
                var pageSize = 4;
                var paginatedMedications = medications
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling(medications.Count() / (double)pageSize);
                ViewBag.TotalItems = medications.Count();
                return View(paginatedMedications);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while searching for medications. Please try again.";
                return View(new List<MedicationViewModel>());
            }
        }

        public IActionResult CreateMedication()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedication(Medication medication)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _adminRepository.AddMedicationAsync(medication);
                    TempData["SuccessMessage"] = "Medication added successfully!";
                    return RedirectToAction(nameof(ViewMedication));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while adding the medication. Please try again.";
                }
            }
            return View(medication);
        }
        [HttpGet]
        public async Task<IActionResult> EditMedication(int id)
        {
            try
            {
                var medication = await _adminRepository.GetMedicationByIdAsync(id);
                if (medication == null)
                {
                    return NotFound();
                }
                return View(medication);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the medication. Please try again.";
                return RedirectToAction(nameof(ViewMedication));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedication(int id, [Bind("MedicationId,Name,Description")] Medication medication)
        {
            if (id != medication.MedicationId)
            {
                TempData["ErrorMessage"] = "Invalid medication ID.";
                return RedirectToAction(nameof(ViewMedication));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _adminRepository.UpdateMedicationAsync(medication);
                    TempData["SuccessMessage"] = "Medication updated successfully!";
                    return RedirectToAction(nameof(ViewMedication));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the medication. Please try again.";
                }
            }
            return View(medication);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedication(int id)
        {
            try
            {
                await _adminRepository.SoftDeleteMedicationAsync(id);
                TempData["SuccessMessage"] = "Medication deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the medication. Please try again.";
            }
            return RedirectToAction(nameof(ViewMedication));
        }

        [HttpGet]
        public async Task<IActionResult> AddCondition()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCondition(Condidtion condidtion)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _adminRepository.AddConditionAsync(condidtion);
                    TempData["SuccessMessage"] = "Condition added successfully!";
                    return RedirectToAction(nameof(ConditionList));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while adding the condition. Please try again.";
                }
            }
            return View(condidtion);
        }

        [HttpGet]
        public async Task<IActionResult> ConditionList(int page = 1, string getCondition = null)
        {
            try
            {
                var conditions = await _adminRepository.GetAllConditionsAsync(getCondition);
                var pageSize = 3;

                var paginatedConditions = conditions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling(conditions.Count() / (double)pageSize);
                ViewBag.TotalItems = conditions.Count();

                return View(paginatedConditions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading conditions. Please try again.";
                return View(new List<ConditionViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConditionList(string getCondition, int page = 1)
        {
            try
            {
                var conditions = await _adminRepository.GetAllConditionsAsync(getCondition);
                if (conditions == null || !conditions.Any())
                {
                    TempData["msg"] = "Condition Not Found";
                    conditions = await _adminRepository.GetAllConditionsAsync(null);
                }

                var pageSize = 5;

                var paginatedConditions = conditions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling(conditions.Count() / (double)pageSize);
                ViewBag.TotalItems = conditions.Count();

                return View(paginatedConditions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while searching for conditions. Please try again.";
                return View(new List<ConditionViewModel>());
            }


        }

    
        [HttpGet]
        public async Task<IActionResult> EditAllergy(int id)
        {
            var allergy = await _adminRepository.GetAllergyByIdAsync(id);
            if (allergy == null)
            {
                return NotFound();
            }
            var model = new AllergyViewModel
            {
                AllergyId = allergy.AllergyId,
                Name = allergy.Name,
                Severity = allergy.Severity
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAllergy(AllergyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditAllergy", model);
            }
            var allergy = new Allergy
            {
                AllergyId = model.AllergyId,
                Name = model.Name,
                Severity = model.Severity
            };
            await _adminRepository.UpdateAllergyAsync(allergy);
            TempData["SuccessMessage"] = "Allergy updated successfully!";
            return RedirectToAction("AllergyList");
        }

        // Edit Condition
        [HttpGet]
        public async Task<IActionResult> EditCondition(int id)
        {
            var condition = await _adminRepository.GetConditionByIdAsync(id);
            if (condition == null)
            {
                return NotFound();
            }
            var model = new ConditionViewModel
            {
                ConditionId = condition.ConditionId,
                ConditionName = condition.Name,
                Description = condition.Description
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCondition(ConditionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditCondition", model);
            }
            var condition = new Condidtion
            {
                ConditionId = model.ConditionId,
                Name = model.ConditionName,
                Description = model.Description
            };
            await _adminRepository.UpdateConditionAsync(condition);
            TempData["SuccessMessage"] = "Condition updated successfully!";
            return RedirectToAction("ConditionList");
        }

       
   

        [HttpPost]
        public async Task<IActionResult> EditCondition(ConditionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var conditionToUpdate = await _adminRepository.GetConditionByIdAsync(model.ConditionId);
                if (conditionToUpdate == null)
                {
                    return NotFound();
                }

                conditionToUpdate.Name = model.ConditionName;
                conditionToUpdate.Description = model.Description;

                await _adminRepository.UpdateConditionAsync(conditionToUpdate);

                TempData["SuccessMessage"] = "Condition updated successfully!";
                return RedirectToAction(nameof(ConditionList));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Filled to update. Please try again.";
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult AddAllergy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAllergy(Allergy allergy)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _adminRepository.AddAllergyAsync(allergy);
                    TempData["SuccessMessage"] = "Allergy added successfully!";
                    return RedirectToAction(nameof(ViewAllergies));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while adding the allergy. Please try again.";
                }
            }
            return View(allergy);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllergies(int page = 1, string getAllergy = null)
        {
            try
            {
                var allergies = await _adminRepository.GetAllAllergiesAsync(getAllergy);
                var pageSize = 3;
                var paginatedAllergies = allergies
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling(allergies.Count() / (double)pageSize);
                ViewBag.TotalItems = allergies.Count();
                return View(paginatedAllergies);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading allergies. Please try again.";
                return View(new List<AllergyViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ViewAllergies(string getAllergy, int page = 1)
        {
            try
            {
                var allergies = await _adminRepository.GetAllAllergiesAsync(getAllergy);
                if (allergies == null || !allergies.Any())
                {
                    TempData["msg"] = "Allergy Not Found";
                    allergies = await _adminRepository.GetAllAllergiesAsync(null);
                }
                var pageSize = 5;
                var paginatedAllergies = allergies
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling(allergies.Count() / (double)pageSize);
                ViewBag.TotalItems = allergies.Count();
                return View(paginatedAllergies);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while searching for allergies. Please try again.";
                return View(new List<AllergyViewModel>());
            }
        }

       

        [HttpPost]
        public async Task<IActionResult> EditAllergy(AllergyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var allergyToUpdate = await _adminRepository.GetAllergyByIdAsync(model.AllergyId);
                if (allergyToUpdate == null)
                {
                    return NotFound();
                }
                allergyToUpdate.Name = model.Name;
                allergyToUpdate.Severity = model.Severity;
                await _adminRepository.UpdateAllergyAsync(allergyToUpdate);
                TempData["SuccessMessage"] = "Allergy updated successfully!";
                return RedirectToAction(nameof(ViewAllergies));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to update. Please try again.";
                return View(model);
            }
        }

        
       
        public async Task<IActionResult> RoomAndBeds()
        {
            
            var roomsData = await _adminRepository.GetAllRoomsAsync();
            var roomViewModels = roomsData.Select(r => new RoomViewModel
            {
                RoomId = r.RoomId,
                RoomNumber = r.RoomNumber,
                Capacity = r.Capacity,
                AvailabilityStatus = r.AvailabilityStatus,
                WardId = r.WardId,
                WardName = "", // Add logic to get ward name
                OccupiedBeds = 0, // Add logic to calculate
                TotalBeds = 0 // Add logic to calculate
            });

            var viewModel = new WardManagementViewModel
            {
                Rooms = roomViewModels,
                Beds = await _adminRepository.GetAllBedsAsync()
            };
            return View(viewModel);
        }


        #region Room Management API Endpoints
        [HttpPost]
        public async Task<IActionResult> AddRoom([FromBody] Room room)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newRoom = await _adminRepository.AddRoomAsync(room);
                return Json(new { success = true, data = newRoom });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRoom(int id)
        {
            try
            {
                var room = await _adminRepository.GetRoomByIdAsync(id);
                if (room == null)
                    return NotFound();

                return Json(new { success = true, data = room });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRoom([FromBody] Room room)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _adminRepository.UpdateRoomAsync(room);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                var result = await _adminRepository.DeleteRoomAsync(id);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Bed Management API Endpoints
        [HttpPost]
        public async Task<IActionResult> AddBed([FromBody] Bed bed)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newBed = await _adminRepository.AddBedAsync(bed);
                return Json(new { success = true, data = newBed });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetBedsByRoom(int roomId)
        {
            try
            {
                var beds = await _adminRepository.GetBedsByRoomAsync(roomId);
                return Json(new { success = true, data = beds });
            }
            catch (Exception ex)
            {
             
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBed([FromBody] Bed bed)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _adminRepository.UpdateBedAsync(bed);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBed(int id)
        {
            try
            {
                var result = await _adminRepository.DeleteBedAsync(id);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion
    }
}

