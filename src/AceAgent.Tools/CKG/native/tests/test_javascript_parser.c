#include "test_framework.h"
#include "../wrapper/ckg_wrapper.h"
#include <tree_sitter/api.h>

// JavaScript测试代码示例
static const char* javascript_test_code = 
    "// Simple JavaScript example\n"
    "function greet(name) {\n"
    "    console.log('Hello, ' + name + '!');\n"
    "}\n"
    "\n"
    "greet('JavaScript');\n";

static const char* javascript_class_code = 
    "// JavaScript class example\n"
    "class Calculator {\n"
    "    constructor() {\n"
    "        this.result = 0;\n"
    "    }\n"
    "\n"
    "    /**\n"
    "     * Adds two numbers\n"
    "     * @param {number} a - First number\n"
    "     * @param {number} b - Second number\n"
    "     * @returns {number} Sum of a and b\n"
    "     */\n"
    "    add(a, b) {\n"
    "        this.result = a + b;\n"
    "        return this.result;\n"
    "    }\n"
    "\n"
    "    /**\n"
    "     * Multiplies two numbers\n"
    "     * @param {number} a - First number\n"
    "     * @param {number} b - Second number\n"
    "     * @returns {number} Product of a and b\n"
    "     */\n"
    "    multiply(a, b) {\n"
    "        this.result = a * b;\n"
    "        return this.result;\n"
    "    }\n"
    "\n"
    "    /**\n"
    "     * Calculates the area of a circle\n"
    "     * @param {number} radius - Radius of the circle\n"
    "     * @returns {number} Area of the circle\n"
    "     */\n"
    "    circleArea(radius) {\n"
    "        this.result = Math.PI * radius * radius;\n"
    "        return this.result;\n"
    "    }\n"
    "\n"
    "    /**\n"
    "     * Gets the current result\n"
    "     * @returns {number} Current result\n"
    "     */\n"
    "    getResult() {\n"
    "        return this.result;\n"
    "    }\n"
    "}\n"
    "\n"
    "// Export the class\n"
    "module.exports = Calculator;\n";

static const char* javascript_async_code = 
    "// Async/await example\n"
    "const fs = require('fs').promises;\n"
    "\n"
    "/**\n"
    " * Reads a file asynchronously\n"
    " * @param {string} filename - Name of the file to read\n"
    " * @returns {Promise<string>} File contents\n"
    " */\n"
    "async function readFileAsync(filename) {\n"
    "    try {\n"
    "        const data = await fs.readFile(filename, 'utf8');\n"
    "        return data;\n"
    "    } catch (error) {\n"
    "        console.error('Error reading file:', error);\n"
    "        throw error;\n"
    "    }\n"
    "}\n"
    "\n"
    "/**\n"
    " * Writes data to a file asynchronously\n"
    " * @param {string} filename - Name of the file to write\n"
    " * @param {string} data - Data to write\n"
    " * @returns {Promise<void>}\n"
    " */\n"
    "async function writeFileAsync(filename, data) {\n"
    "    try {\n"
    "        await fs.writeFile(filename, data, 'utf8');\n"
    "        console.log('File written successfully');\n"
    "    } catch (error) {\n"
    "        console.error('Error writing file:', error);\n"
    "        throw error;\n"
    "    }\n"
    "}\n"
    "\n"
    "/**\n"
    " * Processes multiple files concurrently\n"
    " * @param {string[]} filenames - Array of filenames\n"
    " * @returns {Promise<string[]>} Array of file contents\n"
    " */\n"
    "async function processFiles(filenames) {\n"
    "    const promises = filenames.map(filename => readFileAsync(filename));\n"
    "    return await Promise.all(promises);\n"
    "}\n";

static const char* javascript_arrow_functions_code = 
    "// Arrow functions and modern JavaScript\n"
    "const numbers = [1, 2, 3, 4, 5];\n"
    "\n"
    "// Simple arrow function\n"
    "const square = x => x * x;\n"
    "\n"
    "// Arrow function with multiple parameters\n"
    "const add = (a, b) => a + b;\n"
    "\n"
    "// Arrow function with block body\n"
    "const processArray = (arr) => {\n"
    "    const doubled = arr.map(x => x * 2);\n"
    "    const filtered = doubled.filter(x => x > 5);\n"
    "    return filtered.reduce((sum, x) => sum + x, 0);\n"
    "};\n"
    "\n"
    "// Higher-order function\n"
    "const createMultiplier = (factor) => {\n"
    "    return (number) => number * factor;\n"
    "};\n"
    "\n"
    "// Object with methods\n"
    "const mathUtils = {\n"
    "    pi: Math.PI,\n"
    "    \n"
    "    circleArea: function(radius) {\n"
    "        return this.pi * radius * radius;\n"
    "    },\n"
    "    \n"
    "    rectangleArea: (width, height) => width * height,\n"
    "    \n"
    "    triangleArea(base, height) {\n"
    "        return 0.5 * base * height;\n"
    "    }\n"
    "};\n";

// 测试JavaScript语言支持
int test_javascript_language_support() {
    TEST_START("JavaScript Language Support");
    
    // 测试语言是否被支持
    TEST_ASSERT(ckg_is_language_supported(CKG_LANG_JAVASCRIPT), "JavaScript language should be supported");
    
    TEST_PASS("JavaScript Language Support");
}

// 测试JavaScript基本解析
int test_javascript_basic_parsing() {
    TEST_START("JavaScript Basic Parsing");
    
    char* temp_file = create_temp_file(javascript_test_code, "js");
    TEST_ASSERT(temp_file != NULL, "Should create temporary JavaScript file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_JAVASCRIPT);
    TEST_ASSERT(result != NULL, "Should parse JavaScript file successfully");
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
    
    TEST_PASS("JavaScript Basic Parsing");
}

// 测试JavaScript类解析
int test_javascript_class_parsing() {
    TEST_START("JavaScript Class Parsing");
    
    char* temp_file = create_temp_file(javascript_class_code, "js");
    TEST_ASSERT(temp_file != NULL, "Should create temporary JavaScript file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_JAVASCRIPT);
    TEST_ASSERT(result != NULL, "Should parse JavaScript file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查函数数量
    TEST_ASSERT(result->function_count >= 4, "Should find at least 4 methods");
    
    // 检查特定方法
    int found_constructor = 0, found_add = 0, found_multiply = 0, found_circle_area = 0, found_get_result = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "constructor") == 0) {
            found_constructor = 1;
        } else if (strcmp(result->functions[i].name, "add") == 0) {
            found_add = 1;
        } else if (strcmp(result->functions[i].name, "multiply") == 0) {
            found_multiply = 1;
        } else if (strcmp(result->functions[i].name, "circleArea") == 0) {
            found_circle_area = 1;
        } else if (strcmp(result->functions[i].name, "getResult") == 0) {
            found_get_result = 1;
        }
    }
    TEST_ASSERT(found_constructor, "Should find 'constructor' method");
    TEST_ASSERT(found_add, "Should find 'add' method");
    TEST_ASSERT(found_multiply, "Should find 'multiply' method");
    TEST_ASSERT(found_circle_area, "Should find 'circleArea' method");
    TEST_ASSERT(found_get_result, "Should find 'getResult' method");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("JavaScript Class Parsing");
}

// 测试JavaScript异步函数解析
int test_javascript_async_parsing() {
    TEST_START("JavaScript Async Parsing");
    
    char* temp_file = create_temp_file(javascript_async_code, "js");
    TEST_ASSERT(temp_file != NULL, "Should create temporary JavaScript file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_JAVASCRIPT);
    TEST_ASSERT(result != NULL, "Should parse JavaScript async file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查异步函数
    TEST_ASSERT(result->function_count >= 3, "Should find async functions");
    
    // 检查特定函数
    int found_read_file = 0, found_write_file = 0, found_process_files = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "readFileAsync") == 0) {
            found_read_file = 1;
        } else if (strcmp(result->functions[i].name, "writeFileAsync") == 0) {
            found_write_file = 1;
        } else if (strcmp(result->functions[i].name, "processFiles") == 0) {
            found_process_files = 1;
        }
    }
    TEST_ASSERT(found_read_file, "Should find 'readFileAsync' function");
    TEST_ASSERT(found_write_file, "Should find 'writeFileAsync' function");
    TEST_ASSERT(found_process_files, "Should find 'processFiles' function");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("JavaScript Async Parsing");
}

// 测试JavaScript箭头函数解析
int test_javascript_arrow_functions_parsing() {
    TEST_START("JavaScript Arrow Functions Parsing");
    
    char* temp_file = create_temp_file(javascript_arrow_functions_code, "js");
    TEST_ASSERT(temp_file != NULL, "Should create temporary JavaScript file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_JAVASCRIPT);
    TEST_ASSERT(result != NULL, "Should parse JavaScript arrow functions file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查函数（注意：箭头函数可能被识别为变量赋值，具体取决于解析器实现）
    TEST_ASSERT(result->function_count >= 2, "Should find functions and methods");
    
    // 检查对象方法
    int found_circle_area = 0, found_triangle_area = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "circleArea") == 0) {
            found_circle_area = 1;
        } else if (strcmp(result->functions[i].name, "triangleArea") == 0) {
            found_triangle_area = 1;
        }
    }
    TEST_ASSERT(found_circle_area, "Should find 'circleArea' method");
    TEST_ASSERT(found_triangle_area, "Should find 'triangleArea' method");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("JavaScript Arrow Functions Parsing");
}

// 测试错误处理
int test_javascript_error_handling() {
    TEST_START("JavaScript Error Handling");
    
    // 测试不存在的文件
    CKGResult* result = ckg_parse("/nonexistent/file.js", CKG_LANG_JAVASCRIPT);
    TEST_ASSERT(result != NULL, "Should return result even for nonexistent file");
    TEST_ASSERT(!result->success, "Should fail for nonexistent file");
    
    ckg_free_result(result);
    
    // 测试语法错误的JavaScript代码
    const char* invalid_js_code = "function test( { console.log('test'); }";
    char* temp_file = create_temp_file(invalid_js_code, "js");
    TEST_ASSERT(temp_file != NULL, "Should create temporary file with invalid JavaScript code");
    
    result = ckg_parse(temp_file, CKG_LANG_JAVASCRIPT);
    TEST_ASSERT(result != NULL, "Should return result for invalid JavaScript code");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("JavaScript Error Handling");
}

int main() {
    printf(ANSI_COLOR_BLUE "=== JavaScript Language Parser Tests ===" ANSI_COLOR_RESET "\n\n");
    
    // 初始化CKG
    ckg_init();
    
    // 运行测试
    test_javascript_language_support();
    test_javascript_basic_parsing();
    test_javascript_class_parsing();
    test_javascript_async_parsing();
    test_javascript_arrow_functions_parsing();
    test_javascript_error_handling();
    
    // 清理
    ckg_cleanup();
    
    // 显示测试结果
    TEST_SUMMARY();
    
    return (tests_failed == 0) ? 0 : 1;
}