namespace FitnessTracker.Domain.Entities;

public class FoodProduct
{
    public int Id { get; set; } 
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public double Calories { get; set; }
    public double Proteins { get; set; }
    public double Fats { get; set; }
    public double Carbs { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}