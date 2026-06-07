// ApiDTOs.cs
// All MVC-side Data Transfer Objects for communicating with the backend API.
// These are plain C# classes — no EF Core, no database references.
// They mirror the shapes returned by the API endpoints so JsonSerializer can
// deserialize HTTP responses into typed objects.

namespace ST10448420_TechMove_GLMS.ApiServices
{
    //Contract DTOs 
    // Used when the API returns a contract (GET /api/contracts, GET /api/contracts/{id})
    public class ContractApiDTO
    {
        public int ContractID { get; set; }
        public int ClientID { get; set; }
        // ClientName is included in the API response so the view does not need
        // to make a separate call to look up the client — reduces round trips.
        public string ClientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ServiceLevel { get; set; } = string.Empty;
        public string SignedAgreementFilePath { get; set; } = string.Empty;
    }

    // Used when the MVC sends a new contract to the API (POST /api/contracts)
    public class CreateContractApiDTO
    {
        public int ClientID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Draft";
        public string ServiceLevel { get; set; } = string.Empty;
        public string SignedAgreementFilePath { get; set; } = string.Empty;
    }

    //Service Request DTO
    // Used when the API returns a service request
    // (GET /api/servicerequests, GET /api/servicerequests/{id})
    public class ServiceRequestApiDTO
    {
        public int RequestID { get; set; }
        public int ContractID { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? DocumentPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Used when the MVC creates a new service request (POST /api/servicerequests)
    public class CreateServiceRequestApiDTO
    {
        public int ContractID { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal CostUSD { get; set; }
        public string? DocumentPath { get; set; }
    }
}