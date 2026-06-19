using FitnessTracker.Presentation.ViewModels;

namespace FitnessTracker.Presentation.Views;

public partial class VoiceInputPage : ContentPage
{
    public VoiceInputPage(ProductScannerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}