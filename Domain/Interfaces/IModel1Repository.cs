using Domain.Models;

namespace Domain.Interfaces
{
    public interface IModel1Repository
    {
        Task<Model1> GetByIdAsync(Guid id);
        Task<bool> AddAsync(Model1 model);
    }
}
