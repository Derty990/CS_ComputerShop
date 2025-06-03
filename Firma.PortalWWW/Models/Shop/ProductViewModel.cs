namespace Firma.PortalWWW.Models.Shop
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public required string Description { get; set; }
        public required string ShortDescription { get; set; }
        public required string ImageUrl { get; set; }
        public required string Category { get; set; }
        public int CategoryId { get; set; }
        public double Rating { get; set; }
        public int Reviews { get; set; }
        public bool IsOnSale { get; set; }
        public bool IsInStock { get; set; }
    }
}
