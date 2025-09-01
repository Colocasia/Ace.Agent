#include "test_framework.h"
#include "../wrapper/ckg_wrapper.h"
#include <tree_sitter/api.h>

// Java测试代码示例
static const char* java_test_code = 
    "public class HelloWorld {\n"
    "    public static void main(String[] args) {\n"
    "        System.out.println(\"Hello, Java!\");\n"
    "    }\n"
    "}\n";

static const char* java_class_code = 
    "public class Calculator {\n"
    "    private int value;\n"
    "    \n"
    "    public Calculator() {\n"
    "        this.value = 0;\n"
    "    }\n"
    "    \n"
    "    public Calculator(int initialValue) {\n"
    "        this.value = initialValue;\n"
    "    }\n"
    "    \n"
    "    public int add(int x) {\n"
    "        this.value += x;\n"
    "        return this.value;\n"
    "    }\n"
    "    \n"
    "    public int getValue() {\n"
    "        return this.value;\n"
    "    }\n"
    "    \n"
    "    public static int multiply(int a, int b) {\n"
    "        return a * b;\n"
    "    }\n"
    "}\n";

static const char* java_interface_code = 
    "public interface Drawable {\n"
    "    void draw();\n"
    "    void setColor(String color);\n"
    "    String getColor();\n"
    "}\n"
    "\n"
    "public class Circle implements Drawable {\n"
    "    private String color;\n"
    "    private double radius;\n"
    "    \n"
    "    public Circle(double radius) {\n"
    "        this.radius = radius;\n"
    "        this.color = \"black\";\n"
    "    }\n"
    "    \n"
    "    @Override\n"
    "    public void draw() {\n"
    "        System.out.println(\"Drawing a circle with radius \" + radius);\n"
    "    }\n"
    "    \n"
    "    @Override\n"
    "    public void setColor(String color) {\n"
    "        this.color = color;\n"
    "    }\n"
    "    \n"
    "    @Override\n"
    "    public String getColor() {\n"
    "        return this.color;\n"
    "    }\n"
    "}\n";

static const char* java_generic_code = 
    "import java.util.List;\n"
    "import java.util.ArrayList;\n"
    "\n"
    "public class GenericContainer<T> {\n"
    "    private List<T> items;\n"
    "    \n"
    "    public GenericContainer() {\n"
    "        this.items = new ArrayList<>();\n"
    "    }\n"
    "    \n"
    "    public void add(T item) {\n"
    "        items.add(item);\n"
    "    }\n"
    "    \n"
    "    public T get(int index) {\n"
    "        return items.get(index);\n"
    "    }\n"
    "    \n"
    "    public int size() {\n"
    "        return items.size();\n"
    "    }\n"
    "}\n";

// 测试Java语言支持
int test_java_language_support() {
    TEST_START("Java Language Support");
    
    // 测试语言是否被支持
    TEST_ASSERT(ckg_is_language_supported(CKG_LANG_JAVA), "Java language should be supported");
    
    TEST_PASS("Java Language Support");
}

// 测试Java基本解析
int test_java_basic_parsing() {
    TEST_START("Java Basic Parsing");
    
    char* temp_file = create_temp_file(java_test_code, "java");
    TEST_ASSERT(temp_file != NULL, "Should create temporary Java file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_JAVA);
    TEST_ASSERT(result != NULL, "Should parse Java file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到类
    TEST_ASSERT(result->classes != NULL, "Should find classes");
    TEST_ASSERT(result->class_count > 0, "Should find at least one class");
    
    // 检查HelloWorld类
    int found_hello_world = 0;
    for (int i = 0; i < result->class_count; i++) {
        if (strcmp(result->classes[i].name, "HelloWorld") == 0) {
            found_hello_world = 1;
            break;
        }
    }
    TEST_ASSERT(found_hello_world, "Should find HelloWorld class");
    
    // 检查是否找到main方法
    TEST_ASSERT(result->functions != NULL, "Should find methods");
    TEST_ASSERT(result->function_count > 0, "Should find at least one method (main)");
    
    int found_main = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "main") == 0) {
            found_main = 1;
            break;
        }
    }
    TEST_ASSERT(found_main, "Should find main method");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Java Basic Parsing");
}

// 测试Java类和方法解析
int test_java_class_parsing() {
    TEST_START("Java Class Parsing");
    
    char* temp_file = create_temp_file(java_class_code, "java");
    TEST_ASSERT(temp_file != NULL, "Should create temporary Java file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_JAVA);
    TEST_ASSERT(result != NULL, "Should parse Java file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到Calculator类
    TEST_ASSERT(result->classes != NULL, "Should find classes");
    TEST_ASSERT(result->class_count > 0, "Should find at least one class");
    
    int found_calculator = 0;
    for (int i = 0; i < result->class_count; i++) {
        if (strcmp(result->classes[i].name, "Calculator") == 0) {
            found_calculator = 1;
            break;
        }
    }
    TEST_ASSERT(found_calculator, "Should find Calculator class");
    
    // 检查方法
    TEST_ASSERT(result->function_count >= 4, "Should find multiple methods");
    
    // 检查特定方法
    int found_add = 0, found_get_value = 0, found_multiply = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "add") == 0) {
            found_add = 1;
        } else if (strcmp(result->functions[i].name, "getValue") == 0) {
            found_get_value = 1;
        } else if (strcmp(result->functions[i].name, "multiply") == 0) {
            found_multiply = 1;
        }
    }
    TEST_ASSERT(found_add, "Should find 'add' method");
    TEST_ASSERT(found_get_value, "Should find 'getValue' method");
    TEST_ASSERT(found_multiply, "Should find 'multiply' method");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Java Class Parsing");
}

// 测试Java接口解析
int test_java_interface_parsing() {
    TEST_START("Java Interface Parsing");
    
    char* temp_file = create_temp_file(java_interface_code, "java");
    TEST_ASSERT(temp_file != NULL, "Should create temporary Java file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_JAVA);
    TEST_ASSERT(result != NULL, "Should parse Java interface file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到接口和类
    TEST_ASSERT(result->classes != NULL, "Should find classes/interfaces");
    TEST_ASSERT(result->class_count >= 2, "Should find interface and implementing class");
    
    // 检查方法
    TEST_ASSERT(result->function_count >= 3, "Should find interface and implementation methods");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Java Interface Parsing");
}

// 测试Java泛型解析
int test_java_generic_parsing() {
    TEST_START("Java Generic Parsing");
    
    char* temp_file = create_temp_file(java_generic_code, "java");
    TEST_ASSERT(temp_file != NULL, "Should create temporary Java file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_JAVA);
    TEST_ASSERT(result != NULL, "Should parse Java generic file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到泛型类
    TEST_ASSERT(result->classes != NULL, "Should find generic classes");
    TEST_ASSERT(result->class_count > 0, "Should find at least one generic class");
    
    // 检查方法
    TEST_ASSERT(result->function_count >= 3, "Should find generic methods");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Java Generic Parsing");
}

// 测试错误处理
int test_java_error_handling() {
    TEST_START("Java Error Handling");
    
    // 测试不存在的文件
    CKGResult* result = ckg_parse("/nonexistent/file.java", CKG_LANG_JAVA);
    TEST_ASSERT(result != NULL, "Should return result even for nonexistent file");
    TEST_ASSERT(!result->success, "Should fail for nonexistent file");
    
    ckg_free_result(result);
    
    // 测试语法错误的Java代码
    const char* invalid_java_code = "public class Test { public void method( { } }";
    char* temp_file = create_temp_file(invalid_java_code, "java");
    TEST_ASSERT(temp_file != NULL, "Should create temporary file with invalid Java code");
    
    result = ckg_parse(temp_file, CKG_LANG_JAVA);
    TEST_ASSERT(result != NULL, "Should return result for invalid Java code");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Java Error Handling");
}

int main() {
    printf(ANSI_COLOR_BLUE "=== Java Language Parser Tests ===" ANSI_COLOR_RESET "\n\n");
    
    // 初始化CKG
    ckg_init();
    
    // 运行测试
    test_java_language_support();
    test_java_basic_parsing();
    test_java_class_parsing();
    test_java_interface_parsing();
    test_java_generic_parsing();
    test_java_error_handling();
    
    // 清理
    ckg_cleanup();
    
    // 显示测试结果
    TEST_SUMMARY();
    
    return (tests_failed == 0) ? 0 : 1;
}