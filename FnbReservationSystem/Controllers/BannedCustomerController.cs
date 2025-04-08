using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FnbReservationSystem.Models;
using FnbReservationSystem.Data;  // Add this line if missing

namespace FnbReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannedCustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BannedCustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/BannedCustomer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BannedCustomer>>> GetBannedCustomers()
        {
            return await _context.BannedCustomers.ToListAsync();
        }

        // GET: api/BannedCustomer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BannedCustomer>> GetBannedCustomer(int id)
        {
            var bannedCustomer = await _context.BannedCustomers.FindAsync(id);

            if (bannedCustomer == null)
            {
                return NotFound();
            }

            return bannedCustomer;
        }

        // POST: api/BannedCustomer
        [HttpPost]
        public async Task<ActionResult<BannedCustomer>> PostBannedCustomer(BannedCustomer bannedCustomer)
        {
            bannedCustomer.BanDate = DateTime.UtcNow;
            _context.BannedCustomers.Add(bannedCustomer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBannedCustomer), new { id = bannedCustomer.Id }, bannedCustomer);
        }

        // PUT: api/BannedCustomer/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBannedCustomer(int id, BannedCustomer bannedCustomer)
        {
            if (id != bannedCustomer.Id)
            {
                return BadRequest();
            }

            _context.Entry(bannedCustomer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BannedCustomerExists(id))
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

        // DELETE: api/BannedCustomer/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBannedCustomer(int id)
        {
            var bannedCustomer = await _context.BannedCustomers.FindAsync(id);
            if (bannedCustomer == null)
            {
                return NotFound();
            }

            _context.BannedCustomers.Remove(bannedCustomer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BannedCustomerExists(int id)
        {
            return _context.BannedCustomers.Any(e => e.Id == id);
        }
    }
}
