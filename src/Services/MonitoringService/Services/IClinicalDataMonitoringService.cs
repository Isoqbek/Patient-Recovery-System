namespace MonitoringService.Services;

public interface IClinicalDataMonitoringService
{
    Task MonitorAllPatientsAsync();
    Task MonitorPatientAsync(Guid patientId);
}