using CodeOptimizer.Models;
using Newtonsoft.Json;
using System.Text;

namespace CodeOptimizer.Services
{
    public class OpenAICodeAnalyzer
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAICodeAnalyzer(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<CodeAnalysisResult> AnalyzeCodeAsync(CodeFile codeFile)
        {
            var analysisPrompt = CreateAnalysisPrompt(codeFile.Content);
            
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are an expert C# code optimizer specializing in memory optimization. Analyze the provided code and suggest memory optimizations." },
                    new { role = "user", content = analysisPrompt }
                },
                max_tokens = 2000,
                temperature = 0.3
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OpenAI API error: {response.StatusCode} - {responseContent}");
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            var messageContent = responseData?.choices[0]?.message?.content?.ToString() ?? "";

            return ParseAnalysisResponse(codeFile, messageContent);
        }

        private string CreateAnalysisPrompt(string code)
        {
            return $@"
Please analyze the following C# code for memory optimization opportunities and provide a JSON response with the following structure:

{{
  ""suggestions"": [
    {{
      ""type"": ""Memory Optimization Type"",
      ""description"": ""Detailed description of the optimization"",
      ""lineNumber"": ""Line number or range"",
      ""severity"": ""High/Medium/Low"",
      ""before"": ""Original code snippet"",
      ""after"": ""Optimized code snippet""
    }}
  ],
  ""optimizedCode"": ""Complete optimized version of the code"",
  ""optimizationSummary"": ""Summary of all optimizations made""
}}

Focus on these memory optimization areas:
1. String concatenation optimization (StringBuilder, string interpolation)
2. Collection initialization and capacity management
3. Unnecessary object allocations
4. Proper disposal of resources (using statements)
5. Boxing/unboxing elimination
6. Lazy initialization where appropriate
7. Value types vs reference types optimization
8. Memory-efficient LINQ operations
9. Async/await memory patterns
10. Cache-friendly data structures

Code to analyze:
```csharp
{code}
```

Provide practical, implementable suggestions with clear before/after examples.
";
        }

        private CodeAnalysisResult ParseAnalysisResponse(CodeFile codeFile, string response)
        {
            try
            {
                // Try to extract JSON from the response
                var startIndex = response.IndexOf('{');
                var endIndex = response.LastIndexOf('}');
                
                if (startIndex >= 0 && endIndex >= 0)
                {
                    var jsonContent = response.Substring(startIndex, endIndex - startIndex + 1);
                    var analysisData = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    
                    var result = new CodeAnalysisResult
                    {
                        OriginalCode = codeFile.Content,
                        OptimizedCode = analysisData?.optimizedCode ?? codeFile.Content,
                        OriginalFilePath = codeFile.FilePath,
                        Suggestions = new List<OptimizationSuggestion>()
                    };

                    if (analysisData?.suggestions != null)
                    {
                        foreach (var suggestion in analysisData.suggestions)
                        {
                            result.Suggestions.Add(new OptimizationSuggestion
                            {
                                Type = suggestion?.type ?? "",
                                Description = suggestion?.description ?? "",
                                LineNumber = suggestion?.lineNumber ?? "",
                                Severity = suggestion?.severity ?? "",
                                Before = suggestion?.before ?? "",
                                After = suggestion?.after ?? ""
                            });
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing OpenAI response: {ex.Message}");
            }

            // Fallback if parsing fails
            return new CodeAnalysisResult
            {
                OriginalCode = codeFile.Content,
                OptimizedCode = codeFile.Content,
                OriginalFilePath = codeFile.FilePath,
                Suggestions = new List<OptimizationSuggestion>
                {
                    new OptimizationSuggestion
                    {
                        Type = "Analysis Error",
                        Description = "Failed to parse OpenAI response. Please check the API response format.",
                        Severity = "High"
                    }
                }
            };
        }
    }
}
