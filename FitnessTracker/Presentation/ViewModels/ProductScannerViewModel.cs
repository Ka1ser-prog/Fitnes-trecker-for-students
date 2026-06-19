using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Repositories;

namespace FitnessTracker.Presentation.ViewModels;

public partial class ProductScannerViewModel : ObservableObject
{
    private readonly IFoodRepository _foodRepository;
    private readonly IVoiceRecognitionService _voiceService;
    private readonly IFoodDatabaseService _databaseService; // НОВЫЙ СЕРВИС
    private IDispatcherTimer? _reminderTimer;

    // Коллекция для вывода списка продуктов на экран (ObservableCollection сама обновляет UI при изменениях)
    public ObservableCollection<FoodProduct> SavedProducts { get; set; } = new();

    [ObservableProperty] private string _barcodeText = string.Empty;
    [ObservableProperty] private FoodProduct? _foodData;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isProductVisible;
    [ObservableProperty] private bool _isErrorVisible;

    // Свойства ручного ввода
    [ObservableProperty] private string _manualName = string.Empty;
    [ObservableProperty] private string _manualCalories = string.Empty;
    [ObservableProperty] private string _manualProteins = string.Empty;
    [ObservableProperty] private string _manualFats = string.Empty;
    [ObservableProperty] private string _manualCarbs = string.Empty;
    [ObservableProperty] private string _manualSuccessMessage = string.Empty;
    [ObservableProperty] private bool _isManualSuccessVisible;

    // Свойства воды, сна и калорий дневника
    [ObservableProperty] private int _targetWater;
    [ObservableProperty] private double _waterProgress;
    [ObservableProperty] private bool _isReminderEnabled;
    [ObservableProperty] private TimeSpan _sleepTime = new(23, 0, 0);
    [ObservableProperty] private TimeSpan _wakeTime = new(7, 0, 0);
    [ObservableProperty] private string _sleepDurationText = string.Empty;
    [ObservableProperty] private string _sleepRecommendation = string.Empty;
    [ObservableProperty] private bool _isSleepResultVisible;
    [ObservableProperty] private string _voiceText = "Нажмите микрофон...";
    [ObservableProperty] private bool _isRecording;

    [ObservableProperty] private double _totalCaloriesToday; // Сводка за день

    // Конструктор: Внедряем три сервиса через DI
    public ProductScannerViewModel(IFoodRepository foodRepository, IVoiceRecognitionService voiceService, IFoodDatabaseService databaseService)
    {
        _foodRepository = foodRepository;
        _voiceService = voiceService;
        _databaseService = databaseService;

        _voiceService.PartialResultReceived += OnVoicePartialResult;
        _voiceService.FinalResultReceived += OnVoiceFinalResult;

        _reminderTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_reminderTimer != null)
        {
            _reminderTimer.Interval = TimeSpan.FromSeconds(10);
            _reminderTimer.Tick += OnReminderTimerTick;
            _reminderTimer.Start();
        }

        _ = LoadSavedProductsAsync();
    }

    public async Task LoadSavedProductsAsync()
    {
        var products = await _databaseService.GetProductsAsync();

        SavedProducts.Clear();
        double caloriesSum = 0;

        foreach (var product in products)
        {
            SavedProducts.Add(product);
            caloriesSum += product.Calories;
        }

        TotalCaloriesToday = caloriesSum;
    }

    [RelayCommand]
    private async Task SearchProduct()
    {
        if (string.IsNullOrWhiteSpace(BarcodeText)) return;
        IsBusy = true; ErrorMessage = string.Empty; IsErrorVisible = false; FoodData = null; IsProductVisible = false;

        var product = await _foodRepository.GetProductByBarcodeAsync(BarcodeText);
        IsBusy = false;

        if (product != null)
        {
            FoodData = product;
            IsProductVisible = true;

            await _databaseService.InsertProductAsync(product);
            await LoadSavedProductsAsync();
        }
        else
        {
            ErrorMessage = "Продукт отсутствует в базе Open Food Facts.";
            IsErrorVisible = true;
        }
    }


    [RelayCommand]
    private async Task SaveManualProduct()
    {
        if (string.IsNullOrWhiteSpace(ManualName))
        {
            ErrorMessage = "Введите название продукта!"; IsErrorVisible = true; return;
        }

        double.TryParse(ManualCalories, out double calories);
        double.TryParse(ManualProteins, out double proteins);
        double.TryParse(ManualFats, out double fats);
        double.TryParse(ManualCarbs, out double carbs);

        var userProduct = new FoodProduct
        {
            Barcode = "Введено вручную",
            Name = ManualName,
            Brand = "Мой продукт",
            Calories = calories,
            Proteins = proteins,
            Fats = fats,
            Carbs = carbs,
            ImageUrl = "dotnet_bot.png" 
        };

        await _databaseService.InsertProductAsync(userProduct);
        await LoadSavedProductsAsync(); 

        IsErrorVisible = false;
        ManualSuccessMessage = $"Продукт '{ManualName}' успешно добавлен!";
        IsManualSuccessVisible = true;

        ManualName = ManualCalories = ManualProteins = ManualFats = ManualCarbs = string.Empty;
        await Task.Delay(2000);
        IsManualSuccessVisible = false;
    }


    [RelayCommand]
    private async Task ClearHistory()
    {
        await _databaseService.ClearAllProductsAsync();
        await LoadSavedProductsAsync();
    }

    [RelayCommand]
    private void AddWater(string? amountStr)
    {
        string input = !string.IsNullOrEmpty(amountStr) ? amountStr : CustomWaterAmount;

        if (int.TryParse(input, out int amount) && amount > 0)
        {
            TargetWater += amount;

            WaterProgress = Math.Min((double)TargetWater / 2000, 1.0);

            CustomWaterAmount = string.Empty;
        }
    }
    [RelayCommand] private async Task ToggleVoice() { if (!IsRecording) { IsRecording = true; VoiceText = "Слушаю вас..."; await _voiceService.StartListeningAsync(); _ = Task.Run(async () => { string[] chunks = { "выпил", "выпил стакан воды" }; foreach (var c in chunks) { if (!IsRecording) break; await Task.Delay(800); MainThread.BeginInvokeOnMainThread(() => VoiceText = $"[Распознаю]: {c}"); } }); } else { IsRecording = false; await _voiceService.StopListeningAsync(); string res = "выпил стакан воды"; VoiceText = $"Успешно распознано: \"{res}\""; ProcessVoiceCommand(res); } }
    private void OnReminderTimerTick(object? sender, EventArgs e) { if (IsReminderEnabled) MainThread.BeginInvokeOnMainThread(async () => { if (Application.Current?.MainPage != null) await Application.Current.MainPage.DisplayAlert("Пора выпить воды! 💧", "Сделайте пару глотков чистой воды.", "ОК"); }); }
    private void OnVoicePartialResult(string text) => MainThread.BeginInvokeOnMainThread(() => VoiceText = $"[Распознаю]: {text}");
    private void OnVoiceFinalResult(string text) => MainThread.BeginInvokeOnMainThread(() => { VoiceText = $"Успешно распознано: \"{text}\""; ProcessVoiceCommand(text); });
    private void ProcessVoiceCommand(string text) { if (string.IsNullOrWhiteSpace(text)) return; string lower = text.ToLower(); if (lower.Contains("вод") || lower.Contains("выпил")) { AddWater("250"); VoiceText = "Голосовая команда: Добавлено 250 мл воды!"; } }
    [RelayCommand] private async Task NavigateToScanner() => await Shell.Current.GoToAsync("ProductScannerPage");
    [RelayCommand]
    private async Task NavigateToDashboard()
    {
        await Shell.Current.GoToAsync("///MainDashboardPage");
    }
    [ObservableProperty]
    private string _customWaterAmount = string.Empty;
    // Новые свойства для автоматического трекера сна
    [ObservableProperty] private string _autoSleepText = "Приложение пока не зафиксировало длительный ночной перерыв.";
    [ObservableProperty] private string _autoSleepRecommendation = string.Empty;
    [ObservableProperty] private bool _isAutoSleepVisible;

    // Метод вызывается из App.xaml.cs, когда зафиксирован долгий перерыв в работе приложения
    public void SetAutomaticSleepData(DateTime startSleep, DateTime endSleep, TimeSpan duration)
    {
        // Форматируем текст
        AutoSleepText = $"Вы отсутствовали с {startSleep:HH:mm} до {endSleep:HH:mm}.\nПредположительное время сна: {duration.Hours} ч. {duration.Minutes} мин.";

        // Экспертная ИИ-оценка автоматического сна
        if (duration.TotalHours >= 7 && duration.TotalHours <= 9)
        {
            AutoSleepRecommendation = "🟢 Отличный показатель! Время вашего отдыха полностью укладывается в медицинскую норму здорового сна (7-9 часов).";
        }
        else if (duration.TotalHours < 7)
        {
            AutoSleepRecommendation = "🔴 Зафиксирован недосып. Если вы спали всё это время, ваш организм не успел полностью восстановиться.";
        }
        else
        {
            AutoSleepRecommendation = "🟡 Внимание: Время отсутствия превышает 9 часов. Постарайтесь не пересыпать, чтобы избежать вялости.";
        }

        IsAutoSleepVisible = true;
    }

}
