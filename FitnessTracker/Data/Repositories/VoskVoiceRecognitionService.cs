using FitnessTracker.Domain.Repositories;

namespace FitnessTracker.Data.Repositories;

public class VoskVoiceRecognitionService : IVoiceRecognitionService, IDisposable
{
    public event Action<string>? PartialResultReceived;
    public event Action<string>? FinalResultReceived;

#if ANDROID || IOS
    // Код Vosk оставляем строго для мобильных платформ, где нет конфликтов CLR
    private Vosk.Model? _voskModel;
    private Vosk.VoskRecognizer? _recognizer;
    private bool _isInitialized;
#endif

    public async Task InitializeAsync()
    {
#if ANDROID || IOS
        if (_isInitialized) return;
        string targetModelPath = Path.Combine(FileSystem.AppDataDirectory, "vosk-model");
        
        await Task.Run(() =>
        {
            Vosk.Vosk.SetLogLevel(-1);
            _voskModel = new Vosk.Model(targetModelPath);
            _recognizer = new Vosk.VoskRecognizer(_voskModel, 16000.0f);
        });
        _isInitialized = true;
#else
        // На Windows метод просто мгновенно завершается, не вызывая сбойных DLL
        await Task.CompletedTask;
#endif
    }

    public async Task StartListeningAsync()
    {
        await InitializeAsync();

#if ANDROID || IOS
        // Здесь запускается мобильный AudioRecorder
#else
        // На Windows эмулируем начало прослушивания
        PartialResultReceived?.Invoke("слушаю...");
        await Task.CompletedTask;
#endif
    }

    public Task StopListeningAsync()
    {
#if ANDROID || IOS
        // Останавливаем мобильный микрофон
#endif
        return Task.CompletedTask;
    }

    public void Dispose()
    {
#if ANDROID || IOS
        _recognizer?.Dispose();
        _voskModel?.Dispose();
#endif
    }
}
