#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>
#include <stdint.h>
#ifndef _WIN32
#include <unistd.h>
#endif
#include "tree_sitter/api.h"
#include "ckg_wrapper.h"

// External language declarations
extern const TSLanguage *tree_sitter_c_sharp(void);
extern const TSLanguage *tree_sitter_javascript(void);
extern const TSLanguage *tree_sitter_python(void);
extern const TSLanguage *tree_sitter_c(void);
extern const TSLanguage *tree_sitter_cpp(void);
extern const TSLanguage *tree_sitter_java(void);
extern const TSLanguage *tree_sitter_typescript(void);
extern const TSLanguage *tree_sitter_go(void);

static TSParser *parser = NULL;
static bool initialized = false;

// Internal structures for parsing
typedef struct {
    char name[256];
    int start_line;
    int end_line;
} ExtractedClass;

typedef struct {
    char name[256];
    char class_name[256];
    int start_line;
    int end_line;
} ExtractedFunction;

typedef struct {
    ExtractedClass* classes;
    int class_count;
    int class_capacity;
    ExtractedFunction* functions;
    int function_count;
    int function_capacity;
} ParsedData;

// Forward declarations
static char* get_node_text(TSNode node, const char* source_code);
static void add_class(ParsedData* data, const char* name, int start_line, int end_line);
static void add_function(ParsedData* data, const char* name, const char* class_name, int start_line, int end_line);
static void walk_tree(TSNode node, const char* source_code, ParsedData* data, const char* current_class);

// Initialize the CKG wrapper
CKG_API int ckg_init(void) {
    if (initialized) {
        return 1;
    }
    
    // Create a real Tree-sitter parser
    parser = ts_parser_new();
    if (!parser) {
        return 0;
    }
    
    initialized = true;
    return 1;
}

// Cleanup resources
CKG_API void ckg_cleanup(void) {
    if (parser) {
        ts_parser_delete(parser);
        parser = NULL;
    }
    initialized = false;
}

// Get version string
CKG_API const char* ckg_get_version(void) {
    static const char* version = "1.0.0-mock";
    return version;
}

// Check if language is supported
CKG_API bool ckg_is_language_supported(CKGLanguage language) {
    switch (language) {
        case CKG_LANG_C:
        case CKG_LANG_CPP:
        case CKG_LANG_CSHARP:
        case CKG_LANG_JAVA:
        case CKG_LANG_JAVASCRIPT:
        case CKG_LANG_TYPESCRIPT:
        case CKG_LANG_PYTHON:
        case CKG_LANG_GO:
            return true;
        default:
            return false;
    }
}

// Get language parser based on file extension
static const TSLanguage* get_language_from_extension(const char* extension) {
    if (!extension) return NULL;
    
    if (strcmp(extension, ".cs") == 0) {
        return tree_sitter_c_sharp();
    } else if (strcmp(extension, ".js") == 0 || strcmp(extension, ".jsx") == 0) {
        return tree_sitter_javascript();
    } else if (strcmp(extension, ".py") == 0) {
        return tree_sitter_python();
    } else if (strcmp(extension, ".c") == 0 || strcmp(extension, ".h") == 0) {
        return tree_sitter_c();
    } else if (strcmp(extension, ".cpp") == 0 || strcmp(extension, ".cc") == 0 || strcmp(extension, ".cxx") == 0 || strcmp(extension, ".hpp") == 0) {
        return tree_sitter_cpp();
    } else if (strcmp(extension, ".java") == 0) {
        return tree_sitter_java();
    } else if (strcmp(extension, ".ts") == 0 || strcmp(extension, ".tsx") == 0) {
        return tree_sitter_typescript();
    } else if (strcmp(extension, ".go") == 0) {
        return tree_sitter_go();
    }
    
    return NULL;
}

// Get tree-sitter language for CKG language
static const TSLanguage* get_ts_language(CKGLanguage language) {
    switch (language) {
        case CKG_LANG_C:
            return tree_sitter_c();
        case CKG_LANG_CPP:
            return tree_sitter_cpp();
        case CKG_LANG_CSHARP:
            return tree_sitter_c_sharp();
        case CKG_LANG_JAVA:
            return tree_sitter_java();
        case CKG_LANG_JAVASCRIPT:
            return tree_sitter_javascript();
        case CKG_LANG_TYPESCRIPT:
            return tree_sitter_typescript();
        case CKG_LANG_PYTHON:
            return tree_sitter_python();
        case CKG_LANG_GO:
            return tree_sitter_go();
        default:
            return NULL;
    }
}

// Parse source code and return results
CKG_API CKGParseResult* ckg_parse(CKGLanguage language, const char* source_code, const char* file_path) {
    if (!initialized || !parser || !source_code) {
        return NULL;
    }
    
    const TSLanguage* ts_language = get_ts_language(language);
    if (!ts_language) {
        CKGParseResult* result = (CKGParseResult*)malloc(sizeof(CKGParseResult));
        if (result) {
            result->function_count = 0;
            result->class_count = 0;
            result->property_count = 0;
            result->field_count = 0;
            result->variable_count = 0;
            result->functions = NULL;
            result->classes = NULL;
            result->properties = NULL;
            result->fields = NULL;
            result->variables = NULL;
            result->error_message = strdup("Unsupported language");
        }
        return result;
    }
    
    // Set the language for the parser
    bool set_result = ts_parser_set_language(parser, ts_language);
    if (!set_result) {
        CKGParseResult* result = (CKGParseResult*)malloc(sizeof(CKGParseResult));
        if (result) {
            result->function_count = 0;
            result->class_count = 0;
            result->property_count = 0;
            result->field_count = 0;
            result->variable_count = 0;
            result->functions = NULL;
            result->classes = NULL;
            result->properties = NULL;
            result->fields = NULL;
            result->variables = NULL;
            result->error_message = strdup("Failed to set language");
        }
        return result;
    }
    
    // Parse the source code
    printf("Parsing source code with length: %zu\n", strlen(source_code));
    TSTree* tree = ts_parser_parse_string(parser, NULL, source_code, strlen(source_code));
    if (!tree) {
        printf("Failed to parse source code\n");
        CKGParseResult* result = (CKGParseResult*)malloc(sizeof(CKGParseResult));
        if (result) {
            result->function_count = 0;
            result->class_count = 0;
            result->property_count = 0;
            result->field_count = 0;
            result->variable_count = 0;
            result->functions = NULL;
            result->classes = NULL;
            result->properties = NULL;
            result->fields = NULL;
            result->variables = NULL;
            result->error_message = strdup("Failed to parse code");
        }
        return result;
    }
    printf("Parse successful, tree created\n");
    
    CKGParseResult* result = (CKGParseResult*)malloc(sizeof(CKGParseResult));
    if (!result) {
        ts_tree_delete(tree);
        return NULL;
    }
    
    // Initialize result structure
    result->function_count = 0;
    result->class_count = 0;
    result->property_count = 0;
    result->field_count = 0;
    result->variable_count = 0;
    result->functions = NULL;
    result->classes = NULL;
    result->properties = NULL;
    result->fields = NULL;
    result->variables = NULL;
    result->error_message = NULL;
    
    // Get the root node and walk the syntax tree
    TSNode root_node = ts_tree_root_node(tree);
    
    // Initialize parsed data
    ParsedData data = {0};
    
    // Walk the tree to extract functions, classes, etc.
    printf("Starting tree walk...\n");
    walk_tree(root_node, source_code, &data, NULL);
    printf("Tree walk completed. Found %d functions, %d classes\n", data.function_count, data.class_count);
    
    // Convert ParsedData to CKGParseResult
    if (data.function_count > 0) {
        result->functions = (CKGFunction*)malloc(data.function_count * sizeof(CKGFunction));
        result->function_count = data.function_count;
        
        for (int i = 0; i < data.function_count; i++) {
            result->functions[i].name = strdup(data.functions[i].name);
            result->functions[i].start_line = data.functions[i].start_line;
            result->functions[i].end_line = data.functions[i].end_line;
            result->functions[i].return_type = NULL;
            result->functions[i].parameters = NULL;
            result->functions[i].start_column = 0;
            result->functions[i].end_column = 0;
            result->functions[i].is_public = false;
            result->functions[i].is_private = false;
            result->functions[i].is_protected = false;
            result->functions[i].is_static = false;
            result->functions[i].is_async = false;
            result->functions[i].parent_class = NULL;
        }
    }
    
    if (data.class_count > 0) {
        result->classes = (CKGClass*)malloc(data.class_count * sizeof(CKGClass));
        result->class_count = data.class_count;
        
        for (int i = 0; i < data.class_count; i++) {
            result->classes[i].name = strdup(data.classes[i].name);
            result->classes[i].start_line = data.classes[i].start_line;
            result->classes[i].end_line = data.classes[i].end_line;
            result->classes[i].namespace_name = NULL;
            result->classes[i].base_class = NULL;
            result->classes[i].interfaces = NULL;
            result->classes[i].start_column = 0;
            result->classes[i].end_column = 0;
            result->classes[i].is_public = false;
            result->classes[i].is_private = false;
            result->classes[i].is_protected = false;
            result->classes[i].is_static = false;
            result->classes[i].is_abstract = false;
            result->classes[i].is_sealed = false;
        }
    }
    
    // Clean up parsed data
    if (data.classes) free(data.classes);
    if (data.functions) free(data.functions);
    
    ts_tree_delete(tree);
    return result;
}

// Parse source code and return JSON result
// Simple data structures for extracted information
// Helper function to get node text
static char* get_node_text(TSNode node, const char* source_code) {
    uint32_t start_byte = ts_node_start_byte(node);
    uint32_t end_byte = ts_node_end_byte(node);
    uint32_t length = end_byte - start_byte;
    
    char* text = malloc(length + 1);
    if (text) {
        memcpy(text, source_code + start_byte, length);
        text[length] = '\0';
    }
    return text;
}

// Add class to parsed data
static void add_class(ParsedData* data, const char* name, int start_line, int end_line) {
    if (data->class_count >= data->class_capacity) {
        data->class_capacity = data->class_capacity == 0 ? 10 : data->class_capacity * 2;
        data->classes = realloc(data->classes, data->class_capacity * sizeof(ExtractedClass));
    }
    
    if (data->classes && data->class_count < data->class_capacity) {
        strncpy(data->classes[data->class_count].name, name, 255);
        data->classes[data->class_count].name[255] = '\0';
        data->classes[data->class_count].start_line = start_line;
        data->classes[data->class_count].end_line = end_line;
        data->class_count++;
    }
}

// Add function to parsed data
static void add_function(ParsedData* data, const char* name, const char* class_name, int start_line, int end_line) {
    if (data->function_count >= data->function_capacity) {
        data->function_capacity = data->function_capacity == 0 ? 10 : data->function_capacity * 2;
        data->functions = realloc(data->functions, data->function_capacity * sizeof(ExtractedFunction));
    }
    
    if (data->functions && data->function_count < data->function_capacity) {
        strncpy(data->functions[data->function_count].name, name, 255);
        data->functions[data->function_count].name[255] = '\0';
        strncpy(data->functions[data->function_count].class_name, class_name ? class_name : "", 255);
        data->functions[data->function_count].class_name[255] = '\0';
        data->functions[data->function_count].start_line = start_line;
        data->functions[data->function_count].end_line = end_line;
        data->function_count++;
    }
}

// Recursive function to walk the syntax tree
static void walk_tree(TSNode node, const char* source_code, ParsedData* data, const char* current_class) {
    const char* node_type = ts_node_type(node);
    
    // Debug: print node type
    printf("Node type: %s\n", node_type);
    
    if (strcmp(node_type, "class_declaration") == 0) {
        // Find the class name
        uint32_t child_count = ts_node_child_count(node);
        for (uint32_t i = 0; i < child_count; i++) {
            TSNode child = ts_node_child(node, i);
            const char* child_type = ts_node_type(child);
            
            if (strcmp(child_type, "identifier") == 0) {
                char* class_name = get_node_text(child, source_code);
                if (class_name) {
                    TSPoint start_point = ts_node_start_point(node);
                    TSPoint end_point = ts_node_end_point(node);
                    add_class(data, class_name, start_point.row + 1, end_point.row + 1);
                    
                    // Continue walking with this class as context
                    for (uint32_t j = 0; j < child_count; j++) {
                        TSNode class_child = ts_node_child(node, j);
                        walk_tree(class_child, source_code, data, class_name);
                    }
                    
                    free(class_name);
                    return;
                }
                break;
            }
        }
    } else if (strcmp(node_type, "method_declaration") == 0 || strcmp(node_type, "constructor_declaration") == 0) {
        // Find the method name
        uint32_t child_count = ts_node_child_count(node);
        for (uint32_t i = 0; i < child_count; i++) {
            TSNode child = ts_node_child(node, i);
            const char* child_type = ts_node_type(child);
            
            if (strcmp(child_type, "identifier") == 0) {
                char* method_name = get_node_text(child, source_code);
                if (method_name) {
                    TSPoint start_point = ts_node_start_point(node);
                    TSPoint end_point = ts_node_end_point(node);
                    add_function(data, method_name, current_class, start_point.row + 1, end_point.row + 1);
                    free(method_name);
                }
                break;
            }
        }
    } else if (strcmp(node_type, "function_definition") == 0) {
        // C language function definition
        uint32_t child_count = ts_node_child_count(node);
        for (uint32_t i = 0; i < child_count; i++) {
            TSNode child = ts_node_child(node, i);
            const char* child_type = ts_node_type(child);
            
            if (strcmp(child_type, "function_declarator") == 0) {
                // Look for identifier in function_declarator
                uint32_t declarator_child_count = ts_node_child_count(child);
                for (uint32_t j = 0; j < declarator_child_count; j++) {
                    TSNode declarator_child = ts_node_child(child, j);
                    const char* declarator_child_type = ts_node_type(declarator_child);
                    
                    if (strcmp(declarator_child_type, "identifier") == 0) {
                        char* function_name = get_node_text(declarator_child, source_code);
                        if (function_name) {
                            TSPoint start_point = ts_node_start_point(node);
                            TSPoint end_point = ts_node_end_point(node);
                            add_function(data, function_name, current_class, start_point.row + 1, end_point.row + 1);
                            free(function_name);
                        }
                        break;
                    }
                }
                break;
            }
        }
    }
    
    // Recursively walk all children
    uint32_t child_count = ts_node_child_count(node);
    for (uint32_t i = 0; i < child_count; i++) {
        TSNode child = ts_node_child(node, i);
        walk_tree(child, source_code, data, current_class);
    }
}

CKG_API char* ckg_parse_json(void* parser_ptr, const char* source_code, const char* language, const char* file_path) {
    if (!initialized || !source_code || !language || !file_path || !parser) {
        return NULL;
    }
    
    // Determine language from file extension
    const TSLanguage* ts_language = NULL;
    const char* ext = strrchr(file_path, '.');

    if (ext) {
        ts_language = get_language_from_extension(ext);
    }


    if (!ts_language) {

        // Return empty result for unsupported languages
        const char* empty_json = 
            "{"
            "\"functions\": [],"
            "\"classes\": [],"
            "\"properties\": [],"
            "\"fields\": [],"
            "\"variables\": []"
            "}";
        return strdup(empty_json);
    }
    
    // Set the language for the parser
    if (!ts_parser_set_language(parser, ts_language)) {
        return NULL;
    }
    
    // Parse the source code
    TSTree* tree = ts_parser_parse_string(parser, NULL, source_code, strlen(source_code));
    if (!tree) {
        return NULL;
    }
    
    // Initialize parsed data
    ParsedData data = {0};
    
    // Get the root node and walk the tree
    TSNode root_node = ts_tree_root_node(tree);

    walk_tree(root_node, source_code, &data, NULL);

    
    // Build JSON result
    size_t json_size = 4096; // Start with 4KB
    char* result_json = malloc(json_size);
    if (!result_json) {
        ts_tree_delete(tree);
        if (data.classes) free(data.classes);
        if (data.functions) free(data.functions);
        return NULL;
    }
    
    // Start building JSON
    strcpy(result_json, "{\"functions\": [");
    
    // Add functions
    for (int i = 0; i < data.function_count; i++) {
        char func_json[512];
        snprintf(func_json, sizeof(func_json),
            "%s{\"name\": \"%s\", \"class_name\": \"%s\", \"start_line\": %d, \"end_line\": %d}",
            i > 0 ? ", " : "",
            data.functions[i].name,
            data.functions[i].class_name,
            data.functions[i].start_line,
            data.functions[i].end_line
        );
        strcat(result_json, func_json);
    }
    
    strcat(result_json, "], \"classes\": [");
    
    // Add classes
    for (int i = 0; i < data.class_count; i++) {
        char class_json[512];
        snprintf(class_json, sizeof(class_json),
            "%s{\"name\": \"%s\", \"start_line\": %d, \"end_line\": %d}",
            i > 0 ? ", " : "",
            data.classes[i].name,
            data.classes[i].start_line,
            data.classes[i].end_line
        );
        strcat(result_json, class_json);
    }
    
    strcat(result_json, "], \"properties\": [], \"fields\": [], \"variables\": []}");
    
    // Cleanup
    ts_tree_delete(tree);
    if (data.classes) free(data.classes);
    if (data.functions) free(data.functions);
    
    return result_json;
}

// Free JSON result
CKG_API void ckg_free_json_result(char* json_result) {
    if (json_result) {
        free(json_result);
    }
}

// Free parse result
CKG_API void ckg_free_result(CKGParseResult* result) {
    if (!result) {
        return;
    }
    
    // Free arrays if they exist
    if (result->functions) {
        free(result->functions);
    }
    if (result->classes) {
        free(result->classes);
    }
    if (result->properties) {
        free(result->properties);
    }
    if (result->fields) {
        free(result->fields);
    }
    if (result->variables) {
        free(result->variables);
    }
    if (result->error_message) {
        free((void*)result->error_message);
    }
    
    free(result);
}