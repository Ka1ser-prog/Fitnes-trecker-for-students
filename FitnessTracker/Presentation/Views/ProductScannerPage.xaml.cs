using FitnessTracker.Presentation.ViewModels;
using ZXing.Net.Maui;

namespace FitnessTracker.Presentation.Views;

public partial class ProductScannerPage : ContentPage
{
    private readonly ProductScannerViewModel _viewModel;

    public ProductScannerPage(ProductScannerViewModel viewModel)
    {
        InitializeComponent(); // Ошибка пропадет, как только мы очистим кэш
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Настройка камеры (теперь BarcodeScanner распознается)
        BarcodeScanner.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.Ean13 | BarcodeFormat.Ean8,
            AutoRotate = true,
            Multiple = false
        };
    }

    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var firstBarcode = e.Results.FirstOrDefault();
        if (firstBarcode == null) return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (_viewModel.IsBusy) return;
            _viewModel.BarcodeText = firstBarcode.Value;

            if (_viewModel.SearchProductCommand.CanExecute(null))
            {
                await _viewModel.SearchProductCommand.ExecuteAsync(null);
            }
        });
    }
}
