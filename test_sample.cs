using System;

namespace TestNamespace
{
    public class TestClass
    {
        public string Name { get; set; }
        
        public void TestMethod()
        {
            Console.WriteLine("Hello World");
        }
        
        public int Calculate(int a, int b)
        {
            return a + b;
        }
    }
}