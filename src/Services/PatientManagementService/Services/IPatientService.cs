using PatientManagementService.DTOs;

namespace PatientManagementService.Services;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetAllPatientsAsync();
    Task<PatientDto?> GetPatientByIdAsync(Guid id);
    Task<PatientDto> CreatePatientAsync(CreatePatientDto createPatientDto);
    Task<PatientDto?> UpdatePatientAsync(Guid id, UpdatePatientDto updatePatientDto);
    Task<bool> DeletePatientAsync(Guid id);
    Task<IEnumerable<PatientDto>> SearchPatientsAsync(string searchTerm);
    Task<bool> PatientExistsAsync(Guid id);
}