#include "test_framework.h"
#include "../wrapper/ckg_wrapper.h"
#include <tree_sitter/api.h>

// TypeScript测试代码示例
static const char* ts_test_code = 
    "interface User {\n"
    "    name: string;\n"
    "    age: number;\n"
    "    email?: string;\n"
    "}\n"
    "\n"
    "function greetUser(user: User): string {\n"
    "    return `Hello, ${user.name}!`;\n"
    "}\n"
    "\n"
    "const user: User = {\n"
    "    name: \"Alice\",\n"
    "    age: 30\n"
    "};\n"
    "\n"
    "console.log(greetUser(user));\n";

static const char* ts_class_code = 
    "abstract class Animal {\n"
    "    protected name: string;\n"
    "    \n"
    "    constructor(name: string) {\n"
    "        this.name = name;\n"
    "    }\n"
    "    \n"
    "    abstract makeSound(): void;\n"
    "    \n"
    "    getName(): string {\n"
    "        return this.name;\n"
    "    }\n"
    "}\n"
    "\n"
    "class Dog extends Animal {\n"
    "    private breed: string;\n"
    "    \n"
    "    constructor(name: string, breed: string) {\n"
    "        super(name);\n"
    "        this.breed = breed;\n"
    "    }\n"
    "    \n"
    "    makeSound(): void {\n"
    "        console.log(\"Woof!\");\n"
    "    }\n"
    "    \n"
    "    getBreed(): string {\n"
    "        return this.breed;\n"
    "    }\n"
    "}\n";

static const char* ts_generic_code = 
    "interface Repository<T> {\n"
    "    findById(id: number): T | null;\n"
    "    save(entity: T): void;\n"
    "    delete(id: number): boolean;\n"
    "}\n"
    "\n"
    "class UserRepository implements Repository<User> {\n"
    "    private users: User[] = [];\n"
    "    \n"
    "    findById(id: number): User | null {\n"
    "        return this.users.find(user => user.id === id) || null;\n"
    "    }\n"
    "    \n"
    "    save(user: User): void {\n"
    "        this.users.push(user);\n"
    "    }\n"
    "    \n"
    "    delete(id: number): boolean {\n"
    "        const index = this.users.findIndex(user => user.id === id);\n"
    "        if (index !== -1) {\n"
    "            this.users.splice(index, 1);\n"
    "            return true;\n"
    "        }\n"
    "        return false;\n"
    "    }\n"
    "}\n";

static const char* ts_module_code = 
    "export namespace MathUtils {\n"
    "    export const PI = 3.14159;\n"
    "    \n"
    "    export function square(x: number): number {\n"
    "        return x * x;\n"
    "    }\n"
    "    \n"
    "    export function circle_area(radius: number): number {\n"
    "        return PI * square(radius);\n"
    "    }\n"
    "    \n"
    "    export class Calculator {\n"
    "        add(a: number, b: number): number {\n"
    "            return a + b;\n"
    "        }\n"
    "        \n"
    "        multiply(a: number, b: number): number {\n"
    "            return a * b;\n"
    "        }\n"
    "    }\n"
    "}\n";

// 测试TypeScript语言支持
int test_typescript_language_support() {
    TEST_START("TypeScript Language Support");
    
    // 测试语言是否被支持
    TEST_ASSERT(ckg_is_language_supported(CKG_LANG_TYPESCRIPT), "TypeScript language should be supported");
    
    TEST_PASS("TypeScript Language Support");
}

// 测试TypeScript基本解析
int test_typescript_basic_parsing() {
    TEST_START("TypeScript Basic Parsing");
    
    char* temp_file = create_temp_file(ts_test_code, "ts");
    TEST_ASSERT(temp_file != NULL, "Should create temporary TypeScript file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_TYPESCRIPT);
    TEST_ASSERT(result != NULL, "Should parse TypeScript file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到函数
    TEST_ASSERT(result->functions != NULL, "Should find functions");
    TEST_ASSERT(result->function_count > 0, "Should find at least one function");
    
    // 检查greetUser函数
    int found_greet_user = 0;
    for (int i = 0; i < result->function_count; i++) {
        if (strcmp(result->functions[i].name, "greetUser") == 0) {
            found_greet_user = 1;
            break;
        }
    }
    TEST_ASSERT(found_greet_user, "Should find greetUser function");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("TypeScript Basic Parsing");
}

// 测试TypeScript类解析
int test_typescript_class_parsing() {
    TEST_START("TypeScript Class Parsing");
    
    char* temp_file = create_temp_file(ts_class_code, "ts");
    TEST_ASSERT(temp_file != NULL, "Should create temporary TypeScript file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_TYPESCRIPT);
    TEST_ASSERT(result != NULL, "Should parse TypeScript file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到类
    TEST_ASSERT(result->classes != NULL, "Should find classes");
    TEST_ASSERT(result->class_count >= 2, "Should find at least two classes (Animal, Dog)");
    
    // 检查Animal和Dog类
    int found_animal = 0, found_dog = 0;
    for (int i = 0; i < result->class_count; i++) {
        if (strcmp(result->classes[i].name, "Animal") == 0) {
            found_animal = 1;
        } else if (strcmp(result->classes[i].name, "Dog") == 0) {
            found_dog = 1;
        }
    }
    TEST_ASSERT(found_animal, "Should find Animal class");
    TEST_ASSERT(found_dog, "Should find Dog class");
    
    // 检查方法
    TEST_ASSERT(result->function_count >= 4, "Should find multiple methods");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("TypeScript Class Parsing");
}

// 测试TypeScript泛型解析
int test_typescript_generic_parsing() {
    TEST_START("TypeScript Generic Parsing");
    
    char* temp_file = create_temp_file(ts_generic_code, "ts");
    TEST_ASSERT(temp_file != NULL, "Should create temporary TypeScript file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_TYPESCRIPT);
    TEST_ASSERT(result != NULL, "Should parse TypeScript generic file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到类
    TEST_ASSERT(result->classes != NULL, "Should find classes");
    TEST_ASSERT(result->class_count > 0, "Should find at least one class");
    
    // 检查UserRepository类
    int found_user_repository = 0;
    for (int i = 0; i < result->class_count; i++) {
        if (strcmp(result->classes[i].name, "UserRepository") == 0) {
            found_user_repository = 1;
            break;
        }
    }
    TEST_ASSERT(found_user_repository, "Should find UserRepository class");
    
    // 检查方法
    TEST_ASSERT(result->function_count >= 3, "Should find repository methods");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("TypeScript Generic Parsing");
}

// 测试TypeScript模块解析
int test_typescript_module_parsing() {
    TEST_START("TypeScript Module Parsing");
    
    char* temp_file = create_temp_file(ts_module_code, "ts");
    TEST_ASSERT(temp_file != NULL, "Should create temporary TypeScript file");
    
    CKGResult* result = ckg_parse(temp_file, CKG_LANG_TYPESCRIPT);
    TEST_ASSERT(result != NULL, "Should parse TypeScript module file successfully");
    TEST_ASSERT(result->success, "Parsing should succeed");
    
    // 检查是否找到函数
    TEST_ASSERT(result->function_count >= 2, "Should find functions in namespace");
    
    // 检查是否找到类
    TEST_ASSERT(result->classes != NULL, "Should find classes in namespace");
    TEST_ASSERT(result->class_count > 0, "Should find at least one class");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("TypeScript Module Parsing");
}

// 测试错误处理
int test_typescript_error_handling() {
    TEST_START("TypeScript Error Handling");
    
    // 测试不存在的文件
    CKGResult* result = ckg_parse("/nonexistent/file.ts", CKG_LANG_TYPESCRIPT);
    TEST_ASSERT(result != NULL, "Should return result even for nonexistent file");
    TEST_ASSERT(!result->success, "Should fail for nonexistent file");
    
    ckg_free_result(result);
    
    // 测试语法错误的TypeScript代码
    const char* invalid_ts_code = "function test(: string { return \"test\"; }";
    char* temp_file = create_temp_file(invalid_ts_code, "ts");
    TEST_ASSERT(temp_file != NULL, "Should create temporary file with invalid TypeScript code");
    
    result = ckg_parse(temp_file, CKG_LANG_TYPESCRIPT);
    TEST_ASSERT(result != NULL, "Should return result for invalid TypeScript code");
    
    ckg_free_result(result);
    cleanup_temp_file(temp_file);
    
    TEST_PASS("TypeScript Error Handling");
}

int main() {
    printf(ANSI_COLOR_BLUE "=== TypeScript Language Parser Tests ===" ANSI_COLOR_RESET "\n\n");
    
    // 初始化CKG
    ckg_init();
    
    // 运行测试
    test_typescript_language_support();
    test_typescript_basic_parsing();
    test_typescript_class_parsing();
    test_typescript_generic_parsing();
    test_typescript_module_parsing();
    test_typescript_error_handling();
    
    // 清理
    ckg_cleanup();
    
    // 显示测试结果
    TEST_SUMMARY();
    
    return (tests_failed == 0) ? 0 : 1;
}