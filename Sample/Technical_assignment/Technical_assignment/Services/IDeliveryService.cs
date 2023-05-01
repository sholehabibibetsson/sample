using Technical_assignment.Contracts;

namespace Technical_assignment.Services
{
    public interface IDeliveryService
    {
        public Task<List<Delivery>> CheckDeliveryDates(string postalCode, List<Product> products);
    }
}