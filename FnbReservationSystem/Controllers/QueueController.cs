using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FnbReservationSystem.Models;
using FnbReservationSystem.Data;  // Add this line if missing

namespace FnbReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QueueController(ApplicationDbContext context)
        {
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
            var queueEntry = await _context.Queues.FindAsync(id);
            if (queueEntry == null)
                return NotFound();

            queueEntry.IsSeated = true;
            await _context.SaveChangesAsync();

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

            return NoContent();
        }

        private bool QueueEntryExists(int id)
        {
            return _context.Queues.Any(e => e.Id == id);
        }
    }
}
