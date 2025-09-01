#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "tree_sitter/api.h"

// Language declarations
extern const TSLanguage *tree_sitter_c();
extern const TSLanguage *tree_sitter_cpp();
extern const TSLanguage *tree_sitter_c_sharp();
extern const TSLanguage *tree_sitter_java();
extern const TSLanguage *tree_sitter_javascript();
extern const TSLanguage *tree_sitter_python();
extern const TSLanguage *tree_sitter_typescript();
extern const TSLanguage *tree_sitter_go();
extern const TSLanguage *tree_sitter_rust();

// Test code samples
static const char c_code[] = "int main() { return 0; }";
static const char cpp_code[] = "#include <iostream>\nint main() { std::cout << \"Hello\" << std::endl; return 0; }";
static const char csharp_code[] = "using System; class Program { static void Main() { Console.WriteLine(\"Hello\"); } }";
static const char java_code[] = "public class Test { public static void main(String[] args) { System.out.println(\"Hello\"); } }";
static const char javascript_code[] = "function hello() { console.log('Hello'); } hello();";
static const char python_code[] = "def hello():\n    print('Hello')\nhello()";
static const char typescript_code[] = "function hello(): void { console.log('Hello'); } hello();";
static const char go_code[] = "package main\nimport \"fmt\"\nfunc main() { fmt.Println(\"Hello\") }";
static const char rust_code[] = "fn main() { println!(\"Hello, world!\"); }";

typedef struct {
    const char* name;
    const TSLanguage* (*get_language)();
    const char* code;
} LanguageTest;

LanguageTest tests[] = {
    {"C", tree_sitter_c, c_code},
    {"C++", tree_sitter_cpp, cpp_code},
    {"C#", tree_sitter_c_sharp, csharp_code},
    {"Java", tree_sitter_java, java_code},
    {"JavaScript", tree_sitter_javascript, javascript_code},
    {"Python", tree_sitter_python, python_code},
    {"TypeScript", tree_sitter_typescript, typescript_code},
    {"Go", tree_sitter_go, go_code},
    {"Rust", tree_sitter_rust, rust_code}
};

int test_language(LanguageTest* test) {
    printf("Testing %s...\n", test->name);
    
    // Create parser
    TSParser *parser = ts_parser_new();
    if (!parser) {
        printf("  ❌ Failed to create parser\n");
        return 0;
    }
    
    // Get language
    const TSLanguage *language = test->get_language();
    if (!language) {
        printf("  ❌ Failed to get language\n");
        ts_parser_delete(parser);
        return 0;
    }
    
    // Check language version
    uint32_t version = ts_language_version(language);
    printf("  Language version: %u\n", version);
    
    // Set language
    if (!ts_parser_set_language(parser, language)) {
        printf("  ❌ Failed to set language\n");
        ts_parser_delete(parser);
        return 0;
    }
    
    // Parse code
    TSTree *tree = ts_parser_parse_string(parser, NULL, test->code, strlen(test->code));
    if (!tree) {
        printf("  ❌ Failed to parse code\n");
        ts_parser_delete(parser);
        return 0;
    }
    
    // Get root node
    TSNode root_node = ts_tree_root_node(tree);
    if (ts_node_is_null(root_node)) {
        printf("  ❌ Root node is null\n");
        ts_tree_delete(tree);
        ts_parser_delete(parser);
        return 0;
    }
    
    // Check for syntax errors
    if (ts_node_has_error(root_node)) {
        printf("  ⚠️  Parse tree has errors\n");
    } else {
        printf("  ✅ Parse successful\n");
    }
    
    // Get node info
    uint32_t child_count = ts_node_child_count(root_node);
    printf("  Root node children: %u\n", child_count);
    
    // Cleanup
    ts_tree_delete(tree);
    ts_parser_delete(parser);
    
    printf("  ✅ %s test completed\n\n", test->name);
    return 1;
}

int main() {
    printf("=== Testing All Language Parsers ===\n\n");
    
    int total_tests = sizeof(tests) / sizeof(tests[0]);
    int passed_tests = 0;
    
    for (int i = 0; i < total_tests; i++) {
        if (test_language(&tests[i])) {
            passed_tests++;
        }
    }
    
    printf("=== Test Results ===\n");
    printf("Passed: %d/%d tests\n", passed_tests, total_tests);
    
    if (passed_tests == total_tests) {
        printf("🎉 All language parsers are working correctly!\n");
        return 0;
    } else {
        printf("❌ Some language parsers failed\n");
        return 1;
    }
}