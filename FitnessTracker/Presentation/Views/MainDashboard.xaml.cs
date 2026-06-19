using FitnessTracker.Presentation.ViewModels;

namespace FitnessTracker.Presentation.Views;

public partial class MainDashboardPage : ContentPage
{
    private readonly ProductScannerViewModel _viewModel;

    public MainDashboardPage(ProductScannerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    // Этот метод срабатывает КАЖДЫЙ РАЗ, когда страница появляется на экране пользователя
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Принудительно заставляем ViewModel заново прочитать базу данных Microsoft.Data.Sqlite
        if (_viewModel != null)
        {
            await _viewModel.LoadSavedProductsAsync();
        }
    }
}
