using CareConnect.Models;
using Connect.Models;
using Dapper;
using Connect.Repositories;
using Connect.Repositories.WardAdmin;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using Connect.Repositories.Account;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Drawing.Printing;
using Microsoft.Extensions.Logging;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using DinkToPdf.Contracts;
using Connect.Services;

namespace Connect.Controllers
{
    public class WardAdminController : BaseController
    {
        private readonly IWardAdminRepository _adminRepository;
        private readonly IUserRepository _userService;
        private readonly ILogger<WardAdminController> _logger;
        private readonly IConverter _converter;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IViewRenderService _viewRenderService;

        public WardAdminController(
            IWardAdminRepository adminRepository,
            IUserRepository userService,
            ILogger<WardAdminController> logger,
            IConverter converter,
            IWebHostEnvironment webHostEnvironment,
            IViewRenderService viewRenderService)
        {
            _adminRepository = adminRepository;
            _userService = userService;
            _logger = logger;
            _converter = converter;
            _webHostEnvironment = webHostEnvironment;
            _viewRenderService = viewRenderService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("No user ID found in claims");
                    return RedirectToAction("Login", "Account");
                }

                // Get user with their ward assignments
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"No user found for ID {userId}");
                    return RedirectToAction("Login", "Account");
                }

                // Get user's ward assignments
                var userWards = await _userService.GetUserWardsAsync(userId);
                if (!userWards.Any())
                {
                    _logger.LogWarning($"No wards assigned to user {userId}");
                    TempData["ErrorMessage"] = "You are not assigned to any wards.";
                    return View(new WardDashboardData());
                }

                // Handle multiple ward assignments
                if (userWards.Count() > 1)
                {
                    ViewBag.Wards = userWards.Select(uw => new SelectListItem
                    {
                        Value = uw.WardId.ToString(),
                        Text = uw.Ward?.WardName ?? "Unknown Ward"
                    }).ToList();
                }

                // Use the first ward if no specific ward is selected
                var firstWard = userWards.FirstOrDefault()?.Ward;
                if (firstWard == null)
                {
                    _logger.LogError($"Ward data not found for user {userId}");
                    TempData["ErrorMessage"] = "Ward information is incomplete. Please contact your administrator.";
                    return View(new WardDashboardData());
                }

                var wardId = firstWard.WardId;
                _logger.LogInformation($"Loading ward dashboard for wardId: {wardId}");

                // Get dashboard data
                var wardData = await _adminRepository.GetWardDashboardDataAsync(wardId);

                // Store current ward info in ViewBag
                ViewBag.CurrentWard = firstWard;
                ViewBag.CurrentWardId = wardId;

                return View(wardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ward dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the ward dashboard.";
                return View(new WardDashboardData());
            }
        }

        [HttpGet]
        public async Task<IActionResult> SwitchWard(int wardId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if user has access to the ward
                var userWards = await _userService.GetUserWardsAsync(userId);
                var hasAccess = userWards.Any(uw => uw.WardId == wardId);

                if (!hasAccess)
                {
                    TempData["ErrorMessage"] = "You don't have access to this ward.";
                    return RedirectToAction(nameof(Index));
                }

                var wardData = await _adminRepository.GetWardDashboardDataAsync(wardId);

                // Get ward details
                var ward = userWards.FirstOrDefault(uw => uw.WardId == wardId)?.Ward;
                ViewBag.CurrentWard = ward;
                ViewBag.CurrentWardId = wardId;

                // Set up ward dropdown
                ViewBag.Wards = userWards.Select(uw => new SelectListItem
                {
                    Value = uw.WardId.ToString(),
                    Text = uw.Ward?.WardName ?? "Unknown Ward",
                    Selected = uw.WardId == wardId
                }).ToList();

                return View("Index", wardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error switching to ward {wardId}");
                TempData["ErrorMessage"] = "An error occurred while switching wards.";
                return RedirectToAction(nameof(Index));
            }
        }
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst("FullName").Value);
        }

        [HttpGet]
        public IActionResult AdmitPatient()
        {
            ViewBag.CurrentPhase = 1;
            PopulateViewBags();
            return View(new PatientAdmissionViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AdmitPatient(PatientAdmissionViewModel model, string action)
        {
            switch (action)
            {
                case "search":
                    return await SearchPatient(model.UserId);
                case "backToPhase1":
                    ViewBag.CurrentPhase = 1;
                    break;
                case "nextToPhase3":
                    ViewBag.CurrentPhase = 3;
                    TempData["PatientInfo"] = JsonSerializer.Serialize(new
                    {
                        model.UserId,
                        model.PatientName,
                        model.PatientSurname
                    });
                    break;
                case "backToPhase2":
                    ViewBag.CurrentPhase = 2;
                    break;
                case "admit":
                    if (TempData["PatientInfo"] is string patientInfo)
                    {
                        var info = JsonSerializer.Deserialize<PatientAdmissionViewModel>(patientInfo);
                        model.UserId = info.UserId;
                        model.PatientName = info.PatientName;
                        model.PatientSurname = info.PatientSurname;
                    }
                    return await AdmitPatientFinal(model);
                default:
                    ViewBag.CurrentPhase = 1;
                    break;
            }
            PopulateViewBags();
            return View(model);
        }

       

        private async Task<IActionResult> SearchPatient(int userId)
        {
            var user = await _adminRepository.SearchPatientAsync(userId);
            if (user != null)
            {
                var model = new PatientAdmissionViewModel
                {
                    UserId = user.UserId,
                    PatientName = user.Name,
                    PatientSurname = user.Surname,
                };
                ViewBag.CurrentPhase = 2;
                ViewBag.SearchResult = true;
                PopulateViewBags();
                return View("AdmitPatient", model);
            }
            else
            {
                ViewBag.CurrentPhase = 1;
                ViewBag.SearchResult = false;
                PopulateViewBags();
                return View("AdmitPatient", new PatientAdmissionViewModel { UserId = userId });
            }
        }

        private async Task<IActionResult> AdmitPatientFinal(PatientAdmissionViewModel model)
        {
            if (TempData["PatientInfo"] is string patientInfo)
            {
                var info = JsonSerializer.Deserialize<PatientAdmissionViewModel>(patientInfo);
                model.UserId = info.UserId;
                model.PatientName = info.PatientName;
                model.PatientSurname = info.PatientSurname;

                ModelState.Remove("PatientName");
                ModelState.Remove("PatientSurname");
            }

            FilterModelStateByPhase(3);

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["ValidationErrors"] = errorMessages;

                ViewBag.CurrentPhase = 3;
                PopulateViewBags();
                return View("AdmitPatient", model);
            }

            var patient = new Patient
            {
                UserId = model.UserId,
                PatientType = model.PatientType,
                ConditionId = model.ConditionId,
                MedicationId = model.MedicationId,
                AllergyId = model.AllergyId
            };

            try
            {
                await _adminRepository.AdmitPatientAsync(patient);
                TempData["SuccessMessage"] = "Patient admitted successfully.";
                return RedirectToAction(nameof(AdmitPatient));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while admitting the patient: {ex.Message}";
                ViewBag.CurrentPhase = 3;
                PopulateViewBags();
                return View("AdmitPatient", model);
            }
        }

        private void PopulateViewBags()
        {
            ViewBag.Conditions = new SelectList(_adminRepository.GetConditions(), "ConditionId", "Name");
            ViewBag.Medications = new SelectList(_adminRepository.GetMedications(), "MedicationId", "Name");
            ViewBag.Allergies = new SelectList(_adminRepository.GetAllergies(), "AllergyId", "Name");
        }

        private void FilterModelStateByPhase(int phase)
        {
            var keysToRemove = new List<string>();

            foreach (var key in ModelState.Keys)
            {
                bool keepKey = phase switch
                {
                    1 => key == "UserId",
                    2 => new[] { "UserId", "PatientName", "PatientSurname" }.Contains(key),
                    3 => true,
                    _ => false
                };

                if (!keepKey)
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProfilePhoto(int userId)
        {
            var user = await _adminRepository.SearchPatientAsync(userId);
            if (user != null && user.ProfilePhoto != null)
            {
                return File(user.ProfilePhoto, "image/jpeg");
            }
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> PatientList(int page = 1, string searchTerm = null)
        {
            try
            {
                _logger.LogInformation($"PatientList GET called with page: {page}, searchPatient: {searchTerm}");
                var patients = await _adminRepository.GetAllPatientsAsync(searchTerm, null);
                _logger.LogInformation($"Retrieved {patients.Count()} patients from repository");

                var PageSize = 5;
                var paginatedPatients = patients
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                _logger.LogInformation($"Paginated to {paginatedPatients.Count} patients");

                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling(patients.Count() / (double)PageSize);
                ViewBag.TotalItems = patients.Count();

                return View(paginatedPatients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in PatientList GET action");
                TempData["ErrorMessage"] = "An error occurred while loading patients. Please try again.";
                return View(new List<PatientListViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> PatientList(string searchTerm, int page = 1)
        {
            try
            {
                var patients = await _adminRepository.GetAllPatientsAsync(searchTerm, null);
                var PageSize = 5;
                if (patients == null || !patients.Any())
                {
                    TempData["msg"] = "Patient Not Found";
                    patients = await _adminRepository.GetAllPatientsAsync(null);
                }

                var paginatedPatients = patients
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling(patients.Count() / (double)PageSize);
                ViewBag.TotalItems = patients.Count();

                return View(paginatedPatients);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while loading patients");
                TempData["ErrorMessage"] = "An error occurred while loading patients. Please try again.";
                return View(new List<PatientListViewModel>());
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditPatient()
        {
           

            return View();
        }
        public async Task<IActionResult> EditPatient(int patientFileId)
        {
            var patient = await _adminRepository.GetPatientByPatientFileIdAsync(patientFileId);

            if (patient == null)
            {
                // Handle patient not found
                return NotFound();
            }

            var conditions = await _adminRepository.GetAllConditionsAsync();

            var viewModel = new EditPatientViewModel
            {
                PatientFileId = patient.PatientFileId,
                Name = patient.Name,
                Surname = patient.Surname,
                PatientType = patient.PatientType,
                ConditionId = patient.ConditionId,
                ProfilePhoto = patient.ProfilePhoto,
                IsActive = patient.Status == "Active", 
                Conditions = conditions.ToList() 
            };

            return View(viewModel);
        }



        [HttpGet]
        public IActionResult AssignDoctor()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SearchPatientDoctor(int patientFileId)
        {
            try
            {
                _logger.LogInformation($"Searching for PatientFileId: {patientFileId}");

                var patientFile = await _adminRepository.GetPatientFileDetailsAsync(patientFileId);
                if (patientFile == null)
                {
                    _logger.LogWarning($"Patient file not found for PatientFileId: {patientFileId}");
                    TempData["Error"] = "Patient file not found";
                    return View("AssignDoctor", new AssignDoctorViewModel());
                }

                var doctors = await _adminRepository.GetAvailableDoctorsForWardAsync(patientFile.WardId);
                var viewModel = new AssignDoctorViewModel
                {
                    PatientFile = patientFile,
                    AvailableDoctors = doctors.Select(d => new SelectListItem
                    {
                        Value = d.UserId.ToString(),
                        Text = d.FullName
                    }).ToList()
                };

                return View("AssignDoctor", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching for PatientFileId: {patientFileId}");
                TempData["Error"] = "An error occurred while searching for the patient file";
                return View("AssignDoctor", new AssignDoctorViewModel());
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> AssignDoctor(int patientFileId, int userId)
        {
            try
            {
                _logger.LogInformation($"Assigning doctor (UserId: {userId}) to PatientFileId: {patientFileId}");

                await _adminRepository.AssignDoctorToPatientFileAsync(patientFileId, userId);

                TempData["Success"] = "Doctor assigned successfully";
                return RedirectToAction(nameof(AssignDoctor));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning doctor (UserId: {userId}) to PatientFileId: {patientFileId}");
                TempData["Error"] = "Failed to assign doctor. Please ensure doctor and patient are in the same ward.";

                // Reload the patient details
                var patientFile = await _adminRepository.GetPatientFileDetailsAsync(patientFileId);
                var doctors = await _adminRepository.GetAvailableDoctorsForWardAsync(patientFile.WardId);
                var viewModel = new AssignDoctorViewModel
                {
                    PatientFile = patientFile,
                    AvailableDoctors = doctors.Select(d => new SelectListItem
                    {
                        Value = d.UserId.ToString(),
                        Text = d.FullName
                    }).ToList()
                };

                return View("PatientAssignment", viewModel);
            }
        }

        [HttpGet]
        public IActionResult AssignRoomAndBed()
        {
            var model = new AssignRoomAndBedViewModel
            {
                Request = new AssignRoomAndBedRequest { AdmissionDate = DateTime.Now },
                Rooms = new List<RoomViewModel>(),
                Beds = new List<BedViewModel>()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SearchPatientward(int userId)
        {
            _logger.LogInformation($"SearchPatientward called with userId: {userId}");

            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "Please enter a valid Patient ID.";
                return RedirectToAction(nameof(AssignRoomAndBed));
            }

            var patientDetails = await _adminRepository.GetPatientDetailsAsync(userId);
            if (patientDetails == null)
            {
                _logger.LogWarning($"Patient not found for userId: {userId}");
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction(nameof(AssignRoomAndBed));
            }

            var rooms = await _adminRepository.GetRoomsByWardIdAsync(patientDetails.WardId);
            var model = new AssignRoomAndBedViewModel
            {
                Request = new AssignRoomAndBedRequest
                {
                    UserId = userId, 
                    PatientName = patientDetails.PatientName,
                    PatientSurname = patientDetails.PatientSurname,
                    WardId = patientDetails.WardId,
                    WardName = patientDetails.WardName,
                    AdmissionDate = DateTime.Now
                },
                Rooms = rooms.ToList(),
                Beds = new List<BedViewModel>()
            };

            return View(nameof(AssignRoomAndBed), model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRoomAndBed(AssignRoomAndBedViewModel model)
        {
            _logger.LogInformation($"AssignRoomAndBed POST called with UserId: {model.Request?.UserId}");

            if (model.Request?.UserId <= 0)
            {
                ModelState.AddModelError("", "Invalid User ID. Please search for a patient first.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning($"Validation failed for {state.Key}: {error.ErrorMessage}");
                    }
                }

                // Repopulate rooms and beds
                if (model.Request?.WardId > 0)
                {
                    model.Rooms = (await _adminRepository.GetRoomsByWardIdAsync(model.Request.WardId)).ToList();
                }

                if (model.Request?.RoomNumber > 0)
                {
                    model.Beds = (await _adminRepository.GetAvailableBedsByRoomIdAsync(model.Request.RoomNumber)).ToList();
                }

                return View(model);
            }

            try
            {
                await _adminRepository.AssignRoomAndBedAsync(model.Request);
                TempData["SuccessMessage"] = "Room and bed assigned successfully.";
                return RedirectToAction(nameof(AssignRoomAndBed));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning room and bed");
                ModelState.AddModelError("", ex.Message);

                // Repopulate rooms and beds
                model.Rooms = (await _adminRepository.GetRoomsByWardIdAsync(model.Request.WardId)).ToList();
                model.Beds = (await _adminRepository.GetAvailableBedsByRoomIdAsync(model.Request.RoomNumber)).ToList();

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchPatientId(int userId)
        {
            var patient = await _adminRepository.SearchPatientAsync(userId);
            if (patient != null)
            {
                return Json(new { exists = true, name = patient.Name, surname = patient.Surname });
            }
            return Json(new { exists = false });
        }

        [HttpGet]
        public async Task<IActionResult> Movement(int? roomId, int? patientFileId)
        {
            if (roomId.HasValue)
            {
                var patients = await _adminRepository.GetPatientsByRoomAsync(roomId.Value);
                return View(patients);
            }
            else if (patientFileId.HasValue)
            {
                var patient = await _adminRepository.GetPatientByFileIdAsync(patientFileId.Value);
                return View(new List<PatientFileViewModel> { patient });
            }
            else
            {
                return View(new List<PatientFileViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Movement(int patientFileId, string status, int roomId)
        {
            var result = await _adminRepository.UpdatePatientMovementStatusAsync(patientFileId, status);
            TempData["Message"] = $"Movement status updated successfully for {result.Name} {result.Surname}.";
            return RedirectToAction("Movement", new { roomId });
        }
        public async Task<IActionResult> Discharge()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(int patientFileId)
        {
            try
            {
                var patient = await _adminRepository.SearchPatientForDischargeAsync(patientFileId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found or already discharged.";
                    return RedirectToAction("Discharge");
                }
                return View("Discharge", patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for patient with ID: {PatientFileId}", patientFileId);
                TempData["ErrorMessage"] = "An error occurred while searching for the patient.";
                return RedirectToAction("Discharge");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Discharge(PatientDischargeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model validation failed for patient discharge. UserId: {UserId}", model.UserId);
                    var patient = await _adminRepository.SearchPatientForDischargeAsync(model.PatientFileId);
                    if (patient != null)
                    {
                        // Copy the submitted values back to the model
                        patient.DischargeSummary = model.DischargeSummary;
                        patient.TreatmentProvided = model.TreatmentProvided;
                        patient.VitalSignsAtDischarge = model.VitalSignsAtDischarge;
                        patient.DoctorFee = model.DoctorFee;
                        patient.MedicationFee = model.MedicationFee;
                        patient.RoomPerDay = model.RoomPerDay;
                        patient.OtherCosts = model.OtherCosts;
                        patient.Total = model.Total;
                        patient.DischargeDate = model.DischargeDate;
                    }
                    return View(patient ?? model);
                }

                _logger.LogInformation("Starting discharge process for UserId: {UserId}", model.UserId);

                // Validate required data and amounts
                if (!model.DischargeDate.HasValue)
                {
                    ModelState.AddModelError("DischargeDate", "Discharge date is required");
                    return View(model);
                }

                if (model.DoctorFee < 0 || model.MedicationFee < 0 || model.RoomPerDay < 0 || model.OtherCosts < 0)
                {
                    ModelState.AddModelError("", "All fees must be non-negative values");
                    return View(model);
                }

                if (model.Total <= 0)
                {
                    ModelState.AddModelError("Total", "Total amount must be greater than zero");
                    return View(model);
                }

                // Calculate and validate the room fee based on length of stay
                var roomFee = model.LengthOfStay * model.RoomPerDay;

                // Calculate and validate the expected total
                decimal expectedTotal = roomFee + model.DoctorFee + model.MedicationFee + model.OtherCosts;

                // Allow for small floating-point differences (e.g., 0.01)
                if (Math.Abs(expectedTotal - model.Total) > 0.01m)
                {
                    _logger.LogWarning("Total amount mismatch. Expected: {Expected}, Actual: {Actual}, UserId: {UserId}",
                        expectedTotal, model.Total, model.UserId);
                    ModelState.AddModelError("Total", "Total amount calculation mismatch. Please recalculate the total.");
                    return View(model);
                }

                // Create the discharge report
                var dischargeReport = new DischargeReport
                {
                    PatientFileId = model.PatientFileId,
                    DischargeDate = model.DischargeDate.Value,
                    TotalBill = model.Total,
                    DischargeSummary = model.DischargeSummary?.Trim() ?? string.Empty,
                    TreatmentProvided = model.TreatmentProvided?.Trim() ?? string.Empty,
                    MedicationPrescribed = model.MedicationName?.Trim() ?? string.Empty,
                    DoctorFee = model.DoctorFee,
                    MedicationFee = model.MedicationFee,
                    RoomFee = roomFee,
                    OtherCosts = model.OtherCosts,
                    LengthOfStay = model.LengthOfStay,
                    VitalSignsAtDischarge = model.VitalSignsAtDischarge?.Trim() ?? string.Empty
                };

                // Create the discharge report first
                int reportId;
                try
                {
                    _logger.LogInformation("Creating discharge report for PatientFileId: {PatientFileId}, Total: {Total}",
                        model.PatientFileId, model.Total);

                    reportId = await _adminRepository.CreateDischargeReportAsync(dischargeReport);

                    if (reportId <= 0)
                    {
                        throw new Exception("Failed to create discharge report - invalid report ID returned");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create discharge report. PatientFileId: {PatientFileId}", model.PatientFileId);
                    ModelState.AddModelError("", "Failed to create discharge report. Please try again.");
                    return View(model);
                }

                // Then discharge the patient
                try
                {
                    _logger.LogInformation("Attempting to discharge patient. UserId: {UserId}, DischargeDate: {DischargeDate}, Total: {Total}",
                        model.UserId, model.DischargeDate, model.Total);

                    var success = await _adminRepository.DischargePatientAsync(
                        model.UserId,
                        model.DischargeDate.Value,
                        model.Total);

                    if (!success)
                    {
                        _logger.LogError("Failed to discharge patient. UserId: {UserId}", model.UserId);
                        ModelState.AddModelError("", "Failed to discharge the patient. Please try again.");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to discharge patient. UserId: {UserId}", model.UserId);
                    ModelState.AddModelError("", "Failed to discharge the patient. Please try again.");
                    return View(model);
                }

                _logger.LogInformation("Patient successfully discharged. UserId: {UserId}, ReportId: {ReportId}",
                    model.UserId, reportId);

                return RedirectToAction("DischargeSuccess", new { id = model.PatientFileId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during patient discharge process. UserId: {UserId}", model.UserId);
                ModelState.AddModelError("", "An unexpected error occurred during discharge. Please try again.");
                return View(model);
            }
        }

        public IActionResult DischargeSuccess(int id)
        {
            return View(id);
        }
        public IActionResult DischargeReport()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DischargeReport(int patientFileId)
        {
            try
            {
                var report = await _adminRepository.GetDischargeReportDetailsAsync(patientFileId);
                if (report != null)
                {
                    // Instead of using ViewBag, redirect directly to the ViewDischargeReport action
                    return RedirectToAction("ViewDischargeReport", new { patientFileId = patientFileId });
                }
                else
                {
                    ViewBag.ReportExists = false;
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for discharge report");
                TempData["Error"] = "An error occurred while searching for the report.";
                return RedirectToAction("DischargeReport");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewDischargeReport(int patientFileId)
        {
            try
            {
                var report = await _adminRepository.GetDischargeReportDetailsAsync(patientFileId);
                if (report == null)
                {
                    TempData["Error"] = "Discharge report not found.";
                    return RedirectToAction("DischargeReport");
                }

                // Make sure we're returning the correct view with the correct model
                return View("ViewDischargeReport", report);  // Note the view name change
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving discharge report");
                TempData["Error"] = "An error occurred while retrieving the report.";
                return RedirectToAction("DischargeReport");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadDischargeReport(int patientFileId)
        {
            try
            {
                var report = await _adminRepository.GetDischargeReportDetailsAsync(patientFileId);
                if (report == null)
                {
                    TempData["Error"] = "Discharge report not found.";
                    return RedirectToAction("DischargeReport");
                }

                // Use the ViewRenderService to convert the view to HTML
                var htmlContent = await _viewRenderService.RenderToStringAsync("DischargeReport", report);

                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = DinkToPdf.PaperKind.A4,
                    Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                    DocumentTitle = $"Discharge Report - {report.PatientFileId}"
                };

                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8" },
                    HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                    FooterSettings = { FontSize = 9, Line = true, Center = "Discharge Report" }
                };

                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };

                var file = _converter.Convert(pdf);
                return File(file, "application/pdf", $"DischargeReport_{patientFileId}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading discharge report");
                TempData["Error"] = "An error occurred while downloading the report.";
                return RedirectToAction("DischargeReport");
            }
        }

    }

}





