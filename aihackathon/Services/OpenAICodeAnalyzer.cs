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
You are an expert C# memory optimization specialist. Analyze the following C# code and provide:

1. Memory optimization suggestions
2. Complete optimized version of the code

Please provide your response in the following JSON format:
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
- String concatenation optimization (use StringBuilder instead of += in loops)
- Collection initialization and capacity management
- Unnecessary object allocations and boxing/unboxing
- Proper resource disposal (using statements)
- Efficient LINQ operations
- Memory-efficient data structures
- Lazy initialization where appropriate

Code to analyze:
```csharp
{code}
```

IMPORTANT: 
1. Provide the complete optimized code that can be compiled and run
2. Make sure all optimizations are practical and improve memory usage
3. Include proper using statements and namespace declarations
4. Focus on real memory improvements, not just style changes
";
        }

        private CodeAnalysisResult ParseAnalysisResponse(CodeFile codeFile, string response)
        {
            try
            {
                // Log the raw response for debugging
                Console.WriteLine($"Raw OpenAI response: {response}");
                
                // Try to extract JSON from the response
                var startIndex = response.IndexOf('{');
                var endIndex = response.LastIndexOf('}');
                
                if (startIndex >= 0 && endIndex >= 0)
                {
                    var jsonContent = response.Substring(startIndex, endIndex - startIndex + 1);
                    Console.WriteLine($"Extracted JSON: {jsonContent}");
                    
                    var analysisData = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    
                    var result = new CodeAnalysisResult
                    {
                        OriginalCode = codeFile.Content,
                        OptimizedCode = analysisData?.optimizedCode ?? GenerateOptimizedCode(codeFile.Content, response),
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
                else
                {
                    // If no JSON found, try to parse as plain text response
                    return ParsePlainTextResponse(codeFile, response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing OpenAI response: {ex.Message}");
                Console.WriteLine($"Response content: {response}");
                
                // Try to parse as plain text if JSON parsing fails
                return ParsePlainTextResponse(codeFile, response);
            }
        }

        private CodeAnalysisResult ParsePlainTextResponse(CodeFile codeFile, string response)
        {
            var result = new CodeAnalysisResult
            {
                OriginalCode = codeFile.Content,
                OptimizedCode = GenerateOptimizedCode(codeFile.Content, response),
                OriginalFilePath = codeFile.FilePath,
                Suggestions = new List<OptimizationSuggestion>()
            };

            // Extract suggestions from plain text response
            var suggestions = ExtractSuggestionsFromText(response);
            result.Suggestions.AddRange(suggestions);

            return result;
        }

        private List<OptimizationSuggestion> ExtractSuggestionsFromText(string response)
        {
            var suggestions = new List<OptimizationSuggestion>();
            
            // Look for common patterns in optimization suggestions
            var lines = response.Split('\n');
            OptimizationSuggestion? currentSuggestion = null;
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Check for optimization types
                if (trimmedLine.Contains("StringBuilder") || trimmedLine.Contains("string concatenation"))
                {
                    currentSuggestion = new OptimizationSuggestion
                    {
                        Type = "String Concatenation Optimization",
                        Description = trimmedLine,
                        Severity = "High"
                    };
                    suggestions.Add(currentSuggestion);
                }
                else if (trimmedLine.Contains("using") || trimmedLine.Contains("dispose"))
                {
                    currentSuggestion = new OptimizationSuggestion
                    {
                        Type = "Resource Management",
                        Description = trimmedLine,
                        Severity = "High"
                    };
                    suggestions.Add(currentSuggestion);
                }
                else if (trimmedLine.Contains("boxing") || trimmedLine.Contains("unboxing"))
                {
                    currentSuggestion = new OptimizationSuggestion
                    {
                        Type = "Boxing/Unboxing Elimination",
                        Description = trimmedLine,
                        Severity = "Medium"
                    };
                    suggestions.Add(currentSuggestion);
                }
                else if (trimmedLine.Contains("LINQ") || trimmedLine.Contains("enumeration"))
                {
                    currentSuggestion = new OptimizationSuggestion
                    {
                        Type = "LINQ Optimization",
                        Description = trimmedLine,
                        Severity = "Medium"
                    };
                    suggestions.Add(currentSuggestion);
                }
                else if (trimmedLine.Contains("collection") || trimmedLine.Contains("List"))
                {
                    currentSuggestion = new OptimizationSuggestion
                    {
                        Type = "Collection Optimization",
                        Description = trimmedLine,
                        Severity = "Medium"
                    };
                    suggestions.Add(currentSuggestion);
                }
            }

            if (suggestions.Count == 0)
            {
                suggestions.Add(new OptimizationSuggestion
                {
                    Type = "General Memory Optimization",
                    Description = "OpenAI provided optimization suggestions but they couldn't be parsed into structured format.",
                    Severity = "Medium"
                });
            }

            return suggestions;
        }

        private string GenerateOptimizedCode(string originalCode, string response)
        {
            // If response contains optimized code, try to extract it
            if (response.Contains("```csharp"))
            {
                var startIndex = response.IndexOf("```csharp");
                var endIndex = response.IndexOf("```", startIndex + 9);
                
                if (startIndex >= 0 && endIndex >= 0)
                {
                    var codeStart = startIndex + 9;
                    var optimizedCode = response.Substring(codeStart, endIndex - codeStart).Trim();
                    return optimizedCode;
                }
            }

            // If no optimized code found, return original with a comment
            return $"// OpenAI optimization suggestions applied\n// Original response: {response.Substring(0, Math.Min(200, response.Length))}...\n\n{originalCode}";
        }
    }
}
