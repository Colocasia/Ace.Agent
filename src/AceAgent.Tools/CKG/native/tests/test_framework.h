#ifndef TEST_FRAMEWORK_H
#define TEST_FRAMEWORK_H

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>
#include <unistd.h>
#include "../wrapper/ckg_wrapper.h"
// 测试统计
static int tests_run = 0;
static int tests_passed = 0;
static int tests_failed = 0;

// 颜色输出宏
#define ANSI_COLOR_RED     "\x1b[31m"
#define ANSI_COLOR_GREEN   "\x1b[32m"
#define ANSI_COLOR_YELLOW  "\x1b[33m"
#define ANSI_COLOR_BLUE    "\x1b[34m"
#define ANSI_COLOR_RESET   "\x1b[0m"

// 测试宏定义
#define TEST_START(name) \
    do { \
        printf(ANSI_COLOR_YELLOW "[TEST] Starting: %s" ANSI_COLOR_RESET "\n", name); \
        tests_run++; \
    } while(0)

#define TEST_ASSERT(condition, message) \
    do { \
        if (!(condition)) { \
            printf(ANSI_COLOR_RED "[FAIL] %s: %s" ANSI_COLOR_RESET "\n", __func__, message); \
            tests_failed++; \
            return 1; \
        } \
    } while(0)

#define TEST_PASS(name) \
    do { \
        printf(ANSI_COLOR_GREEN "[PASS] %s" ANSI_COLOR_RESET "\n", name); \
        tests_passed++; \
        return 0; \
    } while(0)

#define TEST_FAIL(name, message) \
    do { \
        printf(ANSI_COLOR_RED "[FAIL] %s: %s" ANSI_COLOR_RESET "\n", name, message); \
        tests_failed++; \
        return 1; \
    } while(0)

// 测试结果汇总
#define TEST_SUMMARY() \
    do { \
        printf("\n" ANSI_COLOR_YELLOW "=== Test Summary ===" ANSI_COLOR_RESET "\n"); \
        printf("Total tests: %d\n", tests_run); \
        printf(ANSI_COLOR_GREEN "Passed: %d" ANSI_COLOR_RESET "\n", tests_passed); \
        printf(ANSI_COLOR_RED "Failed: %d" ANSI_COLOR_RESET "\n", tests_failed); \
        if (tests_failed == 0) { \
            printf(ANSI_COLOR_GREEN "All tests passed!" ANSI_COLOR_RESET "\n"); \
        } else { \
            printf(ANSI_COLOR_RED "Some tests failed!" ANSI_COLOR_RESET "\n"); \
        } \
        printf("\n"); \
    } while(0)

// 辅助函数：创建临时文件
char* create_temp_file(const char* content, const char* extension) {
    static char temp_path[256];
    snprintf(temp_path, sizeof(temp_path), "/tmp/ckg_test_%d.%s", rand(), extension);
    
    FILE* file = fopen(temp_path, "w");
    if (!file) {
        return NULL;
    }
    
    fprintf(file, "%s", content);
    fclose(file);
    
    return temp_path;
}

// 辅助函数：清理临时文件
void cleanup_temp_file(char* file_path) {
    if (file_path) {
        unlink(file_path);
    }
}

// 辅助函数：读取文件内容
char* read_file_content(const char* file_path) {
    FILE* file = fopen(file_path, "r");
    if (!file) {
        return NULL;
    }
    
    fseek(file, 0, SEEK_END);
    long length = ftell(file);
    fseek(file, 0, SEEK_SET);
    
    char* content = malloc(length + 1);
    if (!content) {
        fclose(file);
        return NULL;
    }
    
    fread(content, 1, length, file);
    content[length] = '\0';
    fclose(file);
    
    return content;
}

#endif // TEST_FRAMEWORK_H