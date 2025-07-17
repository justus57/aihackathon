namespace CodeOptimizer.Models
{
    public class CodeAnalysisResult
    {
        public string OriginalCode { get; set; } = string.Empty;
        public string OptimizedCode { get; set; } = string.Empty;
        public string OriginalFilePath { get; set; } = string.Empty;
        public List<OptimizationSuggestion> Suggestions { get; set; } = new();
        public MemoryUsageInfo MemoryBefore { get; set; } = new();
        public MemoryUsageInfo MemoryAfter { get; set; } = new();
        public double ImprovementPercentage { get; set; }
    }

    public class OptimizationSuggestion
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LineNumber { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Before { get; set; } = string.Empty;
        public string After { get; set; } = string.Empty;
    }

    public class MemoryUsageInfo
    {
        public long AllocatedMemory { get; set; }
        public long WorkingSet { get; set; }
        public long Gen0Collections { get; set; }
        public long Gen1Collections { get; set; }
        public long Gen2Collections { get; set; }
        public DateTime MeasurementTime { get; set; }
    }

    public class CodeFile
    {
        public string FilePath { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Language { get; set; } = "csharp";
    }

    public class SolutionOptimizationResult
    {
        public string SolutionPath { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<FileOptimizationResult> FileResults { get; set; } = new();
        public SolutionOptimizationSummary Summary { get; set; } = new();
        public string Error { get; set; } = string.Empty;
    }

    public class FileOptimizationResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string OriginalCode { get; set; } = string.Empty;
        public string OptimizedCode { get; set; } = string.Empty;
        public List<OptimizationSuggestion> Suggestions { get; set; } = new();
        public MemoryUsageInfo MemoryBefore { get; set; } = new();
        public MemoryUsageInfo MemoryAfter { get; set; } = new();
        public double ImprovementPercentage { get; set; }
        public bool HasOptimizations { get; set; }
        public int OptimizationCount { get; set; }
        public DateTime AnalysisTime { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    public class SolutionOptimizationSummary
    {
        public int TotalFilesAnalyzed { get; set; }
        public int FilesWithOptimizations { get; set; }
        public int TotalOptimizations { get; set; }
        public double AverageMemoryImprovement { get; set; }
        public TimeSpan AnalysisDuration { get; set; }
        public Dictionary<string, int> TopOptimizationTypes { get; set; } = new();
        public List<FileOptimizationResult> MostOptimizedFiles { get; set; } = new();
    }
}
