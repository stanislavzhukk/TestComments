using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Repositories
{
    public class Model1Repository : IModel1Repository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Model1Repository> _logger;

        public Model1Repository(ApplicationDbContext context, ILogger<Model1Repository> logger)
        {
            _context = context;
            _logger = logger;
        }


        //Task<Model1Response> GetByIdAsync(Guid id) w normalnym świecie lub mapujemy dto na response
        public async Task<Model1> GetByIdAsync(Guid id)
        {
            var model = await _context.FindAsync<Model1>(id);
            if(model == null)
            {
                throw new Exception($"Model1 with id {id} not found.");
            }
            return model;
        }

        //Task<Model1Response> AddAsync
        public async Task<bool> AddAsync(Model1 model)
        {
            try
            {
                Validator.ValidateObject(model, new ValidationContext(model), validateAllProperties: true);
                await _context.AddAsync(model);
                await _context.SaveChangesAsync();
                return true;
            }
            catch(ValidationException ex)
            {
                _logger.LogCritical($"Validation error: {ex.Message}");
                return false;
            }
        }

        public async Task<int> CountAsync()
        {
            return await _context.Set<Model1>().CountAsync();
        }
    }
}
