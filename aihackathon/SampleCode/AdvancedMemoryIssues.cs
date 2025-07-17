// Advanced sample code with various memory optimization opportunities
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleCode
{
    public class AdvancedMemoryIssues
    {
        // Memory leak: Static event handler
        public static event Action<string> StaticEvent;
        
        // Memory inefficient: Large array initialized unnecessarily
        private readonly int[] _largeArray = new int[1000000];
        
        public async Task ProcessLargeDataset()
        {
            // Memory inefficient: Loading all data into memory at once
            var allData = await LoadAllDataAsync();
            
            // Memory inefficient: Multiple enumerations
            var count = allData.Count();
            var sum = allData.Sum();
            var average = allData.Average();
            
            // Memory inefficient: String concatenation in async context
            var report = "";
            foreach (var item in allData)
            {
                report += $"Item: {item}, ";
            }
            
            // Memory inefficient: Unnecessary Task.Run
            var result = await Task.Run(() => ProcessDataSynchronously(allData));
            
            // Memory inefficient: Creating unnecessary closures
            var processors = new List<Func<int, int>>();
            for (int i = 0; i < 10; i++)
            {
                processors.Add(x => x * i); // Closure captures loop variable
            }
            
            // Memory inefficient: Not disposing HttpClient
            var client = new System.Net.Http.HttpClient();
            var response = await client.GetStringAsync("https://api.example.com/data");
            
            // Memory inefficient: Boxing in dictionary
            var mixedData = new Dictionary<string, object>();
            for (int i = 0; i < 1000; i++)
            {
                mixedData[$"key{i}"] = i; // Boxing int to object
            }
        }
        
        // Memory inefficient: Synchronous method that should be async
        private List<int> ProcessDataSynchronously(IEnumerable<int> data)
        {
            var result = new List<int>();
            foreach (var item in data)
            {
                // Simulate heavy computation
                System.Threading.Thread.Sleep(1);
                result.Add(item * 2);
            }
            return result;
        }
        
        // Memory inefficient: Loading all data at once instead of streaming
        private async Task<List<int>> LoadAllDataAsync()
        {
            var data = new List<int>();
            for (int i = 0; i < 100000; i++)
            {
                data.Add(i);
            }
            return data;
        }
        
        // Memory inefficient: Struct with large size
        public struct LargeStruct
        {
            public decimal Value1;
            public decimal Value2;
            public decimal Value3;
            public decimal Value4;
            public decimal Value5;
            public string Description; // Reference type in struct
        }
        
        // Memory inefficient: Exception handling in loops
        public void ProcessWithExceptions(List<string> items)
        {
            foreach (var item in items)
            {
                try
                {
                    var number = int.Parse(item);
                    // Process number
                }
                catch (FormatException)
                {
                    // Handle exception
                }
            }
        }
        
        // Memory inefficient: Recursive method without tail call optimization
        public long CalculateFactorial(int n)
        {
            if (n <= 1) return 1;
            return n * CalculateFactorial(n - 1); // Stack overflow risk
        }
        
        // Memory inefficient: Delegate allocation in hot path
        public void ProcessItems(List<int> items)
        {
            foreach (var item in items)
            {
                // Delegate allocation on each iteration
                DoWork(() => Console.WriteLine($"Processing {item}"));
            }
        }
        
        private void DoWork(Action action)
        {
            action();
        }
        
        // Memory inefficient: Finalizer without proper disposal pattern
        ~AdvancedMemoryIssues()
        {
            // Finalizer can delay garbage collection
            Dispose();
        }
        
        public void Dispose()
        {
            // Cleanup code
        }
    }
    
    // Memory inefficient: Class instead of record for immutable data
    public class DataPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Label { get; set; }
        
        public override bool Equals(object obj)
        {
            if (obj is DataPoint other)
            {
                return X == other.X && Y == other.Y && Label == other.Label;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Label);
        }
    }
    
    // Memory inefficient: Mutable collections exposed publicly
    public class DataContainer
    {
        public List<DataPoint> Data { get; set; } = new List<DataPoint>();
        
        public void AddRange(IEnumerable<DataPoint> points)
        {
            // Memory inefficient: Multiple enumerations
            if (points.Any())
            {
                Data.AddRange(points.ToList()); // Unnecessary ToList()
            }
        }
    }
}
