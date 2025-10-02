using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Interface for PCI DSS compliant data handling
    /// </summary>
    public interface IPCIDataHandlingService
    {
        /// <summary>
        /// Tokenize sensitive payment data
        /// </summary>
        Task<string> TokenizePaymentData(string sensitiveData);

        /// <summary>
        /// Detokenize payment data
        /// </summary>
        Task<string> DetokenizePaymentData(string token);

        /// <summary>
        /// Mask sensitive payment information for display
        /// </summary>
        string MaskPaymentData(string paymentData, PaymentDataType dataType);

        /// <summary>
        /// Validate PCI DSS compliance for payment data
        /// </summary>
        Task<PCIValidationResult> ValidatePaymentDataCompliance(object paymentData);

        /// <summary>
        /// Securely purge payment data
        /// </summary>
        Task<bool> SecurePurgePaymentData(string dataIdentifier);
    }

    /// <summary>
    /// Interface for audit logging service
    /// </summary>
    public interface IAuditLoggingService
    {
        /// <summary>
        /// Log payment-related activities
        /// </summary>
        Task LogPaymentActivity(PaymentAuditLog auditLog);

        /// <summary>
        /// Log security events
        /// </summary>
        Task LogSecurityEvent(SecurityAuditLog auditLog);

        /// <summary>
        /// Log data access events
        /// </summary>
        Task LogDataAccess(DataAccessAuditLog auditLog);

        /// <summary>
        /// Get audit logs for compliance reporting
        /// </summary>
        Task<List<AuditLogEntry>> GetAuditLogs(AuditLogFilter filter);
    }

    /// <summary>
    /// Interface for security monitoring service
    /// </summary>
    public interface ISecurityMonitoringService
    {
        /// <summary>
        /// Monitor for suspicious payment activities
        /// </summary>
        Task<SecurityThreatAssessment> AssessPaymentSecurity(PaymentSecurityContext context);

        /// <summary>
        /// Detect fraud patterns
        /// </summary>
        Task<FraudDetectionResult> DetectFraud(FraudDetectionRequest request);

        /// <summary>
        /// Monitor for data breaches
        /// </summary>
        Task<bool> MonitorDataIntegrity(string dataIdentifier);

        /// <summary>
        /// Generate security alerts
        /// </summary>
        Task SendSecurityAlert(SecurityAlert alert);
    }
}

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// Payment audit log entry
    /// </summary>
    public class PaymentAuditLog
    {
        public string UserId { get; set; }
        public string Action { get; set; }
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    /// <summary>
    /// Security audit log entry
    /// </summary>
    public class SecurityAuditLog
    {
        public string EventType { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        public string Source { get; set; }
        public Dictionary<string, object> Details { get; set; }
    }

    /// <summary>
    /// Data access audit log entry
    /// </summary>
    public class DataAccessAuditLog
    {
        public string UserId { get; set; }
        public string DataType { get; set; }
        public string DataIdentifier { get; set; }
        public string AccessType { get; set; }
        public string Purpose { get; set; }
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; }
        public bool Authorized { get; set; }
        public string AuthorizationMethod { get; set; }
    }

    /// <summary>
    /// Generic audit log entry
    /// </summary>
    public class AuditLogEntry
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; }
        public string Result { get; set; }
        public Dictionary<string, object> Details { get; set; }
    }

    /// <summary>
    /// Audit log filter for querying
    /// </summary>
    public class AuditLogFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UserId { get; set; }
        public string EventType { get; set; }
        public string Resource { get; set; }
        public int PageSize { get; set; } = 50;
        public int PageNumber { get; set; } = 1;
    }

    /// <summary>
    /// Security threat assessment result
    /// </summary>
    public class SecurityThreatAssessment
    {
        public string ThreatLevel { get; set; }
        public double RiskScore { get; set; }
        public List<string> ThreatIndicators { get; set; }
        public List<string> RecommendedActions { get; set; }
        public DateTime AssessmentTimestamp { get; set; }
    }

    /// <summary>
    /// Payment security context
    /// </summary>
    public class PaymentSecurityContext
    {
        public string UserId { get; set; }
        public string PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public string DeviceFingerprint { get; set; }
        public string GeolocationData { get; set; }
        public DateTime TransactionTime { get; set; }
    }

    /// <summary>
    /// Fraud detection request
    /// </summary>
    public class FraudDetectionRequest
    {
        public string UserId { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethodId { get; set; }
        public string MerchantId { get; set; }
        public string IPAddress { get; set; }
        public string DeviceFingerprint { get; set; }
        public Dictionary<string, object> TransactionData { get; set; }
    }

    /// <summary>
    /// Fraud detection result
    /// </summary>
    public class FraudDetectionResult
    {
        public bool IsFraudulent { get; set; }
        public double FraudScore { get; set; }
        public string RiskLevel { get; set; }
        public List<string> FraudIndicators { get; set; }
        public string RecommendedAction { get; set; }
        public DateTime AnalysisTimestamp { get; set; }
    }

    /// <summary>
    /// Security alert
    /// </summary>
    public class SecurityAlert
    {
        public string AlertType { get; set; }
        public string Severity { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Details { get; set; }
        public List<string> Recipients { get; set; }
    }

    /// <summary>
    /// PCI compliance report
    /// </summary>
    public class PCIComplianceReport
    {
        public string ReportId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string ReportPeriod { get; set; }
        public PCIComplianceStatus OverallStatus { get; set; }
        public List<PCIRequirementStatus> RequirementStatuses { get; set; }
        public List<string> Violations { get; set; }
        public List<string> Recommendations { get; set; }
        public string GeneratedBy { get; set; }
    }

    /// <summary>
    /// PCI requirement status
    /// </summary>
    public class PCIRequirementStatus
    {
        public string RequirementId { get; set; }
        public string RequirementName { get; set; }
        public string Description { get; set; }
        public PCIComplianceStatus Status { get; set; }
        public DateTime LastAssessed { get; set; }
        public string Evidence { get; set; }
        public List<string> Issues { get; set; }
    }

    /// <summary>
    /// PCI compliance status enum
    /// </summary>
    public enum PCIComplianceStatus
    {
        Compliant,
        NonCompliant,
        PartiallyCompliant,
        NotAssessed,
        InProgress
    }

    /// <summary>
    /// Data retention policy
    /// </summary>
    public class DataRetentionPolicy
    {
        public string DataType { get; set; }
        public int RetentionPeriodDays { get; set; }
        public string PurgeMethod { get; set; }
        public bool RequiresApproval { get; set; }
        public string ApprovalWorkflow { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

    /// <summary>
    /// Encryption configuration
    /// </summary>
    public class EncryptionConfiguration
    {
        public string Algorithm { get; set; }
        public int KeySize { get; set; }
        public string KeyManagementService { get; set; }
        public string KeyRotationPolicy { get; set; }
        public bool AtRestEncryption { get; set; }
        public bool InTransitEncryption { get; set; }
        public string CertificateAuthority { get; set; }
    }

    /// <summary>
    /// Enum representing the type of payment data being processed
    /// </summary>
    public enum PaymentDataType
    {
        CardNumber,
        ExpiryDate,
        CVV,
        CardHolderName,
        BankAccountNumber,
        RoutingNumber,
        OtherSensitiveData
    }

    /// <summary>
    /// Result of validating payment data against PCI requirements
    /// </summary>
    public class PCIValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Violations { get; set; } = new();
        public PCIComplianceStatus OverallStatus { get; set; } = PCIComplianceStatus.NotAssessed;
    }
}

