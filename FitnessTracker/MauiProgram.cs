using CommunityToolkit.Maui;
using FitnessTracker.Data.Repositories;
using FitnessTracker.Domain.Repositories;
using FitnessTracker.Presentation.ViewModels;
using FitnessTracker.Presentation.Views;
using ZXing.Net.Maui.Controls;

namespace FitnessTracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // ИСПРАВЛЕНО: Используем правильный метод CreateBuilder() вместо CreateMauiAppBuilder()
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() // Активируем пакет Toolkit
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Регистрация инфраструктурных зависимостей
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<IFoodRepository, OpenFoodFactsRepository>();
        builder.Services.AddSingleton<IVoiceRecognitionService, VoskVoiceRecognitionService>();
        builder.Services.AddSingleton<IFoodDatabaseService, SqliteFoodDatabaseService>();
        // Регистрация слоев представления (MVVM)
        builder.Services.AddTransient<ProductScannerViewModel>();
        builder.Services.AddTransient<ProductScannerPage>();
        builder.Services.AddTransient<VoiceInputPage>();
        builder.Services.AddTransient<MainDashboardPage>();
        builder.Services.AddTransient<WaterTrackerPage>();
        builder.Services.AddTransient<SleepTrackerPage>();
        return builder.Build();
    }
}
