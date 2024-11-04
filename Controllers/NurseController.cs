using CareConnect.Models;
using Connect.Models;
using Connect.Repositories.Account;
using Connect.Repositories.Nurse;
using Microsoft.AspNetCore.Mvc;

namespace Connect.Controllers
{
    public class NurseController : BaseController
    {

        private readonly INurseRepository _nurseRepository;
        private readonly IUserRepository _userService;

        public NurseController(INurseRepository nurseRepository, IUserRepository userService)
        {
            _nurseRepository = nurseRepository;
            _userService = userService;
        }

        // GET: Dashboard with dummy data
        public IActionResult Index()
        {
            var viewModel = new NurseDashboardViewModel
            {
                TotalPatients = 0,
                PendingVitals = 0,
                MedicationsDue = 0,
                DoctorRequests = 0,
                PatientDistribution = new List<PatientDistributionData>(),
                UpcomingTasks = new List<UpcomingTask>() // Dummy data for testing
            };

            return View(viewModel);
        }

        // GET: List of patients assigned to the nurse
        public async Task<IActionResult> PatientList()
        {
            try
            {
                var patients = await _nurseRepository.GetPatientsAsync();
                return View(patients);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error fetching patient list: {ex.Message}");
                return View(new List<Patient>()); // Return an empty list in case of error
            }
        }

        // GET: Details of a single patient
        public async Task<IActionResult> PatientDetails(int id)
        {
            try
            {
                var patientFile = await _nurseRepository.GetPatientByIdAsync(id);
                if (patientFile
                        == null)
                {
                    return NotFound();
                }
                return View(patientFile);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error fetching patient details: {ex.Message}");
                return View(); // Optionally, return an error view
            }
        }

        [HttpGet]
        public IActionResult RecordVitals()
        {
            return View(new RecordVitalsViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> SearchPatient(int patientFileId)
        {
            var patient = await _nurseRepository.GetPatientByFileIdAsync(patientFileId);
            if (patient == null)
            {
                ModelState.AddModelError("", "Patient not found.");
                return View("RecordVitals", new RecordVitalsViewModel());
            }

            var model = new RecordVitalsViewModel
            {
                PatientId = patient.UserId,
                PatientName = patient.Name,
                PatientSurname = patient.Surname,
                PatientFileId = patientFileId
            };

            return View("RecordVitals", model);
        }

        [HttpPost]
        public async Task<IActionResult> RecordVitals(RecordVitalsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _nurseRepository.RecordVitalsAsync(model);
                    TempData["SuccessMessage"] = "Vitals recorded successfully.";
                    return RedirectToAction(nameof(RecordVitals));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error recording vitals: {ex.Message}");
                }
            }
            return View(model);
        }

        // GET: Administer medication form
        public IActionResult MedicationDispense(int patientFileId)
        {
            var model = new MedicationDispenseViewModel { PatientFileId = patientFileId };
            return View(model);
        }

        // POST: Administer medication
        [HttpPost]
        public async Task<IActionResult> MedicationDispense(MedicationDispenseViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _nurseRepository.AdministerMedicationAsync(model);
                    return RedirectToAction("PatientDetails", new { id = model.PatientFileId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error dispensing medication: {ex.Message}");
                }
            }
            return View(model);
        }

        // GET: Treat patient form
        public IActionResult TreatPatient(int patientFileId)
        {
            var model = new TreatPatientViewModel { PatientFileId = patientFileId };
            return View(model);
        }

        // POST: Save treatment details
        [HttpPost]
        public async Task<IActionResult> TreatPatient(TreatPatientViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _nurseRepository.TreatPatientAsync(model);
                    return RedirectToAction("PatientDetails", new { id = model.PatientFileId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error treating patient: {ex.Message}");
                }
            }
            return View(model);
        }
    }
}
