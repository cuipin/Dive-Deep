using Dive_Deep.Models;

namespace Dive_Deep.Persistence
{
    public interface IRoomRepository
    {
        void Add(Product prodoct);
        void Delete(int id);
        List<Product> GetAll();
        Product? GetById(int id);
        void Update(Product product);
    }
}
