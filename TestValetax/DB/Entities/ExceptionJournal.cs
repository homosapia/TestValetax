namespace TestValetax.DB.Entities
{
    public class ExceptionJournal
    {
        public long EventId { get; set; }
        public DateTime Timestamp { get; set; }
        public string QueryParams { get; set; } // JSON
        public string BodyParams { get; set; } // JSON
        public string StackTrace { get; set; }
        public string ExceptionType { get; set; } // "Secure" или "Exception"
    }
}
