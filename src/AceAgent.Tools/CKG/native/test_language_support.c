#include <stdio.h>
#include <string.h>
#include "wrapper/ckg_wrapper.h"

int main() {
    printf("Testing CKG language support...\n");
    
    if (!ckg_init()) {
        printf("Failed to initialize CKG\n");
        return 1;
    }
    
    // Test all supported languages
    CKGLanguage languages[] = {
        CKG_LANG_C,
        CKG_LANG_CPP,
        CKG_LANG_CSHARP,
        CKG_LANG_JAVA,
        CKG_LANG_JAVASCRIPT,
        CKG_LANG_TYPESCRIPT,
        CKG_LANG_PYTHON,
        CKG_LANG_GO
    };
    
    const char* lang_names[] = {
        "C",
        "C++",
        "C#",
        "Java",
        "JavaScript",
        "TypeScript",
        "Python",
        "Go"
    };
    
    int num_languages = sizeof(languages) / sizeof(languages[0]);
    
    for (int i = 0; i < num_languages; i++) {
        bool supported = ckg_is_language_supported(languages[i]);
        printf("Language: %-12s -> Supported: %s\n", lang_names[i], supported ? "Yes" : "No");
    }
    
    // Test a simple parse for each language
    printf("\nTesting basic parsing...\n");
    
    const char* test_codes[] = {
        "int main() { return 0; }",  // C
        "int main() { return 0; }",  // C++
        "class Test { }",            // C#
        "class Test { }",            // Java
        "function test() { }",       // JavaScript
        "function test(): void { }", // TypeScript
        "def test(): pass",          // Python
        "func main() { }",           // Go
    };
    
    for (int i = 0; i < num_languages; i++) {
        CKGParseResult* result = ckg_parse(languages[i], test_codes[i], "test");
        if (result) {
            printf("Parse %-12s: Success (functions: %d, classes: %d)\n", 
                   lang_names[i], result->function_count, result->class_count);
            ckg_free_result(result);
        } else {
            printf("Parse %-12s: Failed\n", lang_names[i]);
        }
    }
    
    ckg_cleanup();
    return 0;
}