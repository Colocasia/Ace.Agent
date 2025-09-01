#include <stdio.h>
#include <stdlib.h>
#include "../wrapper/ckg_wrapper.h"

int main() {
    printf("Starting debug test...\n");
    fflush(stdout);
    
    // Initialize the parser
    int init_result = ckg_init();
    printf("ckg_init returned: %d\n", init_result);
    fflush(stdout);
    
    const char* test_code = "int main() { return 0; }";
    printf("About to call ckg_parse...\n");
    fflush(stdout);
    
    CKGParseResult* result = ckg_parse(CKG_LANG_C, test_code, "test.c");
    
    printf("ckg_parse returned: %p\n", (void*)result);
    fflush(stdout);
    
    if (result) {
        printf("Function count: %u\n", result->function_count);
        if (result->function_count > 0 && result->functions) {
            printf("First function: %s\n", result->functions[0].name ? result->functions[0].name : "(null)");
        }
        ckg_free_result(result);
    }
    
    ckg_cleanup();
    return 0;
}