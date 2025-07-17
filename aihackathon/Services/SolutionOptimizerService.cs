using CodeOptimizer.Models;
using CodeOptimizer.Services;

namespace CodeOptimizer.Services
{
    public class SolutionOptimizerService
    {
        private readonly OpenAICodeAnalyzer _aiAnalyzer;
        private readonly ProjectSolutionScanner _solutionScanner;
        private readonly CodeOptimizerService _codeOptimizer;

        public SolutionOptimizerService(string openAiApiKey)
        {
            _aiAnalyzer = new OpenAICodeAnalyzer(openAiApiKey);
            _solutionScanner = new ProjectSolutionScanner();
            _codeOptimizer = new CodeOptimizerService(openAiApiKey);
        }

        public async Task<SolutionOptimizationResult> OptimizeSolutionAsync(string solutionPath)
        {
            var result = new SolutionOptimizationResult
            {
                SolutionPath = solutionPath,
                StartTime = DateTime.Now
            };

            try
            {
                // Step 1: Scan the solution for C# files
                Console.WriteLine("=== SCANNING SOLUTION ===");
                var codeFiles = await _solutionScanner.ScanSolutionAsync(solutionPath);
                
                if (codeFiles.Count == 0)
                {
                    Console.WriteLine("No C# files found in the solution.");
                    return result;
                }

                // Step 2: Prioritize files for optimization
                var prioritizedFiles = _solutionScanner.PrioritizeFiles(codeFiles);
                
                Console.WriteLine($"\n=== ANALYZING {prioritizedFiles.Count} FILES ===");
                
                // Step 3: Analyze each file
                var fileResults = new List<FileOptimizationResult>();
                int fileCount = 0;
                
                foreach (var file in prioritizedFiles)
                {
                    fileCount++;
                    Console.WriteLine($"\n[{fileCount}/{prioritizedFiles.Count}] Analyzing: {Path.GetFileName(file.FilePath)}");
                    
                    try
                    {
                        var fileResult = await AnalyzeFileAsync(file);
                        fileResults.Add(fileResult);
                        
                        // Show progress
                        if (fileResult.HasOptimizations)
                        {
                            Console.WriteLine($"  ✓ Found {fileResult.OptimizationCount} optimizations");
                        }
                        else
                        {
                            Console.WriteLine($"  ✓ No optimizations needed");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ✗ Error analyzing file: {ex.Message}");
                        fileResults.Add(new FileOptimizationResult
                        {
                            FilePath = file.FilePath,
                            Error = ex.Message,
                            HasOptimizations = false
                        });
                    }
                    
                    // Add small delay to avoid rate limiting
                    await Task.Delay(500);
                }

                result.FileResults = fileResults;
                result.EndTime = DateTime.Now;
                
                // Step 4: Generate summary
                GenerateSolutionSummary(result);
                
                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                result.EndTime = DateTime.Now;
                return result;
            }
        }

        private async Task<FileOptimizationResult> AnalyzeFileAsync(CodeFile file)
        {
            var analysisResult = await _codeOptimizer.OptimizeCodeAsync(file);
            
            return new FileOptimizationResult
            {
                FilePath = file.FilePath,
                OriginalCode = file.Content,
                OptimizedCode = analysisResult.OptimizedCode,
                Suggestions = analysisResult.Suggestions,
                MemoryBefore = analysisResult.MemoryBefore,
                MemoryAfter = analysisResult.MemoryAfter,
                ImprovementPercentage = analysisResult.ImprovementPercentage,
                HasOptimizations = analysisResult.Suggestions.Any(),
                OptimizationCount = analysisResult.Suggestions.Count,
                AnalysisTime = DateTime.Now
            };
        }

        private void GenerateSolutionSummary(SolutionOptimizationResult result)
        {
            var filesWithOptimizations = result.FileResults.Where(f => f.HasOptimizations).ToList();
            var totalOptimizations = result.FileResults.Sum(f => f.OptimizationCount);
            var averageImprovement = filesWithOptimizations.Any() 
                ? filesWithOptimizations.Average(f => f.ImprovementPercentage) 
                : 0;

            result.Summary = new SolutionOptimizationSummary
            {
                TotalFilesAnalyzed = result.FileResults.Count,
                FilesWithOptimizations = filesWithOptimizations.Count,
                TotalOptimizations = totalOptimizations,
                AverageMemoryImprovement = averageImprovement,
                AnalysisDuration = result.EndTime - result.StartTime,
                TopOptimizationTypes = GetTopOptimizationTypes(result.FileResults),
                MostOptimizedFiles = filesWithOptimizations
                    .OrderByDescending(f => f.OptimizationCount)
                    .Take(5)
                    .ToList()
            };
        }

        private Dictionary<string, int> GetTopOptimizationTypes(List<FileOptimizationResult> fileResults)
        {
            var optimizationTypes = new Dictionary<string, int>();
            
            foreach (var file in fileResults)
            {
                foreach (var suggestion in file.Suggestions)
                {
                    if (optimizationTypes.ContainsKey(suggestion.Type))
                    {
                        optimizationTypes[suggestion.Type]++;
                    }
                    else
                    {
                        optimizationTypes[suggestion.Type] = 1;
                    }
                }
            }
            
            return optimizationTypes.OrderByDescending(kvp => kvp.Value)
                .Take(10)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task OverwriteOriginalFilesAsync(SolutionOptimizationResult result)
        {
            Console.WriteLine("⚠️  WARNING: This will overwrite the original files with optimized versions!");
            Console.WriteLine("Backups will be created automatically.");
            Console.Write("Are you sure you want to proceed? (y/n): ");
            
            var confirm = Console.ReadLine();
            if (confirm?.ToLower() != "y")
            {
                Console.WriteLine("Overwrite cancelled.");
                return;
            }

            var backupDir = Path.Combine(Path.GetDirectoryName(result.SolutionPath) ?? ".", "backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(backupDir);
            
            foreach (var fileResult in result.FileResults.Where(f => f.HasOptimizations))
            {
                try
                {
                    // Create backup
                    var backupFileName = Path.Combine(backupDir, Path.GetFileName(fileResult.FilePath));
                    File.Copy(fileResult.FilePath, backupFileName);
                    
                    // Overwrite with optimized code
                    await File.WriteAllTextAsync(fileResult.FilePath, fileResult.OptimizedCode);
                    Console.WriteLine($"✅ Overwritten: {Path.GetFileName(fileResult.FilePath)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error overwriting {Path.GetFileName(fileResult.FilePath)}: {ex.Message}");
                }
            }
            
            Console.WriteLine($"💾 Backups saved to: {backupDir}");
        }

        public async Task SaveOptimizedFilesAsync(SolutionOptimizationResult result, string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);
            
            foreach (var fileResult in result.FileResults.Where(f => f.HasOptimizations))
            {
                var originalFileName = Path.GetFileName(fileResult.FilePath);
                var outputFileName = Path.Combine(outputDirectory, $"optimized_{originalFileName}");
                
                await File.WriteAllTextAsync(outputFileName, fileResult.OptimizedCode);
                Console.WriteLine($"Saved optimized file: {outputFileName}");
            }
        }

        public void DisplaySolutionResults(SolutionOptimizationResult result)
        {
            Console.WriteLine("\n" + new string('=', 100));
            Console.WriteLine("SOLUTION OPTIMIZATION RESULTS");
            Console.WriteLine(new string('=', 100));
            
            if (!string.IsNullOrEmpty(result.Error))
            {
                Console.WriteLine($"Error: {result.Error}");
                return;
            }
            
            var summary = result.Summary;
            
            Console.WriteLine($"Solution Path: {result.SolutionPath}");
            Console.WriteLine($"Analysis Duration: {summary.AnalysisDuration.TotalSeconds:F2} seconds");
            Console.WriteLine($"Total Files Analyzed: {summary.TotalFilesAnalyzed}");
            Console.WriteLine($"Files with Optimizations: {summary.FilesWithOptimizations}");
            Console.WriteLine($"Total Optimizations Found: {summary.TotalOptimizations}");
            Console.WriteLine($"Average Memory Improvement: {summary.AverageMemoryImprovement:F2}%");
            
            // Display top optimization types
            Console.WriteLine("\n" + new string('-', 50));
            Console.WriteLine("TOP OPTIMIZATION TYPES:");
            Console.WriteLine(new string('-', 50));
            
            foreach (var optimizationType in summary.TopOptimizationTypes)
            {
                Console.WriteLine($"{optimizationType.Key}: {optimizationType.Value} occurrences");
            }
            
            // Display most optimized files
            Console.WriteLine("\n" + new string('-', 50));
            Console.WriteLine("MOST OPTIMIZED FILES:");
            Console.WriteLine(new string('-', 50));
            
            foreach (var file in summary.MostOptimizedFiles)
            {
                Console.WriteLine($"{Path.GetFileName(file.FilePath)}: {file.OptimizationCount} optimizations ({file.ImprovementPercentage:F2}% improvement)");
            }
            
            // Display detailed results for each file
            Console.WriteLine("\n" + new string('-', 50));
            Console.WriteLine("DETAILED FILE ANALYSIS:");
            Console.WriteLine(new string('-', 50));
            
            foreach (var fileResult in result.FileResults.Where(f => f.HasOptimizations))
            {
                Console.WriteLine($"\n📁 {Path.GetFileName(fileResult.FilePath)}");
                Console.WriteLine($"   Optimizations: {fileResult.OptimizationCount}");
                Console.WriteLine($"   Memory Improvement: {fileResult.ImprovementPercentage:F2}%");
                
                foreach (var suggestion in fileResult.Suggestions.Take(3)) // Show top 3 suggestions
                {
                    Console.WriteLine($"   • [{suggestion.Severity}] {suggestion.Type}");
                    Console.WriteLine($"     {suggestion.Description}");
                }
                
                if (fileResult.Suggestions.Count > 3)
                {
                    Console.WriteLine($"   ... and {fileResult.Suggestions.Count - 3} more optimizations");
                }
            }
        }
    }
}
