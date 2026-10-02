
namespace Domain.Models
{
    public class Model1
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<Model2>? Model2s { get; set; }
    }
}
