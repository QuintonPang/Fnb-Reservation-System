using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FnbReservationSystem.Models;
using FnbReservationSystem.Data;  // Add this line if missing

namespace FnbReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations.ToListAsync();
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
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

        // PUT: api/Reservation/5/cancel
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            reservation.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/Reservation/5/done
        [HttpPut("{id}/done")]
        public async Task<IActionResult> MarkReservationDone(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            reservation.Status = "Done";
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
    var reservation = await _context.Reservations.FindAsync(id);
    if (reservation == null) return NotFound();

    reservation.Status = update.Status;
    await _context.SaveChangesAsync();

    return NoContent();
}

    }
}
