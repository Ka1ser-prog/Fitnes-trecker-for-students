namespace FitnessTracker.Domain.Repositories;

public interface IVoiceRecognitionService
{
    event Action<string>? PartialResultReceived;
    event Action<string>? FinalResultReceived;

    Task InitializeAsync();
    Task StartListeningAsync();
    Task StopListeningAsync();
}
