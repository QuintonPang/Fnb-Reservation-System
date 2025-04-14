using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FnbReservationSystem.Models;
using FnbReservationSystem.Data;  // Add this line if missing
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using FnbReservationSystem.Services; // Ensure this namespace is included for WhatsAppService
namespace FnbReservationSystem.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class QueueController : ControllerBase
    {
                private readonly WhatsAppService _whatsAppService;

            private readonly WebSocketManager _webSocketManager;

        private readonly ApplicationDbContext _context;

        public QueueController(ApplicationDbContext context,WebSocketManager webSocketManager, WhatsAppService whatsAppService)
        {
                    _webSocketManager = webSocketManager;

_whatsAppService = whatsAppService;
            _context = context;
        }

        // GET: api/Queue
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Queue>>> GetAllQueue()
        {
            return await _context.Queues
                .Where(q => !q.IsSeated)
                .OrderBy(q => q.Id)
                .ToListAsync();
        }

        // POST: api/Queue
        [HttpPost]
        public async Task<ActionResult<Queue>> AddToQueue(Queue queue)
        {
            queue.IsSeated = false;
            _context.Queues.Add(queue);
            await _context.SaveChangesAsync();

 var queues = await _context.Queues
                .Where(q => !q.IsSeated)
                .OrderBy(q => q.Id)
                .ToListAsync();

  var outlet = await _context.Outlets
            .Where(o => o.Id == queue.outletId)
            .FirstOrDefaultAsync();
             var parameters = new List<WhatsAppService.TemplateParameter>
{
    new() { type = "text", text = queue.CustomerName, parameter_name = "" },
    new() { type = "text", text = outlet.Name, parameter_name = "" },
        new() { type = "text", text = queue.Id.ToString(), parameter_name = "" }
};

await _whatsAppService.SendTemplateMessageAsync(
    toPhoneNumber: "60193903300",
    templateName: "queue_number_confirmation",
    languageCode: "en",
    parameters: parameters
);

// Instead of _webSocketManager.BroadcastQueueUpdate, use the class name directly
await WebSocketManager.BroadcastQueueUpdate(queues);
            return CreatedAtAction(nameof(GetQueueById), new { id = queue.Id }, queue);
        }

        // GET: api/Queue/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Queue>> GetQueueById(int id)
        {
            var entry = await _context.Queues.FindAsync(id);

            if (entry == null)
                return NotFound();

            return entry;
        }

        // PUT: api/Queue/5/dequeue
        [HttpPut("{id}/dequeue")]
        public async Task<IActionResult> MarkAsSeated(int id)
        {
 
// Instead of _webSocketManager.BroadcastQueueUpdate, use the class name directly

            var queueEntry = await _context.Queues.FindAsync(id);
            if (queueEntry == null)
                return NotFound();

            queueEntry.IsSeated = true;
            await _context.SaveChangesAsync();
            var queues = await _context.Queues
                .Where(q => !q.IsSeated)
                .OrderBy(q => q.Id)
                .ToListAsync();


var firstInQueue = await _context.Queues
    .Where(q => !q.IsSeated&& q.outletId == queueEntry.outletId)
    .OrderBy(q => q.Id )
    .FirstOrDefaultAsync();

    

    if (firstInQueue != null)
{

      var outlet = await _context.Outlets
            .Where(o => o.Id == firstInQueue.outletId)  
            .FirstOrDefaultAsync();

    var parameters = new List<WhatsAppService.TemplateParameter>
{
    new() { type = "text", text = firstInQueue.CustomerName, parameter_name = "customer_name" },
    new() { type = "text", text = outlet.Name, parameter_name = "outlet_name" },
};

await _whatsAppService.SendTemplateMessageAsync(
    toPhoneNumber: "60193903300",
    templateName: "your_turn_has_arrived",
    languageCode: "en",
    parameters: parameters
);
}

                      
await WebSocketManager.BroadcastQueueUpdate(queues);

            return NoContent();
        }

        // DELETE: api/Queue/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveQueueEntry(int id)
        {
            var queueEntry = await _context.Queues.FindAsync(id);
            if (queueEntry == null)
                return NotFound();

            _context.Queues.Remove(queueEntry);
            
            await _context.SaveChangesAsync();

 var queues = await _context.Queues
                .Where(q => !q.IsSeated)
                .OrderBy(q => q.Id)
                .ToListAsync();
                
var firstInQueue = await _context.Queues
    .Where(q => !q.IsSeated&& q.outletId == queueEntry.outletId)
    .OrderBy(q => q.Id)
    .FirstOrDefaultAsync();

   

    if (firstInQueue != null)
{
     
  var outlet = await _context.Outlets
            .Where(o => o.Id == firstInQueue.outletId)  
            .FirstOrDefaultAsync();
    var parameters = new List<WhatsAppService.TemplateParameter>
{
    new() { type = "text", text = firstInQueue.CustomerName, parameter_name = "customer_name" },
    new() { type = "text", text = outlet.Name, parameter_name = "outlet_name" },
};

await _whatsAppService.SendTemplateMessageAsync(
    toPhoneNumber: "60193903300",
    templateName: "your_turn_has_arrived",
    languageCode: "en",
    parameters: parameters
);
}

// Instead of _webSocketManager.BroadcastQueueUpdate, use the class name directly
await WebSocketManager.BroadcastQueueUpdate(queues);
            return NoContent();
        }

         // WebSocket endpoint for queue updates
        [HttpGet("/queue-updates")]
        public async Task GetQueueUpdates()
        {
            
             // Add logging
    Console.WriteLine("WebSocket request received.");
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                        Console.WriteLine("WebSocket connection accepted.");

   var queues = await _context.Queues
                .Where(q => !q.IsSeated)
                .OrderBy(q => q.Id)
                .ToListAsync();
                await _webSocketManager.HandleWebSocketAsync(webSocket,queues);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

       

        private bool QueueEntryExists(int id)
        {
            return _context.Queues.Any(e => e.Id == id);
        }
    }
}
