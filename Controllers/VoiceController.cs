using BuddyBee.Api.DTOs;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BuddyBee.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoiceController : ControllerBase
    {
        private readonly IVoiceService _voiceService;
        private readonly ILogger<VoiceController> _logger;

        public VoiceController(
            IVoiceService voiceService,
            ILogger<VoiceController> logger)
        {
            _voiceService = voiceService;
            _logger = logger;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test(
            CancellationToken cancellationToken)
        {
            const string testSentence =
                "Hello Mayank. I am BuddyBee. This is my new voice.";

            try
            {
                _logger.LogInformation(
                    "Voice test endpoint called.");

                var audioStream = await _voiceService.SynthesizeAsync(
                    testSentence,
                    cancellationToken);

                _logger.LogInformation(
                    "Voice test succeeded, returning audio stream.");

                return File(audioStream, "audio/mpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Voice test failed.");

                return StatusCode(
                    500,
                    new { error = ex.Message });
            }
        }

        [HttpPost("speak")]
        public async Task<IActionResult> Speak(
            [FromBody] SpeakRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest(
                    new { error = "Text is required." });
            }

            var speechText =
                SpeechTextNormalizer.Normalize(request.Text);

            _logger.LogInformation(
                "Voice speak endpoint called. " +
                "Original: {OriginalLength} chars. " +
                "Normalized: {NormalizedLength} chars. " +
                "Speech text: {SpeechText}",
                request.Text.Length,
                speechText.Length,
                speechText);

            try
            {
                var audioStream =
                    await _voiceService.SynthesizeAsync(
                        speechText,
                        cancellationToken);

                _logger.LogInformation(
                    "Voice speak succeeded, returning audio stream.");

                return File(
                    audioStream,
                    "audio/mpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Voice speak failed.");

                return StatusCode(
                    500,
                    new { error = ex.Message });
            }
        }
    }
}