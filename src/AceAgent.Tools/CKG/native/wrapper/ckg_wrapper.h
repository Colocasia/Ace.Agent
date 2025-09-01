#ifndef CKG_WRAPPER_H
#define CKG_WRAPPER_H

#include <stdbool.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

// Language enumeration
typedef enum {
    CKG_LANG_C = 0,
    CKG_LANG_CPP = 1,
    CKG_LANG_CSHARP = 2,
    CKG_LANG_JAVA = 3,
    CKG_LANG_JAVASCRIPT = 4,
    CKG_LANG_TYPESCRIPT = 5,
    CKG_LANG_PYTHON = 6,
    CKG_LANG_GO = 7,
    CKG_LANG_RUST = 8,
    CKG_LANG_LUA = 9,
    CKG_LANG_PHP = 10
} CKGLanguage;

// Function element structure
typedef struct {
    char* name;
    char* return_type;
    char* parameters;
    uint32_t start_line;
    uint32_t end_line;
    uint32_t start_column;
    uint32_t end_column;
    bool is_public;
    bool is_private;
    bool is_protected;
    bool is_static;
    bool is_async;
    char* parent_class;
} CKGFunction;

// Class element structure
typedef struct {
    char* name;
    char* namespace_name;
    char* base_class;
    char* interfaces;
    uint32_t start_line;
    uint32_t end_line;
    uint32_t start_column;
    uint32_t end_column;
    bool is_public;
    bool is_private;
    bool is_protected;
    bool is_static;
    bool is_abstract;
    bool is_sealed;
} CKGClass;

// Property element structure
typedef struct {
    char* name;
    char* property_type;
    uint32_t start_line;
    uint32_t end_line;
    uint32_t start_column;
    uint32_t end_column;
    bool is_public;
    bool is_private;
    bool is_protected;
    bool is_static;
    bool has_getter;
    bool has_setter;
    char* parent_class;
} CKGProperty;

// Field element structure
typedef struct {
    char* name;
    char* field_type;
    char* default_value;
    uint32_t start_line;
    uint32_t end_line;
    uint32_t start_column;
    uint32_t end_column;
    bool is_public;
    bool is_private;
    bool is_protected;
    bool is_static;
    bool is_readonly;
    bool is_const;
    char* parent_class;
} CKGField;

// Variable element structure
typedef struct {
    char* name;
    char* variable_type;
    char* default_value;
    uint32_t start_line;
    uint32_t end_line;
    uint32_t start_column;
    uint32_t end_column;
    bool is_local;
    bool is_parameter;
    char* parent_function;
} CKGVariable;

// Parse result structure
typedef struct {
    uint32_t function_count;
    uint32_t class_count;
    uint32_t property_count;
    uint32_t field_count;
    uint32_t variable_count;
    CKGFunction* functions;
    CKGClass* classes;
    CKGProperty* properties;
    CKGField* fields;
    CKGVariable* variables;
    const char* error_message;
} CKGParseResult;

// API functions
#ifdef _WIN32
#define CKG_API __declspec(dllexport)
#else
#define CKG_API __attribute__((visibility("default")))
#endif

CKG_API int ckg_init(void);
CKG_API void ckg_cleanup(void);
const char* ckg_get_version(void);
bool ckg_is_language_supported(CKGLanguage language);
CKGParseResult* ckg_parse(CKGLanguage language, const char* source_code, const char* file_path);
CKG_API char* ckg_parse_json(void* parser_ptr, const char* source_code, const char* language, const char* file_path);
void ckg_free_result(CKGParseResult* result);
CKG_API void ckg_free_json_result(char* json_result);

#ifdef __cplusplus
}
#endif

#endif // CKG_WRAPPER_H