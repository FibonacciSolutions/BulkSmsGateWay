using System;

namespace OmniRoute.Core.Entities
{
    public class MessageLog
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public string DestinationNumber { get; set; } = string.Empty;
        public string RequestedParametersJson { get; set; } = string.Empty;
        public string DispatchedChannel { get; set; } = "SMS"; // WhatsApp, RCS, SMS
        public string DeliveryStatus { get; set; } = "Queued"; // Queued, Dispatched, Delivered, Failed
        public decimal CostCharged { get; set; }
        public string? ProviderMessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Tenant Tenant { get; set; } = null!;
    }
}