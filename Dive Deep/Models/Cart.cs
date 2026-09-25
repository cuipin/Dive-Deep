namespace Dive_Deep.Models
{
    public class Cart
    {
        public int CartId { get; set; } // P rimary key for cart og EF
        public string? UserId { get; set; } //guest carts

        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}