using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Plugin.Shared.OCR;
#if IOS
using UIKit;
#endif
using Vision;

namespace Plugin.Xamarin.OCR
{
#if !MACOS
    internal class OcrImplementation : IOcrService
    {
        private static readonly object s_initLock = new();
        private bool _isInitialized;

        /// <summary>
        /// Initialize the OCR on the platform
        /// </summary>
        /// <param name="ct">An optional cancellation token</param>
        public Task InitAsync(CancellationToken ct = default)
        {
            lock (s_initLock)
            {
                if (_isInitialized) return Task.CompletedTask;
                _isInitialized = true;
            }

            // Perform any necessary initialization here.
            // Example: Loading models, setting up resources, etc.

            return Task.CompletedTask;
        }

        /// <summary>
        /// Takes an image and returns the text found in the image.
        /// </summary>
        /// <param name="imageData">The image data</param>
        /// <param name="ct">An optional cancellation token</param>
        /// <returns>The OCR result</returns>
        public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken ct = default)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException($"{nameof(InitAsync)} must be called before {nameof(RecognizeTextAsync)}.");
            }

            ct.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<OcrResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            ct.Register(() => tcs.TrySetCanceled());

            try
            {
                var image = ImageFromByteArray(imageData) ?? throw new ArgumentException("Invalid image data");

                var recognizeTextRequest = new VNRecognizeTextRequest((request, error) =>
                {
                    if (error != null)
                    {
                        tcs.TrySetException(new Exception(error.ToString()));
                        return;
                    }

                    if (ct.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled(ct);
                        return;
                    }

                    var result = ProcessRecognitionResults(request);
                    tcs.TrySetResult(result);
                });

                var ocrHandler = new VNImageRequestHandler(image.CGImage, new NSDictionary());
                ocrHandler.Perform(new VNRequest[] { recognizeTextRequest }, out var performError);

                if (performError != null)
                {
                    throw new Exception(performError.ToString());
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return await tcs.Task;
        }

        private static OcrResult ProcessRecognitionResults(VNRequest request)
        {
            var ocrResult = new OcrResult();

            var observations = request.GetResults<VNRecognizedTextObservation>();
            if (observations == null || observations.Length == 0)
            {
                ocrResult.Success = false;
                return ocrResult;
            }

            foreach (var observation in observations)
            {
                var topCandidate = observation.TopCandidates(1).FirstOrDefault();
                if (topCandidate != null)
                {
                    ocrResult.AllText += " " + topCandidate.String;
                    ocrResult.Lines.Add(topCandidate.String);

                    // Splitting by spaces to create elements might not be accurate for all languages/scripts
                    topCandidate.String.Split(" ").ToList().ForEach(e => ocrResult.Elements.Add(new OcrResult.OcrElement { Text = e, Confidence = topCandidate.Confidence }));
                }
            }

            ocrResult.Success = true;
            return ocrResult;
        }

        private static UIImage? ImageFromByteArray(byte[] data)
        {
            return data != null ? new UIImage(NSData.FromArray(data)) : null;
        }
    }
#endif
}