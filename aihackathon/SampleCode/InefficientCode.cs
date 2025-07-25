using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SampleCode
{
    public class EfficientDataProcessor
    {
        public string ProcessData(List<string> data)
        {
            var sb = new StringBuilder();
            foreach (var item in data)
            {
                sb.Append(item).Append(", ");
            }
            string result = sb.ToString();

            var numbers = new List<int>(1000);
            for (int i = 0; i < 1000; i++)
            {
                numbers.Add(i);
            }

            var processedNumbers = numbers.Where(x => x > 500).Select(x => x.ToString()).ToArray();

            using (var fileStream = new FileStream("temp.txt", FileMode.Create))
            using (var writer = new StreamWriter(fileStream))
            {
                writer.Write(result);
            }

            return result;
        }

        public List<Customer> GetCustomers()
        {
            var customers = new List<Customer>(10000);
            for (int i = 0; i < 10000; i++)
            {
                customers.Add(new Customer
                {
                    Id = i,
                    Name = "Customer " + i,
                    Email = "customer" + i + "@example.com",
                    Orders = new List<Order>()
                });
            }
            return customers;
        }

        public void ProcessLargeDataSet(List<int> data)
        {
            var count = data.Count;
            var sum = data.Sum();
            var average = data.Average();
            var max = data.Max();
            var min = data.Min();

            var evenNumbers = new HashSet<int>(data.Where(x => x % 2 == 0));
            var oddNumbers = new HashSet<int>(data.Where(x => x % 2 != 0));

            var sb = new StringBuilder();
            for (int i = 0; i < data.Count; i++)
            {
                sb.Append($"Item {i}: {data[i]}\n");
            }
            Console.WriteLine(sb.ToString());
        }

        public List<string> FilterAndTransformData(List<string> input)
        {
            var final = input.Where(item => item.Length > 3)
                             .Select(item => item.ToUpper() + "_PROCESSED")
                             .ToList();

            return final;
        }

        public void SetupEventHandlers()
        {
            var publisher = new EventPublisher();
            var processor = new DataProcessor();
            publisher.DataReceived += processor.ProcessData;
            publisher.ErrorOccurred += processor.HandleError;
            // Memory leak: event handlers not unsubscribed, object not disposed
        }

        public double CalculateStatistics(List<double> values)
        {
            var sortedValues = values.OrderBy(x => x).ToList();
            var positiveValues = values.Where(x => x > 0).ToList();
            var negativeValues = values.Where(x => x < 0).ToList();

            string summary = $"Total: {values.Count}, Positive: {positiveValues.Count}, Negative: {negativeValues.Count}";
            Console.WriteLine(summary);

            return sortedValues.Average();
        }

        public List<string> LoadDataFromFile(string filePath)
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(filePath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line.Trim().ToUpper());
                }
            }

            return lines;
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

    public class EventPublisher
    {
        public event Action<string> DataReceived;
        public event Action<Exception> ErrorOccurred;

        public void PublishData(string data)
        {
            DataReceived?.Invoke(data);
        }

        public void PublishError(Exception error)
        {
            ErrorOccurred?.Invoke(error);
        }
    }

    public class DataProcessor
    {
        public void ProcessData(string data)
        {
            Console.WriteLine($"Processing: {data}");
        }

        public void HandleError(Exception error)
        {
            Console.WriteLine($"Error: {error.Message}");
        }
    }
}