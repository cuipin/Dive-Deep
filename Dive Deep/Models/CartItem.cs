namespace Dive_Deep.Models
{
    public class CartItem

    {
        public int CartItemId { get; set; } // primary key
        public int CartId { get; set; } // foreign key

        public Cart Cart { get; set; } //navigation property

        public int ProductId { get; set; }  //fkey for product
        public Product Product { get; set; } // nav property

        public string ProductName { get; set; }
        public string Brand { get; set; }
        public string ImageFileName { get; set; }
        public decimal PricePerDay { get; set; }
        public string SelectedSize { get; set; }
        public string SelectedGender { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime StartDato { get; set; }
        public DateTime SlutDato { get; set; }
    }
}