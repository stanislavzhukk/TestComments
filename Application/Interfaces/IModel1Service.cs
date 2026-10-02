using Domain.Models;

namespace Application.Interfaces
{
    public interface IModel1Service
    {
        Task<Model1> GetByIdAsync(Guid id);
    }
}
