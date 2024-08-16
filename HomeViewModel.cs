namespace NBRMproject.Models
{
    public class HomeViewModel
    {
        public IEnumerable<ExchangeRate> ExchangeRates { get; set; }
        public DateRangeModel DateRange { get; set; }
    }
}
