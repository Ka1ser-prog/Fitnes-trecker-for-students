using FitnessTracker.Domain.Entities;
namespace FitnessTracker.Domain.Repositories;

public interface IFoodRepository
{
    Task<FoodProduct?> GetProductByBarcodeAsync(string barcode);
}