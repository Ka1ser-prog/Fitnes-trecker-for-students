using FitnessTracker.Presentation.ViewModels;

namespace FitnessTracker;

public partial class App : Application
{
    private readonly ProductScannerViewModel _viewModel;

    public App(ProductScannerViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        MainPage = new AppShell();
    }

    protected override void OnStart()
    {
        base.OnStart();
        CalculateAutomaticSleep();
    }

    protected override void OnResume()
    {
        base.OnResume();
        CalculateAutomaticSleep();
    }

    protected override void OnSleep()
    {
        base.OnSleep();

        Preferences.Default.Set("LastAppCloseTime", DateTime.Now);
    }

    private void CalculateAutomaticSleep()
    {
        if (Preferences.Default.ContainsKey("LastAppCloseTime"))
        {
            DateTime closeTime = Preferences.Default.Get<DateTime>("LastAppCloseTime", DateTime.Now);
            DateTime openTime = DateTime.Now;

            TimeSpan timeAway = openTime - closeTime;

            if (timeAway.TotalHours >= 3)
            {
                _viewModel.SetAutomaticSleepData(closeTime, openTime, timeAway);
            }
        }
    }
}
