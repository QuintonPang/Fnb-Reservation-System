using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FnbReservationSystem.Models;
using FnbReservationSystem.Data;  // Add this line if missing

namespace FnbReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutletController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OutletController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Outlet
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Outlet>>> GetOutlets()
        {
            return await _context.Outlets.ToListAsync();
        }

        // GET: api/Outlet/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Outlet>> GetOutlet(int id)
        {
            var outlet = await _context.Outlets.FindAsync(id);

            if (outlet == null)
            {
                return NotFound();
            }

            return outlet;
        }

        // POST: api/Outlet
        [HttpPost]
        public async Task<ActionResult<Outlet>> PostOutlet(Outlet outlet)
        {
            _context.Outlets.Add(outlet);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOutlet), new { id = outlet.Id }, outlet);
        }

        // PUT: api/Outlet/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOutlet(int id, Outlet outlet)
        {
            if (id != outlet.Id)
            {
                return BadRequest();
            }

            _context.Entry(outlet).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OutletExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Outlet/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOutlet(int id)
        {
            var outlet = await _context.Outlets.FindAsync(id);
            if (outlet == null)
            {
                return NotFound();
            }

            _context.Outlets.Remove(outlet);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OutletExists(int id)
        {
            return _context.Outlets.Any(e => e.Id == id);
        }
    }
}
