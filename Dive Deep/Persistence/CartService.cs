using System.Text.Json;

namespace Dive_Deep.Persistence
{
    public static class CartService
    {
        private const string SessionKey = "Cart";

        public static List<int> GetCart(HttpContext context)
        {
            var json = context.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json))
            {
                return new List<int>();
            }

            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }

        public static void SaveCart(HttpContext context, List<int> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            context.Session.SetString(SessionKey, json);
        }

        public static void AddToCart(HttpContext context, int productId)
        {
            var cart = GetCart(context);
            cart.Add(productId);
            SaveCart(context, cart);
        }

        public static void RemoveAt(HttpContext context, int index)
        {
            var cart = GetCart(context);
            if (index >= 0 && index < cart.Count)
            {
                cart.RemoveAt(index);
                SaveCart(context, cart);
            }
        }

        public static void Clear(HttpContext context)
        {
            context.Session.Remove(SessionKey);
        }
    }
}