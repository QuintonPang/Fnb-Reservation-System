using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FnbReservationSystem.Models;
using FnbReservationSystem.Data;  // Add this line if missing
[Route("api/[controller]")]
[ApiController]
public class TableController : ControllerBase
{
        private readonly ApplicationDbContext _context;

    public TableController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Table
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Table>>> GetTables()
    {
        return await _context.Tables.ToListAsync();
    }

    // GET: api/Table/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Table>> GetTable(string id)
    {
        var table = await _context.Tables.FindAsync(id);
        if (table == null)
            return NotFound();

        return table;
    }

    // POST: api/Table
    [HttpPost]
    public async Task<ActionResult<Table>> PostTable(Table table)
    {
        _context.Tables.Add(table);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTable), new { id = table.Id }, table);
    }

    // PUT: api/Table/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTable(string id, Table table)
    {
        if (id != table.Id)
            return BadRequest();

        _context.Entry(table).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Tables.Any(e => e.Id == id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/Table/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTable(string id)
    {
        var table = await _context.Tables.FindAsync(id);
        if (table == null)
            return NotFound();

        _context.Tables.Remove(table);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
