#include "test_framework.h"
#include "../wrapper/ckg_wrapper.h"
#include <tree_sitter/api.h>

// Python测试代码示例
static const char* python_test_code = 
    "# Simple Python example\n"
    "def greet(name):\n"
    "    \"\"\"Greets a person by name\"\"\"\n"
    "    print(f'Hello, {name}!')\n"
    "\n"
    "if __name__ == '__main__':\n"
    "    greet('Python')\n";

static const char* python_class_code = 
    "# Python class example\n"
    "import math\n"
    "\n"
    "class Calculator:\n"
    "    \"\"\"A simple calculator class\"\"\"\n"
    "    \n"
    "    def __init__(self):\n"
    "        \"\"\"Initialize the calculator\"\"\"\n"
    "        self.result = 0\n"
    "    \n"
    "    def add(self, a, b):\n"
    "        \"\"\"\n"
    "        Adds two numbers\n"
    "        \n"
    "        Args:\n"
    "            a (float): First number\n"
    "            b (float): Second number\n"
    "        \n"
    "        Returns:\n"
    "            float: Sum of a and b\n"
    "        \"\"\"\n"
    "        self.result = a + b\n"
    "        return self.result\n"
    "    \n"
    "    def multiply(self, a, b):\n"
    "        \"\"\"\n"
    "        Multiplies two numbers\n"
    "        \n"
    "        Args:\n"
    "            a (float): First number\n"
    "            b (float): Second number\n"
    "        \n"
    "        Returns:\n"
    "            float: Product of a and b\n"
    "        \"\"\"\n"
    "        self.result = a * b\n"
    "        return self.result\n"
    "    \n"
    "    def circle_area(self, radius):\n"
    "        \"\"\"\n"
    "        Calculates the area of a circle\n"
    "        \n"
    "        Args:\n"
    "            radius (float): Radius of the circle\n"
    "        \n"
    "        Returns:\n"
    "            float: Area of the circle\n"
    "        \"\"\"\n"
    "        self.result = math.pi * radius * radius\n"
    "        return self.result\n"
    "    \n"
    "    def get_result(self):\n"
    "        \"\"\"\n"
    "        Gets the current result\n"
    "        \n"
    "        Returns:\n"
    "            float: Current result\n"
    "        \"\"\"\n"
    "        return self.result\n"
    "    \n"
    "    @staticmethod\n"
    "    def power(base, exponent):\n"
    "        \"\"\"\n"
    "        Calculates base raised to the power of exponent\n"
    "        \n"
    "        Args:\n"
    "            base (float): Base number\n"
    "            exponent (float): Exponent\n"
    "        \n"
    "        Returns:\n"
    "            float: Result of base^exponent\n"
    "        \"\"\"\n"
    "        return base ** exponent\n"
    "    \n"
    "    @classmethod\n"
    "    def create_with_initial_value(cls, initial_value):\n"
    "        \"\"\"\n"
    "        Creates a calculator with an initial value\n"
    "        \n"
    "        Args:\n"
    "            initial_value (float): Initial value for the calculator\n"
    "        \n"
    "        Returns:\n"
    "            Calculator: New calculator instance\n"
    "        \"\"\"\n"
    "        calc = cls()\n"
    "        calc.result = initial_value\n"
    "        return calc\n";

static const char* python_async_code = 
    "# Python async/await example\n"
    "import asyncio\n"
    "import aiofiles\n"
    "\n"
    "async def read_file_async(filename):\n"
    "    \"\"\"\n"
    "    Reads a file asynchronously\n"
    "    \n"
    "    Args:\n"
    "        filename (str): Name of the file to read\n"
    "    \n"
    "    Returns:\n"
    "        str: File contents\n"
    "    \"\"\"\n"
    "    try:\n"
    "        async with aiofiles.open(filename, 'r') as file:\n"
    "            data = await file.read()\n"
    "            return data\n"
    "    except Exception as error:\n"
    "        print(f'Error reading file: {error}')\n"
    "        raise\n"
    "\n"
    "async def write_file_async(filename, data):\n"
    "    \"\"\"\n"
    "    Writes data to a file asynchronously\n"
    "    \n"
    "    Args:\n"
    "        filename (str): Name of the file to write\n"
    "        data (str): Data to write\n"
    "    \"\"\"\n"
    "    try:\n"
    "        async with aiofiles.open(filename, 'w') as file:\n"
    "            await file.write(data)\n"
    "            print('File written successfully')\n"
    "    except Exception as error:\n"
    "        print(f'Error writing file: {error}')\n"
    "        raise\n"
    "\n"
    "async def process_files(filenames):\n"
    "    \"\"\"\n"
    "    Processes multiple files concurrently\n"
    "    \n"
    "    Args:\n"
    "        filenames (list): List of filenames\n"
    "    \n"
    "    Returns:\n"
    "        list: List of file contents\n"
    "    \"\"\"\n"
    "    tasks = [read_file_async(filename) for filename in filenames]\n"
    "    return await asyncio.gather(*tasks)\n"
    "\n"
    "async def main():\n"
    "    \"\"\"Main async function\"\"\"\n"
    "    files = ['file1.txt', 'file2.txt', 'file3.txt']\n"
    "    contents = await process_files(files)\n"
    "    for i, content in enumerate(contents):\n"
    "        print(f'File {i+1}: {len(content)} characters')\n";

static const char* python_decorators_code = 
    "# Python decorators and advanced features\n"
    "from functools import wraps\n"
    "import time\n"
    "\n"
    "def timing_decorator(func):\n"
    "    \"\"\"Decorator to measure function execution time\"\"\"\n"
    "    @wraps(func)\n"
    "    def wrapper(*args, **kwargs):\n"
    "        start_time = time.time()\n"
    "        result = func(*args, **kwargs)\n"
    "        end_time = time.time()\n"
    "        print(f'{func.__name__} took {end_time - start_time:.4f} seconds')\n"
    "        return result\n"
    "    return wrapper\n"
    "\n"
    "def retry_decorator(max_attempts=3):\n"
    "    \"\"\"Decorator to retry function execution\"\"\"\n"
    "    def decorator(func):\n"
    "        @wraps(func)\n"
    "        def wrapper(*args, **kwargs):\n"
    "            for attempt in range(max_attempts):\n"
    "                try:\n"
    "                    return func(*args, **kwargs)\n"
    "                except Exception as e:\n"
    "                    if attempt == max_attempts - 1:\n"
    "                        raise\n"
    "                    print(f'Attempt {attempt + 1} failed: {e}')\n"
    "        return wrapper\n"
    "    return decorator\n"
    "\n"
    "class MathUtils:\n"
    "    \"\"\"Utility class with decorated methods\"\"\"\n"
    "    \n"
    "    @staticmethod\n"
    "    @timing_decorator\n"
    "    def fibonacci(n):\n"
    "        \"\"\"Calculate fibonacci number\"\"\"\n"
    "        if n <= 1:\n"
    "            return n\n"
    "        return MathUtils.fibonacci(n-1) + MathUtils.fibonacci(n-2)\n"
    "    \n"
    "    @classmethod\n"
    "    @retry_decorator(max_attempts=5)\n"
    "    def divide_with_retry(cls, a, b):\n"
    "        \"\"\"Division with retry on failure\"\"\"\n"
    "        if b == 0:\n"
    "            raise ValueError('Cannot divide by zero')\n"
    "        return a / b\n"
    "\n"
    "def generator_function(n):\n"
    "    \"\"\"Generator function example\"\"\"\n"
    "    for i in range(n):\n"
    "        yield i * i\n"
    "\n"
    "def list_comprehension_example(numbers):\n"
    "    \"\"\"Example using list comprehensions\"\"\"\n"
    "    squares = [x**2 for x in numbers if x % 2 == 0]\n"
    "    return squares\n";

// 测试Python语言支持
int test_python_language_support() {
    TEST_START("Python Language Support");
    
    // 测试语言是否被支持
    TEST_ASSERT(ckg_is_language_supported(CKG_LANG_PYTHON), "Python language should be supported");
    
    TEST_PASS("Python Language Support");
}

// 测试Python基本解析
int test_python_basic_parsing() {
    TEST_START("Python Basic Parsing");
    
    char* temp_file = create_temp_file(python_test_code, "py");
    TEST_ASSERT(temp_file != NULL, "Should create temporary Python file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_PYTHON);
    TEST_ASSERT(result != NULL, "Should parse Python file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到函数
    TEST_ASSERT(result->functions != NULL, "Should find functions");
    TEST_ASSERT(result->function_count > 0, "Should find at least one function (greet)");
    
    // 检查greet函数
    int found_greet = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "greet") == 0) {
            found_greet = 1;
            break;
        }
    }
    TEST_ASSERT(found_greet, "Should find greet function");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Python Basic Parsing");
}

// 测试Python类解析
int test_python_class_parsing() {
    TEST_START("Python Class Parsing");
    
    char* temp_file = create_temp_file(python_class_code, "py");
    TEST_ASSERT(temp_file != NULL, "Should create temporary Python file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_PYTHON);
    TEST_ASSERT(result != NULL, "Should parse Python file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查函数数量
    TEST_ASSERT(result->function_count >= 6, "Should find at least 6 methods");
    
    // 检查特定方法
    int found_init = 0, found_add = 0, found_multiply = 0, found_circle_area = 0, found_get_result = 0;
    int found_power = 0, found_create_with_initial_value = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "__init__") == 0) {
            found_init = 1;
        } else if (strcmp(result->functions[i].name, "add") == 0) {
            found_add = 1;
        } else if (strcmp(result->functions[i].name, "multiply") == 0) {
            found_multiply = 1;
        } else if (strcmp(result->functions[i].name, "circle_area") == 0) {
            found_circle_area = 1;
        } else if (strcmp(result->functions[i].name, "get_result") == 0) {
            found_get_result = 1;
        } else if (strcmp(result->functions[i].name, "power") == 0) {
            found_power = 1;
        } else if (strcmp(result->functions[i].name, "create_with_initial_value") == 0) {
            found_create_with_initial_value = 1;
        }
    }
    TEST_ASSERT(found_init, "Should find '__init__' method");
    TEST_ASSERT(found_add, "Should find 'add' method");
    TEST_ASSERT(found_multiply, "Should find 'multiply' method");
    TEST_ASSERT(found_circle_area, "Should find 'circle_area' method");
    TEST_ASSERT(found_get_result, "Should find 'get_result' method");
    TEST_ASSERT(found_power, "Should find 'power' static method");
    TEST_ASSERT(found_create_with_initial_value, "Should find 'create_with_initial_value' class method");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Python Class Parsing");
}

// 测试Python异步函数解析
int test_python_async_parsing() {
    TEST_START("Python Async Parsing");
    
    char* temp_file = create_temp_file(python_async_code, "py");
    TEST_ASSERT(temp_file != NULL, "Should create temporary Python file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_PYTHON);
    TEST_ASSERT(result != NULL, "Should parse Python async file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查异步函数
    TEST_ASSERT(result->function_count >= 4, "Should find async functions");
    
    // 检查特定函数
    int found_read_file = 0, found_write_file = 0, found_process_files = 0, found_main = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "read_file_async") == 0) {
            found_read_file = 1;
        } else if (strcmp(result->functions[i].name, "write_file_async") == 0) {
            found_write_file = 1;
        } else if (strcmp(result->functions[i].name, "process_files") == 0) {
            found_process_files = 1;
        } else if (strcmp(result->functions[i].name, "main") == 0) {
            found_main = 1;
        }
    }
    TEST_ASSERT(found_read_file, "Should find 'read_file_async' function");
    TEST_ASSERT(found_write_file, "Should find 'write_file_async' function");
    TEST_ASSERT(found_process_files, "Should find 'process_files' function");
    TEST_ASSERT(found_main, "Should find 'main' function");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Python Async Parsing");
}

// 测试Python装饰器解析
int test_python_decorators_parsing() {
    TEST_START("Python Decorators Parsing");
    
    char* temp_file = create_temp_file(python_decorators_code, "py");
    TEST_ASSERT(temp_file != NULL, "Should create temporary Python file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_PYTHON);
    TEST_ASSERT(result != NULL, "Should parse Python decorators file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查函数
    TEST_ASSERT(result->function_count >= 6, "Should find decorator functions and methods");
    
    // 检查特定函数
    int found_timing_decorator = 0, found_retry_decorator = 0, found_fibonacci = 0;
    int found_divide_with_retry = 0, found_generator_function = 0, found_list_comprehension = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "timing_decorator") == 0) {
            found_timing_decorator = 1;
        } else if (strcmp(result->functions[i].name, "retry_decorator") == 0) {
            found_retry_decorator = 1;
        } else if (strcmp(result->functions[i].name, "fibonacci") == 0) {
            found_fibonacci = 1;
        } else if (strcmp(result->functions[i].name, "divide_with_retry") == 0) {
            found_divide_with_retry = 1;
        } else if (strcmp(result->functions[i].name, "generator_function") == 0) {
            found_generator_function = 1;
        } else if (strcmp(result->functions[i].name, "list_comprehension_example") == 0) {
            found_list_comprehension = 1;
        }
    }
    TEST_ASSERT(found_timing_decorator, "Should find 'timing_decorator' function");
    TEST_ASSERT(found_retry_decorator, "Should find 'retry_decorator' function");
    TEST_ASSERT(found_fibonacci, "Should find 'fibonacci' method");
    TEST_ASSERT(found_divide_with_retry, "Should find 'divide_with_retry' method");
    TEST_ASSERT(found_generator_function, "Should find 'generator_function' function");
    TEST_ASSERT(found_list_comprehension, "Should find 'list_comprehension_example' function");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Python Decorators Parsing");
}

// 测试错误处理
int test_python_error_handling() {
    TEST_START("Python Error Handling");
    
    // 测试不存在的文件
    CKGResult* result = ckg_parse("/nonexistent/file.py", CKG_LANG_PYTHON);
    TEST_ASSERT(result != NULL, "Should return result even for nonexistent file");
    TEST_ASSERT(!result->success, "Should fail for nonexistent file");
    
    ckg_free_result(result);
    
    // 测试语法错误的Python代码
    const char* invalid_python_code = "def test(:\n    print('test')";
    char* temp_file = create_temp_file(invalid_python_code, "py");
    TEST_ASSERT(temp_file != NULL, "Should create temporary file with invalid Python code");
    
    result = ckg_parse(temp_file, CKG_LANG_PYTHON);
    TEST_ASSERT(result != NULL, "Should return result for invalid Python code");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Python Error Handling");
}

int main() {
    printf(ANSI_COLOR_BLUE "=== Python Language Parser Tests ===" ANSI_COLOR_RESET "\n\n");
    
    // 初始化CKG
    ckg_init();
    
    // 运行测试
    test_python_language_support();
    test_python_basic_parsing();
    test_python_class_parsing();
    test_python_async_parsing();
    test_python_decorators_parsing();
    test_python_error_handling();
    
    // 清理
    ckg_cleanup();
    
    // 显示测试结果
    TEST_SUMMARY();
    
    return (tests_failed == 0) ? 0 : 1;
}