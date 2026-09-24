using Dive_Deep.Models;

namespace Dive_Deep.Persistence
{
    public class BookingRepository : IBookingRepository
    {
        private static readonly List<Booking> bookings = new List<Booking>();

        public void Add(Booking booking)
        {
            booking.BookingId = bookings.Count + 1;
            bookings.Add(booking);
        }

        public void Delete(int id)
        {
            var booking = GetById(id);

            if (booking != null)
            {
                bookings.Remove(booking);
            }
        }

        public List<Booking> GetAll()
        {
            return bookings;
        }

        public Booking? GetById(int id)
        {
            return bookings.FirstOrDefault(b => b.BookingId == id);
        }

        public void Update(Booking booking)
        {
            var existing = GetById(booking.BookingId);

            if (existing != null)
            {
                existing.ProductId = booking.ProductId;
                existing.UserId = booking.UserId;
                existing.StartTime = booking.StartTime;
                existing.EndTime = booking.EndTime;
            }
        }
    }
}