using CodeOptimizer.Models;
using System.Diagnostics;

namespace CodeOptimizer.Services
{
    public class MemoryProfiler
    {
        public static MemoryUsageInfo GetCurrentMemoryUsage()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var process = Process.GetCurrentProcess();
            
            return new MemoryUsageInfo
            {
                AllocatedMemory = GC.GetTotalMemory(false),
                WorkingSet = process.WorkingSet64,
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                MeasurementTime = DateTime.Now
            };
        }

        public static async Task<MemoryUsageInfo> MeasureMemoryUsage(Func<Task> action)
        {
            var beforeMemory = GetCurrentMemoryUsage();
            
            await action();
            
            var afterMemory = GetCurrentMemoryUsage();
            
            return new MemoryUsageInfo
            {
                AllocatedMemory = afterMemory.AllocatedMemory - beforeMemory.AllocatedMemory,
                WorkingSet = afterMemory.WorkingSet - beforeMemory.WorkingSet,
                Gen0Collections = afterMemory.Gen0Collections - beforeMemory.Gen0Collections,
                Gen1Collections = afterMemory.Gen1Collections - beforeMemory.Gen1Collections,
                Gen2Collections = afterMemory.Gen2Collections - beforeMemory.Gen2Collections,
                MeasurementTime = DateTime.Now
            };
        }

        public static void DisplayMemoryUsage(MemoryUsageInfo memoryInfo, string title)
        {
            Console.WriteLine($"\n--- {title} ---");
            Console.WriteLine($"Allocated Memory: {FormatBytes(memoryInfo.AllocatedMemory)}");
            Console.WriteLine($"Working Set: {FormatBytes(memoryInfo.WorkingSet)}");
            Console.WriteLine($"Gen 0 Collections: {memoryInfo.Gen0Collections}");
            Console.WriteLine($"Gen 1 Collections: {memoryInfo.Gen1Collections}");
            Console.WriteLine($"Gen 2 Collections: {memoryInfo.Gen2Collections}");
            Console.WriteLine($"Measurement Time: {memoryInfo.MeasurementTime:yyyy-MM-dd HH:mm:ss}");
        }

        private static string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return $"{decimal.Divide(bytes, max):##.##} {order}";
                max /= scale;
            }
            return "0 Bytes";
        }
    }
}
