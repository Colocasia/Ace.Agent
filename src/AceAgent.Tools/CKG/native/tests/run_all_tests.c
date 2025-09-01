#include "test_framework.h"
#include "../wrapper/ckg_wrapper.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/wait.h>
#include <unistd.h>

// 测试程序列表
static const char* test_programs[] = {
    "test_c_parser",
    "test_cpp_parser",
    "test_csharp_parser",
    "test_java_parser",
    "test_javascript_parser",
    "test_python_parser",
    "test_typescript_parser",
    "test_go_parser"
};

static const int num_test_programs = sizeof(test_programs) / sizeof(test_programs[0]);

// 运行单个测试程序
int run_test_program(const char* program_name) {
    printf(ANSI_COLOR_CYAN "\n=== Running %s ===" ANSI_COLOR_RESET "\n", program_name);
    
    // 构建测试程序路径
    char test_path[512];
    snprintf(test_path, sizeof(test_path), "./%s", program_name);
    
    // 使用fork和exec运行测试程序
    pid_t pid = fork();
    if (pid == 0) {
        // 子进程：执行测试程序
        execl(test_path, program_name, NULL);
        // 如果execl失败
        printf(ANSI_COLOR_RED "Failed to execute %s" ANSI_COLOR_RESET "\n", program_name);
        exit(1);
    } else if (pid > 0) {
        // 父进程：等待子进程完成
        int status;
        waitpid(pid, &status, 0);
        
        if (WIFEXITED(status)) {
            int exit_code = WEXITSTATUS(status);
            if (exit_code == 0) {
                printf(ANSI_COLOR_GREEN "✓ %s PASSED" ANSI_COLOR_RESET "\n", program_name);
                return 0;
            } else {
                printf(ANSI_COLOR_RED "✗ %s FAILED (exit code: %d)" ANSI_COLOR_RESET "\n", program_name, exit_code);
                return 1;
            }
        } else {
            printf(ANSI_COLOR_RED "✗ %s TERMINATED ABNORMALLY" ANSI_COLOR_RESET "\n", program_name);
            return 1;
        }
    } else {
        // fork失败
        printf(ANSI_COLOR_RED "Failed to fork process for %s" ANSI_COLOR_RESET "\n", program_name);
        return 1;
    }
}

// 检查测试程序是否存在
int check_test_program_exists(const char* program_name) {
    char test_path[512];
    snprintf(test_path, sizeof(test_path), "./%s", program_name);
    
    if (access(test_path, X_OK) == 0) {
        return 1; // 文件存在且可执行
    }
    return 0; // 文件不存在或不可执行
}

// 显示使用说明
void show_usage(const char* program_name) {
    printf("Usage: %s [options] [test_name]\n", program_name);
    printf("\nOptions:\n");
    printf("  -h, --help     Show this help message\n");
    printf("  -l, --list     List all available tests\n");
    printf("  -v, --verbose  Enable verbose output\n");
    printf("\nTest names:\n");
    for (int i = 0; i < num_test_programs; i++) {
        printf("  %s\n", test_programs[i]);
    }
    printf("\nIf no test name is specified, all tests will be run.\n");
}

// 列出所有可用的测试
void list_tests() {
    printf("Available tests:\n");
    for (int i = 0; i < num_test_programs; i++) {
        const char* status = check_test_program_exists(test_programs[i]) ? "[AVAILABLE]" : "[MISSING]";
        printf("  %-25s %s\n", test_programs[i], status);
    }
}

int main(int argc, char* argv[]) {
    int verbose = 0;
    const char* specific_test = NULL;
    
    // 解析命令行参数
    for (int i = 1; i < argc; i++) {
        if (strcmp(argv[i], "-h") == 0 || strcmp(argv[i], "--help") == 0) {
            show_usage(argv[0]);
            return 0;
        } else if (strcmp(argv[i], "-l") == 0 || strcmp(argv[i], "--list") == 0) {
            list_tests();
            return 0;
        } else if (strcmp(argv[i], "-v") == 0 || strcmp(argv[i], "--verbose") == 0) {
            verbose = 1;
        } else if (argv[i][0] != '-') {
            specific_test = argv[i];
        } else {
            printf("Unknown option: %s\n", argv[i]);
            show_usage(argv[0]);
            return 1;
        }
    }
    
    printf(ANSI_COLOR_BLUE "=== CKG Language Parser Test Suite ===" ANSI_COLOR_RESET "\n");
    
    // 检查CKG库是否可用
    printf("Initializing CKG library...\n");
    ckg_init();
    
    // 显示支持的语言
    printf("\nSupported languages:\n");
    const char* languages[] = {"C", "C++", "C#", "Java", "JavaScript", "Python", "TypeScript", "Go"};
    const CKGLanguage lang_codes[] = {
        CKG_LANG_C, CKG_LANG_CPP, CKG_LANG_CSHARP, CKG_LANG_JAVA,
        CKG_LANG_JAVASCRIPT, CKG_LANG_PYTHON, CKG_LANG_TYPESCRIPT, CKG_LANG_GO
    };
    
    for (int i = 0; i < 8; i++) {
        const char* status = ckg_is_language_supported(lang_codes[i]) ? "✓" : "✗";
        printf("  %s %-12s %s\n", status, languages[i], 
               ckg_is_language_supported(lang_codes[i]) ? "[SUPPORTED]" : "[NOT SUPPORTED]");
    }
    
    ckg_cleanup();
    
    int total_tests = 0;
    int passed_tests = 0;
    int failed_tests = 0;
    int missing_tests = 0;
    
    if (specific_test) {
        // 运行特定测试
        printf("\nRunning specific test: %s\n", specific_test);
        
        // 检查测试是否在列表中
        int found = 0;
        for (int i = 0; i < num_test_programs; i++) {
            if (strcmp(test_programs[i], specific_test) == 0) {
                found = 1;
                break;
            }
        }
        
        if (!found) {
            printf(ANSI_COLOR_RED "Error: Test '%s' not found." ANSI_COLOR_RESET "\n", specific_test);
            printf("Use -l or --list to see available tests.\n");
            return 1;
        }
        
        if (!check_test_program_exists(specific_test)) {
            printf(ANSI_COLOR_RED "Error: Test program '%s' not found or not executable." ANSI_COLOR_RESET "\n", specific_test);
            printf("Make sure to compile the test programs first.\n");
            return 1;
        }
        
        total_tests = 1;
        if (run_test_program(specific_test) == 0) {
            passed_tests = 1;
        } else {
            failed_tests = 1;
        }
    } else {
        // 运行所有测试
        printf("\nRunning all tests...\n");
        
        for (int i = 0; i < num_test_programs; i++) {
            total_tests++;
            
            if (!check_test_program_exists(test_programs[i])) {
                printf(ANSI_COLOR_YELLOW "⚠ %s MISSING (not compiled)" ANSI_COLOR_RESET "\n", test_programs[i]);
                missing_tests++;
                continue;
            }
            
            if (run_test_program(test_programs[i]) == 0) {
                passed_tests++;
            } else {
                failed_tests++;
            }
        }
    }
    
    // 显示测试结果摘要
    printf("\n" ANSI_COLOR_BLUE "=== Test Results Summary ===" ANSI_COLOR_RESET "\n");
    printf("Total tests:   %d\n", total_tests);
    printf(ANSI_COLOR_GREEN "Passed tests:  %d" ANSI_COLOR_RESET "\n", passed_tests);
    if (failed_tests > 0) {
        printf(ANSI_COLOR_RED "Failed tests:  %d" ANSI_COLOR_RESET "\n", failed_tests);
    }
    if (missing_tests > 0) {
        printf(ANSI_COLOR_YELLOW "Missing tests: %d" ANSI_COLOR_RESET "\n", missing_tests);
    }
    
    // 计算成功率
    int executable_tests = total_tests - missing_tests;
    if (executable_tests > 0) {
        double success_rate = (double)passed_tests / executable_tests * 100.0;
        printf("Success rate:  %.1f%% (%d/%d)\n", success_rate, passed_tests, executable_tests);
    }
    
    if (missing_tests > 0) {
        printf("\n" ANSI_COLOR_YELLOW "Note: Some test programs are missing. Run 'make tests' to compile them." ANSI_COLOR_RESET "\n");
    }
    
    // 返回适当的退出码
    if (failed_tests > 0) {
        return 1; // 有测试失败
    } else if (missing_tests > 0 && passed_tests == 0) {
        return 2; // 没有可执行的测试
    } else {
        return 0; // 所有测试通过
    }
}