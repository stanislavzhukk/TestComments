using Application.Interfaces;
using Domain.Interfaces;
using Domain.Models;


namespace Application.Services
{
    public class Model1Service : IModel1Service
    {
        private readonly IModel1Repository _repository;
        public Model1Service(IModel1Repository repository)
        {
            _repository = repository;
        }

        public async Task<Model1> GetByIdAsync(Guid id)
        {
            var model = await _repository.GetByIdAsync(id);
            if (model == null)
            {
                throw new KeyNotFoundException($"Model1 with id {id} not found.");
            }
            return model;
        }

        public async Task AddAsync(Model1 model)
        {
            await _repository.AddAsync(model);
        }
    }
}
