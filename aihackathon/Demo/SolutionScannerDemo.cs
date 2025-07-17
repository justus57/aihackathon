using CodeOptimizer.Services;
using CodeOptimizer.Models;

namespace CodeOptimizer.Demo
{
    public class SolutionScannerDemo
    {
        public static async Task RunDemoAsync()
        {
            Console.WriteLine("=== SOLUTION SCANNER DEMONSTRATION ===");
            Console.WriteLine("This demo shows how the solution scanner works without API calls.\n");

            // Demo: Scanning the current solution - use the actual project directory
            var currentDirectory = Directory.GetCurrentDirectory();
            var solutionPath = Directory.GetParent(currentDirectory)?.FullName ?? currentDirectory;
            var scanner = new ProjectSolutionScanner();

            Console.WriteLine("1. SCANNING SOLUTION FOR C# FILES...");
            var files = await scanner.ScanSolutionAsync(solutionPath);
            
            Console.WriteLine($"Found {files.Count} C# files:");
            foreach (var file in files)
            {
                Console.WriteLine($"  - {Path.GetFileName(file.FilePath)}");
            }

            Console.WriteLine("\n2. PRIORITIZING FILES FOR OPTIMIZATION...");
            var prioritizedFiles = scanner.PrioritizeFiles(files);
            
            Console.WriteLine("Files ordered by optimization potential:");
            for (int i = 0; i < Math.Min(5, prioritizedFiles.Count); i++)
            {
                var file = prioritizedFiles[i];
                Console.WriteLine($"  {i + 1}. {Path.GetFileName(file.FilePath)} (High potential)");
            }

            Console.WriteLine("\n3. ANALYSIS SIMULATION...");
            Console.WriteLine("In real execution, each file would be:");
            Console.WriteLine("  • Sent to OpenAI for analysis");
            Console.WriteLine("  • Analyzed for memory optimization opportunities");
            Console.WriteLine("  • Rewritten with optimizations");
            Console.WriteLine("  • Scored for improvement potential");

            Console.WriteLine("\n4. SAMPLE OUTPUT SIMULATION...");
            SimulateAnalysisOutput(files);

            Console.WriteLine("\n5. REPORTING SIMULATION...");
            Console.WriteLine("The tool would generate:");
            Console.WriteLine("  • HTML report with detailed analysis");
            Console.WriteLine("  • Optimized code files");
            Console.WriteLine("  • Summary statistics");
            Console.WriteLine("  • Optimization recommendations");

            Console.WriteLine("\n=== DEMO COMPLETE ===");
            Console.WriteLine("The solution scanner is ready to analyze your entire codebase!");
        }

        private static void SimulateAnalysisOutput(List<CodeFile> files)
        {
            var random = new Random();
            var optimizationTypes = new[]
            {
                "String Concatenation",
                "Collection Initialization",
                "Boxing Elimination",
                "Resource Disposal",
                "LINQ Optimization",
                "Async/Await Patterns",
                "Memory Pooling",
                "Lazy Initialization"
            };

            Console.WriteLine("\nSample analysis results:");
            
            for (int i = 0; i < Math.Min(3, files.Count); i++)
            {
                var file = files[i];
                var optimizationCount = random.Next(1, 6);
                var improvement = random.Next(10, 40);
                
                Console.WriteLine($"\n📁 {Path.GetFileName(file.FilePath)}");
                Console.WriteLine($"   Optimizations: {optimizationCount}");
                Console.WriteLine($"   Memory Improvement: {improvement}%");
                
                for (int j = 0; j < optimizationCount; j++)
                {
                    var type = optimizationTypes[random.Next(optimizationTypes.Length)];
                    var severity = random.Next(3) switch
                    {
                        0 => "HIGH",
                        1 => "MEDIUM",
                        _ => "LOW"
                    };
                    
                    Console.WriteLine($"   • [{severity}] {type}");
                }
            }
        }
    }
}
