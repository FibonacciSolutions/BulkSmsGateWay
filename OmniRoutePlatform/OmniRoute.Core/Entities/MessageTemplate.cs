using System;

namespace OmniRoute.Core.Entities
{
    // CRITICAL: Ensure 'public' is written here
    public class MessageTemplate
    {
        public Guid TemplateId { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public string TemplateCode { get; set; } = string.Empty;
        public string MetaTemplateName { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = "en";
        public string SmsRawTextBody { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; } = null!;
    }
}