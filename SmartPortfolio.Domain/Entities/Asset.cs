namespace SmartPortfolio.Domain.Entities;

public class Asset
{
    public Guid Id { get; private set; }
    public Guid PortfolioId { get; private set; }

    public string Ticker { get; private set; } = string.Empty;

    private readonly List<AssetLot> _assetLots = new();
    public IReadOnlyCollection<AssetLot> AssetLots => _assetLots.AsReadOnly(); 
    public decimal TotalQuantity => _assetLots.Sum(l => l.Quantity);

    private Asset() { }

    public Asset(Guid portfolioId, string ticker)
    {
        if (string.IsNullOrEmpty(ticker))
        {
            throw new ArgumentException("Ticker cant be empty.");
        }

        Id = Guid.NewGuid();
        PortfolioId = portfolioId;
        Ticker = ticker.ToUpper();
    }
    public void AddLot(decimal quantity, decimal pricePerShare, DateTime? purchaseDate = null)
    {
        _assetLots.Add(new AssetLot(Id, quantity, pricePerShare, purchaseDate));
    }

    public void RemoveQuantity(decimal quantityToRemove)
    {
        if (quantityToRemove > TotalQuantity)
            throw new InvalidOperationException($"Not enough shares of {Ticker} to sell. You have {TotalQuantity}, trying to sell {quantityToRemove}.");

        var remainingToRemove = quantityToRemove;

        var oldestLots = _assetLots.Where(l => l.Quantity > 0).OrderBy(l => l.PurchaseDate).ToList();

        foreach (var lot in oldestLots)
        {
            if (remainingToRemove == 0) break;

            if (lot.Quantity <= remainingToRemove)
            {
                remainingToRemove -= lot.Quantity;
                lot.ReduceQuantity(lot.Quantity);
            }
            else
            {
                lot.ReduceQuantity(remainingToRemove);
                remainingToRemove = 0;
            }
        }
        _assetLots.RemoveAll(l => l.Quantity == 0);
    }
}