using FitnessTracker.Presentation.Views;

namespace FitnessTracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // РЕГИСТРАЦИЯ МАРШРУТА: Связываем текстовое имя "ProductScannerPage" с классом страницы
        Routing.RegisterRoute("ProductScannerPage", typeof(ProductScannerPage));
    }
}