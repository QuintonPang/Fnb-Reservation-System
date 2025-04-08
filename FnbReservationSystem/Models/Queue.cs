public class Queue
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public string ContactNumber { get; set; }
    public int NumberOfGuests { get; set; }
    public string SpecialRequests { get; set; }
    public bool IsSeated { get; set; } // True if seated, false otherwise
        public int outletId { get; set; }

}
