using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FnbReservationSystem.Models;
using FnbReservationSystem.Data;  // Add this line if missing
    using System.Security.Cryptography;
using System.Text;
using System; // Needed for StringBuilder

namespace FnbReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Staff
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Staff>>> GetStaffs()
        {
            return await _context.Staffs.ToListAsync();
        }

        // GET: api/Staff/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Staff>> GetStaff(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);

            if (staff == null)
            {
                return NotFound();
            }

            return staff;
        }

   

[HttpPost]
public async Task<ActionResult<Staff>> PostStaff(Staff staff)
{
    // Get last 4 digits of IC No
            Console.WriteLine("HELOOOOOO"); // For debugging purposes

    if (!string.IsNullOrEmpty(staff.IcNo) && staff.IcNo.Length >= 12) 
    {
        string lastFourDigits = staff.IcNo.Substring(staff.IcNo.Length - 4);
        staff.Password = HashPassword(lastFourDigits); // Hashing the password
    }
    else
    {
        return BadRequest(new { message = "Invalid IC number" });
    }

    staff.Username = staff.Name.Split(' ')[0].ToLower(); // Ensure username is in lowercase

    _context.Staffs.Add(staff);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetStaff), new { id = staff.Id }, staff);
}


        // PUT: api/Staff/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStaff(int id, Staff staff)
        {
            if (id != staff.Id)
            {
                return BadRequest();
            }

            _context.Entry(staff).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StaffExists(id))
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

        // DELETE: api/Staff/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }

            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StaffExists(int id)
        {
            return _context.Staffs.Any(e => e.Id == id);
        }

        private string HashPassword(string input)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        StringBuilder builder = new StringBuilder();
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2")); // convert to hex
        }
        return builder.ToString();
    }
    
}
    }

    

}
