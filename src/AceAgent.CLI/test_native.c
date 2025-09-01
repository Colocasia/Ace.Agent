#include <stdio.h>
#include <dlfcn.h>

int main() {
    // 尝试加载动态库
    void* handle = dlopen("/Users/gaoxiang/Projects/AceAgent/src/AceAgent.Tools/CKG/runtimes/osx-arm64/native/ckg_wrapper.dylib", RTLD_NOW);
    if (!handle) {
        printf("Failed to load library: %s\n", dlerror());
        return 1;
    }
    
    // 查找符号 - 先尝试不带下划线的
    typedef int (*ckg_init_func)(void);
    ckg_init_func ckg_init = (ckg_init_func)dlsym(handle, "ckg_init");
    if (!ckg_init) {
        printf("Failed to find ckg_init, trying _ckg_init: %s\n", dlerror());
        // 尝试带下划线的
        ckg_init = (ckg_init_func)dlsym(handle, "_ckg_init");
        if (!ckg_init) {
            printf("Failed to find _ckg_init: %s\n", dlerror());
            dlclose(handle);
            return 1;
        }
        printf("Successfully found _ckg_init\n");
    } else {
        printf("Successfully found ckg_init\n");
    }
    
    // 尝试调用函数
    int result = ckg_init();
    printf("ckg_init returned: %d\n", result);
    
    dlclose(handle);
    return 0;
}