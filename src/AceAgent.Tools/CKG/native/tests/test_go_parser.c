#include "test_framework.h"
#include "../wrapper/ckg_wrapper.h"
#include <tree_sitter/api.h>

// Go测试代码示例
static const char* go_test_code = 
    "package main\n"
    "\n"
    "import \"fmt\"\n"
    "\n"
    "func main() {\n"
    "    fmt.Println(\"Hello, Go!\")\n"
    "}\n";

static const char* go_function_code = 
    "package math\n"
    "\n"
    "import \"math\"\n"
    "\n"
    "// Add returns the sum of two integers\n"
    "func Add(a, b int) int {\n"
    "    return a + b\n"
    "}\n"
    "\n"
    "// Multiply returns the product of two integers\n"
    "func Multiply(a, b int) int {\n"
    "    return a * b\n"
    "}\n"
    "\n"
    "// CircleArea calculates the area of a circle\n"
    "func CircleArea(radius float64) float64 {\n"
    "    return math.Pi * radius * radius\n"
    "}\n";

static const char* go_struct_code = 
    "package models\n"
    "\n"
    "import \"fmt\"\n"
    "\n"
    "// Person represents a person with name and age\n"
    "type Person struct {\n"
    "    Name string\n"
    "    Age  int\n"
    "}\n"
    "\n"
    "// NewPerson creates a new Person instance\n"
    "func NewPerson(name string, age int) *Person {\n"
    "    return &Person{\n"
    "        Name: name,\n"
    "        Age:  age,\n"
    "    }\n"
    "}\n"
    "\n"
    "// String returns a string representation of the person\n"
    "func (p *Person) String() string {\n"
    "    return fmt.Sprintf(\"Person{Name: %s, Age: %d}\", p.Name, p.Age)\n"
    "}\n"
    "\n"
    "// GetAge returns the person's age\n"
    "func (p *Person) GetAge() int {\n"
    "    return p.Age\n"
    "}\n"
    "\n"
    "// SetAge sets the person's age\n"
    "func (p *Person) SetAge(age int) {\n"
    "    p.Age = age\n"
    "}\n";

static const char* go_interface_code = 
    "package shapes\n"
    "\n"
    "import \"math\"\n"
    "\n"
    "// Shape interface defines methods for geometric shapes\n"
    "type Shape interface {\n"
    "    Area() float64\n"
    "    Perimeter() float64\n"
    "}\n"
    "\n"
    "// Rectangle represents a rectangle\n"
    "type Rectangle struct {\n"
    "    Width  float64\n"
    "    Height float64\n"
    "}\n"
    "\n"
    "// Area calculates the area of the rectangle\n"
    "func (r Rectangle) Area() float64 {\n"
    "    return r.Width * r.Height\n"
    "}\n"
    "\n"
    "// Perimeter calculates the perimeter of the rectangle\n"
    "func (r Rectangle) Perimeter() float64 {\n"
    "    return 2 * (r.Width + r.Height)\n"
    "}\n"
    "\n"
    "// Circle represents a circle\n"
    "type Circle struct {\n"
    "    Radius float64\n"
    "}\n"
    "\n"
    "// Area calculates the area of the circle\n"
    "func (c Circle) Area() float64 {\n"
    "    return math.Pi * c.Radius * c.Radius\n"
    "}\n"
    "\n"
    "// Perimeter calculates the perimeter of the circle\n"
    "func (c Circle) Perimeter() float64 {\n"
    "    return 2 * math.Pi * c.Radius\n"
    "}\n";

// 测试Go语言支持
int test_go_language_support() {
    TEST_START("Go Language Support");
    
    // 测试语言是否被支持
    TEST_ASSERT(ckg_is_language_supported(CKG_LANG_GO), "Go language should be supported");
    
    TEST_PASS("Go Language Support");
}

// 测试Go基本解析
int test_go_basic_parsing() {
    TEST_START("Go Basic Parsing");
    
    char* temp_file = create_temp_file(go_test_code, "go");
    TEST_ASSERT(temp_file != NULL, "Should create temporary Go file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_GO);
    TEST_ASSERT(result != NULL, "Should parse Go file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到函数
    TEST_ASSERT(result->functions != NULL, "Should find functions");
    TEST_ASSERT(result->function_count > 0, "Should find at least one function (main)");
    
    // 检查main函数
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
    
    TEST_PASS("Go Basic Parsing");
}

// 测试Go函数解析
int test_go_function_parsing() {
    TEST_START("Go Function Parsing");
    
    char* temp_file = create_temp_file(go_function_code, "go");
    TEST_ASSERT(temp_file != NULL, "Should create temporary Go file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_GO);
    TEST_ASSERT(result != NULL, "Should parse Go file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查函数数量
    TEST_ASSERT(result->function_count >= 3, "Should find at least 3 functions");
    
    // 检查特定函数
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
    TEST_ASSERT(found_add, "Should find 'Add' function");
    TEST_ASSERT(found_multiply, "Should find 'Multiply' function");
    TEST_ASSERT(found_circle_area, "Should find 'CircleArea' function");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Go Function Parsing");
}

// 测试Go结构体解析
int test_go_struct_parsing() {
    TEST_START("Go Struct Parsing");
    
    char* temp_file = create_temp_file(go_struct_code, "go");
    TEST_ASSERT(temp_file != NULL, "Should create temporary Go file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_GO);
    TEST_ASSERT(result != NULL, "Should parse Go file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到结构体（在Go中可能被识别为类型或类）
    // 注意：具体的实现可能因CKG的设计而异
    
    // 检查方法
    TEST_ASSERT(result->function_count >= 4, "Should find struct methods");
    
    // 检查特定方法
    int found_new_person = 0, found_string = 0, found_get_age = 0, found_set_age = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "NewPerson") == 0) {
            found_new_person = 1;
        } else if (strcmp(result->functions[i].name, "String") == 0) {
            found_string = 1;
        } else if (strcmp(result->functions[i].name, "GetAge") == 0) {
            found_get_age = 1;
        } else if (strcmp(result->functions[i].name, "SetAge") == 0) {
            found_set_age = 1;
        }
    }
    TEST_ASSERT(found_new_person, "Should find 'NewPerson' function");
    TEST_ASSERT(found_string, "Should find 'String' method");
    TEST_ASSERT(found_get_age, "Should find 'GetAge' method");
    TEST_ASSERT(found_set_age, "Should find 'SetAge' method");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Go Struct Parsing");
}

// 测试Go接口解析
int test_go_interface_parsing() {
    TEST_START("Go Interface Parsing");
    
    char* temp_file = create_temp_file(go_interface_code, "go");
    TEST_ASSERT(temp_file != NULL, "Should create temporary Go file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_GO);
    TEST_ASSERT(result != NULL, "Should parse Go interface file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查方法
    TEST_ASSERT(result->function_count >= 4, "Should find interface methods");
    
    // 检查特定方法
    int found_area = 0, found_perimeter = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "Area") == 0) {
            found_area = 1;
        } else if (strcmp(result->functions[i].name, "Perimeter") == 0) {
            found_perimeter = 1;
        }
    }
    TEST_ASSERT(found_area, "Should find 'Area' methods");
    TEST_ASSERT(found_perimeter, "Should find 'Perimeter' methods");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Go Interface Parsing");
}

// 测试错误处理
int test_go_error_handling() {
    TEST_START("Go Error Handling");
    
    // 测试不存在的文件
    CKGResult* result = ckg_parse("/nonexistent/file.go", CKG_LANG_GO);
    TEST_ASSERT(result != NULL, "Should return result even for nonexistent file");
    TEST_ASSERT(!result->success, "Should fail for nonexistent file");
    
    ckg_free_result(result);
    
    // 测试语法错误的Go代码
    const char* invalid_go_code = "package main\nfunc main( { fmt.Println(\"test\") }";
    char* temp_file = create_temp_file(invalid_go_code, "go");
    TEST_ASSERT(temp_file != NULL, "Should create temporary file with invalid Go code");
    
    result = ckg_parse(temp_file, CKG_LANG_GO);
    TEST_ASSERT(result != NULL, "Should return result for invalid Go code");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("Go Error Handling");
}

int main() {
    printf(ANSI_COLOR_BLUE "=== Go Language Parser Tests ===" ANSI_COLOR_RESET "\n\n");
    
    // 初始化CKG
    ckg_init();
    
    // 运行测试
    test_go_language_support();
    test_go_basic_parsing();
    test_go_function_parsing();
    test_go_struct_parsing();
    test_go_interface_parsing();
    test_go_error_handling();
    
    // 清理
    ckg_cleanup();
    
    // 显示测试结果
    TEST_SUMMARY();
    
    return (tests_failed == 0) ? 0 : 1;
}