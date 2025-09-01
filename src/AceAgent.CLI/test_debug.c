#include <stdio.h>
#include <stdlib.h>
#include <dlfcn.h>

int main() {
    // Load the dynamic library
    void* handle = dlopen("./bin/Debug/net8.0/runtimes/osx-arm64/native/ckg_wrapper.dylib", RTLD_LAZY);
    if (!handle) {
        fprintf(stderr, "Cannot load library: %s\n", dlerror());
        return 1;
    }

    // Get function pointers
    int (*ckg_init)(void) = dlsym(handle, "ckg_init");
    char* (*ckg_parse_json)(void*, const char*, const char*, const char*) = dlsym(handle, "ckg_parse_json");
    void (*ckg_free_json_result)(char*) = dlsym(handle, "ckg_free_json_result");
    void (*ckg_cleanup)(void) = dlsym(handle, "ckg_cleanup");

    if (!ckg_init || !ckg_parse_json || !ckg_free_json_result || !ckg_cleanup) {
        fprintf(stderr, "Cannot load functions: %s\n", dlerror());
        dlclose(handle);
        return 1;
    }

    // Initialize
    int init_result = ckg_init();
    printf("Init result: %d\n", init_result);
    if (init_result != 1) {
        fprintf(stderr, "Failed to initialize\n");
        dlclose(handle);
        return 1;
    }

    // Test with more detailed C# code
    const char* test_code = "using System;\n\npublic class TestClass\n{\n    public void TestMethod()\n    {\n        Console.WriteLine(\"Hello\");\n    }\n\n    public int TestProperty { get; set; }\n}\n\npublic interface ITestInterface\n{\n    void InterfaceMethod();\n}";
    
    printf("Calling ckg_parse_json...\n");
    char* result = ckg_parse_json(NULL, test_code, "csharp", "test.cs");
    
    if (result) {
        printf("Result: %s\n", result);
        ckg_free_json_result(result);
    } else {
        printf("No result returned\n");
    }

    // Cleanup
    ckg_cleanup();
    dlclose(handle);
    
    return 0;
}