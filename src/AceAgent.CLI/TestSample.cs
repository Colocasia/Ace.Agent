using System;
using System.Collections.Generic;

namespace TestNamespace
{
    public class TestClass
    {
        private string _name;
        public int Age { get; set; }
        
        public TestClass(string name)
        {
            _name = name;
        }
        
        public void PrintInfo()
        {
            Console.WriteLine($"Name: {_name}, Age: {Age}");
        }
        
        public static int CalculateSum(int a, int b)
        {
            return a + b;
        }
        
        private bool IsValid()
        {
            return !string.IsNullOrEmpty(_name) && Age > 0;
        }
    }
    
    public interface ITestInterface
    {
        void DoSomething();
    }
}