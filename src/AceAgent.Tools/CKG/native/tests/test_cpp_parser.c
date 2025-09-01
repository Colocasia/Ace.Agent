#include "test_framework.h"
#include "../wrapper/ckg_wrapper.h"
#include <tree_sitter/api.h>

// C++测试代码示例
static const char* cpp_test_code = 
    "#include <iostream>\n"
    "#include <string>\n"
    "\n"
    "int main() {\n"
    "    std::string message = \"Hello, C++!\";\n"
    "    std::cout << message << std::endl;\n"
    "    return 0;\n"
    "}\n";

static const char* cpp_class_code = 
    "class Calculator {\n"
    "private:\n"
    "    int value;\n"
    "\n"
    "public:\n"
    "    Calculator(int initial = 0) : value(initial) {}\n"
    "    \n"
    "    int add(int x) {\n"
    "        value += x;\n"
    "        return value;\n"
    "    }\n"
    "    \n"
    "    int getValue() const {\n"
    "        return value;\n"
    "    }\n"
    "};\n";

static const char* cpp_template_code = 
    "template<typename T>\n"
    "class Vector {\n"
    "private:\n"
    "    T* data;\n"
    "    size_t size;\n"
    "\n"
    "public:\n"
    "    Vector() : data(nullptr), size(0) {}\n"
    "    \n"
    "    void push_back(const T& item) {\n"
    "        // Implementation\n"
    "    }\n"
    "    \n"
    "    T& operator[](size_t index) {\n"
    "        return data[index];\n"
    "    }\n"
    "};\n";

static const char* cpp_namespace_code = 
    "namespace math {\n"
    "    double pi = 3.14159;\n"
    "    \n"
    "    double square(double x) {\n"
    "        return x * x;\n"
    "    }\n"
    "    \n"
    "    namespace geometry {\n"
    "        double circle_area(double radius) {\n"
    "            return pi * square(radius);\n"
    "        }\n"
    "    }\n"
    "}\n";

// 测试C++语言支持
int test_cpp_language_support() {
    TEST_START("C++ Language Support");
    
    // 测试语言是否被支持
    TEST_ASSERT(ckg_is_language_supported(CKG_LANG_CPP), "C++ language should be supported");
    
    TEST_PASS("C++ Language Support");
}

// 测试C++基本解析
int test_cpp_basic_parsing() {
    TEST_START("C++ Basic Parsing");
    
    char* temp_file = create_temp_file(cpp_test_code, "cpp");
    TEST_ASSERT(temp_file != NULL, "Should create temporary C++ file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_CPP);
    TEST_ASSERT(result != NULL, "Should parse C++ file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    TEST_ASSERT(result->functions != NULL, "Should find functions");
    TEST_ASSERT(result->function_count > 0, "Should find at least one function (main)");
    
    // 检查是否找到main函数
    int found_main = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "main") == 0) {
            found_main = 1;
            break;
        }
    }
    TEST_ASSERT(found_main, "Should find main function");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C++ Basic Parsing");
}

// 测试C++类解析
int test_cpp_class_parsing() {
    TEST_START("C++ Class Parsing");
    
    char* temp_file = create_temp_file(cpp_class_code, "cpp");
    TEST_ASSERT(temp_file != NULL, "Should create temporary C++ file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_CPP);
    TEST_ASSERT(result != NULL, "Should parse C++ file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到类
    TEST_ASSERT(result->classes != NULL, "Should find classes");
    TEST_ASSERT(result->class_count > 0, "Should find at least one class");
    
    // 检查Calculator类
    int found_calculator = 0;
    for (int i = 0; i < result->class_count; i++) {
        if (strcmp(result->classes[i].name, "Calculator") == 0) {
            found_calculator = 1;
            break;
        }
    }
    TEST_ASSERT(found_calculator, "Should find Calculator class");
    
    // 检查方法
    TEST_ASSERT(result->function_count >= 2, "Should find class methods");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C++ Class Parsing");
}

// 测试C++模板解析
int test_cpp_template_parsing() {
    TEST_START("C++ Template Parsing");
    
    char* temp_file = create_temp_file(cpp_template_code, "cpp");
    TEST_ASSERT(temp_file != NULL, "Should create temporary C++ file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_CPP);
    TEST_ASSERT(result != NULL, "Should parse C++ template file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 模板类应该被识别为类
    TEST_ASSERT(result->classes != NULL, "Should find template classes");
    TEST_ASSERT(result->class_count > 0, "Should find at least one template class");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C++ Template Parsing");
}

// 测试C++命名空间解析
int test_cpp_namespace_parsing() {
    TEST_START("C++ Namespace Parsing");
    
    char* temp_file = create_temp_file(cpp_namespace_code, "cpp");
    TEST_ASSERT(temp_file != NULL, "Should create temporary C++ file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_CPP);
    TEST_ASSERT(result != NULL, "Should parse C++ namespace file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到函数
    TEST_ASSERT(result->function_count >= 2, "Should find functions in namespaces");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C++ Namespace Parsing");
}

// 测试错误处理
int test_cpp_error_handling() {
    TEST_START("C++ Error Handling");
    
    // 测试不存在的文件
    CKGResult* result = ckg_parse("/nonexistent/file.cpp", CKG_LANG_CPP);
    TEST_ASSERT(result != NULL, "Should return result even for nonexistent file");
    TEST_ASSERT(!result->success, "Should fail for nonexistent file");
    
    ckg_free_result(result);
    
    // 测试语法错误的C++代码
    const char* invalid_cpp_code = "class Test { public int x; }";
    char* temp_file = create_temp_file(invalid_cpp_code, "cpp");
    TEST_ASSERT(temp_file != NULL, "Should create temporary file with invalid C++ code");
    
    result = ckg_parse(temp_file, CKG_LANG_CPP);
    TEST_ASSERT(result != NULL, "Should return result for invalid C++ code");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C++ Error Handling");
}

int main() {
    printf(ANSI_COLOR_BLUE "=== C++ Language Parser Tests ===" ANSI_COLOR_RESET "\n\n");
    
    // 初始化CKG
    ckg_init();
    
    // 运行测试
    test_cpp_language_support();
    test_cpp_basic_parsing();
    test_cpp_class_parsing();
    test_cpp_template_parsing();
    test_cpp_namespace_parsing();
    test_cpp_error_handling();
    
    // 清理
    ckg_cleanup();
    
    // 显示测试结果
    TEST_SUMMARY();
    
    return (tests_failed == 0) ? 0 : 1;
}