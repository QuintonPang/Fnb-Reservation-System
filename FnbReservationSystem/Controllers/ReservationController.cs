using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FnbReservationSystem.Models;
using FnbReservationSystem.Data;  // Add this line if missing
using FnbReservationSystem.Services; // Ensure this namespace is included for WhatsAppService

namespace FnbReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {

        private readonly WhatsAppService _whatsAppService;

        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context, WhatsAppService whatsAppService)
        {
            _context = context;
                _whatsAppService = whatsAppService;
                

        }

        // GET: api/Reservation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations([FromQuery] int? outletId)
        {
             var query = _context.Reservations.AsQueryable();

    if (outletId.HasValue)
    {
        query = query.Where(r => r.outletId == outletId.Value);
    }

    var result = await query.ToListAsync();
    return result;
        }

        // GET: api/Reservation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
                return NotFound();

            return reservation;
        }

        // POST: api/Reservation
        [HttpPost]
        public async Task<ActionResult<Reservation>> CreateReservation(Reservation reservation)
        {

            reservation.Status = "Pending"; // default
            _context.Reservations.Add(reservation);


            
    // 🔍 Get all suitable tables (same outlet, not seated, order by smallest first)
    var availableTables = await _context.Tables
        .Where(t => t.outletId == reservation.outletId)
        .OrderBy(t => t.max)
        .ToListAsync();

    var selectedTables = new List<Table>();
    int totalSeats = 0;

    foreach (var table in availableTables)
    {
        if (totalSeats < reservation.NumberOfGuests)
        {
            selectedTables.Add(table);
            totalSeats += table.max;
        }

        if (totalSeats >= reservation.NumberOfGuests)
            break;
    }

    if (totalSeats < reservation.NumberOfGuests)
    {
        return BadRequest("Not enough tables available to accommodate the group.");
    }

    await _context.SaveChangesAsync();

    // 🔗 Insert into QueueTables (many-to-many)
    foreach (var table in selectedTables)
    {
        var queueTable = new QueueTable
        {
            queueId = reservation.Id,
            tableId = table.Id,
            type = "R"
        };
        _context.QueueTables.Add(queueTable);
    }
            await _context.SaveChangesAsync();


            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

       

        // PUT: api/Reservation/5/done
        [HttpPut("{id}/done")]  
        public async Task<IActionResult> MarkReservationDone(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            reservation.Status = "Done";
                 _context.Entry(reservation).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }

        [HttpPut("{id}/status")]
public async Task<IActionResult> UpdateReservationStatus(int id, [FromBody] Reservation update)
{

    try
{
    var reservation = await _context.Reservations.FindAsync(id);
    if (reservation == null) return NotFound();

if(update.Status=="Confirmed"){
    
  var outlet = await _context.Outlets
            .Where(o => o.Id == reservation.outletId)
            .FirstOrDefaultAsync();
            string formattedDate = reservation.ReservationDateTime.ToString("dd-MM-yyyy HH:mm:ss");

        var parameters = new List<WhatsAppService.TemplateParameter>
{
    new() { type = "text", text = reservation.CustomerName, parameter_name = "customer_name" },
    new() { type = "text", text = outlet.Name, parameter_name = "outlet_name" },
        new() { type = "text", text =   formattedDate, parameter_name = "datetime" }
};

await _whatsAppService.SendTemplateMessageAsync(
    toPhoneNumber: "60193903300",
    templateName: "reservation_confirmation",
    languageCode: "en",
    parameters: parameters
);
}else if(update.Status=="Cancelled"){
 
  var outlet = await _context.Outlets
            .Where(o => o.Id == reservation.outletId)
            .FirstOrDefaultAsync();
            string formattedDate = reservation.ReservationDateTime.ToString("dd-MM-yyyy HH:mm:ss");

        var parameters = new List<WhatsAppService.TemplateParameter>
{
    new() { type = "text", text = reservation.CustomerName, parameter_name = "customer_name" },
        new() { type = "text", text =   formattedDate, parameter_name = "datetime" }
};

await _whatsAppService.SendTemplateMessageAsync(
    toPhoneNumber: "60193903300",
    templateName: "your_reservation_has_been_cancelled ",
    languageCode: "en",
    parameters: parameters
);
}else if(update.Status=="Noshow"){
 var noShow = await _context.NoShows
        .FirstOrDefaultAsync(n => n.ContactNumber == reservation.ContactNumber);

    if (noShow != null)
    {
        // Update existing record
        noShow.count += 1;
        _context.NoShows.Update(noShow);

              if(noShow.count>=3){
            BannedCustomer bannedCustomer = new BannedCustomer();
        
            bannedCustomer.BanDate = DateTime.UtcNow;
            bannedCustomer.CustomerName = reservation.CustomerName;
            bannedCustomer.ContactNumber = reservation.ContactNumber;
            bannedCustomer.Reason = "Automatically banned for not showing up for 3 times.";

            _context.BannedCustomers.Add(bannedCustomer);

            await _whatsAppService.SendTemplateMessageAsync(
    toPhoneNumber: "60193903300",
    templateName: "banned",
    languageCode: "en",
    parameters: []
);
}
    }
    else
    {
        // Add new record
        var newNoShow = new NoShow
        {
            ContactNumber = reservation.ContactNumber,
            count = 1
        };
        await _context.NoShows.AddAsync(newNoShow);
    }
}
    reservation.Status = update.Status;
     _context.Entry(reservation).State = EntityState.Modified;
    await _context.SaveChangesAsync();

    return NoContent();

  }  catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return BadRequest(new { message = "Error updating reservation", error = ex.Message });
}

}

public class CancelRequest
{
    public int Id { get; set; }
    public string ContactNumber { get; set; }
}


[HttpPost("cancel")]
public async Task<IActionResult> CancelReservation([FromBody] CancelRequest request)
{
    var reservation = await _context.Reservations.FindAsync(request.Id);

    if (reservation == null)
        return NotFound(new { message = "Reservation not found." });

    if (reservation.ContactNumber != request.ContactNumber)
        return BadRequest(new { message = "Contact number does not match." });
 if (reservation.Status != "Pending")
        return BadRequest(new { message = "Invalid Action." });
    reservation.Status = "Cancelled";
    await _context.SaveChangesAsync();


  var outlet = await _context.Outlets
            .Where(o => o.Id == reservation.outletId)
            .FirstOrDefaultAsync();
            string formattedDate = reservation.ReservationDateTime.ToString("dd-MM-yyyy HH:mm:ss");

        var parameters = new List<WhatsAppService.TemplateParameter>
{
    new() { type = "text", text = reservation.CustomerName, parameter_name = "customer_name" },
        new() { type = "text", text =   formattedDate, parameter_name = "datetime" }
};

await _whatsAppService.SendTemplateMessageAsync(
    toPhoneNumber: "60193903300",
    templateName: "your_reservation_has_been_cancelled ",
    languageCode: "en",
    parameters: parameters
);
    return Ok(new { message = "Reservation cancelled successfully." });
}



    }
}
