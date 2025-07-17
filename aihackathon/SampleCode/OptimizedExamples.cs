// Example of optimized code that the AI might generate
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleCode.Optimized
{
    public class OptimizedDataProcessor
    {
        // Memory optimized: Use StringBuilder for string concatenation
        public string ProcessData(List<string> data)
        {
            var result = new StringBuilder(data.Count * 10); // Pre-allocate capacity
            
            foreach (var item in data)
            {
                result.Append(item).Append(", ");
            }
            
            // Memory optimized: Use strongly-typed collection
            var numbers = new List<int>(1000); // Pre-allocate capacity
            for (int i = 0; i < 1000; i++)
            {
                numbers.Add(i); // No boxing
            }
            
            // Memory optimized: Chain LINQ operations to avoid intermediate collections
            var processedNumbers = numbers
                .Where(x => x > 500)
                .Select(x => x.ToString())
                .ToList();
            
            // Memory optimized: Use 'using' statements for proper disposal
            using var fileStream = new System.IO.FileStream("temp.txt", System.IO.FileMode.Create);
            using var writer = new System.IO.StreamWriter(fileStream);
            writer.Write(result.ToString());
            
            return result.ToString();
        }
        
        // Memory optimized: Use IEnumerable for lazy evaluation
        public IEnumerable<Customer> GetCustomers()
        {
            for (int i = 0; i < 10000; i++)
            {
                yield return new Customer(
                    i, 
                    $"Customer {i}", // Use string interpolation
                    $"customer{i}@example.com"
                )
                {
                    Orders = new List<Order>() // Only create when needed
                };
            }
        }
    }
    
    // Memory optimized: Use record for immutable data
    public record Customer(int Id, string Name, string Email)
    {
        public List<Order> Orders { get; init; } = new();
    }
    
    // Memory optimized: Use record for value semantics
    public record Order(int Id, DateTime OrderDate, decimal Amount);
    
    // Memory optimized: Async streaming and proper resource management
    public class OptimizedAdvancedProcessor
    {
        // Memory optimized: Use async enumerable for streaming
        public async IAsyncEnumerable<int> LoadDataStreamAsync()
        {
            for (int i = 0; i < 100000; i++)
            {
                await Task.Delay(1); // Simulate async operation
                yield return i;
            }
        }
        
        // Memory optimized: Process data in chunks
        public async Task ProcessLargeDatasetAsync()
        {
            const int chunkSize = 1000;
            var chunk = new List<int>(chunkSize);
            
            await foreach (var item in LoadDataStreamAsync())
            {
                chunk.Add(item);
                
                if (chunk.Count >= chunkSize)
                {
                    await ProcessChunkAsync(chunk);
                    chunk.Clear();
                }
            }
            
            // Process remaining items
            if (chunk.Count > 0)
            {
                await ProcessChunkAsync(chunk);
            }
        }
        
        private async Task ProcessChunkAsync(List<int> chunk)
        {
            // Memory optimized: Use StringBuilder for string building
            var report = new StringBuilder(chunk.Count * 20);
            
            foreach (var item in chunk)
            {
                report.AppendLine($"Item: {item}");
            }
            
            // Memory optimized: Use HttpClient as singleton with proper disposal
            using var client = new System.Net.Http.HttpClient();
            var response = await client.GetStringAsync("https://api.example.com/data");
            
            // Process response...
        }
        
        // Memory optimized: Use value types for better performance
        public readonly struct OptimizedDataPoint
        {
            public int X { get; }
            public int Y { get; }
            public ReadOnlyMemory<char> Label { get; }
            
            public OptimizedDataPoint(int x, int y, ReadOnlyMemory<char> label)
            {
                X = x;
                Y = y;
                Label = label;
            }
        }
        
        // Memory optimized: Use Span<T> for stack-allocated processing
        public void ProcessNumbers(ReadOnlySpan<int> numbers)
        {
            Span<int> results = numbers.Length <= 1024 ? stackalloc int[numbers.Length] : new int[numbers.Length];
            
            for (int i = 0; i < numbers.Length; i++)
            {
                results[i] = numbers[i] * 2;
            }
            
            // Process results...
        }
        
        // Memory optimized: Use object pooling for frequently created objects
        private readonly System.Collections.Concurrent.ConcurrentQueue<StringBuilder> _stringBuilderPool = new();
        
        public string ProcessWithPooling(IEnumerable<string> items)
        {
            if (!_stringBuilderPool.TryDequeue(out var sb))
            {
                sb = new StringBuilder();
            }
            
            try
            {
                foreach (var item in items)
                {
                    sb.AppendLine(item);
                }
                
                return sb.ToString();
            }
            finally
            {
                sb.Clear();
                _stringBuilderPool.Enqueue(sb);
            }
        }
        
        // Memory optimized: Use ArrayPool for temporary arrays
        public void ProcessWithArrayPool(int count)
        {
            var pool = System.Buffers.ArrayPool<int>.Shared;
            var array = pool.Rent(count);
            
            try
            {
                // Use array...
                for (int i = 0; i < count; i++)
                {
                    array[i] = i * 2;
                }
            }
            finally
            {
                pool.Return(array);
            }
        }
    }
    
    // Memory optimized: Immutable data container
    public class OptimizedDataContainer
    {
        private readonly List<Customer> _data = new();
        
        public IReadOnlyList<Customer> Data => _data.AsReadOnly();
        
        public void AddRange(IEnumerable<Customer> points)
        {
            if (points is ICollection<Customer> collection)
            {
                // Memory optimized: Use collection count for capacity
                _data.EnsureCapacity(_data.Count + collection.Count);
                _data.AddRange(collection);
            }
            else
            {
                // Memory optimized: Single enumeration
                _data.AddRange(points);
            }
        }
    }
}
