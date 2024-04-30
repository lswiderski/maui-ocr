namespace Plugin.Maui.OCR;

/// <summary>
/// OCR API.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Event triggered when OCR recognition is completed.
    /// </summary>
    event EventHandler<OcrCompletedEventArgs> RecognitionCompleted;

    /// <summary>
    /// BCP-47 language codes supported by the OCR service.
    /// </summary>
    IReadOnlyCollection<string> SupportedLanguages { get; }

    /// <summary>
    /// Initialize the OCR on the platform
    /// </summary>
    /// <param name="ct">An optional cancellation token</param>
    Task InitAsync(CancellationToken ct = default);

    /// <summary>
    /// Takes an image and returns the text found in the image.
    /// </summary>
    /// <param name="imageData">The image data</param>
    /// <param name="tryHard">True to try and tell the API to be more accurate, otherwise just be fast.</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    Task<OcrResult> RecognizeTextAsync(byte[] imageData, bool tryHard = false, CancellationToken ct = default);

    /// <summary>
    /// Takes an image and returns the text found in the image.
    /// </summary>
    /// <param name="imageData">The image data</param>
    /// <param name="options">The options for OCR</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    Task<OcrResult> RecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default);

    /// <summary>
    /// Takes an image, starts the OCR process and triggers the RecognitionCompleted event when completed.
    /// </summary>
    /// <param name="imageData">The image data</param>
    /// <param name="options">The options for OCR</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    Task StartRecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default);
}

/// <summary>
/// The options for OCR.
/// </summary>
/// <param name="Language">The BCP-47 language code</param>
/// <param name="TryHard">True to try and tell the API to be more accurate, otherwise just be fast.</param>
public record OcrOptions(string? Language = null, bool TryHard = false);

/// <summary>
/// Provides data for the RecognitionCompleted event.
/// </summary>
public class OcrCompletedEventArgs : EventArgs
{
    public OcrCompletedEventArgs(OcrResult? result, string? errorMessage = null)
    {
        Result = result;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    /// <summary>
    /// Any error message if the OCR operation failed, or empty string otherwise.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Indicates whether the OCR operation was successful.
    /// </summary>
    public bool IsSuccessful => Result?.Success ?? false;

    /// <summary>
    /// The result of the OCR operation.
    /// </summary>
    public OcrResult? Result { get; }
}

/// <summary>
/// The result of an OCR operation.
/// </summary>
public class OcrResult
{
    /// <summary>
    /// The full text of the OCR result.
    /// </summary>
    public string AllText { get; set; }

    /// <summary>
    /// The individual elements of the OCR result.
    /// </summary>
    public IList<OcrElement> Elements { get; set; } = new List<OcrElement>();

    /// <summary>
    /// The lines of the OCR result.
    /// </summary>
    public IList<string> Lines { get; set; } = new List<string>();

    /// <summary>
    /// Was the OCR successful?
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The words of the OCR result.
    /// </summary>
    public class OcrElement
    {
        /// <summary>
        /// The confidence of the OCR result.
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// The height of the element.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The text of the element.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The width of the element.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The X coordinates of the element.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The Y coordinates of the element.
        /// </summary>
        public int Y { get; set; }
    }
}
