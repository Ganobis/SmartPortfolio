namespace SmartPortfolio.Domain.Entities;

public class AssetLot
{
    public Guid Id { get; private set; }
    public Guid AssetId { get; private set; }

    public decimal Quantity { get; private set; }
    public decimal PricePerShare { get; private set; }
    public DateTime PurchaseDate { get; private set; }

    private AssetLot() { }

    public AssetLot(Guid assetId, decimal quantity, decimal pricePerShare, DateTime? purchaseDate = null)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.");
        }
        if (pricePerShare < 0)
        {
            throw new ArgumentException("Price cannot be negative.");
        }

        Id = Guid.NewGuid();
        AssetId = assetId;
        Quantity = quantity;
        PricePerShare = pricePerShare;
        PurchaseDate = purchaseDate ?? DateTime.UtcNow;
    }

    public void ReduceQuantity(decimal amount)
    {
        if (amount > Quantity)
        {
            throw new InvalidOperationException("Cannot reduce more than the lot contains.");
        }

        Quantity -= amount;
    }
}