using System.Net.Http.Json;
using System.Text.Json.Nodes;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Repositories;

namespace FitnessTracker.Data.Repositories;

public class OpenFoodFactsRepository : IFoodRepository
{
    private readonly HttpClient _httpClient;

    public OpenFoodFactsRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;

        // ОБЯЗАТЕЛЬНО: Без User-Agent сервер Open Food Facts сбрасывает соединение (HttpRequestException)
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "FitnessTrackerMVP - Android/iOS - Version 1.0");
        }
    }

    public async Task<FoodProduct?> GetProductByBarcodeAsync(string barcode)
    {
        try
        {
            // Используем стабильный международный эндпоинт v2 API
            string url = $"https://world.openfoodfacts.org/api/v2/product/{barcode}.json";

            // Настройка таймаута на случай медленного мобильного интернета (7 секунд)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(7));

            var response = await _httpClient.GetAsync(url, cts.Token);
            if (!response.IsSuccessStatusCode) return null;

            var jsonNode = await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cts.Token);
            if (jsonNode == null || jsonNode["status"]?.GetValue<int>() == 0)
            {
                return null; // Продукт не найден в базе данных OFF
            }

            var productData = jsonNode["product"];
            if (productData == null) return null;

            var nutriments = productData["nutriments"];

            return new FoodProduct
            {
                Barcode = barcode,
                Name = productData["product_name_ru"]?.GetValue<string>()
                       ?? productData["product_name"]?.GetValue<string>()
                       ?? "Неизвестный продукт",
                Brand = productData["brands"]?.GetValue<string>() ?? "Без бренда",
                ImageUrl = productData["image_front_url"]?.GetValue<string>() ?? string.Empty,

                Calories = nutriments?["energy-kcal_100g"]?.GetValue<double>() ?? 0,
                Proteins = nutriments?["proteins_100g"]?.GetValue<double>() ?? 0,
                Fats = nutriments?["fat_100g"]?.GetValue<double>() ?? 0,
                Carbs = nutriments?["carbohydrates_100g"]?.GetValue<double>() ?? 0
            };
        }
        catch (Exception ex)
        {
            // Выводим реальную причину сетевой ошибки в окно отладки Visual Studio
            System.Diagnostics.Debug.WriteLine($"[OFF API Error]: {ex.Message}");
            return null;
        }
    }
}
