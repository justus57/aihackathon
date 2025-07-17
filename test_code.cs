using System;
using System.Collections.Generic;

namespace TestCode
{
    public class TestClass
    {
        public string ProcessData(List<string> data)
        {
            // Memory inefficient: String concatenation in loop
            string result = "";
            foreach (var item in data)
            {
                result += item + ", ";
            }
            return result;
        }
    }
}
