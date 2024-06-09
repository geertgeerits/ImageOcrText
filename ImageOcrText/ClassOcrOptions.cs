using Plugin.Maui.OCR;

namespace ImageOcrText
{
    internal sealed class ClassOcrOptions
    {
        public class OcrOptions
        {
            public string? Language { get; }
            public bool TryHard { get; }
            public List<OcrPatternConfig> PatternConfigs { get; }
            public CustomOcrValidationCallback? CustomCallback { get; }

            private OcrOptions(string? language, bool tryHard, List<OcrPatternConfig> patternConfigs, CustomOcrValidationCallback? customCallback)
            {
                Language = language;
                TryHard = tryHard;
                PatternConfigs = patternConfigs;
                CustomCallback = customCallback;
            }

            public class Builder
            {
                private string? _language;
                private bool _tryHard;
                private List<OcrPatternConfig> _patternConfigs = new List<OcrPatternConfig>();
                private CustomOcrValidationCallback? _customCallback;

                public Builder SetLanguage(string language)
                {
                    _language = language;
                    return this;
                }

                public Builder SetTryHard(bool tryHard)
                {
                    _tryHard = tryHard;
                    return this;
                }

                public Builder AddPatternConfig(OcrPatternConfig patternConfig)
                {
                    _patternConfigs.Add(patternConfig);
                    return this;
                }

                public Builder SetPatternConfigs(List<OcrPatternConfig> patternConfigs)
                {
                    _patternConfigs = patternConfigs ?? new List<OcrPatternConfig>();
                    return this;
                }

                public Builder SetCustomCallback(CustomOcrValidationCallback customCallback)
                {
                    _customCallback = customCallback;
                    return this;
                }

                public OcrOptions Build()
                {
                    return new OcrOptions(_language, _tryHard, _patternConfigs, _customCallback);
                }
            }
        }
    }
}

/*
// Using the OcrOptions.Builder to create an OcrOptions instance is straightforward and flexible:

var options = new OcrOptions.Builder()
    .SetLanguage("en-US")
    .SetTryHard(true)
    .AddPatternConfig(new OcrPatternConfig(@"\d{10}"))
    .SetCustomCallback(myCustomCallback)
    .Build();


// Recognize text from an image. OcrOptions contains options for the OCR service, including the language to use and whether to try hard.
RecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)

// SupportedLanguages
// A list of supported languages for the OCR service.
// This is populated after calling InitAsync. Allows you to know what language codes can be used in OcrOptions.


var options = new OcrOptions.Builder().SetTryHard(TryHardSwitch.IsToggled).Build();

// Process the image data using the OCR service
return await _ocr.RecognizeTextAsync(imageData, options, cancellationTokenSource.Token);
 */