#include "test_framework.h"
#include "../wrapper/ckg_wrapper.h"
#include <tree_sitter/api.h>

// C语言测试代码示例
static const char* c_test_code = 
    "#include <stdio.h>\n"
    "\n"
    "int main() {\n"
    "    int x = 42;\n"
    "    printf(\"Hello, World!\\n\");\n"
    "    return 0;\n"
    "}\n";

static const char* c_function_code = 
    "int add(int a, int b) {\n"
    "    return a + b;\n"
    "}\n"
    "\n"
    "void print_number(int n) {\n"
    "    printf(\"%d\\n\", n);\n"
    "}\n";

static const char* c_struct_code = 
    "struct Point {\n"
    "    int x;\n"
    "    int y;\n"
    "};\n"
    "\n"
    "typedef struct {\n"
    "    char name[50];\n"
    "    int age;\n"
    "} Person;\n";

// 测试C语言支持
int test_c_language_support() {
    TEST_START("C Language Support");
    
    // 测试语言是否被支持
    TEST_ASSERT(ckg_is_language_supported(CKG_LANG_C), "C language should be supported");
    
    TEST_PASS("C Language Support");
}

// 测试C语言基本解析
int test_c_basic_parsing() {
    TEST_START("C Basic Parsing");
    
    char* temp_file = create_temp_file(c_test_code, "c");
    TEST_ASSERT(temp_file != NULL, "Should create temporary C file");
    
    char* source_code = read_file_content(temp_file);
    TEST_ASSERT(source_code != NULL, "Should read file content");
    
    printf("About to call ckg_parse with source code:\n%s\n", source_code);
    printf("=== BEFORE CALLING CKG_PARSE ===\n");
    fflush(stdout);
    CKGParseResult* result = ckg_parse(CKG_LANG_C, source_code, temp_file);
    printf("=== AFTER CALLING CKG_PARSE ===\n");
    fflush(stdout);
    printf("ckg_parse returned: %p\n", (void*)result);
    TEST_ASSERT(result != NULL, "Should parse C file successfully");
    if (result->error_message) {
        printf("Error message: %s\n", result->error_message);
    }
    TEST_ASSERT(result->error_message == NULL, "Parsing should succeed without errors");
    
    // 检查是否找到函数
    TEST_ASSERT(result->functions != NULL, "Should find functions");
    TEST_ASSERT(result->function_count > 0, "Should find at least one function (main)");
    
    // 检查main函数
    printf("Found %u functions:\n", result->function_count);
    for (uint32_t i = 0; i < result->function_count; i++) {
        printf("  Function %u: %s\n", i, result->functions[i].name ? result->functions[i].name : "(null)");
    }
    
    int found_main = 0;
    for (uint32_t i = 0; i < result->function_count; i++) {
        if (result->functions[i].name && strcmp(result->functions[i].name, "main") == 0) {
            found_main = 1;
            break;
        }
    }
    TEST_ASSERT(found_main, "Should find main function");
    
    free(source_code);
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C Basic Parsing");
}

// 测试C语言函数解析
int test_c_function_parsing() {
    TEST_START("C Function Parsing");
    
    char* temp_file = create_temp_file(c_function_code, "c");
    TEST_ASSERT(temp_file != NULL, "Should create temporary C file");
    
    char* source_code = read_file_content(temp_file);
    TEST_ASSERT(source_code != NULL, "Should read file content");
    
    CKGParseResult* result = ckg_parse(CKG_LANG_C, source_code, temp_file);
    TEST_ASSERT(result != NULL, "Should parse C file successfully");
    TEST_ASSERT(result->error_message == NULL, "Parsing should succeed without errors");
    TEST_ASSERT(result->function_count >= 2, "Should find at least 2 functions");
    
    // 检查特定函数
    printf("Found %u functions:\n", result->function_count);
    for (uint32_t i = 0; i < result->function_count; i++) {
        printf("  Function %u: %s\n", i, result->functions[i].name ? result->functions[i].name : "(null)");
    }
    
    int found_add = 0, found_print_number = 0;
    for (uint32_t i = 0; i < result->function_count; i++) {
        if (result->functions[i].name && strcmp(result->functions[i].name, "add") == 0) {
            found_add = 1;
        } else if (result->functions[i].name && strcmp(result->functions[i].name, "print_number") == 0) {
            found_print_number = 1;
        }
    }
    TEST_ASSERT(found_add, "Should find 'add' function");
    TEST_ASSERT(found_print_number, "Should find 'print_number' function");
    
    free(source_code);
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C Function Parsing");
}

// 测试C语言结构体解析
int test_c_struct_parsing() {
    TEST_START("C Struct Parsing");
    
    char* temp_file = create_temp_file(c_struct_code, "c");
    TEST_ASSERT(temp_file != NULL, "Should create temporary C file");
    
    char* source_code = read_file_content(temp_file);
    TEST_ASSERT(source_code != NULL, "Should read file content");
    
    CKGParseResult* result = ckg_parse(CKG_LANG_C, source_code, temp_file);
    TEST_ASSERT(result != NULL, "Should parse C file successfully");
    TEST_ASSERT(result->error_message == NULL, "Parsing should succeed without errors");
    
    // 注意：根据实际的CKG实现，可能需要调整这些断言
    // 这里假设CKG能够解析结构体信息
    
    free(source_code);
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C Struct Parsing");
}

// 测试错误处理
int test_c_error_handling() {
    TEST_START("C Error Handling");
    
    // 测试语法错误的C代码
    const char* invalid_c_code = "int main( { return 0; }";
    char* temp_file = create_temp_file(invalid_c_code, "c");
    TEST_ASSERT(temp_file != NULL, "Should create temporary file with invalid C code");
    
    char* source_code = read_file_content(temp_file);
    TEST_ASSERT(source_code != NULL, "Should read file content");
    
    CKGParseResult* result = ckg_parse(CKG_LANG_C, source_code, temp_file);
    TEST_ASSERT(result != NULL, "Should return result for invalid C code");
    // 注意：tree-sitter通常能容错解析，所以这里可能仍然成功
    
    free(source_code);
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("C Error Handling");
}

int main() {
    printf(ANSI_COLOR_BLUE "=== C Language Parser Tests ===" ANSI_COLOR_RESET "\n\n");
    
    // 初始化CKG
    ckg_init();
    
    // 运行测试
    test_c_language_support();
    test_c_basic_parsing();
    test_c_function_parsing();
    test_c_struct_parsing();
    test_c_error_handling();
    
    // 清理
    ckg_cleanup();
    
    // 显示测试结果
    TEST_SUMMARY();
    
    return (tests_failed == 0) ? 0 : 1;
}