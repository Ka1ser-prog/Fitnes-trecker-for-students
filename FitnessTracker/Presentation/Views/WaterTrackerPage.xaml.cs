using FitnessTracker.Presentation.ViewModels;

namespace FitnessTracker.Presentation.Views;

public partial class WaterTrackerPage : ContentPage
{
    public WaterTrackerPage(ProductScannerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}