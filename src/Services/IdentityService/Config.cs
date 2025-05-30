using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using System.Security.Claims;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource("roles", "User Roles", new[] { "role" })
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            // Patient Management Scopes
            new ApiScope("patient.read", "Read patient information"),
            new ApiScope("patient.write", "Write patient information"),
            
            // Clinical Record Scopes
            new ApiScope("clinicalrecord.read", "Read clinical records"),
            new ApiScope("clinicalrecord.write", "Write clinical records"),
            
            // Monitoring Scopes
            new ApiScope("monitoring.read", "Read monitoring alerts"),
            new ApiScope("monitoring.write", "Write monitoring alerts"),
            
            // Rehabilitation Scopes
            new ApiScope("rehabilitation.read", "Read rehabilitation plans"),
            new ApiScope("rehabilitation.write", "Write rehabilitation plans"),
            
            // Knowledge Base Scopes
            new ApiScope("knowledge.read", "Read knowledge base"),
            new ApiScope("knowledge.write", "Write knowledge base"),
            
            // Diagnosis Support Scopes
            new ApiScope("diagnosis.read", "Access diagnosis support"),
            
            // Notification Scopes
            new ApiScope("notification.read", "Read notifications"),
            new ApiScope("notification.write", "Send notifications"),
            
            // Admin Scopes
            new ApiScope("admin.full", "Full administrative access")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // Patient Recovery Management System API Gateway
            new Client
            {
                ClientId = "patient-recovery-gateway",
                ClientName = "Patient Recovery Gateway",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("patient-recovery-secret".Sha256()) },
                AllowedScopes = {
                    "patient.read", "patient.write",
                    "clinicalrecord.read", "clinicalrecord.write",
                    "monitoring.read", "monitoring.write",
                    "rehabilitation.read", "rehabilitation.write",
                    "knowledge.read", "knowledge.write",
                    "diagnosis.read",
                    "notification.read", "notification.write",
                    "admin.full"
                }
            },
            
            // Web Client (Frontend)
            new Client
            {
                ClientId = "patient-recovery-web",
                ClientName = "Patient Recovery Web Client",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,
                AllowOfflineAccess = true,
                RedirectUris = { "http://localhost:3000/callback", "https://localhost:3000/callback" },
                PostLogoutRedirectUris = { "http://localhost:3000", "https://localhost:3000" },
                AllowedCorsOrigins = { "http://localhost:3000", "https://localhost:3000" },
                AllowedScopes = {
                    "openid", "profile", "email", "roles",
                    "patient.read", "patient.write",
                    "clinicalrecord.read", "clinicalrecord.write",
                    "monitoring.read", "monitoring.write",
                    "rehabilitation.read", "rehabilitation.write",
                    "knowledge.read", "knowledge.write",
                    "diagnosis.read",
                    "notification.read"
                },
                AccessTokenLifetime = 3600,
                RefreshTokenUsage = TokenUsage.ReUse
            },
            
            // Mobile Client
            new Client
            {
                ClientId = "patient-recovery-mobile",
                ClientName = "Patient Recovery Mobile App",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,
                AllowOfflineAccess = true,
                RedirectUris = { "patientrecovery://callback" },
                AllowedScopes = {
                    "openid", "profile", "email", "roles",
                    "patient.read",
                    "clinicalrecord.read", "clinicalrecord.write",
                    "rehabilitation.read", "rehabilitation.write",
                    "knowledge.read",
                    "notification.read"
                },
                AccessTokenLifetime = 3600,
                RefreshTokenUsage = TokenUsage.ReUse
            }
        };

    public static List<TestUser> TestUsers =>
        new List<TestUser>
        {
            // Patient Test User
            new TestUser
            {
                SubjectId = "patient-001",
                Username = "patient001",
                Password = "password",
                Claims = new List<Claim>
                {
                    new Claim("given_name", "John"),
                    new Claim("family_name", "Doe"),
                    new Claim("email", "john.doe@patient.com"),
                    new Claim("role", "Patient"),
                    new Claim("patient_id", "11111111-1111-1111-1111-111111111111")
                }
            },
            
            // Nurse Test User
            new TestUser
            {
                SubjectId = "nurse-001",
                Username = "nurse001",
                Password = "password",
                Claims = new List<Claim>
                {
                    new Claim("given_name", "Jane"),
                    new Claim("family_name", "Smith"),
                    new Claim("email", "jane.smith@hospital.com"),
                    new Claim("role", "Nurse"),
                    new Claim("employee_id", "N001")
                }
            },
            
            // Physician Test User
            new TestUser
            {
                SubjectId = "physician-001",
                Username = "doctor001",
                Password = "password",
                Claims = new List<Claim>
                {
                    new Claim("given_name", "Dr. Michael"),
                    new Claim("family_name", "Johnson"),
                    new Claim("email", "michael.johnson@hospital.com"),
                    new Claim("role", "Physician"),
                    new Claim("employee_id", "D001"),
                    new Claim("specialization", "Cardiology")
                }
            },
            
            // Administrator Test User
            new TestUser
            {
                SubjectId = "admin-001",
                Username = "admin001",
                Password = "password",
                Claims = new List<Claim>
                {
                    new Claim("given_name", "System"),
                    new Claim("family_name", "Administrator"),
                    new Claim("email", "admin@hospital.com"),
                    new Claim("role", "Administrator"),
                    new Claim("employee_id", "A001")
                }
            }
        };
}