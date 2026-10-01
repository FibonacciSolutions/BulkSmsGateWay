using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OmniRoute.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly DbContext _context;

        // System Default Tenant Guid for NMC DiTi Client
        private static readonly Guid SystemDefaultTenantId = Guid.Parse("e88adad2-82c1-48e1-a9f2-732bbcc3fbab");

        public MessagesController(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Simplified Batch Send for Clients (Automatically assigns TenantId and Priority)
        /// </summary>
        [HttpPost("batch-send")]
        public async Task<IActionResult> BatchSend([FromBody] ClientBulkMessageRequest request)
        {
            if (request == null || request.Recipients == null || !request.Recipients.Any())
            {
                return BadRequest(new { error = "No recipients provided." });
            }

            if (string.IsNullOrWhiteSpace(request.MessageText))
            {
                return BadRequest(new { error = "Message text cannot be empty." });
            }

            try
            {
                int successCount = 0;

                foreach (var phone in request.Recipients)
                {
                    if (string.IsNullOrWhiteSpace(phone)) continue;

                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO SmsOutbox (SmsId, TenantId, DestinationNumber, MessageText, DeliveryStatus, Priority, Source, CreatedAt, UpdatedAt) " +
                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                        Guid.NewGuid(), SystemDefaultTenantId, phone.Trim(), request.MessageText, "Pending", 1, "CLIENT_PORTAL", DateTime.UtcNow, DateTime.UtcNow
                    );

                    successCount++;
                }

                return Ok(new
                {
                    success = true,
                    totalQueued = successCount,
                    message = $"Successfully queued {successCount} SMS job(s)."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Processing exception: {ex.Message}" });
            }
        }

        /// <summary>
        /// Android Gateway Device Polling Endpoint
        /// </summary>
        [HttpGet("android-poll")]
        public async Task<IActionResult> AndroidPollPendingSms()
        {
            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "SELECT TOP 1 SmsId, DestinationNumber, MessageText FROM SmsOutbox WHERE DeliveryStatus IN ('Pending', 'Processing') ORDER BY Priority ASC, CreatedAt ASC";

                if (_context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                {
                    await _context.Database.OpenConnectionAsync();
                }

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var smsId = reader.GetGuid(0);
                    var destinationNumber = reader.GetString(1);
                    var messageText = reader.GetString(2);

                    await reader.CloseAsync();

                    // Transition status to Dispatched
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE SmsOutbox SET DeliveryStatus = 'Dispatched', UpdatedAt = {0} WHERE SmsId = {1}",
                        DateTime.UtcNow, smsId
                    );

                    return Ok(new[]
                    {
                        new
                        {
                            id = smsId,
                            phoneNumber = destinationNumber,
                            message = messageText
                        }
                    });
                }

                return Ok(new object[] { });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Polling exception: {ex.Message}" });
            }
        }
    }

    public class ClientBulkMessageRequest
    {
        public List<string> Recipients { get; set; } = new List<string>();
        public string MessageText { get; set; } = string.Empty;
    }
}