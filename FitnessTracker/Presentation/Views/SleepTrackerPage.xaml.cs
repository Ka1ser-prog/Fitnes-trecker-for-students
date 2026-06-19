using FitnessTracker.Presentation.ViewModels;

namespace FitnessTracker.Presentation.Views;

public partial class SleepTrackerPage : ContentPage
{
    // Конструктор принимает ViewModel через Dependency Injection
    public SleepTrackerPage(ProductScannerViewModel viewModel)
    {
        InitializeComponent();

        // Связываем интерфейс XAML со встроенным контекстом данных
        BindingContext = viewModel;
    }
}
