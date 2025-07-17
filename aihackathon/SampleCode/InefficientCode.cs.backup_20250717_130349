// Sample code with memory optimization opportunities
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleCode
{
    public class InefficiuentDataProcessor
    {
        public string ProcessData(List<string> data)
        {
            // Memory inefficient: String concatenation in loop
            string result = "";
            foreach (var item in data)
            {
                result += item + ", ";
            }
            
            // Memory inefficient: Boxing in LINQ
            var numbers = new List<object>();
            for (int i = 0; i < 1000; i++)
            {
                numbers.Add(i); // Boxing int to object
            }
            
            // Memory inefficient: Unnecessary collection creation
            var filteredNumbers = numbers.Where(x => (int)x > 500).ToList();
            var processedNumbers = filteredNumbers.Select(x => x.ToString()).ToList();
            
            // Memory inefficient: Not disposing resources
            var fileStream = new System.IO.FileStream("temp.txt", System.IO.FileMode.Create);
            var writer = new System.IO.StreamWriter(fileStream);
            writer.Write(result);
            writer.Close();
            fileStream.Close();
            
            return result;
        }
        
        public List<Customer> GetCustomers()
        {
            // Memory inefficient: Loading all data at once
            var customers = new List<Customer>();
            for (int i = 0; i < 10000; i++)
            {
                customers.Add(new Customer 
                { 
                    Id = i, 
                    Name = "Customer " + i.ToString(),
                    Email = "customer" + i.ToString() + "@example.com",
                    Orders = new List<Order>()
                });
            }
            return customers;
        }
    }
    
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Order> Orders { get; set; }
    }
    
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Amount { get; set; }
    }
}
