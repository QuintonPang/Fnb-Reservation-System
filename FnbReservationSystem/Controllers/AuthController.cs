using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using FnbReservationSystem.Models;
using FnbReservationSystem.Data; 
 using System.Security.Cryptography;
using System.Text;
using System; // Needed for StringBuilder
[Route("api/[controller]")]
[ApiController]

public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    // Verify the hashed password
    public static bool VerifyPassword(string storedHash, string inputPassword)
    {
        // Hash the input password again and compare with the stored hash
        string hashedInput = HashPassword(inputPassword);
        return storedHash == hashedInput;
    }
    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Username == loginRequest.Username);

        if (staff == null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        // Compare hashed passwords
        if (VerifyPassword(staff.Password, loginRequest.Password))
        {
            // Generate token (you can use JWT or any other method)
            var token = GenerateToken(staff);

            return Ok(new { Token = token, Role = staff.Role, OutletId = staff.OutletId });
        }

        return Unauthorized(new { message = "Invalid credentials." });
    }

    private string GenerateToken(Staff staff)
    {
        // Implement your token generation logic here (e.g., JWT)
        return "your_generated_token";
    }
    // Hash the input password using SHA256
    public static string HashPassword(string input)
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

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
