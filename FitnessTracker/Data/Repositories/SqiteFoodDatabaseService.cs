using Microsoft.Data.Sqlite;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Repositories;

namespace FitnessTracker.Data.Repositories;

public class SqliteFoodDatabaseService : IFoodDatabaseService
{
    // Строка подключения указывает путь к файлу БД в защищенной папке приложения
    private readonly string _connectionString = $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "FitnessTracker.db")}";

    public async Task InitializeAsync()
    {
        // Открываем соединение с базой данных
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Пишем классический SQL-запрос для создания таблицы, если её еще нет
        string createTableSql = @"
            CREATE TABLE IF NOT EXISTS FoodProducts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Barcode TEXT,
                Name TEXT,
                Brand TEXT,
                Calories REAL,
                Proteins REAL,
                Fats REAL,
                Carbs REAL,
                ImageUrl TEXT
            );";

        using var command = new SqliteCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<FoodProduct>> GetProductsAsync()
    {
        await InitializeAsync();
        var products = new List<FoodProduct>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Запрос на выборку всех продуктов, сортируем: новые вверху
        string selectSql = "SELECT * FROM FoodProducts ORDER BY Id DESC;";

        using var command = new SqliteCommand(selectSql, connection);
        using var reader = await command.ExecuteReaderAsync();

        // Построчно читаем данные из ответа базы данных
        while (await reader.ReadAsync())
        {
            products.Add(new FoodProduct
            {
                Id = reader.GetInt32(0),
                Barcode = reader.GetString(1),
                Name = reader.GetString(2),
                Brand = reader.GetString(3),
                Calories = reader.GetDouble(4),
                Proteins = reader.GetDouble(5),
                Fats = reader.GetDouble(6),
                Carbs = reader.GetDouble(7),
                ImageUrl = reader.GetString(8)
            });
        }

        return products;
    }

    public async Task InsertProductAsync(FoodProduct product)
    {
        await InitializeAsync();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Безопасный SQL-запрос с параметрами (защита от SQL-инъекций)
        string insertSql = @"
            INSERT INTO FoodProducts (Barcode, Name, Brand, Calories, Proteins, Fats, Carbs, ImageUrl)
            VALUES (@Barcode, @Name, @Brand, @Calories, @Proteins, @Fats, @Carbs, @ImageUrl);";

        using var command = new SqliteCommand(insertSql, connection);
        command.Parameters.AddWithValue("@Barcode", product.Barcode);
        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@Brand", product.Brand);
        command.Parameters.AddWithValue("@Calories", product.Calories);
        command.Parameters.AddWithValue("@Proteins", product.Proteins);
        command.Parameters.AddWithValue("@Fats", product.Fats);
        command.Parameters.AddWithValue("@Carbs", product.Carbs);
        command.Parameters.AddWithValue("@ImageUrl", string.IsNullOrEmpty(product.ImageUrl) ? "dotnet_bot.png" : product.ImageUrl);

        await command.ExecuteNonQueryAsync();
    }

    public async Task ClearAllProductsAsync()
    {
        await InitializeAsync();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // SQL-запрос полной очистки таблицы
        string deleteSql = "DELETE FROM FoodProducts;";

        using var command = new SqliteCommand(deleteSql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
