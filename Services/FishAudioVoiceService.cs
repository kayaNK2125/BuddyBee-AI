using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BuddyBee.Api.Configuration;
using BuddyBee.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace BuddyBee.Api.Services
{
    public class FishAudioVoiceService : IVoiceService
    {
        private readonly HttpClient _httpClient;
        private readonly FishAudioSettings _settings;
        private readonly ILogger<FishAudioVoiceService> _logger;

        private const string FishTtsEndpoint = "https://api.fish.audio/v1/tts";
        private const string Model = "s2.1-pro-free";
        private const string ReferenceId = "bf322df2096a46f18c579d0baa36f41d";

        public FishAudioVoiceService(
            HttpClient httpClient,
            IOptions<FishAudioSettings> settings,
            ILogger<FishAudioVoiceService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<Stream> SynthesizeAsync(
            string text,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty.", nameof(text));

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                throw new InvalidOperationException(
                    "Fish Audio API key is not configured. " +
                    "Set FishAudio:ApiKey in User Secrets.");

            var requestBody = new
            {
                text,
                reference_id = ReferenceId,
                format = "mp3"
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                FishTtsEndpoint);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            request.Headers.TryAddWithoutValidation("model", Model);

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation(
                "Fish Audio TTS request started, text length: {Length}",
                text.Length);

            var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody =
                    await response.Content.ReadAsStringAsync(cancellationToken);

                response.Dispose();

                _logger.LogError(
                    "Fish Audio TTS failed. Status: {Status}. Response: {Response}",
                    (int)response.StatusCode,
                    errorBody);

                throw new HttpRequestException(
                    $"Fish Audio TTS failed with status {(int)response.StatusCode}: {errorBody}");
            }

            var audioStream =
                await response.Content.ReadAsStreamAsync(cancellationToken);

            _logger.LogInformation(
                "Fish Audio TTS stream opened successfully.");

            // The returned stream owns the HttpResponseMessage.
            // When the caller finishes/disposes the stream,
            // the underlying HTTP response is disposed as well.
            return new ResponseOwnedStream(audioStream, response);
        }

        private sealed class ResponseOwnedStream : Stream
        {
            private readonly Stream _innerStream;
            private readonly HttpResponseMessage _response;

            public ResponseOwnedStream(
                Stream innerStream,
                HttpResponseMessage response)
            {
                _innerStream = innerStream;
                _response = response;
            }

            public override bool CanRead => _innerStream.CanRead;
            public override bool CanSeek => _innerStream.CanSeek;
            public override bool CanWrite => _innerStream.CanWrite;
            public override long Length => _innerStream.Length;

            public override long Position
            {
                get => _innerStream.Position;
                set => _innerStream.Position = value;
            }

            public override void Flush() =>
                _innerStream.Flush();

            public override Task FlushAsync(
                CancellationToken cancellationToken) =>
                _innerStream.FlushAsync(cancellationToken);

            public override int Read(
                byte[] buffer,
                int offset,
                int count) =>
                _innerStream.Read(buffer, offset, count);

            public override int Read(
                Span<byte> buffer) =>
                _innerStream.Read(buffer);

            public override Task<int> ReadAsync(
                byte[] buffer,
                int offset,
                int count,
                CancellationToken cancellationToken) =>
                _innerStream.ReadAsync(
                    buffer,
                    offset,
                    count,
                    cancellationToken);

            public override ValueTask<int> ReadAsync(
                Memory<byte> buffer,
                CancellationToken cancellationToken = default) =>
                _innerStream.ReadAsync(
                    buffer,
                    cancellationToken);

            public override long Seek(
                long offset,
                SeekOrigin origin) =>
                _innerStream.Seek(offset, origin);

            public override void SetLength(long value) =>
                _innerStream.SetLength(value);

            public override void Write(
                byte[] buffer,
                int offset,
                int count) =>
                _innerStream.Write(buffer, offset, count);

            public override void Write(
                ReadOnlySpan<byte> buffer) =>
                _innerStream.Write(buffer);

            public override Task WriteAsync(
                byte[] buffer,
                int offset,
                int count,
                CancellationToken cancellationToken) =>
                _innerStream.WriteAsync(
                    buffer,
                    offset,
                    count,
                    cancellationToken);

            public override ValueTask WriteAsync(
                ReadOnlyMemory<byte> buffer,
                CancellationToken cancellationToken = default) =>
                _innerStream.WriteAsync(
                    buffer,
                    cancellationToken);

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _innerStream.Dispose();
                    _response.Dispose();
                }

                base.Dispose(disposing);
            }

            public override async ValueTask DisposeAsync()
            {
                await _innerStream.DisposeAsync();
                _response.Dispose();

                GC.SuppressFinalize(this);
            }
        }
    }
}