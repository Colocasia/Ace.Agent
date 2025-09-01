cmake_minimum_required(VERSION 3.16)
project(ckg_wrapper)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 17)

# Enable position independent code for all targets
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# Detect platform
if(APPLE)
    # Check if CMAKE_OSX_ARCHITECTURES is set (from command line)
    if(CMAKE_OSX_ARCHITECTURES MATCHES "arm64")
        set(PLATFORM_DIR "osx-arm64")
    elseif(CMAKE_SYSTEM_PROCESSOR MATCHES "arm64|aarch64")
        set(PLATFORM_DIR "osx-arm64")
    else()
        set(PLATFORM_DIR "osx-x64")
    endif()
elseif(WIN32)
    if(CMAKE_SIZEOF_VOID_P EQUAL 8)
        set(PLATFORM_DIR "win-x64")
    else()
        set(PLATFORM_DIR "win-x86")
    endif()
else()
    if(CMAKE_SIZEOF_VOID_P EQUAL 8)
        set(PLATFORM_DIR "linux-x64")
    else()
        set(PLATFORM_DIR "linux-x86")
    endif()
endif()

# Create platform-specific output directory
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}/runtimes/${PLATFORM_DIR}/native)
file(MAKE_DIRECTORY ${CMAKE_LIBRARY_OUTPUT_DIRECTORY})

# For Windows, also set the output directory for different configurations
if(WIN32)
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_DEBUG ${CMAKE_LIBRARY_OUTPUT_DIRECTORY})
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_RELEASE ${CMAKE_LIBRARY_OUTPUT_DIRECTORY})
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_LIBRARY_OUTPUT_DIRECTORY})
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_DEBUG ${CMAKE_LIBRARY_OUTPUT_DIRECTORY})
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELEASE ${CMAKE_LIBRARY_OUTPUT_DIRECTORY})
endif()

# Common include directories
set(TREE_SITTER_INCLUDE tree-sitter/lib/include)
set(TYPESCRIPT_COMMON_INCLUDE languages/tree-sitter-typescript/common)

# Function to create language library with common settings
function(add_language_library name sources)
    add_library(${name} STATIC ${sources})
    target_include_directories(${name} PRIVATE ${TREE_SITTER_INCLUDE} ${ARGN})
endfunction()

# All language libraries for linking
set(LANGUAGE_LIBRARIES
    tree-sitter-csharp
    tree-sitter-javascript
    tree-sitter-python
    tree-sitter-c
    tree-sitter-cpp
    tree-sitter-java
    tree-sitter-typescript
    tree-sitter-tsx
    tree-sitter-go
    tree-sitter-rust
)

# Build tree-sitter core library
# Note: lib.c includes all other source files, so we only need to compile lib.c
add_library(tree-sitter STATIC
    tree-sitter/lib/src/lib.c
)

# Add language parsers using simplified function
add_language_library(tree-sitter-csharp
    "languages/tree-sitter-c-sharp/src/parser.c;languages/tree-sitter-c-sharp/src/scanner.c")

add_language_library(tree-sitter-javascript
    "languages/tree-sitter-javascript/src/parser.c;languages/tree-sitter-javascript/src/scanner.c")

add_language_library(tree-sitter-python
    "languages/tree-sitter-python/src/parser.c;languages/tree-sitter-python/src/scanner.c")

add_language_library(tree-sitter-c
    "languages/tree-sitter-c/src/parser.c")

add_language_library(tree-sitter-cpp
    "languages/tree-sitter-cpp/src/parser.c;languages/tree-sitter-cpp/src/scanner.c")

add_language_library(tree-sitter-java
    "languages/tree-sitter-java/src/parser.c")

add_language_library(tree-sitter-typescript
    "languages/tree-sitter-typescript/typescript/src/parser.c;languages/tree-sitter-typescript/typescript/src/scanner.c"
    ${TYPESCRIPT_COMMON_INCLUDE} languages/tree-sitter-typescript/typescript/src)

add_language_library(tree-sitter-tsx
    "languages/tree-sitter-typescript/tsx/src/parser.c;languages/tree-sitter-typescript/tsx/src/scanner.c"
    ${TYPESCRIPT_COMMON_INCLUDE} languages/tree-sitter-typescript/tsx/src)

add_language_library(tree-sitter-go
    "languages/tree-sitter-go/src/parser.c")

add_language_library(tree-sitter-rust
    "languages/tree-sitter-rust/src/parser.c;languages/tree-sitter-rust/src/scanner.c")

# Create wrapper library
add_library(ckg_wrapper SHARED
    wrapper/ckg_wrapper.c
)

# Link with tree-sitter and language parsers
target_link_libraries(ckg_wrapper tree-sitter ${LANGUAGE_LIBRARIES})

# Include directories
target_include_directories(ckg_wrapper PRIVATE ${TREE_SITTER_INCLUDE} wrapper)
target_include_directories(tree-sitter PRIVATE ${TREE_SITTER_INCLUDE} tree-sitter/lib/src)

# Set library properties
set_target_properties(ckg_wrapper PROPERTIES
    OUTPUT_NAME "ckg_wrapper"
    PREFIX ""
    C_VISIBILITY_PRESET default
    VISIBILITY_INLINES_HIDDEN OFF
    INSTALL_RPATH "${CMAKE_LIBRARY_OUTPUT_DIRECTORY}"
    BUILD_WITH_INSTALL_RPATH TRUE
)

# Platform-specific settings
if(APPLE)
    set_target_properties(ckg_wrapper PROPERTIES
        SUFFIX ".dylib"
    )
elseif(WIN32)
    set_target_properties(ckg_wrapper PROPERTIES
        SUFFIX ".dll"
    )
else()
    set_target_properties(ckg_wrapper PROPERTIES
        SUFFIX ".so"
    )
endif()

# Create test executable
add_executable(test_all_languages test_all_languages.c)
target_link_libraries(test_all_languages tree-sitter ${LANGUAGE_LIBRARIES})
target_include_directories(test_all_languages PRIVATE ${TREE_SITTER_INCLUDE})

# Add clean rule
add_custom_target(clean-all
    COMMAND ${CMAKE_COMMAND} -E remove_directory ${CMAKE_BINARY_DIR}
    COMMAND ${CMAKE_COMMAND} -E make_directory ${CMAKE_BINARY_DIR}
    COMMENT "Cleaning all build files"
)