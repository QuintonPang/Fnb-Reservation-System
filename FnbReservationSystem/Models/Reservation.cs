
public class Reservation
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public string ContactNumber { get; set; }
    public DateTime ReservationDateTime { get; set; }
    public int NumberOfGuests { get; set; }
    public string Status { get; set; } // Pending, Confirmed, Cancelled
    public int outletId { get; set; }

}
