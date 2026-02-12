namespace SmartPortfolio.Infrastructure.Services;

public class NbpResponse
{
    public class NbpTable
    {
        public required string Table { get; set; }
        public required string No { get; set; }
        public required string EffectiveDate { get; set; }
        public required List<NbpRate> Rates { get; set; }
    }

    public class NbpRate
    {
        public required string Currency { get; set; }
        public required string Code { get; set; }
        public decimal Mid { get; set; }
    }
}

