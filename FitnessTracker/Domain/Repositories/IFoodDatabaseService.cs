using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Domain.Repositories;

public interface IFoodDatabaseService
{
    Task InitializeAsync();
    Task<List<FoodProduct>> GetProductsAsync();
    Task InsertProductAsync(FoodProduct product);
    Task ClearAllProductsAsync();
}
