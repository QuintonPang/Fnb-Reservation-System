public class Queue
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public string ContactNumber { get; set; }
    public int NumberOfGuests { get; set; }
    public string SpecialRequests { get; set; }
    public bool IsSeated { get; set; } // True if seated, false otherwise
        public bool NoShow { get; set; } // True if seated, false otherwise
        public string tableId { get; set; } // Foreign key to the Table entity
        public int outletId { get; set; }
            public DateTime DateTime { get; set; }

             // Constructor to initialize DateTime with a default value (e.g., current date and time)
    public Queue()
    {
        DateTime = DateTime.Now; // Default value is the current date and time
        NoShow = false; // Default value for NoShow
    }


}
