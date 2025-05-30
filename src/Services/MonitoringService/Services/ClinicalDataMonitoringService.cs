using MonitoringService.DTOs;
using MonitoringService.Models;
using Newtonsoft.Json;

namespace MonitoringService.Services;

public class ClinicalDataMonitoringService : IClinicalDataMonitoringService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAlertService _alertService;
    private readonly ILogger<ClinicalDataMonitoringService> _logger;
    private readonly IConfiguration _configuration;

    public ClinicalDataMonitoringService(
        IHttpClientFactory httpClientFactory,
        IAlertService alertService,
        ILogger<ClinicalDataMonitoringService> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _alertService = alertService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task MonitorAllPatientsAsync()
    {
        try
        {
            _logger.LogInformation("Starting monitoring cycle for all patients");

            // Get all patients from PatientManagementService
            var patients = await GetAllPatientsAsync();

            foreach (var patient in patients)
            {
                await MonitorPatientAsync(patient.Id);
                await Task.Delay(1000); // Small delay between patients to avoid overwhelming the system
            }

            _logger.LogInformation("Completed monitoring cycle for {PatientCount} patients", patients.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during monitoring cycle");
        }
    }

    public async Task MonitorPatientAsync(Guid patientId)
    {
        try
        {
            _logger.LogDebug("Monitoring patient: {PatientId}", patientId);

            // Get recent clinical records for the patient
            var recentEntries = await GetRecentClinicalEntriesAsync(patientId);

            foreach (var entry in recentEntries)
            {
                await AnalyzeClinicalEntry(entry);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while monitoring patient: {PatientId}", patientId);
        }
    }

    private async Task<IEnumerable<dynamic>> GetAllPatientsAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["Services:PatientManagementService:BaseUrl"];

            var response = await httpClient.GetAsync($"{baseUrl}/api/Patients");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<dynamic>>(content) ?? new List<dynamic>();
            }

            _logger.LogWarning("Failed to get patients from PatientManagementService. Status: {StatusCode}", response.StatusCode);
            return new List<dynamic>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting patients from PatientManagementService");
            return new List<dynamic>();
        }
    }

    private async Task<IEnumerable<dynamic>> GetRecentClinicalEntriesAsync(Guid patientId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["Services:ClinicalRecordService:BaseUrl"];

            var fromDate = DateTime.UtcNow.AddHours(-24); // Last 24 hours
            var toDate = DateTime.UtcNow;

            var response = await httpClient.GetAsync(
                $"{baseUrl}/api/ClinicalRecords/patient/{patientId}/daterange?fromDate={fromDate:yyyy-MM-ddTHH:mm:ss}&toDate={toDate:yyyy-MM-ddTHH:mm:ss}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<dynamic>>(content) ?? new List<dynamic>();
            }

            _logger.LogDebug("No recent clinical entries found for patient: {PatientId}", patientId);
            return new List<dynamic>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting clinical entries for patient: {PatientId}", patientId);
            return new List<dynamic>();
        }
    }

    private async Task AnalyzeClinicalEntry(dynamic entry)
    {
        try
        {
            var entryType = entry.entryType?.ToString();
            var patientId = Guid.Parse(entry.patientId.ToString());
            var entryDateTime = DateTime.Parse(entry.entryDateTime.ToString());
            var data = entry.data;

            // Skip if entry is older than 1 hour to avoid duplicate alerts
            if (DateTime.UtcNow.Subtract(entryDateTime).TotalHours > 1)
                return;

            switch (entryType?.ToLower())
            {
                case "vitalsign":
                    await AnalyzeVitalSigns(patientId, entryDateTime, data, entry.id.ToString());
                    break;
                case "symptom":
                    await AnalyzeSymptoms(patientId, entryDateTime, data, entry.id.ToString());
                    break;
                case "testresult":
                    await AnalyzeTestResults(patientId, entryDateTime, data, entry.id.ToString());
                    break;
                default:
                    // No specific monitoring rules for other entry types
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while analyzing clinical entry");
        }
    }

    private async Task AnalyzeVitalSigns(Guid patientId, DateTime entryDateTime, dynamic data, string clinicalEntryId)
    {
        try
        {
            if (data == null) return;

            var temperature = data.Temperature?.ToString();
            var bloodPressure = data.BloodPressure?.ToString();
            var heartRate = data.HeartRate?.ToString();

            // Check temperature
            if (double.TryParse(temperature, out double temp))
            {
                if (temp >= 38.0) // High fever
                {
                    await CreateAlertIfNotExists(patientId, entryDateTime, clinicalEntryId,
                        "High Temperature Alert",
                        $"Patient temperature ({temp}°C) indicates fever",
                        AlertSeverity.Critical);
                }
                else if (temp <= 35.0) // Hypothermia
                {
                    await CreateAlertIfNotExists(patientId, entryDateTime, clinicalEntryId,
                        "Low Temperature Alert",
                        $"Patient temperature ({temp}°C) is below normal range",
                        AlertSeverity.Warning);
                }
            }

            // Check blood pressure - YANAda YAXSHI VARIANT
            if (!string.IsNullOrEmpty(bloodPressure) && bloodPressure.Contains("/"))
            {
                var parts = bloodPressure.Split('/');
                if (parts.Length == 2)
                {
                    var systolicSuccess = int.TryParse(parts[0], out int systolic);
                    var diastolicSuccess = int.TryParse(parts[1], out int diastolic);

                    if (systolicSuccess && diastolicSuccess)
                    {
                        if (systolic >= 140 || diastolic >= 90) // Hypertension
                        {
                            await CreateAlertIfNotExists(patientId, entryDateTime, clinicalEntryId,
                                "High Blood Pressure Alert",
                                $"Patient blood pressure ({bloodPressure}) exceeds normal range",
                                AlertSeverity.Warning);
                        }
                        else if (systolic <= 90 || diastolic <= 60) // Hypotension
                        {
                            await CreateAlertIfNotExists(patientId, entryDateTime, clinicalEntryId,
                                "Low Blood Pressure Alert",
                                $"Patient blood pressure ({bloodPressure}) is below normal range",
                                AlertSeverity.Warning);
                        }
                    }
                }
            }

            // Check heart rate
            if (int.TryParse(heartRate, out int hr))
            {
                if (hr >= 100) // Tachycardia
                {
                    await CreateAlertIfNotExists(patientId, entryDateTime, clinicalEntryId,
                        "High Heart Rate Alert",
                        $"Patient heart rate ({hr} bpm) is elevated",
                        AlertSeverity.Warning);
                }
                else if (hr <= 60) // Bradycardia
                {
                    await CreateAlertIfNotExists(patientId, entryDateTime, clinicalEntryId,
                        "Low Heart Rate Alert",
                        $"Patient heart rate ({hr} bpm) is below normal range",
                        AlertSeverity.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing vital signs for patient: {PatientId}", patientId);
        }
    }

    private async Task AnalyzeSymptoms(Guid patientId, DateTime entryDateTime, dynamic data, string clinicalEntryId)
    {
        try
        {
            if (data == null) return;

            var severity = data.Severity?.ToString()?.ToLower();

            if (severity == "severe" || severity == "critical")
            {
                await CreateAlertIfNotExists(patientId, entryDateTime, clinicalEntryId,
                    "Severe Symptom Alert",
                    "Patient reported severe symptoms requiring immediate attention",
                    AlertSeverity.Critical);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing symptoms for patient: {PatientId}", patientId);
        }
    }

    private async Task AnalyzeTestResults(Guid patientId, DateTime entryDateTime, dynamic data, string clinicalEntryId)
    {
        try
        {
            if (data == null) return;

            var status = data.Status?.ToString()?.ToLower();

            if (status == "abnormal" || status == "critical")
            {
                await CreateAlertIfNotExists(patientId, entryDateTime, clinicalEntryId,
                    "Abnormal Test Results",
                    "Patient test results show abnormal values requiring review",
                    AlertSeverity.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing test results for patient: {PatientId}", patientId);
        }
    }

    private async Task CreateAlertIfNotExists(Guid patientId, DateTime entryDateTime, string clinicalEntryId,
        string title, string description, AlertSeverity severity)
    {
        try
        {
            // Check if similar alert already exists for this clinical entry
            var existingAlerts = await _alertService.GetAlertsByPatientIdAsync(patientId, 1, 10);

            var similarAlert = existingAlerts.FirstOrDefault(a =>
                a.TriggeringClinicalEntryId?.ToString() == clinicalEntryId &&
                a.IsActive);

            if (similarAlert != null)
            {
                _logger.LogDebug("Similar alert already exists for clinical entry: {ClinicalEntryId}", clinicalEntryId);
                return;
            }

            var createAlertDto = new CreateAlertDto
            {
                PatientId = patientId,
                AlertDateTime = entryDateTime,
                Title = title,
                Description = description,
                Severity = severity,
                TriggeringClinicalEntryId = Guid.Parse(clinicalEntryId)
            };

            await _alertService.CreateAlertAsync(createAlertDto);

            _logger.LogInformation("Created alert for patient {PatientId}: {Title}", patientId, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating alert for patient: {PatientId}", patientId);
        }
    }
}