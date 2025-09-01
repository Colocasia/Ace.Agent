#include "test_framework.h"
#include "../wrapper/ckg_wrapper.h"
#include <tree_sitter/api.h>

// C#测试代码示例
static const char* csharp_test_code = 
    "using System;\n"
    "\n"
    "namespace HelloWorld\n"
    "{\n"
    "    class Program\n"
    "    {\n"
    "        static void Main(string[] args)\n"
    "        {\n"
    "            Console.WriteLine(\"Hello, C#!\");\n"
    "        }\n"
    "    }\n"
    "}\n";

static const char* csharp_class_code = 
    "using System;\n"
    "\n"
    "namespace MathLibrary\n"
    "{\n"
    "    /// <summary>\n"
    "    /// Calculator class for basic mathematical operations\n"
    "    /// </summary>\n"
    "    public class Calculator\n"
    "    {\n"
    "        /// <summary>\n"
    "        /// Adds two integers\n"
    "        /// </summary>\n"
    "        /// <param name=\"a\">First number</param>\n"
    "        /// <param name=\"b\">Second number</param>\n"
    "        /// <returns>Sum of a and b</returns>\n"
    "        public int Add(int a, int b)\n"
    "        {\n"
    "            return a + b;\n"
    "        }\n"
    "\n"
    "        /// <summary>\n"
    "        /// Multiplies two integers\n"
    "        /// </summary>\n"
    "        /// <param name=\"a\">First number</param>\n"
    "        /// <param name=\"b\">Second number</param>\n"
    "        /// <returns>Product of a and b</returns>\n"
    "        public int Multiply(int a, int b)\n"
    "        {\n"
    "            return a * b;\n"
    "        }\n"
    "\n"
    "        /// <summary>\n"
    "        /// Calculates the area of a circle\n"
    "        /// </summary>\n"
    "        /// <param name=\"radius\">Radius of the circle</param>\n"
    "        /// <returns>Area of the circle</returns>\n"
    "        public double CircleArea(double radius)\n"
    "        {\n"
    "            return Math.PI * radius * radius;\n"
    "        }\n"
    "    }\n"
    "}\n";

static const char* csharp_interface_code = 
    "using System;\n"
    "\n"
    "namespace Shapes\n"
    "{\n"
    "    /// <summary>\n"
    "    /// Interface for geometric shapes\n"
    "    /// </summary>\n"
    "    public interface IShape\n"
    "    {\n"
    "        double Area { get; }\n"
    "        double Perimeter { get; }\n"
    "        void Draw();\n"
    "    }\n"
    "\n"
    "    /// <summary>\n"
    "    /// Rectangle implementation of IShape\n"
    "    /// </summary>\n"
    "    public class Rectangle : IShape\n"
    "    {\n"
    "        public double Width { get; set; }\n"
    "        public double Height { get; set; }\n"
    "\n"
    "        public Rectangle(double width, double height)\n"
    "        {\n"
    "            Width = width;\n"
    "            Height = height;\n"
    "        }\n"
    "\n"
    "        public double Area => Width * Height;\n"
    "\n"
    "        public double Perimeter => 2 * (Width + Height);\n"
    "\n"
    "        public void Draw()\n"
    "        {\n"
    "            Console.WriteLine($\"Drawing rectangle: {Width}x{Height}\");\n"
    "        }\n"
    "    }\n"
    "\n"
    "    /// <summary>\n"
    "    /// Circle implementation of IShape\n"
    "    /// </summary>\n"
    "    public class Circle : IShape\n"
    "    {\n"
    "        public double Radius { get; set; }\n"
    "\n"
    "        public Circle(double radius)\n"
    "        {\n"
    "            Radius = radius;\n"
    "        }\n"
    "\n"
    "        public double Area => Math.PI * Radius * Radius;\n"
    "\n"
    "        public double Perimeter => 2 * Math.PI * Radius;\n"
    "\n"
    "        public void Draw()\n"
    "        {\n"
    "            Console.WriteLine($\"Drawing circle with radius: {Radius}\");\n"
    "        }\n"
    "    }\n"
    "}\n";

static const char* csharp_generic_code = 
    "using System;\n"
    "using System.Collections.Generic;\n"
    "\n"
    "namespace Collections\n"
    "{\n"
    "    /// <summary>\n"
    "    /// Generic stack implementation\n"
    "    /// </summary>\n"
    "    /// <typeparam name=\"T\">Type of elements in the stack</typeparam>\n"
    "    public class Stack<T>\n"
    "    {\n"
    "        private List<T> items = new List<T>();\n"
    "\n"
    "        /// <summary>\n"
    "        /// Gets the number of elements in the stack\n"
    "        /// </summary>\n"
    "        public int Count => items.Count;\n"
    "\n"
    "        /// <summary>\n"
    "        /// Pushes an item onto the stack\n"
    "        /// </summary>\n"
    "        /// <param name=\"item\">Item to push</param>\n"
    "        public void Push(T item)\n"
    "        {\n"
    "            items.Add(item);\n"
    "        }\n"
    "\n"
    "        /// <summary>\n"
    "        /// Pops an item from the stack\n"
    "        /// </summary>\n"
    "        /// <returns>The popped item</returns>\n"
    "        /// <exception cref=\"InvalidOperationException\">Thrown when stack is empty</exception>\n"
    "        public T Pop()\n"
    "        {\n"
    "            if (items.Count == 0)\n"
    "                throw new InvalidOperationException(\"Stack is empty\");\n"
    "\n"
    "            T item = items[items.Count - 1];\n"
    "            items.RemoveAt(items.Count - 1);\n"
    "            return item;\n"
    "        }\n"
    "\n"
    "        /// <summary>\n"
    "        /// Peeks at the top item without removing it\n"
    "        /// </summary>\n"
    "        /// <returns>The top item</returns>\n"
    "        /// <exception cref=\"InvalidOperationException\">Thrown when stack is empty</exception>\n"
    "        public T Peek()\n"
    "        {\n"
    "            if (items.Count == 0)\n"
    "                throw new InvalidOperationException(\"Stack is empty\");\n"
    "\n"
    "            return items[items.Count - 1];\n"
    "        }\n"
    "\n"
    "        /// <summary>\n"
    "        /// Checks if the stack is empty\n"
    "        /// </summary>\n"
    "        /// <returns>True if empty, false otherwise</returns>\n"
    "        public bool IsEmpty()\n"
    "        {\n"
    "            return items.Count == 0;\n"
    "        }\n"
    "    }\n"
    "}\n";

// 测试C#语言支持
int test_csharp_language_support() {
    TEST_START("C# Language Support");
    
    // 测试语言是否被支持
    TEST_ASSERT(ckg_is_language_supported(CKG_LANG_CSHARP), "C# language should be supported");
    
    TEST_PASS("C# Language Support");
}

// 测试C#基本解析
int test_csharp_basic_parsing() {
    TEST_START("C# Basic Parsing");
    
    char* temp_file = create_temp_file(csharp_test_code, "cs");
    TEST_ASSERT(temp_file != NULL, "Should create temporary C# file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_CSHARP);
    TEST_ASSERT(result != NULL, "Should parse C# file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到函数
    TEST_ASSERT(result->functions != NULL, "Should find functions");
    TEST_ASSERT(result->function_count > 0, "Should find at least one function (Main)");
    
    // 检查Main函数
    int found_main = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "Main") == 0) {
            found_main = 1;
            break;
        }
    }
    TEST_ASSERT(found_main, "Should find Main function");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C# Basic Parsing");
}

// 测试C#类和方法解析
int test_csharp_class_parsing() {
    TEST_START("C# Class Parsing");
    
    char* temp_file = create_temp_file(csharp_class_code, "cs");
    TEST_ASSERT(temp_file != NULL, "Should create temporary C# file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_CSHARP);
    TEST_ASSERT(result != NULL, "Should parse C# file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查函数数量
    TEST_ASSERT(result->function_count >= 3, "Should find at least 3 methods");
    
    // 检查特定方法
    int found_add = 0, found_multiply = 0, found_circle_area = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "Add") == 0) {
            found_add = 1;
        } else if (strcmp(result->functions[i].name, "Multiply") == 0) {
            found_multiply = 1;
        } else if (strcmp(result->functions[i].name, "CircleArea") == 0) {
            found_circle_area = 1;
        }
    }
    TEST_ASSERT(found_add, "Should find 'Add' method");
    TEST_ASSERT(found_multiply, "Should find 'Multiply' method");
    TEST_ASSERT(found_circle_area, "Should find 'CircleArea' method");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C# Class Parsing");
}

// 测试C#接口解析
int test_csharp_interface_parsing() {
    TEST_START("C# Interface Parsing");
    
    char* temp_file = create_temp_file(csharp_interface_code, "cs");
    TEST_ASSERT(temp_file != NULL, "Should create temporary C# file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_CSHARP);
    TEST_ASSERT(result != NULL, "Should parse C# interface file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查方法和构造函数
    TEST_ASSERT(result->function_count >= 4, "Should find interface methods and constructors");
    
    // 检查特定方法
    int found_draw = 0, found_constructor = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "Draw") == 0) {
            found_draw = 1;
        } else if (strstr(result->functions[i].name, "Rectangle") != NULL || 
                   strstr(result->functions[i].name, "Circle") != NULL) {
            found_constructor = 1;
        }
    }
    TEST_ASSERT(found_draw, "Should find 'Draw' methods");
    TEST_ASSERT(found_constructor, "Should find constructors");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C# Interface Parsing");
}

// 测试C#泛型解析
int test_csharp_generic_parsing() {
    TEST_START("C# Generic Parsing");
    
    char* temp_file = create_temp_file(csharp_generic_code, "cs");
    TEST_ASSERT(temp_file != NULL, "Should create temporary C# file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_CSHARP);
    TEST_ASSERT(result != NULL, "Should parse C# generic file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查方法
    TEST_ASSERT(result->function_count >= 4, "Should find generic class methods");
    
    // 检查特定方法
    int found_push = 0, found_pop = 0, found_peek = 0, found_is_empty = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "Push") == 0) {
            found_push = 1;
        } else if (strcmp(result->functions[i].name, "Pop") == 0) {
            found_pop = 1;
        } else if (strcmp(result->functions[i].name, "Peek") == 0) {
            found_peek = 1;
        } else if (strcmp(result->functions[i].name, "IsEmpty") == 0) {
            found_is_empty = 1;
        }
    }
    TEST_ASSERT(found_push, "Should find 'Push' method");
    TEST_ASSERT(found_pop, "Should find 'Pop' method");
    TEST_ASSERT(found_peek, "Should find 'Peek' method");
    TEST_ASSERT(found_is_empty, "Should find 'IsEmpty' method");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C# Generic Parsing");
}

// 测试错误处理
int test_csharp_error_handling() {
    TEST_START("C# Error Handling");
    
    // 测试不存在的文件
    CKGResult* result = ckg_parse("/nonexistent/file.cs", CKG_LANG_CSHARP);
    TEST_ASSERT(result != NULL, "Should return result even for nonexistent file");
    TEST_ASSERT(!result->success, "Should fail for nonexistent file");
    
    ckg_free_result(result);
    
    // 测试语法错误的C#代码
    const char* invalid_csharp_code = "using System;\nclass Test { void Method( { Console.WriteLine(\"test\"); } }";
    char* temp_file = create_temp_file(invalid_csharp_code, "cs");
    TEST_ASSERT(temp_file != NULL, "Should create temporary file with invalid C# code");
    
    result = ckg_parse(temp_file, CKG_LANG_CSHARP);
    TEST_ASSERT(result != NULL, "Should return result for invalid C# code");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C# Error Handling");
}

int main() {
    printf(ANSI_COLOR_BLUE "=== C# Language Parser Tests ===" ANSI_COLOR_RESET "\n\n");
    
    // 初始化CKG
    ckg_init();
    
    // 运行测试
    test_csharp_language_support();
    test_csharp_basic_parsing();
    test_csharp_class_parsing();
    test_csharp_interface_parsing();
    test_csharp_generic_parsing();
    test_csharp_error_handling();
    
    // 清理
    ckg_cleanup();
    
    // 显示测试结果
    TEST_SUMMARY();
    
    return (tests_failed == 0) ? 0 : 1;
}