#include <stdio.h>
#include <dlfcn.h>

int main() {
    // 尝试加载动态库
    void* handle = dlopen("/Users/gaoxiang/Projects/AceAgent/src/AceAgent.Tools/CKG/runtimes/osx-arm64/native/ckg_wrapper.dylib", RTLD_NOW);
    if (!handle) {
        printf("Failed to load library: %s\n", dlerror());
        return 1;
    }
    
    // 查找ckg_init
    typedef int (*ckg_init_func)(void);
    ckg_init_func ckg_init = (ckg_init_func)dlsym(handle, "ckg_init");
    if (!ckg_init) {
        printf("Failed to find ckg_init: %s\n", dlerror());
        dlclose(handle);
        return 1;
    }
    
    // 初始化
    int init_result = ckg_init();
    printf("ckg_init returned: %d\n", init_result);
    
    // 查找ckg_parse_json - 先尝试不带下划线的
    typedef char* (*ckg_parse_json_func)(void*, const char*, const char*, const char*);
    ckg_parse_json_func ckg_parse_json = (ckg_parse_json_func)dlsym(handle, "ckg_parse_json");
    if (!ckg_parse_json) {
        printf("Failed to find ckg_parse_json, trying _ckg_parse_json: %s\n", dlerror());
        // 尝试带下划线的
        ckg_parse_json = (ckg_parse_json_func)dlsym(handle, "_ckg_parse_json");
        if (!ckg_parse_json) {
            printf("Failed to find _ckg_parse_json: %s\n", dlerror());
            dlclose(handle);
            return 1;
        }
        printf("Successfully found _ckg_parse_json\n");
    } else {
        printf("Successfully found ckg_parse_json\n");
    }
    
    // 测试解析
    const char* test_code = "class Test { public void Method() {} }";
    char* result = ckg_parse_json(NULL, test_code, "csharp", "test.cs");
    if (result) {
        printf("Parse result: %s\n", result);
        // 释放结果
        typedef void (*ckg_free_func)(char*);
        ckg_free_func ckg_free = (ckg_free_func)dlsym(handle, "ckg_free_json_result");
        if (ckg_free) {
            ckg_free(result);
        }
    } else {
        printf("Parse returned NULL\n");
    }
    
    dlclose(handle);
    return 0;
}