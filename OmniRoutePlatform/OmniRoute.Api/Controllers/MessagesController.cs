using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OmniRoute.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MessageController : ControllerBase
    {
        // 🚀 Using a dynamic service lookup to bypass strict compile-time namespace dependency blocks
        private readonly DbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public MessageController(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            // Dynamically locates your registered ApplicationDbContext without needing the explicit using statement
            foreach (var service in serviceProvider.GetServices<DbContext>())
            {
                _context = service;
                break;
            }

            // Fallback default context assign if needed
            _context ??= serviceProvider.GetRequiredService<DbContext>();
        }

        // =========================================================================
        // ROUTE ENTRYPOINT (Hits from Frontend / Client APIs)
        // =========================================================================
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.TemplateCode))
            {
                return BadRequest(new { error = "Invalid parameters context." });
            }

            if (HttpContext.Items["TenantId"] is not Guid tenantId)
            {
                return StatusCode(500, new { error = "Tenant context security mismatch." });
            }

            try
            {
                var channelUsed = !string.IsNullOrWhiteSpace(request.ChannelPreference) ? request.ChannelPreference : "WhatsApp";
                string deliveryStatus = "Dispatched";
                string providerReference = "MOCK_REF";
                string alertText = "OmniRoute SMS Alert:\nDear Parent, your child was marked present at school today.";

                // OPTION A: ROUTE VIA SELF-HOSTED WHATSAPP NODE
                if (channelUsed.ToUpper() == "WHATSAPP")
                {
                    var client = _httpClientFactory.CreateClient();
                    var workerPayload = new { to = request.To, message = alertText };
                    var jsonContent = new StringContent(JsonSerializer.Serialize(workerPayload), Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("http://localhost:5001/api/worker/send-whatsapp", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        deliveryStatus = "Delivered";
                        providerReference = "FREE_WA_NODE_" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                    }
                    else
                    {
                        deliveryStatus = "WorkerConnectionFailed";
                    }

                    await LogMessageToDb(tenantId, request.To, providerReference, channelUsed, deliveryStatus, 0.70m);
                }
                // 🚀 OPTION B: ROUTE VIA LOCAL ANDROID SMS GATEWAY
                else if (channelUsed.ToUpper() == "SMS")
                {
                    deliveryStatus = "Pending";
                    providerReference = "ANDROID_OUTBOX_QUEUE";

                    // Insert into outbox queue table so the phone can pull it down
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO SmsOutbox (SmsId, TenantId, DestinationNumber, MessageText, DeliveryStatus, CreatedAt, UpdatedAt) " +
                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                        Guid.NewGuid(), tenantId, request.To, alertText, "Pending", DateTime.UtcNow, DateTime.UtcNow
                    );
                }

                return Ok(new
                {
                    message = $"Pipeline routing accepted via {channelUsed}.",
                    delivery_status = deliveryStatus,
                    provider_reference = providerReference
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Engine processing exception: {ex.Message}" });
            }
        }

        // =========================================================================
        // ANDROID LOOP ENDPOINT (The phone hits this endpoint to fetch messages)
        // =========================================================================
        [HttpGet("android-poll")]
        public async Task<IActionResult> AndroidPollPendingSms()
        {
            try
            {
                // Clean dynamic execution to pull from the outbox queue cleanly
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "SELECT TOP 1 SmsId, DestinationNumber, MessageText FROM SmsOutbox WHERE DeliveryStatus = 'Pending' ORDER BY CreatedAt ASC";

                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var smsId = reader.GetGuid(0);
                    var destinationNumber = reader.GetString(1);
                    var messageText = reader.GetString(2);

                    // Close reader to allow the update execution transaction loop to execute safely
                    await reader.CloseAsync();

                    // Mark it as Processing so no other device grabs it
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE SmsOutbox SET DeliveryStatus = 'Processing', UpdatedAt = {0} WHERE SmsId = {1}",
                        DateTime.UtcNow, smsId
                    );

                    return Ok(new
                    {
                        sms_available = true,
                        sms_id = smsId,
                        to = destinationNumber,
                        message = messageText
                    });
                }

                return Ok(new { sms_available = false, message = "No pending records inside the outbox table loop." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Polling node exception: {ex.Message}" });
            }
        }

        private async Task LogMessageToDb(Guid tenantId, string to, string refId, string channel, string status, decimal cost)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "INSERT INTO MessageLogs (MessageId, TenantId, DestinationNumber, RequestedParametersJson, DispatchedChannel, DeliveryStatus, CostCharged, CreatedAt, UpdatedAt) " +
                "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                Guid.NewGuid(), tenantId, to, refId, channel, status, cost, DateTime.UtcNow, DateTime.UtcNow
            );
        }
    }

    public class SendMessageRequest
    {
        public string To { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;
        public string? ChannelPreference { get; set; }
    }
}