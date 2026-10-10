using System.Text;
using System.Text.Json;

namespace AJACAS.Services
{
    public class AIResumeAnalysisService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AIResumeAnalysisService(
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> AnalyzeResumeAsync(string resumeText)
        {
            if (string.IsNullOrWhiteSpace(resumeText))
            {
                throw new ArgumentException(
                    "Resume text cannot be empty.");
            }

            var apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Gemini API key is not configured.");
            }

            /*
             * We try the preferred model first.
             * If Gemini is temporarily unavailable,
             * we retry and then fall back to another
             * supported Flash model.
             */
            var models = new[]
            {
                "gemini-3.6-flash",
                "gemini-3.7-flash",
                "gemini-3.5-flash"
            };

            Exception? lastException = null;

            foreach (var model in models)
            {
                try
                {
                    return await AnalyzeWithModelAsync(
                        resumeText,
                        apiKey,
                        model);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw new Exception(
                "Gemini AI is temporarily unavailable. " +
                "Please try analyzing the resume again in a few moments.",
                lastException);
        }


        private async Task<string> AnalyzeWithModelAsync(
            string resumeText,
            string apiKey,
            string model)
        {
            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

            var prompt = $"""
                You are an expert professional resume and career analyst.

                Analyze the following resume carefully.

                RESUME:
                {resumeText}

                Evaluate the resume based on:

                1. Overall resume quality
                2. ATS compatibility
                3. Content quality
                4. Formatting quality
                5. Keyword usage
                6. Professional strengths
                7. Weaknesses
                8. Missing keywords
                9. Specific recommendations for improvement

                Score each category from 0 to 100.

                Do not invent information that is not present in the resume.

                Return ONLY valid JSON.

                The JSON must contain these fields:

                overallScore
                atsScore
                contentScore
                formattingScore
                keywordScore
                strengths
                weaknesses
                recommendations
                missingKeywords
                """;


            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                },

                generationConfig = new
                {
                    responseMimeType = "application/json"
                }
            };


            var json = JsonSerializer.Serialize(requestBody);


            /*
             * Retry temporary Gemini errors.
             *
             * Attempt 1 → wait 2 seconds
             * Attempt 2 → wait 4 seconds
             * Attempt 3 → wait 8 seconds
             */
            const int maxAttempts = 3;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{url}?key={apiKey}");

                request.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");


                HttpResponseMessage response;

                try
                {
                    response = await _httpClient.SendAsync(request);
                }
                catch (HttpRequestException ex)
                {
                    if (attempt == maxAttempts)
                    {
                        throw new Exception(
                            $"Unable to connect to Gemini API: {ex.Message}",
                            ex);
                    }

                    await Task.Delay(
                        TimeSpan.FromSeconds(
                            Math.Pow(2, attempt)));

                    continue;
                }


                var responseContent =
                    await response.Content.ReadAsStringAsync();


                /*
                 * SUCCESS
                 */
                if (response.IsSuccessStatusCode)
                {
                    using var document =
                        JsonDocument.Parse(responseContent);

                    if (!document.RootElement
                        .TryGetProperty("candidates", out var candidates))
                    {
                        throw new Exception(
                            "Gemini returned an invalid response.");
                    }

                    if (candidates.GetArrayLength() == 0)
                    {
                        throw new Exception(
                            "Gemini returned no analysis candidates.");
                    }

                    var text =
                        candidates[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();


                    if (string.IsNullOrWhiteSpace(text))
                    {
                        throw new Exception(
                            "Gemini returned an empty response.");
                    }

                    return text;
                }


                /*
                 * TEMPORARY ERRORS
                 *
                 * 429 = rate limit
                 * 500 = server error
                 * 502 = bad gateway
                 * 503 = service unavailable
                 * 504 = gateway timeout
                 */
                var statusCode =
                    (int)response.StatusCode;

                bool temporaryError =
                    statusCode == 429 ||
                    statusCode == 500 ||
                    statusCode == 502 ||
                    statusCode == 503 ||
                    statusCode == 504;


                if (temporaryError && attempt < maxAttempts)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(
                            Math.Pow(2, attempt)));

                    continue;
                }


                /*
                 * Do not retry permanent errors
                 * such as invalid API keys or bad requests.
                 */
                throw new Exception(
                    $"Gemini API error: {response.StatusCode} - {responseContent}");
            }


            throw new Exception(
                $"Gemini model '{model}' failed after multiple attempts.");
        }
    }
}