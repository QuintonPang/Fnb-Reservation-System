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

  [HttpGet]
public async Task<ActionResult<IEnumerable<QueueWithTablesDto>>> GetAllQueue([FromQuery] int? outletId)
{
    var query = _context.Queues.AsQueryable();

    if (outletId.HasValue)
    {
        query = query.Where(r => r.outletId == outletId.Value && !r.NoShow && !r.IsSeated);
    }

    var queues = await query.ToListAsync();

    var queueIds = queues.Select(q => q.Id).ToList();

    var tableMap = await _context.QueueTables
        .Where(qt => queueIds.Contains(qt.queueId))
        .GroupBy(qt => qt.queueId)
        .ToDictionaryAsync(g => g.Key, g => g.Select(qt => qt.tableId).ToList());

    var result = queues.Select(q => new QueueWithTablesDto
    {
        Queue = q,
        TableIds = tableMap.ContainsKey(q.Id) ? tableMap[q.Id] : new List<string>()
    }).ToList();

    return result;
}


        // POST: api/Queue
        [HttpPost]
        public async Task<ActionResult<Queue>> AddToQueue(Queue queue)
        {
             queue.IsSeated = false;

    // 🔍 Get all suitable tables (same outlet, not seated, order by smallest first)
    var availableTables = await _context.Tables
        .Where(t => t.outletId == queue.outletId)
        .OrderBy(t => t.max)
        .ToListAsync();

    var selectedTables = new List<Table>();
    int totalSeats = 0;

    foreach (var table in availableTables)
    {
        if (totalSeats < queue.NumberOfGuests)
        {
            selectedTables.Add(table);
            totalSeats += table.max;
        }

        if (totalSeats >= queue.NumberOfGuests)
            break;
    }

    if (totalSeats < queue.NumberOfGuests)
    {
        return BadRequest("Not enough tables available to accommodate the group.");
    }

    // Save the queue
    _context.Queues.Add(queue);
    await _context.SaveChangesAsync();

    // 🔗 Insert into QueueTables (many-to-many)
    foreach (var table in selectedTables)
    {
        var queueTable = new QueueTable
        {
            queueId = queue.Id,
            tableId = table.Id,
            type = "Q"
        };
        _context.QueueTables.Add(queueTable);
    }

    await _context.SaveChangesAsync();

 var queues = await _context.Queues
    .Where(q => !q.IsSeated && !q.NoShow)
    .Select(q => new QueueWithTablesDto
    {
        Queue = q,
        TableIds = _context.QueueTables
            .Where(qt => qt.queueId == q.Id)
            .Select(qt => qt.tableId)
            .ToList()
    }).ToListAsync();


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


           // PUT: api/Queue/5/noShow
        [HttpPut("{id}/noShow")]
        public async Task<IActionResult> MarkAsNoShow(int id)
        {
 
// Instead of _webSocketManager.BroadcastQueueUpdate, use the class name directly

            var queueEntry = await _context.Queues.FindAsync(id);
            if (queueEntry == null)
                return NotFound();


    if (queueEntry != null)
    {

        
     var noShow = await _context.NoShows
        .FirstOrDefaultAsync(n => n.ContactNumber == queueEntry.ContactNumber);

    if (noShow != null)
    {
        // Update existing record
        noShow.count += 1;
        _context.NoShows.Update(noShow);
                  
        if(noShow.count==3){
            BannedCustomer bannedCustomer = new BannedCustomer();
        
            bannedCustomer.BanDate = DateTime.UtcNow;
            bannedCustomer.CustomerName = queueEntry.CustomerName;
            bannedCustomer.ContactNumber = queueEntry.ContactNumber;
            bannedCustomer.Reason = "Automatically banned for not showing up for 3 times.";

            _context.BannedCustomers.Add(bannedCustomer);

            await _whatsAppService.SendTemplateMessageAsync(
    toPhoneNumber: "60193903300",
    templateName: "banned",
    languageCode: "en",
    parameters: []
);
}

        }else{
             // Otherwise, create a new NoShow record
            var newNoShow = new NoShow
            {

                ContactNumber = queueEntry.ContactNumber,
                count = 1
            };
            _context.NoShows.Add(newNoShow);
        }
    }
    else
    {
        return NotFound();
    }

 
    
queueEntry.NoShow = true;
            
     _context.Entry(queueEntry).State = EntityState.Modified;
            await _context.SaveChangesAsync();
     
 var queues = await _context.Queues
    .Where(q => !q.IsSeated && !q.NoShow)
    .Select(q => new QueueWithTablesDto
    {
        Queue = q,
        TableIds = _context.QueueTables
            .Where(qt => qt.queueId == q.Id)
            .Select(qt => qt.tableId)
            .ToList()
    }).ToListAsync();  

var firstInQueue = await _context.Queues
    .Where(q => !q.IsSeated&& q.outletId == queueEntry.outletId && !q.NoShow)
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


        // PUT: api/Queue/5/dequeue
        [HttpPut("{id}/dequeue")]
        public async Task<IActionResult> MarkAsSeated(int id)
        {

            
            var queueEntry = await _context.Queues.FindAsync(id);
            if (queueEntry == null)
                return NotFound();

            queueEntry.IsSeated=true;
     _context.Entry(queueEntry).State = EntityState.Modified;

            await _context.SaveChangesAsync();
     
 var queues = await _context.Queues
    .Where(q => !q.IsSeated && !q.NoShow)
    .Select(q => new QueueWithTablesDto
    {
        Queue = q,
        TableIds = _context.QueueTables
            .Where(qt => qt.queueId == q.Id)
            .Select(qt => qt.tableId)
            .ToList()
    }).ToListAsync();
var firstInQueue = await _context.Queues
    .Where(q => !q.IsSeated&& q.outletId == queueEntry.outletId&& !q.NoShow)
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
    .Where(q => !q.IsSeated && !q.NoShow)
    .Select(q => new QueueWithTablesDto
    {
        Queue = q,
        TableIds = _context.QueueTables
            .Where(qt => qt.queueId == q.Id)
            .Select(qt => qt.tableId)
            .ToList()
    }).ToListAsync();
var firstInQueue = await _context.Queues
    .Where(q => !q.IsSeated&& q.outletId == queueEntry.outletId && !q.NoShow)
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

 var queueWithTables = await _context.Queues
    .Where(q => !q.IsSeated && !q.NoShow)
    .Select(q => new QueueWithTablesDto
    {
        Queue = q,
        TableIds = _context.QueueTables
            .Where(qt => qt.queueId == q.Id)
            .Select(qt => qt.tableId)
            .ToList()
    }).ToListAsync();


                await _webSocketManager.HandleWebSocketAsync(webSocket, queueWithTables);
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
