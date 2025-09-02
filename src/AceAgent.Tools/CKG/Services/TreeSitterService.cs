using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text.Json;
using AceAgent.Tools.CKG.Models;
using System.Reflection;

namespace AceAgent.Tools.CKG.Services;

public class TreeSitterService : IDisposable
{
    private readonly ILogger<TreeSitterService> _logger;
    private bool _isInitialized;
    private bool _disposed;

    // Native library imports
    private const string LibraryName = "ckg_wrapper";
    
    static TreeSitterService()
    {
        Console.WriteLine("Setting up DllImportResolver for TreeSitterService");
        NativeLibrary.SetDllImportResolver(typeof(TreeSitterService).Assembly, DllImportResolver);
    }
    
    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        Console.WriteLine($"DllImportResolver called for library: '{libraryName}', expected: '{LibraryName}'");
        Console.WriteLine($"Library name match: {libraryName == LibraryName}");
        
        if (libraryName == LibraryName)
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDir = Path.GetDirectoryName(assemblyLocation);
            
            Console.WriteLine($"Assembly location: {assemblyLocation}");
            Console.WriteLine($"Assembly directory: {assemblyDir}");
            
            // Try runtimes directory first (new structure)
            var runtimeId = GetRuntimeIdentifier();
            var libraryFileName = GetLibraryFileName();
            var runtimeLibraryPath = Path.Combine(assemblyDir!, "runtimes", runtimeId, "native", libraryFileName);
            
            Console.WriteLine($"Runtime ID: {runtimeId}");
            Console.WriteLine($"Library filename: {libraryFileName}");
            Console.WriteLine($"Runtime library path: {runtimeLibraryPath}");
            Console.WriteLine($"File exists: {File.Exists(runtimeLibraryPath)}");
            
            if (File.Exists(runtimeLibraryPath))
            {
                try
                {
                    return NativeLibrary.Load(runtimeLibraryPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load library from {runtimeLibraryPath}: {ex.Message}");
                }
            }
            
            // Fallback to old native/lib directory
            var fallbackLibraryPath = Path.Combine(assemblyDir!, "CKG", "native", "lib", libraryFileName);
            
            if (File.Exists(fallbackLibraryPath))
            {
                try
                {
                    return NativeLibrary.Load(fallbackLibraryPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load library from {fallbackLibraryPath}: {ex.Message}");
                }
            }
            
            Console.WriteLine($"Library {libraryFileName} not found in expected locations:");
            Console.WriteLine($"  - {runtimeLibraryPath}");
            Console.WriteLine($"  - {fallbackLibraryPath}");
        }
        return IntPtr.Zero;
    }
    
    private static string GetRuntimeIdentifier()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return RuntimeInformation.OSArchitecture == Architecture.X64 ? "win-x64" : "win-x86";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.OSArchitecture == Architecture.X64 ? "linux-x64" : "linux-arm64";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";
        }
        
        return "unknown";
    }
    
    private static string GetLibraryFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "ckg_wrapper.dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "ckg_wrapper.so";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "ckg_wrapper.dylib";
        }
        
        return "ckg_wrapper";
    }
    
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ckg_init();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ckg_cleanup();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr ckg_parse_json(IntPtr parser, string source_code, string language, string file_path);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ckg_free_json_result(IntPtr json_result);

    private IntPtr _parser;

    public TreeSitterService(ILogger<TreeSitterService> logger)
    {
        _logger = logger;
        Initialize();
    }

    private void Initialize()
    {
        try
        {
            int result = ckg_init();
            _isInitialized = result == 1;
            _parser = IntPtr.Zero; // We don't need to store parser pointer as it's managed internally
            
            if (_isInitialized)
            {
                _logger.LogInformation("Tree-sitter service initialized successfully");
            }
            else
            {
                _logger.LogError("Failed to initialize Tree-sitter service");
                throw new InvalidOperationException("Failed to initialize Tree-sitter parser");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Tree-sitter service");
            _isInitialized = false;
            throw;
        }
    }

    public async Task<ParseResult> ParseCodeAsync(string sourceCode, string language, string filePath)
    {
        return await Task.Run(() => ParseCode(sourceCode, language, filePath));
    }

    public ParseResult ParseCode(string sourceCode, string language, string filePath)
    {
        if (!_isInitialized)
        {
            return ParseResult.Failure(filePath, language, "Tree-sitter service not initialized");
        }

        if (string.IsNullOrEmpty(sourceCode))
        {
            return ParseResult.Failure(filePath, language, "Source code is empty");
        }

        if (!IsSupportedLanguage(language))
        {
            return ParseResult.Failure(filePath, language, $"Unsupported language: {language}");
        }

        try
        {
            _logger.LogInformation("Calling native parser for file: {FilePath}, language: {Language}", filePath, language);
            _logger.LogInformation("Source code length: {Length} characters", sourceCode.Length);
            _logger.LogInformation("Source code preview: {Preview}", sourceCode.Length > 100 ? sourceCode.Substring(0, 100) + "..." : sourceCode);
            
            var resultPtr = ckg_parse_json(_parser, sourceCode, language, filePath);
            
            if (resultPtr == IntPtr.Zero)
            {
                _logger.LogWarning("Native parser returned null for file: {FilePath}", filePath);
                return ParseResult.Failure(filePath, language, "Native parsing failed");
            }

            try
            {
                var jsonResult = Marshal.PtrToStringAnsi(resultPtr);
                
                _logger.LogInformation("Native parser returned JSON: {JsonResult}", jsonResult ?? "null");
                
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return ParseResult.Failure(filePath, language, "Empty result from native parser");
                }

                var parseResult = ConvertJsonToParseResult(jsonResult, filePath, language);
                return parseResult;
            }
            finally
            {
                ckg_free_json_result(resultPtr);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing code for file: {FilePath}", filePath);
            return ParseResult.Failure(filePath, language, $"Parse error: {ex.Message}");
        }
    }

    private ParseResult ConvertJsonToParseResult(string jsonResult, string filePath, string language)
    {
        try
        {
            var result = ParseResult.Success(filePath, language);
            
            // Parse JSON result from native library
            using var document = JsonDocument.Parse(jsonResult);
            var root = document.RootElement;

            if (root.TryGetProperty("functions", out var functionsElement))
            {
                foreach (var funcElement in functionsElement.EnumerateArray())
                {
                    var function = new Function
                    {
                        Name = GetStringProperty(funcElement, "name"),
                        FilePath = filePath,
                        StartLine = GetIntProperty(funcElement, "start_line"),
                        EndLine = GetIntProperty(funcElement, "end_line"),
                        ReturnType = GetStringProperty(funcElement, "return_type"),
                        Parameters = GetStringProperty(funcElement, "parameters"),
                        Modifiers = GetStringProperty(funcElement, "modifiers"),
                        ClassName = GetStringProperty(funcElement, "class_name"),
                        Namespace = GetStringProperty(funcElement, "namespace"),
                        IsStatic = GetBoolProperty(funcElement, "is_static"),
                        IsPublic = GetBoolProperty(funcElement, "is_public"),
                        IsPrivate = GetBoolProperty(funcElement, "is_private"),
                        IsProtected = GetBoolProperty(funcElement, "is_protected"),
                        Documentation = GetStringProperty(funcElement, "documentation")
                    };
                    result.Functions.Add(function);
                }
            }

            if (root.TryGetProperty("classes", out var classesElement))
            {
                foreach (var classElement in classesElement.EnumerateArray())
                {
                    var cls = new Class
                    {
                        Name = GetStringProperty(classElement, "name"),
                        FilePath = filePath,
                        StartLine = GetIntProperty(classElement, "start_line"),
                        EndLine = GetIntProperty(classElement, "end_line"),
                        Namespace = GetStringProperty(classElement, "namespace"),
                        Modifiers = GetStringProperty(classElement, "modifiers"),
                        BaseClass = GetStringProperty(classElement, "base_class"),
                        Interfaces = GetStringProperty(classElement, "interfaces"),
                        IsStatic = GetBoolProperty(classElement, "is_static"),
                        IsAbstract = GetBoolProperty(classElement, "is_abstract"),
                        IsSealed = GetBoolProperty(classElement, "is_sealed"),
                        IsPublic = GetBoolProperty(classElement, "is_public"),
                        Documentation = GetStringProperty(classElement, "documentation")
                    };
                    result.Classes.Add(cls);
                }
            }

            // Add similar parsing for Properties, Fields, and Variables...
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting JSON result to ParseResult");
            return ParseResult.Failure(filePath, language, $"JSON conversion error: {ex.Message}");
        }
    }

    private static string GetStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString() ?? string.Empty
            : string.Empty;
    }

    private static int GetIntProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number
            ? prop.GetInt32()
            : 0;
    }

    private static bool GetBoolProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.True;
    }

    private bool IsSupportedLanguage(string language)
    {
        var supportedLanguages = new HashSet<string>
        {
            "c", "cpp", "csharp", "go", "java", "javascript", "python", "rust", "typescript"
        };
        return supportedLanguages.Contains(language.ToLowerInvariant());
    }

    public void Dispose()
    {
        if (!_disposed && _isInitialized)
        {
            try
            {
                ckg_cleanup();
                _logger.LogInformation("Tree-sitter service disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing Tree-sitter service");
            }
            finally
            {
                _parser = IntPtr.Zero;
                _isInitialized = false;
                _disposed = true;
            }
        }
    }
}