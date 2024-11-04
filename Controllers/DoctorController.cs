using Connect.Data;
using Connect.Models;
using Connect.Repositories.Doctor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Connect.Controllers
{

    public class DoctorController : BaseController
    {
        private readonly CareConnectDbContext _context;
        private readonly IDoctorRepository _doctorRepository;
        private readonly ILogger<DoctorController> _logger;


        public DoctorController(CareConnectDbContext context, IDoctorRepository doctorRepository, ILogger<DoctorController> logger)
        {
            _context = context;
            _doctorRepository = doctorRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string patient = null)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account");
                }

                var doctorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var dashboardData = await _doctorRepository.GetDashboardAsync(doctorUserId);
                var patientListData = await _doctorRepository.GetAllPatientsAsync(patient, doctorUserId);

                if (patientListData != null && patientListData.Any())
                {
                    ViewBag.PatientLabels = patientListData.Select(p => $"{p.Surname} {p.Name}").ToList();
                    ViewBag.PatientCounts = patientListData.Select(p => 1).ToList();
                    ViewBag.PatientConditions = patientListData.Select(p => p.ConditionName).ToList();
                }
                else
                {
                    ViewBag.PatientLabels = new List<string>();
                    ViewBag.PatientCounts = new List<int>();
                    ViewBag.PatientConditions = new List<string>();
                }

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action for doctor dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard. Please try again.";
                return View(new DoctorDashboardViewModel());
            }
        }

        public async Task<IActionResult> PatientList(int page = 1, string searchTerm = null)
        {
            try
            {
                _logger.LogInformation($"PatientList GET called with page: {page}, searchTerm: {searchTerm}");
                var patients = await _doctorRepository.GetDoctorPatientsAsync(searchTerm, null);
                _logger.LogInformation($"Retrieved {patients.Count()} patients from repository");

                var PageSize = 5;
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
                _logger.LogError(ex, "Error occurred in PatientList GET action");
                TempData["ErrorMessage"] = "An error occurred while loading patients. Please try again.";
                return View(new List<PatientListViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> PatientList(string searchPatient, int page = 1)
        {
            try
            {
                var patients = await _doctorRepository.GetDoctorPatientsAsync(searchPatient, null);
                var PageSize = 5;
                if (patients == null || !patients.Any())
                {
                    TempData["msg"] = "Patient Not Found";
                    patients = await _doctorRepository.GetDoctorPatientsAsync(null);
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
        // Schedules
        public async Task<IActionResult> Schedule(string searchTerm)
        {
            try
            {
                // Get the logged-in doctor's UserId from the claim
                var doctorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Check if searchTerm is null or whitespace and pass it to the repository method
                var appointments = await _doctorRepository.GetAllScheduleAsync(
                    string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                    doctorUserId
                );

                // Ensure we never return null to the view
                var viewModel = appointments ?? new List<AppointmentViewModel>();

                // Return the appointments to the view
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Handle exceptions (log the error and return an error message)
                _logger.LogError(ex, "Error occurred while retrieving appointments");
                TempData["ErrorMessage"] = "An error occurred while retrieving appointments. Please try again.";
                return View(new List<AppointmentViewModel>());
            }
        }

        public async Task<IActionResult> ScheduleDetails(int id)
        {
            var schedule = await _doctorRepository.GetScheduleByIdAsync(id);
            if (schedule == null) return NotFound();
            return View(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> SearchPatientFileAppointment(int patientFileId)
        {
            try
            {
                var patient = await _doctorRepository.GetPatientFilee(patientFileId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient file not found.";
                    return View("ScheduleAssign", new AppointmentViewModel());
                }

                var viewModel = new AppointmentViewModel
                {
                    PatientFileId = patient.PatientFileId,
                    Name = $"{patient.Name}",
                    Date = DateTime.Today,
                    Status = "Scheduled"
                };

                PopulateStatusDropdown();
                return View("ScheduleAssign", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for patient file ID: {PatientFileId}", patientFileId);
                TempData["ErrorMessage"] = "An error occurred while searching for the patient.";
                return View("ScheduleAssign", new AppointmentViewModel());
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int patientFileId,string status)
        {
            try
            {
                // Call repository to update the status for the appointment
                await _doctorRepository.UpdateAppointmentStatusAsync(patientFileId, status);

                TempData["SuccessMessage"] = "Appointment status updated successfully.";
                return RedirectToAction(nameof(Schedule)); // Redirect back to the schedule page or another relevant page
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment status for patient file ID: {patientFileId}", patientFileId);
                ModelState.AddModelError("", "An error occurred while updating the status.");
                return View(); // Return the appropriate view
            }
        }
    
        public async Task<IActionResult> ScheduleAssign()
        {
            PopulateStatusDropdown(); // You'll need to implement this
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleAssign(AppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateStatusDropdown();
                return View(model);
            }

            try
            {
                // Add validation for date and time
                if (model.Date.Date < DateTime.Today)
                {
                    ModelState.AddModelError("Date", "Appointment date cannot be in the past.");
                    PopulateStatusDropdown();
                    return View(model);
                }

                // Check for existing appointments at the same time
                var isTimeSlotTaken = await _doctorRepository.GetAppointmentByDateTime(model.Date, model.Time);
                if (isTimeSlotTaken)
                {
                    ModelState.AddModelError("Time", "This time slot is already booked.");
                    PopulateStatusDropdown();
                    return View(model);
                }

                await _doctorRepository.AddScheduleAsync(model);
                
                TempData["SuccessMessage"] = "Appointment scheduled successfully.";
                return RedirectToAction(nameof(Schedule));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment for patient file ID: {PatientFileId}", model.PatientFileId);
                ModelState.AddModelError("", "An error occurred while scheduling the appointment.");
                PopulateStatusDropdown();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ScheduleEdit(int id)
        {
            var schedule = await _doctorRepository.GetScheduleByIdAsync(id);
            if (schedule == null) return NotFound();

            var viewModel = new AppointmentViewModel
            {
                AppointmentId = schedule.AppointmentId,
                PatientFileId = schedule.PatientFileId,
                Name = schedule.Name,
                Date = schedule.Date,
                Time = schedule.Time,
                Reason = schedule.Reason,
                Status = schedule.Status
            };

            PopulateStatusDropdown();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleEdit(AppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateStatusDropdown();
                return View(model);
            }

            try
            {
                // Add validation for date and time
                if (model.Date.Date < DateTime.Today)
                {
                    ModelState.AddModelError("Date", "Appointment date cannot be in the past.");
                    PopulateStatusDropdown();
                    return View(model);
                }

                // Check for existing appointments at the same time (excluding current appointment)
                var existingAppointment = await _doctorRepository.GetAppointmentByDateTime(model.Date, model.Time, model.AppointmentId);
                if (existingAppointment != null)
                {
                    ModelState.AddModelError("Time", "This time slot is already booked.");
                    PopulateStatusDropdown();
                    return View(model);
                }

                await _doctorRepository.UpdateScheduleAsync(model);
                TempData["SuccessMessage"] = "Appointment updated successfully.";
                return RedirectToAction(nameof(Schedule));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment ID: {AppointmentId}", model.AppointmentId);
                ModelState.AddModelError("", "An error occurred while updating the appointment.");
                PopulateStatusDropdown();
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleDelete(int id)
        {
            try
            {
                await _doctorRepository.SoftDeleteScheduleAsync(id);
                TempData["SuccessMessage"] = "Appointment deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment ID: {AppointmentId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the appointment.";
            }
            return RedirectToAction(nameof(Schedule));
        }

        private void PopulateStatusDropdown()
        {
            ViewBag.Status = new SelectList(new[]
            {
            new { Value = "Scheduled", Text = "Scheduled" },
            new { Value = "Cancelled", Text = "Cancelled" }
        }, "Value", "Text");
        }

        // Instructions
        public async Task<IActionResult> Instructions(int? doctorUserId)
        { 
            var instructions = await _doctorRepository.GetAllInstructionAsync(doctorUserId);
            return View(instructions);
        }

        public async Task<IActionResult> InstructionDetails(int id)
        {
            var instruction = await _doctorRepository.GetInstructionByIdAsync(id);
            if (instruction == null) return NotFound();

            return View(instruction);
        }

        [HttpPost]
        public async Task<IActionResult> SearchPatientFileAssign(int patientFileId)
        {
            try
            {
                await PopulateViewBagLists();
                var patient = await _doctorRepository.GetSchedulePatientFileDetailsAssign(patientFileId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient file not found.";
                    return View("InstructionAssign", new InstructionViewModel());
                }

                return View("InstructionAssign", patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for patient file.");
                TempData["ErrorMessage"] = "An error occurred while searching for the patient.";
                return View("InstructionAssign", new InstructionViewModel());
            }
        }

        public async Task<IActionResult> InstructionAssign()
        {
            await PopulateViewBagLists();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InstructionAssign(InstructionViewModel instruction)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await PopulateViewBagLists();
                    await _doctorRepository.AddInstructionAsync(instruction);
                    return RedirectToAction(nameof(Instructions));
                }
                catch (SqlException ex) when (ex.Number == 547)
                {
                    ModelState.AddModelError("PatientFileId", "Invalid Patient File ID. Please select a valid patient.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while saving the instruction. Please try again.");
                }
            }
            await PopulateViewBagLists();
            return View(instruction);
        }

        [HttpGet]
        public async Task<IActionResult> InstructionEdit(int id)
        {
            var instruction = await _doctorRepository.GetInstructionByIdAsync(id);
            if (instruction == null) return NotFound();
            await PopulateViewBagLists();
            return View(instruction);
        }

        [HttpPost]
        public async Task<IActionResult> InstructionEdit(InstructionViewModel instruction)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _doctorRepository.UpdateInstructionAsync(instruction);
                    TempData["SuccessMessage"] = "Instruction updated successfully.";
                    return RedirectToAction(nameof(Instructions));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the instruction.");
                }
            }
            await PopulateViewBagLists();
            return View(instruction);
        }

        private async Task PopulateViewBagLists()
        {
            // Get doctors list
            var doctors = await _doctorRepository.GetAllDoctorsNameAsync();
            ViewBag.DoctorList = new SelectList(doctors, "UserId", "DoctorName");

            // Populate Importance list
            ViewBag.Importance = new SelectList(new[]
            {
            new { Value = "Low", Text = "Low" },
            new { Value = "Medium", Text = "Medium" },
            new { Value = "High", Text = "High" },
            new { Value = "Critical", Text = "Critical" }
        }, "Value", "Text");
        }

        //[HttpGet]
        //public async Task<IActionResult> InstructionDelete(int id)
        //{
        //    var instruction = await _doctorRepository.GetInstructionByIdAsync(id);
        //    if (instruction == null)
        //        return NotFound();

        //    return View(instruction);
        //}

        //[HttpPost]
        //public async Task<IActionResult> InstructionDeleteConfirmed(int id)
        //{
        //    await _doctorRepository.SoftDeleteInstructionAsync(id);
        //    return RedirectToAction(nameof(Instructions)); // Ensure 'Instructions' action exists
        //}
        [HttpGet]
        public async Task<IActionResult> InstructionDeletee(int id)
        {
            var instruction = await _doctorRepository.GetInstructionByIdAsync(id);
            if (instruction == null)
            {
                return NotFound();
            }
            return View(instruction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InstructionDelete(int id)
        {
            try
            {
                await _doctorRepository.SoftDeleteInstructionAsync(id);
                return RedirectToAction(nameof(Instructions));
            }
            catch (Exception ex)
            {
                // Log the error
                return RedirectToAction(nameof(Instructions));
            }
        }


        // Prescriptions
        public async Task<IActionResult> Prescription(int? doctorUserId)
        {
            var patientDetails = await _doctorRepository.GetAllPrescriptionsAsync(doctorUserId);
            return View(patientDetails);
        }
        public async Task<IActionResult> PrescriptionDetails(int id)
        {
            var prescription = await _doctorRepository.GetPrescriptionByIdAsync(id);
            if (prescription == null) return NotFound();

            return View(prescription);
        }
        [HttpPost]
        public async Task<IActionResult> SearchPatientFilesAssign(int patientFileId) // Changed from patientId
        {
            try
            {
                var patient = await _doctorRepository.GetPatientFileDetailAssign(patientFileId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient file not found.";
                    return View("PrescriptionAssign", new PrescriptionViewModel());
                }

                // Return the complete patient object instead of creating a new one
                return View("PrescriptionAssign", patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for patient file.");
                TempData["ErrorMessage"] = "An error occurred while searching for the patient.";
                return View("PrescriptionAssign", new PrescriptionViewModel());
            }
        }
        [HttpPost]
        public async Task<IActionResult> SearchPatientFiles(int patientFileId) //Edit the prescription// Changed from patientId
        {
            try
            {
                var patient = await _doctorRepository.GetPatientFileDetails(patientFileId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient file not found.";
                    return View("PrescriptionAssign", new PrescriptionViewModel());
                }

                // Return the complete patient object instead of creating a new one
                return View("Prescription", patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for patient file.");
                TempData["ErrorMessage"] = "An error occurred while searching for the patient.";
                return View("PrescriptionAssign", new PrescriptionViewModel());
            }
        }
        [HttpGet]
        public async Task<IActionResult> PrescriptionAssign()
        {
            var medications = await _doctorRepository.GetAllMedicationsAsync();
           // ViewBag.Medication = new SelectList(medications, "MedicationId", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PrescriptionAssign(PrescriptionViewModel prescription)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _doctorRepository.AddPrescriptionAsync(prescription);
                    return RedirectToAction(nameof(Prescription));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while saving the prescription. Please try again.");
                }
            }
            //var medications = await _doctorRepository.GetAllMedicationsAsync();
            //ViewBag.Medication = new SelectList(medications, "MedicationId", "Name");
            return View(prescription);
        }

        [HttpGet]
        public async Task<IActionResult> PrescriptionEdit(int id)
        {
            var prescription = await _doctorRepository.GetPrescriptionByIdAsync(id);
            if (prescription == null) return NotFound();

            return View(prescription);
        }

        [HttpPost]
        public async Task<IActionResult> PrescriptionEdit(PrescriptionViewModel prescription)
        {
            if (ModelState.IsValid)
            {
                await _doctorRepository.UpdatePrescriptionAsync(prescription);
                return RedirectToAction(nameof(Prescription));
            }
            return View(prescription);
        }

        [HttpGet]
        public async Task<IActionResult> PrescriptionDeletee(int id)
        {
            var prescription = await _doctorRepository.GetPrescriptionByIdAsync(id);
            if (prescription == null) return NotFound();

            return View(prescription); // Show confirmation view
        }

        [HttpPost]
        public async Task<IActionResult> PrescriptionDelete(int id)
        {
            await _doctorRepository.SoftDeletePrescriptionAsync(id);
            return RedirectToAction(nameof(Prescription));
        }
        public async Task<IActionResult> ViewPatient(string searchTerm)
        {
            try
            {
                // Get the logged-in doctor's UserId from the claim
                var doctorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Check if searchTerm is null or whitespace and pass it to the repository method
                var patients = await _doctorRepository.GetAllPatientsAsync(
                    string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                    doctorUserId
                );

                // Return the patients to the view
                return View(patients);
            }
            catch (Exception ex)
            {
                // Handle exceptions (log the error and return an error message)
                _logger.LogError(ex, "Error occurred while retrieving patients");
                TempData["ErrorMessage"] = "An error occurred while retrieving patients. Please try again.";
                return View(new List<DoctorPatientViewModel>());
            }


        }

        [HttpGet]  // This handles the initial page load and GET searches
            public async Task<IActionResult> Report(string searchTerm = null)
            {
                try
                {
                    var doctorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                    // Get patients and prescriptions for the logged-in doctor
                    var patients = await _doctorRepository.GetAllPatientsAsync(searchTerm, doctorUserId);
                    var prescriptions = await _doctorRepository.GetAllPrescriptionsAsync(doctorUserId);

                    var reportData = new PatientReportViewModel
                    {
                        Patients = patients ?? new List<DoctorPatientViewModel>(),
                        Prescriptions = prescriptions ?? new List<PrescriptionViewModel>(),
                    
                    };

                    return View(reportData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while generating patient report");
                    TempData["ErrorMessage"] = "An error occurred while retrieving the report data. Please try again.";
                    return View(new PatientReportViewModel
                    {
                        Patients = new List<DoctorPatientViewModel>(),
                        Prescriptions = new List<PrescriptionViewModel>()
                    });
                }
            }
        }
}






