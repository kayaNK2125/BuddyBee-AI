namespace BuddyBee.Api.Interfaces
{
    public interface IVoiceService
    {
        Task<Stream> SynthesizeAsync(
        string text,
        CancellationToken cancellationToken = default);
    }
}
