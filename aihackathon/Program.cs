using CodeOptimizer.Models;
using CodeOptimizer.Services;
using Microsoft.Extensions.Configuration;

namespace CodeOptimizer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== AI-Powered Code Memory Optimizer ===");
            Console.WriteLine("This tool analyzes C# code and provides memory optimization suggestions using OpenAI.\n");

            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Get OpenAI API key
            var openAiApiKey = configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(openAiApiKey))
            {
                // Try to get from environment variable
                openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            }

            if (string.IsNullOrEmpty(openAiApiKey) || openAiApiKey == "YOUR_OPENAI_API_KEY_HERE")
            {
                Console.WriteLine("Please set your OpenAI API key in one of the following ways:");
                Console.WriteLine("1. In appsettings.json under 'OpenAI:ApiKey'");
                Console.WriteLine("2. As an environment variable 'OPENAI_API_KEY'");
                Console.WriteLine("3. You can get an API key from: https://platform.openai.com/api-keys");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
                return;
            }

            try
            {
                // Initialize the optimizer service
                var optimizer = new CodeOptimizerService(openAiApiKey);
                var solutionOptimizer = new SolutionOptimizerService(openAiApiKey);

                // Show menu
                while (true)
                {
                    Console.WriteLine("\nChoose an option:");
                    Console.WriteLine("1. Analyze sample inefficient code");
                    Console.WriteLine("2. Analyze custom code file");
                    Console.WriteLine("3. Analyze code from clipboard");
                    Console.WriteLine("4. Scan and optimize entire solution");
                    Console.WriteLine("5. Exit");
                    Console.Write("\nEnter your choice (1-5): ");

                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            await AnalyzeSampleCode(optimizer);
                            break;
                        case "2":
                            await AnalyzeFileCode(optimizer);
                            break;
                        case "3":
                            await AnalyzeClipboardCode(optimizer);
                            break;
                        case "4":
                            await AnalyzeSolutionCode(solutionOptimizer);
                            break;
                        case "5":
                            Console.WriteLine("Thank you for using the Code Memory Optimizer!");
                            return;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        private static async Task AnalyzeSampleCode(CodeOptimizerService optimizer)
        {
            Console.WriteLine("\nAnalyzing sample inefficient code...");

            // Use the utility method to find the correct project directory
            var projectDirectory = FindProjectDirectory();
            var sampleCodePath = Path.Combine(projectDirectory, "SampleCode", "InefficientCode.cs");
            
            Console.WriteLine($"Looking for sample code at: {sampleCodePath}");
            
            if (!File.Exists(sampleCodePath))
            {
                Console.WriteLine($"Sample code file not found at: {sampleCodePath}");
                Console.WriteLine("Please ensure the SampleCode folder exists in the project directory.");
                
                // Try to find any .cs files in SampleCode directory
                var sampleCodeDir = Path.Combine(projectDirectory, "SampleCode");
                if (Directory.Exists(sampleCodeDir))
                {
                    var csFiles = Directory.GetFiles(sampleCodeDir, "*.cs");
                    if (csFiles.Length > 0)
                    {
                        Console.WriteLine($"Found {csFiles.Length} C# files in SampleCode directory:");
                        foreach (var file in csFiles)
                        {
                            Console.WriteLine($"  - {Path.GetFileName(file)}");
                        }
                        
                        // Use the first file found
                        sampleCodePath = csFiles[0];
                        Console.WriteLine($"Using: {Path.GetFileName(sampleCodePath)}");
                    }
                }
                else
                {
                    Console.WriteLine("SampleCode directory not found.");
                    return;
                }
            }

            var sampleCode = await File.ReadAllTextAsync(sampleCodePath);
            var codeFile = new CodeFile
            {
                FilePath = sampleCodePath,
                Content = sampleCode,
                Language = "csharp"
            };

            await AnalyzeAndDisplayResults(optimizer, codeFile);
        }

        private static async Task AnalyzeFileCode(CodeOptimizerService optimizer)
        {
            Console.Write("Enter the path to your C# code file: ");
            var filePath = Console.ReadLine();

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("File not found. Please check the path and try again.");
                return;
            }

            try
            {
                var code = await File.ReadAllTextAsync(filePath);
                var codeFile = new CodeFile
                {
                    FilePath = filePath,
                    Content = code,
                    Language = "csharp"
                };

                await AnalyzeAndDisplayResults(optimizer, codeFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        private static async Task AnalyzeClipboardCode(CodeOptimizerService optimizer)
        {
            Console.WriteLine("Please paste your C# code below (press Enter twice when done):");
            Console.WriteLine(new string('-', 50));

            var code = "";
            var emptyLineCount = 0;

            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    emptyLineCount++;
                    if (emptyLineCount >= 2)
                        break;
                }
                else
                {
                    emptyLineCount = 0;
                }

                code += line + Environment.NewLine;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                Console.WriteLine("No code provided.");
                return;
            }

            var codeFile = new CodeFile
            {
                FilePath = "clipboard",
                Content = code.Trim(),
                Language = "csharp"
            };

            await AnalyzeAndDisplayResults(optimizer, codeFile);
        }

        private static async Task AnalyzeAndDisplayResults(CodeOptimizerService optimizer, CodeFile codeFile)
        {
            Console.WriteLine("\nAnalyzing code with AI... This may take a moment.");

            try
            {
                var startTime = DateTime.Now;
                var result = await optimizer.OptimizeCodeAsync(codeFile);
                var endTime = DateTime.Now;

                Console.WriteLine($"\nAnalysis completed in {(endTime - startTime).TotalSeconds:F2} seconds.");

                optimizer.DisplayResults(result);

                // Ask if user wants to save the optimized code
                Console.WriteLine("\nWould you like to save the optimized code to a file? (y/n): ");
                var save = Console.ReadLine();

                if (save?.ToLower() == "y")
                {
                    await SaveOptimizedCode(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during analysis: {ex.Message}");
                Console.WriteLine("Please check your OpenAI API key and internet connection.");
            }
        }

        private static async Task SaveOptimizedCode(CodeAnalysisResult result)
        {
            Console.WriteLine("\nSave options:");
            Console.WriteLine("1. Save to new file");
            Console.WriteLine("2. Overwrite original file");
            Console.WriteLine("3. Cancel");
            Console.Write("Enter your choice (1-3): ");
            
            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    await SaveToNewFile(result);
                    break;
                case "2":
                    await OverwriteOriginalFile(result);
                    break;
                case "3":
                    Console.WriteLine("Save cancelled.");
                    break;
                default:
                    Console.WriteLine("Invalid choice. Save cancelled.");
                    break;
            }
        }

        private static async Task SaveToNewFile(CodeAnalysisResult result)
        {
            Console.Write("Enter the filename to save the optimized code: ");
            var filename = Console.ReadLine();

            if (string.IsNullOrEmpty(filename))
            {
                filename = $"optimized_code_{DateTime.Now:yyyyMMdd_HHmmss}.cs";
            }

            try
            {
                await File.WriteAllTextAsync(filename, result.OptimizedCode);
                Console.WriteLine($"Optimized code saved to: {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }

        private static async Task OverwriteOriginalFile(CodeAnalysisResult result)
        {
            var originalPath = result.OriginalCode != null ? GetOriginalFilePath(result) : null;
            
            if (string.IsNullOrEmpty(originalPath) || originalPath == "clipboard")
            {
                Console.WriteLine("Cannot overwrite original file (clipboard input or unknown source).");
                Console.WriteLine("Falling back to save as new file...");
                await SaveToNewFile(result);
                return;
            }

            Console.WriteLine($"⚠️  WARNING: This will overwrite the original file:");
            Console.WriteLine($"   {originalPath}");
            Console.WriteLine("   Make sure you have a backup if needed.");
            Console.Write("Are you sure you want to overwrite? (y/n): ");
            
            var confirm = Console.ReadLine();
            
            if (confirm?.ToLower() == "y")
            {
                try
                {
                    // Create backup first
                    var backupPath = originalPath + ".backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    File.Copy(originalPath, backupPath);
                    Console.WriteLine($"Backup created: {backupPath}");
                    
                    // Overwrite with optimized code
                    await File.WriteAllTextAsync(originalPath, result.OptimizedCode);
                    Console.WriteLine($"✅ Original file overwritten with optimized code: {originalPath}");
                    Console.WriteLine($"💾 Backup available at: {backupPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error overwriting file: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Overwrite cancelled.");
            }
        }

        private static string GetOriginalFilePath(CodeAnalysisResult result)
        {
            return result.OriginalFilePath;
        }

        private static async Task AnalyzeSolutionCode(SolutionOptimizerService solutionOptimizer)
        {
            Console.WriteLine("\n=== SOLUTION OPTIMIZATION ===");
            Console.WriteLine("This will scan all C# files in the solution and optimize them.");
            Console.WriteLine("Warning: This may take several minutes and consume API credits.");

            Console.Write("Enter the path to your solution file (.sln) or project directory: ");
            var solutionPath = Console.ReadLine();

            if (string.IsNullOrEmpty(solutionPath))
            {
                Console.WriteLine("No path provided.");
                return;
            }

            // Check if path exists
            if (!Directory.Exists(solutionPath) && !File.Exists(solutionPath))
            {
                Console.WriteLine("Path not found. Please check the path and try again.");
                return;
            }

            // If it's a directory, look for solution files
            if (Directory.Exists(solutionPath))
            {
                var solutionFiles = Directory.GetFiles(solutionPath, "*.sln", SearchOption.TopDirectoryOnly);
                if (solutionFiles.Length > 0)
                {
                    solutionPath = solutionFiles[0];
                    Console.WriteLine($"Found solution file: {Path.GetFileName(solutionPath)}");
                }
            }

            Console.WriteLine($"\nStarting solution analysis...");
            Console.WriteLine("This will:");
            Console.WriteLine("1. Scan all C# files in the solution");
            Console.WriteLine("2. Prioritize files based on optimization potential");
            Console.WriteLine("3. Analyze each file with AI for memory optimizations");
            Console.WriteLine("4. Generate optimized code for each file");
            Console.WriteLine("5. Provide comprehensive optimization report");

            Console.Write("\nProceed with analysis? (y/n): ");
            var proceed = Console.ReadLine();

            if (proceed?.ToLower() != "y")
            {
                Console.WriteLine("Analysis cancelled.");
                return;
            }

            try
            {
                var startTime = DateTime.Now;
                var result = await solutionOptimizer.OptimizeSolutionAsync(solutionPath);
                var endTime = DateTime.Now;

                Console.WriteLine($"\nSolution analysis completed in {(endTime - startTime).TotalMinutes:F2} minutes.");

                solutionOptimizer.DisplaySolutionResults(result);

                // Ask if user wants to save the optimized files
                if (result.FileResults.Any(f => f.HasOptimizations))
                {
                    Console.WriteLine("\nSave optimized files:");
                    Console.WriteLine("1. Save to new directory");
                    Console.WriteLine("2. Overwrite original files (with backup)");
                    Console.WriteLine("3. Skip saving");
                    Console.Write("Enter your choice (1-3): ");
                    
                    var saveChoice = Console.ReadLine();
                    
                    switch (saveChoice)
                    {
                        case "1":
                            await SaveOptimizedSolution(solutionOptimizer, result);
                            break;
                        case "2":
                            await solutionOptimizer.OverwriteOriginalFilesAsync(result);
                            break;
                        case "3":
                            Console.WriteLine("Skipping file save.");
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Skipping file save.");
                            break;
                    }
                }

                // Generate optimization report
                await GenerateOptimizationReport(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during solution analysis: {ex.Message}");
                Console.WriteLine("Please check your OpenAI API key and internet connection.");
            }
        }

        private static async Task SaveOptimizedSolution(SolutionOptimizerService solutionOptimizer, SolutionOptimizationResult result)
        {
            Console.Write("Enter the output directory for optimized files (press Enter for 'optimized_solution'): ");
            var outputDir = Console.ReadLine();

            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = Path.Combine(Path.GetDirectoryName(result.SolutionPath) ?? ".", "optimized_solution");
            }

            try
            {
                await solutionOptimizer.SaveOptimizedFilesAsync(result, outputDir);
                Console.WriteLine($"Optimized files saved to: {outputDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving optimized files: {ex.Message}");
            }
        }

        private static async Task GenerateOptimizationReport(SolutionOptimizationResult result)
        {
            var reportContent = GenerateHtmlReport(result);
            var reportPath = Path.Combine(Path.GetDirectoryName(result.SolutionPath) ?? ".", "optimization_report.html");

            try
            {
                await File.WriteAllTextAsync(reportPath, reportContent);
                Console.WriteLine($"Optimization report generated: {reportPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating report: {ex.Message}");
            }
        }

        private static string GenerateHtmlReport(SolutionOptimizationResult result)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Solution Optimization Report</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #f0f0f0; padding: 20px; border-radius: 5px; }}
        .summary {{ background-color: #e8f4f8; padding: 15px; margin: 20px 0; border-radius: 5px; }}
        .file-result {{ border: 1px solid #ccc; margin: 10px 0; padding: 15px; border-radius: 5px; }}
        .optimization {{ background-color: #fff3cd; padding: 10px; margin: 5px 0; border-radius: 3px; }}
        .high {{ border-left: 4px solid #dc3545; }}
        .medium {{ border-left: 4px solid #ffc107; }}
        .low {{ border-left: 4px solid #28a745; }}
        code {{ background-color: #f8f9fa; padding: 2px 4px; border-radius: 3px; font-family: 'Courier New', monospace; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Solution Optimization Report</h1>
        <p>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
        <p>Solution: {result.SolutionPath}</p>
    </div>

    <div class='summary'>
        <h2>Summary</h2>
        <p><strong>Total Files Analyzed:</strong> {result.Summary.TotalFilesAnalyzed}</p>
        <p><strong>Files with Optimizations:</strong> {result.Summary.FilesWithOptimizations}</p>
        <p><strong>Total Optimizations:</strong> {result.Summary.TotalOptimizations}</p>
        <p><strong>Average Memory Improvement:</strong> {result.Summary.AverageMemoryImprovement:F2}%</p>
        <p><strong>Analysis Duration:</strong> {result.Summary.AnalysisDuration.TotalMinutes:F2} minutes</p>
    </div>

    <h2>Top Optimization Types</h2>
    <ul>
        {string.Join("", result.Summary.TopOptimizationTypes.Select(kvp => $"<li>{kvp.Key}: {kvp.Value} occurrences</li>"))}
    </ul>

    <h2>File Results</h2>
    {string.Join("", result.FileResults.Where(f => f.HasOptimizations).Select(f => $@"
    <div class='file-result'>
        <h3>{Path.GetFileName(f.FilePath)}</h3>
        <p><strong>Optimizations:</strong> {f.OptimizationCount}</p>
        <p><strong>Memory Improvement:</strong> {f.ImprovementPercentage:F2}%</p>
        <div class='optimizations'>
            {string.Join("", f.Suggestions.Select(s => $@"
            <div class='optimization {s.Severity.ToLower()}'>
                <strong>[{s.Severity}] {s.Type}</strong><br>
                {s.Description}<br>
                {(!string.IsNullOrEmpty(s.Before) ? $"<strong>Before:</strong> <code>{s.Before}</code><br>" : "")}
                {(!string.IsNullOrEmpty(s.After) ? $"<strong>After:</strong> <code>{s.After}</code>" : "")}
            </div>"))}
        </div>
    </div>"))}

</body>
</html>";
            return html;
        }

        private static string FindProjectDirectory()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            
            // Check if we're in the bin directory (running from build output)
            if (currentDirectory.Contains("bin"))
            {
                // Navigate up to find the project directory
                var projectDir = Directory.GetParent(currentDirectory)?.Parent?.Parent?.FullName;
                if (projectDir != null && Directory.Exists(Path.Combine(projectDir, "SampleCode")))
                {
                    return projectDir;
                }
            }
            
            // Check if we're in the project directory already
            if (Directory.Exists(Path.Combine(currentDirectory, "SampleCode")))
            {
                return currentDirectory;
            }
            
            // Check parent directory
            var parentDir = Directory.GetParent(currentDirectory)?.FullName;
            if (parentDir != null && Directory.Exists(Path.Combine(parentDir, "SampleCode")))
            {
                return parentDir;
            }
            
            // Fallback to current directory
            return currentDirectory;
        }
    }
}
