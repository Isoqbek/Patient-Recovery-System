using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PatientManagementService.Data;
using PatientManagementService.DTOs;
using PatientManagementService.Models;

namespace PatientManagementService.Services;

public class PatientService : IPatientService
{
    private readonly PatientDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PatientService> _logger;

    public PatientService(PatientDbContext context, IMapper mapper, ILogger<PatientService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync()
    {
        try
        {
            var patients = await _context.Patients
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PatientDto>>(patients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all patients");
            throw;
        }
    }

    public async Task<PatientDto?> GetPatientByIdAsync(Guid id)
    {
        try
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return null;

            return _mapper.Map<PatientDto>(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving patient with ID: {PatientId}", id);
            throw;
        }
    }

    public async Task<PatientDto> CreatePatientAsync(CreatePatientDto createPatientDto)
    {
        try
        {
            // Check if email already exists
            if (!string.IsNullOrEmpty(createPatientDto.Email))
            {
                var existingPatientWithEmail = await _context.Patients
                    .FirstOrDefaultAsync(p => p.Email == createPatientDto.Email);

                if (existingPatientWithEmail != null)
                {
                    throw new InvalidOperationException($"A patient with email '{createPatientDto.Email}' already exists.");
                }
            }

            var patient = _mapper.Map<Patient>(createPatientDto);
            patient.Id = Guid.NewGuid();

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new patient with ID: {PatientId}", patient.Id);

            return _mapper.Map<PatientDto>(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating patient");
            throw;
        }
    }

    public async Task<PatientDto?> UpdatePatientAsync(Guid id, UpdatePatientDto updatePatientDto)
    {
        try
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return null;

            // Check if email already exists (excluding current patient)
            if (!string.IsNullOrEmpty(updatePatientDto.Email))
            {
                var existingPatientWithEmail = await _context.Patients
                    .FirstOrDefaultAsync(p => p.Email == updatePatientDto.Email && p.Id != id);

                if (existingPatientWithEmail != null)
                {
                    throw new InvalidOperationException($"A patient with email '{updatePatientDto.Email}' already exists.");
                }
            }

            _mapper.Map(updatePatientDto, patient);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated patient with ID: {PatientId}", id);

            return _mapper.Map<PatientDto>(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating patient with ID: {PatientId}", id);
            throw;
        }
    }

    public async Task<bool> DeletePatientAsync(Guid id)
    {
        try
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return false;

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted patient with ID: {PatientId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting patient with ID: {PatientId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<PatientDto>> SearchPatientsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllPatientsAsync();

            var lowercaseSearchTerm = searchTerm.ToLower();

            var patients = await _context.Patients
                .Where(p =>
                    p.FirstName.ToLower().Contains(lowercaseSearchTerm) ||
                    p.LastName.ToLower().Contains(lowercaseSearchTerm) ||
                    (p.MiddleName != null && p.MiddleName.ToLower().Contains(lowercaseSearchTerm)) ||
                    (p.Email != null && p.Email.ToLower().Contains(lowercaseSearchTerm)) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(searchTerm)))
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PatientDto>>(patients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching patients with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<bool> PatientExistsAsync(Guid id)
    {
        try
        {
            return await _context.Patients.AnyAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if patient exists with ID: {PatientId}", id);
            throw;
        }
    }
}