using System.Media;
using System.Reflection;

namespace WorkingCandle;

/// <summary>
/// Service responsible for playing notification sounds.
/// </summary>
public class NotificationService
{
    private readonly SoundPlayer? _soundPlayer;

    /// <summary>
    /// Initializes a new instance of the NotificationService class.
    /// </summary>
    public NotificationService()
    {
        try
        {
            // Load embedded sound resource
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "WorkingCandle.Resources.completion.wav";
            var stream = assembly.GetManifestResourceStream(resourceName);
            
            if (stream != null)
            {
                _soundPlayer = new SoundPlayer(stream);
                _soundPlayer.Load(); // Pre-load the sound
            }
        }
        catch (Exception ex)
        {
            // Silent failure - graceful degradation if sound resource is missing
            Console.WriteLine($"Warning: Could not load completion sound: {ex.Message}");
        }
    }

    /// <summary>
    /// Plays the completion sound when the timer finishes.
    /// </summary>
    public void PlayCompletionSound()
    {
        try
        {
            _soundPlayer?.Play();
        }
        catch (Exception ex)
        {
            // Silent failure - graceful degradation
            Console.WriteLine($"Warning: Could not play completion sound: {ex.Message}");
        }
    }
}
