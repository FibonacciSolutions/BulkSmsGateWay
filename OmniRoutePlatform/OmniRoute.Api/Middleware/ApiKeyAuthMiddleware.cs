using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace OmniRoute.Api.Middleware
{
    public class ApiKeyAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiKeyAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract our active tenant GUID fetched from your SQL Server database
            var devTenantGuid = System.Guid.Parse("A5922109-3357-4BE2-ACE5-5B530AF1DCF0"); // Replace with your real DB Guid if needed!

            context.Items["TenantId"] = devTenantGuid;
            context.Items["Tenant"] = "dev_test_tenant";
            context.Items["ClientId"] = "dev_test_client";
            context.Items["OrganizationId"] = "dev_test_org";

            await _next(context);
        }
    }
}