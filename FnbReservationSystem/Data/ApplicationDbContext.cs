using Microsoft.EntityFrameworkCore;
using FnbReservationSystem.Models;
namespace FnbReservationSystem.Data
{public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Queue> Queues { get; set; }
    public DbSet<Outlet> Outlets { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<BannedCustomer> BannedCustomers { get; set; }
}

}