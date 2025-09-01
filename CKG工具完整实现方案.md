# CKG工具完整实现方案

## 概述

本文档提供了基于官方 `csharp-tree-sitter` 库的CKG（Code Knowledge Graph）工具完整实现方案。该方案整合了多语言代码解析、知识图谱构建和智能搜索功能，支持C/C++、C#、Java、Lua、JavaScript、Python、Go、Rust等9种主流编程语言。

## 技术栈选择

### 1. 核心解析引擎

#### Tree-sitter C# 绑定（官方推荐）
- **仓库**: https://github.com/tree-sitter/csharp-tree-sitter
- **状态**: 官方维护，活跃开发
- **特点**: 通过 P/Invoke 提供 C# 绑定
- **平台**: 跨平台支持（Windows、macOS、Linux）
- **优势**:
  - 官方支持，稳定可靠
  - 高性能增量解析
  - 错误恢复能力强
  - 支持50+种编程语言

#### Microsoft.CodeAnalysis (Roslyn) - C#专用
- **用途**: C#代码的深度语义分析
- **NuGet包**: `Microsoft.CodeAnalysis` (4.14.0+)
- **优势**: 完整的语义信息、类型推断、符号解析

### 2. 数据库选择

#### SQLite + Entity Framework Core
- **NuGet包**:
  - `Microsoft.EntityFrameworkCore.Sqlite`
  - `Microsoft.EntityFrameworkCore.Tools`
- **优势**: 轻量级、无需额外服务、强类型ORM

### 3. Git集成

#### LibGit2Sharp
- **NuGet包**: `LibGit2Sharp`
- **用途**: Git状态获取、代码库哈希计算

## 支持的语言列表

| 语言 | 文件扩展名 | Tree-sitter 语法库 | 优先级 | 状态 |
|------|------------|-------------------|--------|------|
| C# | .cs | tree-sitter-c-sharp | 高 | ✅ Roslyn + Tree-sitter |
| C | .c, .h | tree-sitter-c | 高 | ✅ 计划支持 |
| C++ | .cpp, .cxx, .cc, .hpp, .hxx | tree-sitter-cpp | 高 | ✅ 计划支持 |
| Java | .java | tree-sitter-java | 高 | ✅ 计划支持 |
| Python | .py, .pyw, .pyi | tree-sitter-python | 高 | ✅ 计划支持 |
| JavaScript | .js, .jsx, .mjs, .cjs | tree-sitter-javascript | 高 | ✅ 计划支持 |
| TypeScript | .ts, .tsx, .d.ts | tree-sitter-typescript | 高 | ✅ 计划支持 |
| Go | .go | tree-sitter-go | 中 | ✅ 计划支持 |
| Rust | .rs | tree-sitter-rust | 中 | ✅ 计划支持 |
| Lua | .lua | tree-sitter-lua | 低 | ✅ 计划支持 |

## 架构设计

### 1. 项目结构

```
AceAgent.Tools.CKG/
├── Core/
│   ├── Models/
│   │   ├── LanguageInfo.cs
│   │   ├── FunctionEntry.cs
│   │   ├── ClassEntry.cs
│   │   ├── FieldInfo.cs
│   │   ├── ParameterInfo.cs
│   │   └── ParseResult.cs
│   ├── Interfaces/
│   │   ├── ILanguageProvider.cs
│   │   ├── ILanguageParser.cs
│   │   └── ITreeSitterBinding.cs
│   └── Enums/
│       ├── FunctionType.cs
│       ├── ClassType.cs
│       └── LanguageFeatures.cs
├── Parsers/
│   ├── Base/
│   │   ├── LanguageProviderBase.cs
│   │   ├── TreeSitterParserBase.cs
│   │   └── RoslynParserBase.cs
│   ├── Languages/
│   │   ├── CSharp/
│   │   │   ├── CSharpLanguageProvider.cs
│   │   │   ├── CSharpTreeSitterParser.cs
│   │   │   └── CSharpRoslynParser.cs
│   │   ├── C/
│   │   │   ├── CLanguageProvider.cs
│   │   │   └── CParser.cs
│   │   ├── Cpp/
│   │   │   ├── CppLanguageProvider.cs
│   │   │   └── CppParser.cs
│   │   ├── Java/
│   │   │   ├── JavaLanguageProvider.cs
│   │   │   └── JavaParser.cs
│   │   ├── Python/
│   │   │   ├── PythonLanguageProvider.cs
│   │   │   └── PythonParser.cs
│   │   ├── JavaScript/
│   │   │   ├── JavaScriptLanguageProvider.cs
│   │   │   └── JavaScriptParser.cs
│   │   ├── TypeScript/
│   │   │   ├── TypeScriptLanguageProvider.cs
│   │   │   └── TypeScriptParser.cs
│   │   ├── Go/
│   │   │   ├── GoLanguageProvider.cs
│   │   │   └── GoParser.cs
│   │   ├── Rust/
│   │   │   ├── RustLanguageProvider.cs
│   │   │   └── RustParser.cs
│   │   └── Lua/
│   │       ├── LuaLanguageProvider.cs
│   │       └── LuaParser.cs
│   ├── Services/
│   │   ├── ParserFactory.cs
│   │   ├── LanguageDetector.cs
│   │   └── ParseResultAggregator.cs
│   └── Queries/
│       ├── TreeSitterQueries.cs
│       └── LanguageQueries/
│           ├── CQueries.cs
│           ├── CppQueries.cs
│           ├── JavaQueries.cs
│           ├── PythonQueries.cs
│           ├── JavaScriptQueries.cs
│           ├── TypeScriptQueries.cs
│           ├── GoQueries.cs
│           ├── RustQueries.cs
│           └── LuaQueries.cs
├── Database/
│   ├── CKGDbContext.cs
│   ├── CKGDatabase.cs
│   ├── CKGDatabaseOptions.cs
│   └── Migrations/
├── Services/
│   ├── GitService.cs
│   ├── CacheService.cs
│   ├── ProgressReporter.cs
│   └── PerformanceMonitor.cs
├── Native/
│   ├── TreeSitterBinding.cs
│   ├── NativeLibraryLoader.cs
│   └── runtimes/
│       ├── win-x64/
│       │   └── native/
│       ├── linux-x64/
│       │   └── native/
│       └── osx-x64/
│           └── native/
├── Utils/
│   ├── FileExtensions.cs
│   ├── LanguageDetectionUtils.cs
│   └── PerformanceUtils.cs
└── CKGTool.cs
```

### 2. 核心数据模型

```csharp
// Core/Models/LanguageInfo.cs
public class LanguageInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string[] FileExtensions { get; set; } = Array.Empty<string>();
    public string TreeSitterLibrary { get; set; } = string.Empty;
    public LanguageFeatures Features { get; set; } = new();
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public string[] BuiltinTypes { get; set; } = Array.Empty<string>();
    public CommentStyle CommentStyle { get; set; } = new();
    public bool IsSupported { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class LanguageFeatures
{
    public bool SupportsClasses { get; set; }
    public bool SupportsInterfaces { get; set; }
    public bool SupportsNamespaces { get; set; }
    public bool SupportsGenerics { get; set; }
    public bool SupportsInheritance { get; set; }
    public bool SupportsOverloading { get; set; }
    public bool SupportsLambdas { get; set; }
    public bool SupportsClosures { get; set; }
}

public class CommentStyle
{
    public string SingleLine { get; set; } = string.Empty;  // e.g., "//", "#", "--"
    public string MultiLineStart { get; set; } = string.Empty;  // e.g., "/*", "--[["
    public string MultiLineEnd { get; set; } = string.Empty;    // e.g., "*/", "]]"
}

// Core/Models/FunctionEntry.cs
public class FunctionEntry
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public int StartColumn { get; set; }
    public int EndColumn { get; set; }
    
    // 扩展属性
    public string? ReturnType { get; set; }
    public List<ParameterInfo> Parameters { get; set; } = new();
    public List<string> Modifiers { get; set; } = new();  // public, static, virtual, etc.
    public string? Namespace { get; set; }
    public string? ParentClass { get; set; }
    public bool IsAsync { get; set; }
    public bool IsGeneric { get; set; }
    public List<string> GenericParameters { get; set; } = new();
    public string? Documentation { get; set; }
    public List<string> Annotations { get; set; } = new();  // @Override, [Attribute], etc.
    public FunctionType Type { get; set; }  // Function, Method, Constructor, Destructor, Lambda
    public string Visibility { get; set; } = "public";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // 导航属性
    public LanguageInfo LanguageInfo { get; set; } = null!;
}

public class ParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
    public bool IsOptional { get; set; }
    public bool IsVariadic { get; set; }  // ...args, *args, **kwargs
    public List<string> Modifiers { get; set; } = new();  // const, ref, out, etc.
}

public enum FunctionType
{
    Function,
    Method,
    Constructor,
    Destructor,
    Lambda,
    Closure,
    StaticMethod,
    VirtualMethod,
    AbstractMethod
}

// Core/Models/ClassEntry.cs
public class ClassEntry
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public int StartColumn { get; set; }
    public int EndColumn { get; set; }
    
    // 扩展属性
    public List<string> BaseClasses { get; set; } = new();
    public List<string> Interfaces { get; set; } = new();
    public List<string> Modifiers { get; set; } = new();  // public, abstract, final, etc.
    public string? Namespace { get; set; }
    public string? ParentClass { get; set; }
    public bool IsGeneric { get; set; }
    public List<string> GenericParameters { get; set; } = new();
    public string? Documentation { get; set; }
    public List<string> Annotations { get; set; } = new();
    public ClassType Type { get; set; }  // Class, Interface, Struct, Enum, Trait
    public List<FieldInfo> Fields { get; set; } = new();
    public List<FunctionEntry> Methods { get; set; } = new();
    public string Visibility { get; set; } = "public";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // 导航属性
    public LanguageInfo LanguageInfo { get; set; } = null!;
}

public class FieldInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> Modifiers { get; set; } = new();
    public string? DefaultValue { get; set; }
    public bool IsStatic { get; set; }
    public bool IsConstant { get; set; }
}

public enum ClassType
{
    Class,
    Interface,
    Struct,
    Enum,
    Trait,
    Union,
    Protocol
}

// Core/Models/ParseResult.cs
public class ParseResult
{
    public List<FunctionEntry> Functions { get; set; } = new();
    public List<ClassEntry> Classes { get; set; } = new();
    public string Language { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ParseDuration { get; set; }
    public int LinesOfCode { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

### 3. 核心接口设计

```csharp
// Core/Interfaces/ILanguageProvider.cs
public interface ILanguageProvider
{
    string LanguageName { get; }
    string[] FileExtensions { get; }
    int Priority { get; }
    LanguageInfo GetLanguageInfo();
    bool CanParse(string filePath, string content);
    IntPtr GetTreeSitterLanguage();
}

// Core/Interfaces/ILanguageParser.cs
public interface ILanguageParser
{
    string LanguageName { get; }
    string[] SupportedExtensions { get; }
    int Priority { get; }
    
    bool CanParse(string filePath);
    Task<ParseResult> ParseAsync(string filePath, string content);
    Task<List<FunctionEntry>> ParseFunctionsAsync(string filePath, string content);
    Task<List<ClassEntry>> ParseClassesAsync(string filePath, string content);
}

// Core/Interfaces/ITreeSitterBinding.cs
public interface ITreeSitterBinding
{
    IntPtr CreateParser();
    IntPtr ParseString(IntPtr parser, string content);
    IntPtr GetRootNode(IntPtr tree);
    IntPtr CreateQuery(IntPtr language, string queryString);
    IEnumerable<QueryMatch> ExecuteQuery(IntPtr query, IntPtr node);
    void FreeParser(IntPtr parser);
    void FreeTree(IntPtr tree);
    void FreeQuery(IntPtr query);
}
```

### 4. Tree-sitter 原生绑定

```csharp
// Native/TreeSitterBinding.cs
public class TreeSitterBinding : ITreeSitterBinding
{
    // 基础 Tree-sitter API
    [DllImport("tree-sitter", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ts_parser_new();
    
    [DllImport("tree-sitter", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ts_parser_delete(IntPtr parser);
    
    [DllImport("tree-sitter", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool ts_parser_set_language(IntPtr parser, IntPtr language);
    
    [DllImport("tree-sitter", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ts_parser_parse_string(
        IntPtr parser, IntPtr oldTree, byte[] input, uint length);
    
    [DllImport("tree-sitter", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ts_tree_root_node(IntPtr tree);
    
    [DllImport("tree-sitter", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ts_tree_delete(IntPtr tree);
    
    [DllImport("tree-sitter", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ts_query_new(
        IntPtr language, byte[] source, uint sourceLen, 
        out uint errorOffset, out int errorType);
    
    [DllImport("tree-sitter", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ts_query_delete(IntPtr query);
    
    // 语言特定的导入
    [DllImport("tree-sitter-c", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tree_sitter_c();
    
    [DllImport("tree-sitter-cpp", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tree_sitter_cpp();
    
    [DllImport("tree-sitter-c-sharp", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tree_sitter_c_sharp();
    
    [DllImport("tree-sitter-java", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tree_sitter_java();
    
    [DllImport("tree-sitter-python", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tree_sitter_python();
    
    [DllImport("tree-sitter-javascript", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tree_sitter_javascript();
    
    [DllImport("tree-sitter-typescript", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tree_sitter_typescript();
    
    [DllImport("tree-sitter-go", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tree_sitter_go();
    
    [DllImport("tree-sitter-rust", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tree_sitter_rust();
    
    [DllImport("tree-sitter-lua", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tree_sitter_lua();
    
    // 接口实现
    public IntPtr CreateParser() => ts_parser_new();
    
    public IntPtr ParseString(IntPtr parser, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return ts_parser_parse_string(parser, IntPtr.Zero, bytes, (uint)bytes.Length);
    }
    
    public IntPtr GetRootNode(IntPtr tree) => ts_tree_root_node(tree);
    
    public IntPtr CreateQuery(IntPtr language, string queryString)
    {
        var bytes = Encoding.UTF8.GetBytes(queryString);
        return ts_query_new(language, bytes, (uint)bytes.Length, out _, out _);
    }
    
    public IEnumerable<QueryMatch> ExecuteQuery(IntPtr query, IntPtr node)
    {
        // 实现查询执行逻辑
        yield break; // 占位符
    }
    
    public void FreeParser(IntPtr parser) => ts_parser_delete(parser);
    public void FreeTree(IntPtr tree) => ts_tree_delete(tree);
    public void FreeQuery(IntPtr query) => ts_query_delete(query);
}

// Native/NativeLibraryLoader.cs
public static class NativeLibraryLoader
{
    private static readonly Dictionary<string, IntPtr> LoadedLibraries = new();
    
    public static void LoadLanguageLibrary(string languageName)
    {
        if (LoadedLibraries.ContainsKey(languageName))
            return;
            
        var libraryName = GetLibraryName(languageName);
        var libraryPath = GetLibraryPath(libraryName);
        
        if (!File.Exists(libraryPath))
        {
            throw new FileNotFoundException($"Native library not found: {libraryPath}");
        }
        
        var handle = NativeLibrary.Load(libraryPath);
        LoadedLibraries[languageName] = handle;
    }
    
    private static string GetLibraryName(string languageName)
    {
        var platform = GetPlatform();
        return platform switch
        {
            "win" => $"tree-sitter-{languageName.ToLower()}.dll",
            "linux" => $"libtree-sitter-{languageName.ToLower()}.so",
            "osx" => $"libtree-sitter-{languageName.ToLower()}.dylib",
            _ => throw new PlatformNotSupportedException($"Platform {platform} not supported")
        };
    }
    
    private static string GetLibraryPath(string libraryName)
    {
        var platform = GetPlatform();
        var architecture = GetArchitecture();
        var runtimePath = Path.Combine("runtimes", $"{platform}-{architecture}", "native", libraryName);
        
        return Path.Combine(AppContext.BaseDirectory, runtimePath);
    }
    
    private static string GetPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "win";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "osx";
            
        throw new PlatformNotSupportedException();
    }
    
    private static string GetArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException()
        };
    }
}
```

### 5. 语言提供者基类

```csharp
// Parsers/Base/LanguageProviderBase.cs
public abstract class LanguageProviderBase : ILanguageProvider
{
    public abstract string LanguageName { get; }
    public abstract string[] FileExtensions { get; }
    public virtual int Priority => 100;
    
    protected abstract IntPtr GetLanguageFunction();
    
    public virtual LanguageInfo GetLanguageInfo()
    {
        return new LanguageInfo
        {
            Name = LanguageName,
            FileExtensions = FileExtensions,
            TreeSitterLibrary = $"tree-sitter-{LanguageName.ToLower()}",
            IsSupported = true,
            LastUpdated = DateTime.UtcNow
        };
    }
    
    public virtual IntPtr GetTreeSitterLanguage()
    {
        NativeLibraryLoader.LoadLanguageLibrary(LanguageName);
        return GetLanguageFunction();
    }
    
    public virtual bool CanParse(string filePath, string content)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return FileExtensions.Contains(extension);
    }
}

// Parsers/Base/TreeSitterParserBase.cs
public abstract class TreeSitterParserBase : ILanguageParser
{
    protected readonly ILanguageProvider LanguageProvider;
    protected readonly ITreeSitterBinding TreeSitterBinding;
    protected readonly Dictionary<string, string> Queries;
    
    public string LanguageName => LanguageProvider.LanguageName;
    public string[] SupportedExtensions => LanguageProvider.FileExtensions;
    public virtual int Priority => LanguageProvider.Priority;
    
    protected TreeSitterParserBase(ILanguageProvider languageProvider, ITreeSitterBinding? binding = null)
    {
        LanguageProvider = languageProvider;
        TreeSitterBinding = binding ?? new TreeSitterBinding();
        Queries = InitializeQueries();
    }
    
    protected abstract Dictionary<string, string> InitializeQueries();
    
    public virtual bool CanParse(string filePath)
    {
        return LanguageProvider.CanParse(filePath, string.Empty);
    }
    
    public virtual async Task<ParseResult> ParseAsync(string filePath, string content)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var functions = await ParseFunctionsAsync(filePath, content);
            var classes = await ParseClassesAsync(filePath, content);
            
            stopwatch.Stop();
            
            return new ParseResult
            {
                Functions = functions,
                Classes = classes,
                Language = LanguageName,
                FilePath = filePath,
                Success = true,
                ParseDuration = stopwatch.Elapsed,
                LinesOfCode = content.Split('\n').Length
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            return new ParseResult
            {
                Language = LanguageName,
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message,
                ParseDuration = stopwatch.Elapsed
            };
        }
    }
    
    public abstract Task<List<FunctionEntry>> ParseFunctionsAsync(string filePath, string content);
    public abstract Task<List<ClassEntry>> ParseClassesAsync(string filePath, string content);
    
    protected virtual async Task<List<T>> ExecuteQueryAsync<T>(
        string queryKey, string filePath, string content, 
        Func<QueryMatch, string, string, Task<T?>> extractor) where T : class
    {
        if (!Queries.TryGetValue(queryKey, out var queryString))
            return new List<T>();
        
        var parser = TreeSitterBinding.CreateParser();
        var language = LanguageProvider.GetTreeSitterLanguage();
        
        try
        {
            // 设置语言
            // ts_parser_set_language(parser, language); // 需要在绑定中实现
            
            var tree = TreeSitterBinding.ParseString(parser, content);
            var rootNode = TreeSitterBinding.GetRootNode(tree);
            var query = TreeSitterBinding.CreateQuery(language, queryString);
            
            var results = new List<T>();
            
            foreach (var match in TreeSitterBinding.ExecuteQuery(query, rootNode))
            {
                var result = await extractor(match, filePath, content);
                if (result != null)
                    results.Add(result);
            }
            
            TreeSitterBinding.FreeQuery(query);
            TreeSitterBinding.FreeTree(tree);
            
            return results;
        }
        finally
        {
            TreeSitterBinding.FreeParser(parser);
        }
    }
}
```

## 具体语言实现示例

### 1. C# 语言解析器（双引擎）

```csharp
// Parsers/Languages/CSharp/CSharpLanguageProvider.cs
public class CSharpLanguageProvider : LanguageProviderBase
{
    public override string LanguageName => "C#";
    public override string[] FileExtensions => new[] { ".cs" };
    public override int Priority => 200; // 最高优先级
    
    protected override IntPtr GetLanguageFunction() => TreeSitterBinding.tree_sitter_c_sharp();
    
    public override LanguageInfo GetLanguageInfo()
    {
        return new LanguageInfo
        {
            Name = LanguageName,
            FileExtensions = FileExtensions,
            TreeSitterLibrary = "tree-sitter-c-sharp",
            Features = new LanguageFeatures
            {
                SupportsClasses = true,
                SupportsInterfaces = true,
                SupportsNamespaces = true,
                SupportsGenerics = true,
                SupportsInheritance = true,
                SupportsOverloading = true,
                SupportsLambdas = true,
                SupportsClosures = true
            },
            Keywords = new[] { "class", "interface", "struct", "enum", "namespace", "public", "private", "protected", "internal", "static", "virtual", "override", "abstract", "sealed" },
            BuiltinTypes = new[] { "int", "string", "bool", "double", "float", "decimal", "char", "byte", "object", "var" },
            CommentStyle = new CommentStyle
            {
                SingleLine = "//",
                MultiLineStart = "/*",
                MultiLineEnd = "*/"
            },
            IsSupported = true,
            LastUpdated = DateTime.UtcNow
        };
    }
}

// Parsers/Languages/CSharp/CSharpTreeSitterParser.cs
public class CSharpTreeSitterParser : TreeSitterParserBase
{
    public CSharpTreeSitterParser(CSharpLanguageProvider provider) : base(provider) { }
    
    protected override Dictionary<string, string> InitializeQueries()
    {
        return new Dictionary<string, string>
        {
            ["functions"] = CSharpQueries.Functions,
            ["classes"] = CSharpQueries.Classes
        };
    }
    
    public override async Task<List<FunctionEntry>> ParseFunctionsAsync(string filePath, string content)
    {
        return await ExecuteQueryAsync("functions", filePath, content, ExtractFunctionFromMatch);
    }
    
    public override async Task<List<ClassEntry>> ParseClassesAsync(string filePath, string content)
    {
        return await ExecuteQueryAsync("classes", filePath, content, ExtractClassFromMatch);
    }
    
    private async Task<FunctionEntry?> ExtractFunctionFromMatch(QueryMatch match, string filePath, string content)
    {
        // 实现 C# 函数提取逻辑
        // 这里需要根据 Tree-sitter 的查询结果来提取函数信息
        return null; // 占位符
    }
    
    private async Task<ClassEntry?> ExtractClassFromMatch(QueryMatch match, string filePath, string content)
    {
        // 实现 C# 类提取逻辑
        return null; // 占位符
    }
}

// Parsers/Languages/CSharp/CSharpRoslynParser.cs
public class CSharpRoslynParser : ILanguageParser
{
    public string LanguageName => "C#";
    public string[] SupportedExtensions => new[] { ".cs" };
    public int Priority => 250; // 比 Tree-sitter 更高的优先级
    
    public bool CanParse(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".cs", StringComparison.OrdinalIgnoreCase);
    }
    
    public async Task<ParseResult> ParseAsync(string filePath, string content)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var tree = CSharpSyntaxTree.ParseText(content, path: filePath);
            var root = await tree.GetRootAsync();
            
            var functions = await ParseFunctionsAsync(filePath, content);
            var classes = await ParseClassesAsync(filePath, content);
            
            stopwatch.Stop();
            
            return new ParseResult
            {
                Functions = functions,
                Classes = classes,
                Language = LanguageName,
                FilePath = filePath,
                Success = true,
                ParseDuration = stopwatch.Elapsed,
                LinesOfCode = content.Split('\n').Length,
                Metadata = new Dictionary<string, object>
                {
                    ["UsedRoslyn"] = true,
                    ["HasSemanticModel"] = true
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            return new ParseResult
            {
                Language = LanguageName,
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message,
                ParseDuration = stopwatch.Elapsed
            };
        }
    }
    
    public async Task<List<FunctionEntry>> ParseFunctionsAsync(string filePath, string content)
    {
        var tree = CSharpSyntaxTree.ParseText(content, path: filePath);
        var root = await tree.GetRootAsync();
        
        var functions = new List<FunctionEntry>();
        var walker = new CSharpFunctionWalker(filePath, functions);
        walker.Visit(root);
        
        return functions;
    }
    
    public async Task<List<ClassEntry>> ParseClassesAsync(string filePath, string content)
    {
        var tree = CSharpSyntaxTree.ParseText(content, path: filePath);
        var root = await tree.GetRootAsync();
        
        var classes = new List<ClassEntry>();
        var walker = new CSharpClassWalker(filePath, classes);
        walker.Visit(root);
        
        return classes;
    }
}
```

### 2. Python 语言解析器

```csharp
// Parsers/Languages/Python/PythonLanguageProvider.cs
public class PythonLanguageProvider : LanguageProviderBase
{
    public override string LanguageName => "Python";
    public override string[] FileExtensions => new[] { ".py", ".pyw", ".pyi" };
    public override int Priority => 90;
    
    protected override IntPtr GetLanguageFunction() => TreeSitterBinding.tree_sitter_python();
    
    public override LanguageInfo GetLanguageInfo()
    {
        return new LanguageInfo
        {
            Name = LanguageName,
            FileExtensions = FileExtensions,
            TreeSitterLibrary = "tree-sitter-python",
            Features = new LanguageFeatures
            {
                SupportsClasses = true,
                SupportsInterfaces = false,
                SupportsNamespaces = false,
                SupportsGenerics = false,
                SupportsInheritance = true,
                SupportsOverloading = false,
                SupportsLambdas = true,
                SupportsClosures = true
            },
            Keywords = new[] { "def", "class", "import", "from", "if", "elif", "else", "for", "while", "try", "except", "finally", "with", "as", "lambda", "async", "await" },
            BuiltinTypes = new[] { "int", "str", "bool", "float", "list", "dict", "tuple", "set", "None" },
            CommentStyle = new CommentStyle
            {
                SingleLine = "#",
                MultiLineStart = "\"\"\"" ,
                MultiLineEnd = "\"\"\""
            },
            IsSupported = true,
            LastUpdated = DateTime.UtcNow
        };
    }
    
    public override bool CanParse(string filePath, string content)
    {
        if (base.CanParse(filePath, content))
            return true;
            
        // 检查 shebang
        if (content.StartsWith("#!/usr/bin/env python") || 
            content.StartsWith("#!/usr/bin/python"))
            return true;
            
        return false;
    }
}

// Parsers/Languages/Python/PythonParser.cs
public class PythonParser : TreeSitterParserBase
{
    public PythonParser(PythonLanguageProvider provider) : base(provider) { }
    
    protected override Dictionary<string, string> InitializeQueries()
    {
        return new Dictionary<string, string>
        {
            ["functions"] = PythonQueries.Functions,
            ["classes"] = PythonQueries.Classes
        };
    }
    
    public override async Task<List<FunctionEntry>> ParseFunctionsAsync(string filePath, string content)
    {
        return await ExecuteQueryAsync("functions", filePath, content, ExtractFunctionFromMatch);
    }
    
    public override async Task<List<ClassEntry>> ParseClassesAsync(string filePath, string content)
    {
        return await ExecuteQueryAsync("classes", filePath, content, ExtractClassFromMatch);
    }
    
    private async Task<FunctionEntry?> ExtractFunctionFromMatch(QueryMatch match, string filePath, string content)
    {
        // 实现 Python 函数提取逻辑
        return null; // 占位符
    }
    
    private async Task<ClassEntry?> ExtractClassFromMatch(QueryMatch match, string filePath, string content)
    {
        // 实现 Python 类提取逻辑
        return null; // 占位符
    }
}
```

### 3. Tree-sitter 查询定义

```csharp
// Parsers/Queries/LanguageQueries/CSharpQueries.cs
public static class CSharpQueries
{
    public const string Functions = @"
        (method_declaration
            name: (identifier) @function.name
            parameters: (parameter_list) @function.params
            body: (block) @function.body
            type: (_) @function.return_type
        ) @function.definition
        
        (constructor_declaration
            name: (identifier) @constructor.name
            parameters: (parameter_list) @constructor.params
            body: (block) @constructor.body
        ) @constructor.definition
        
        (local_function_statement
            name: (identifier) @local_function.name
            parameters: (parameter_list) @local_function.params
            body: (block) @local_function.body
            type: (_) @local_function.return_type
        ) @local_function.definition";
    
    public const string Classes = @"
        (class_declaration
            name: (identifier) @class.name
            base_list: (base_list)? @class.bases
            body: (declaration_list) @class.body
        ) @class.definition
        
        (interface_declaration
            name: (identifier) @interface.name
            base_list: (base_list)? @interface.bases
            body: (declaration_list) @interface.body
        ) @interface.definition
        
        (struct_declaration
            name: (identifier) @struct.name
            base_list: (base_list)? @struct.bases
            body: (declaration_list) @struct.body
        ) @struct.definition
        
        (enum_declaration
            name: (identifier) @enum.name
            base_list: (base_list)? @enum.bases
            body: (enum_member_declaration_list) @enum.body
        ) @enum.definition";
}

// Parsers/Queries/LanguageQueries/PythonQueries.cs
public static class PythonQueries
{
    public const string Functions = @"
        (function_definition
            name: (identifier) @function.name
            parameters: (parameters) @function.params
            body: (block) @function.body
        ) @function.definition
        
        (async_function_definition
            name: (identifier) @async_function.name
            parameters: (parameters) @async_function.params
            body: (block) @async_function.body
        ) @async_function.definition";
    
    public const string Classes = @"
        (class_definition
            name: (identifier) @class.name
            superclasses: (argument_list)? @class.superclasses
            body: (block) @class.body
        ) @class.definition";
}

// Parsers/Queries/LanguageQueries/JavaQueries.cs
public static class JavaQueries
{
    public const string Functions = @"
        (method_declaration
            name: (identifier) @method.name
            parameters: (formal_parameters) @method.params
            body: (block) @method.body
            type: (_) @method.return_type
        ) @method.definition
        
        (constructor_declaration
            name: (identifier) @constructor.name
            parameters: (formal_parameters) @constructor.params
            body: (constructor_body) @constructor.body
        ) @constructor.definition";
    
    public const string Classes = @"
        (class_declaration
            name: (identifier) @class.name
            superclass: (superclass)? @class.superclass
            interfaces: (super_interfaces)? @class.interfaces
            body: (class_body) @class.body
        ) @class.definition
        
        (interface_declaration
            name: (identifier) @interface.name
            extends: (extends_interfaces)? @interface.extends
            body: (interface_body) @interface.body
        ) @interface.definition
        
        (enum_declaration
            name: (identifier) @enum.name
            interfaces: (super_interfaces)? @enum.interfaces
            body: (enum_body) @enum.body
        ) @enum.definition";
}
```

## 服务层设计

### 1. 解析器工厂

```csharp
// Parsers/Services/ParserFactory.cs
public class ParserFactory
{
    private readonly Dictionary<string, Func<ILanguageParser>> _parserFactories;
    private readonly LanguageDetector _detector;
    private readonly IServiceProvider _serviceProvider;
    
    public ParserFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _parserFactories = new Dictionary<string, Func<ILanguageParser>>();
        var providers = RegisterLanguages();
        _detector = new LanguageDetector(providers);
    }
    
    private List<ILanguageProvider> RegisterLanguages()
    {
        var providers = new List<ILanguageProvider>();
        
        // 注册 C#（双引擎）
        var csharpProvider = new CSharpLanguageProvider();
        providers.Add(csharpProvider);
        _parserFactories["C#-Roslyn"] = () => new CSharpRoslynParser();
        _parserFactories["C#-TreeSitter"] = () => new CSharpTreeSitterParser(csharpProvider);
        
        // 注册 Python
        var pythonProvider = new PythonLanguageProvider();
        providers.Add(pythonProvider);
        _parserFactories["Python"] = () => new PythonParser(pythonProvider);
        
        // 注册 Java
        var javaProvider = new JavaLanguageProvider();
        providers.Add(javaProvider);
        _parserFactories["Java"] = () => new JavaParser(javaProvider);
        
        // 注册 JavaScript
        var jsProvider = new JavaScriptLanguageProvider();
        providers.Add(jsProvider);
        _parserFactories["JavaScript"] = () => new JavaScriptParser(jsProvider);
        
        // 注册 TypeScript
        var tsProvider = new TypeScriptLanguageProvider();
        providers.Add(tsProvider);
        _parserFactories["TypeScript"] = () => new TypeScriptParser(tsProvider);
        
        // 注册 C
        var cProvider = new CLanguageProvider();
        providers.Add(cProvider);
        _parserFactories["C"] = () => new CParser(cProvider);
        
        // 注册 C++
        var cppProvider = new CppLanguageProvider();
        providers.Add(cppProvider);
        _parserFactories["C++"] = () => new CppParser(cppProvider);
        
        // 注册 Go
        var goProvider = new GoLanguageProvider();
        providers.Add(goProvider);
        _parserFactories["Go"] = () => new GoParser(goProvider);
        
        // 注册 Rust
        var rustProvider = new RustLanguageProvider();
        providers.Add(rustProvider);
        _parserFactories["Rust"] = () => new RustParser(rustProvider);
        
        // 注册 Lua
        var luaProvider = new LuaLanguageProvider();
        providers.Add(luaProvider);
        _parserFactories["Lua"] = () => new LuaParser(luaProvider);
        
        return providers;
    }
    
    public ILanguageParser? CreateParser(string filePath, string content)
    {
        var provider = _detector.DetectLanguage(filePath, content);
        if (provider == null) return null;
        
        // 对于 C#，优先使用 Roslyn
        if (provider.LanguageName == "C#")
        {
            if (_parserFactories.TryGetValue("C#-Roslyn", out var roslynFactory))
            {
                return roslynFactory();
            }
        }
        
        if (_parserFactories.TryGetValue(provider.LanguageName, out var factory))
        {
            return factory();
        }
        
        return null;
    }
    
    public ILanguageParser? CreateParser(string languageName, bool preferTreeSitter = false)
    {
        // 对于 C#，允许选择引擎
        if (languageName == "C#")
        {
            var key = preferTreeSitter ? "C#-TreeSitter" : "C#-Roslyn";
            if (_parserFactories.TryGetValue(key, out var factory))
            {
                return factory();
            }
        }
        
        if (_parserFactories.TryGetValue(languageName, out var defaultFactory))
        {
            return defaultFactory();
        }
        
        return null;
    }
    
    public IEnumerable<string> GetSupportedLanguages()
    {
        return _parserFactories.Keys.Where(k => !k.Contains("-")).Distinct();
    }
    
    public IEnumerable<string> GetSupportedExtensions()
    {
        return _detector.GetSupportedExtensions();
    }
}
```

### 2. 语言检测器

```csharp
// Parsers/Services/LanguageDetector.cs
public class LanguageDetector
{
    private readonly List<ILanguageProvider> _providers;
    private readonly Dictionary<string, List<ILanguageProvider>> _extensionMap;
    
    public LanguageDetector(IEnumerable<ILanguageProvider> providers)
    {
        _providers = providers.OrderByDescending(p => p.Priority).ToList();
        _extensionMap = BuildExtensionMap();
    }
    
    private Dictionary<string, List<ILanguageProvider>> BuildExtensionMap()
    {
        var map = new Dictionary<string, List<ILanguageProvider>>();
        
        foreach (var provider in _providers)
        {
            foreach (var extension in provider.FileExtensions)
            {
                if (!map.ContainsKey(extension))
                    map[extension] = new List<ILanguageProvider>();
                    
                map[extension].Add(provider);
            }
        }
        
        // 按优先级排序
        foreach (var list in map.Values)
        {
            list.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }
        
        return map;
    }
    
    public ILanguageProvider? DetectLanguage(string filePath, string content)
    {
        // 1. 首先尝试文件扩展名匹配
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        if (_extensionMap.TryGetValue(extension, out var providersByExtension))
        {
            // 如果只有一个提供者，直接返回
            if (providersByExtension.Count == 1)
                return providersByExtension[0];
            
            // 如果有多个提供者，使用内容检测
            foreach (var provider in providersByExtension)
            {
                if (provider.CanParse(filePath, content))
                    return provider;
            }
            
            // 如果内容检测都失败，返回优先级最高的
            return providersByExtension[0];
        }
        
        // 2. 如果扩展名不匹配，尝试内容检测
        foreach (var provider in _providers)
        {
            if (provider.CanParse(filePath, content))
                return provider;
        }
        
        return null;
    }
    
    public IEnumerable<string> GetSupportedExtensions()
    {
        return _extensionMap.Keys;
    }
    
    public IEnumerable<ILanguageProvider> GetProvidersForExtension(string extension)
    {
        return _extensionMap.TryGetValue(extension.ToLowerInvariant(), out var providers) 
            ? providers 
            : Enumerable.Empty<ILanguageProvider>();
    }
}
```

### 3. 解析结果聚合器

```csharp
// Parsers/Services/ParseResultAggregator.cs
public class ParseResultAggregator
{
    private readonly ParserFactory _parserFactory;
    private readonly ILogger<ParseResultAggregator> _logger;
    
    public ParseResultAggregator(ParserFactory parserFactory, ILogger<ParseResultAggregator> logger)
    {
        _parserFactory = parserFactory;
        _logger = logger;
    }
    
    public async Task<ParseResult> ParseFileAsync(string filePath, string? content = null)
    {
        content ??= await File.ReadAllTextAsync(filePath);
        
        var parser = _parserFactory.CreateParser(filePath, content);
        if (parser == null)
        {
            return new ParseResult
            {
                FilePath = filePath,
                Success = false,
                ErrorMessage = "No suitable parser found for this file type"
            };
        }
        
        try
        {
            var result = await parser.ParseAsync(filePath, content);
            _logger.LogInformation("Parsed {FilePath} with {Language} parser in {Duration}ms", 
                filePath, result.Language, result.ParseDuration.TotalMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing file {FilePath}", filePath);
            return new ParseResult
            {
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
    
    public async Task<List<ParseResult>> ParseDirectoryAsync(
        string directoryPath, 
        string[]? includePatterns = null, 
        string[]? excludePatterns = null,
        IProgress<(int processed, int total)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var supportedExtensions = _parserFactory.GetSupportedExtensions().ToHashSet();
        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
            .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToList();
        
        // 应用包含/排除模式
        if (includePatterns != null && includePatterns.Length > 0)
        {
            files = files.Where(f => includePatterns.Any(pattern => 
                f.Contains(pattern, StringComparison.OrdinalIgnoreCase))).ToList();
        }
        
        if (excludePatterns != null && excludePatterns.Length > 0)
        {
            files = files.Where(f => !excludePatterns.Any(pattern => 
                f.Contains(pattern, StringComparison.OrdinalIgnoreCase))).ToList();
        }
        
        var results = new List<ParseResult>();
        var processed = 0;
        var total = files.Count;
        
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        var tasks = files.Select(async file =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await ParseFileAsync(file);
                
                lock (results)
                {
                    results.Add(result);
                    processed++;
                    progress?.Report((processed, total));
                }
                
                return result;
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        await Task.WhenAll(tasks);
        
        return results;
    }
    
    public async Task<Dictionary<string, int>> GetLanguageStatisticsAsync(string directoryPath)
    {
        var results = await ParseDirectoryAsync(directoryPath);
        
        return results
            .Where(r => r.Success)
            .GroupBy(r => r.Language)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
```

## 数据库设计

### 1. Entity Framework DbContext

```csharp
// Database/CKGDbContext.cs
public class CKGDbContext : DbContext
{
    public DbSet<LanguageInfo> Languages { get; set; } = null!;
    public DbSet<FunctionEntry> Functions { get; set; } = null!;
    public DbSet<ClassEntry> Classes { get; set; } = null!;
    
    public CKGDbContext(DbContextOptions<CKGDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // LanguageInfo 配置
        modelBuilder.Entity<LanguageInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TreeSitterLibrary).HasMaxLength(100);
            entity.Property(e => e.FileExtensions)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.Property(e => e.Keywords)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.Property(e => e.BuiltinTypes)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.OwnsOne(e => e.Features);
            entity.OwnsOne(e => e.CommentStyle);
            entity.HasIndex(e => e.Name).IsUnique();
        });
        
        // FunctionEntry 配置
        modelBuilder.Entity<FunctionEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Language).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Body).HasColumnType("TEXT");
            entity.Property(e => e.Documentation).HasColumnType("TEXT");
            entity.Property(e => e.Modifiers)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.Property(e => e.GenericParameters)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.Property(e => e.Annotations)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.OwnsMany(e => e.Parameters, p =>
            {
                p.Property(x => x.Name).IsRequired();
                p.Property(x => x.Type).IsRequired();
                p.Property(x => x.Modifiers)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            });
            entity.HasOne(e => e.LanguageInfo)
                .WithMany()
                .HasForeignKey(e => e.Language)
                .HasPrincipalKey(l => l.Name);
            entity.HasIndex(e => new { e.Name, e.FilePath });
            entity.HasIndex(e => e.Language);
        });
        
        // ClassEntry 配置
        modelBuilder.Entity<ClassEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Language).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Body).HasColumnType("TEXT");
            entity.Property(e => e.Documentation).HasColumnType("TEXT");
            entity.Property(e => e.BaseClasses)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.Property(e => e.Interfaces)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.Property(e => e.Modifiers)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.Property(e => e.GenericParameters)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.Property(e => e.Annotations)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.OwnsMany(e => e.Fields, f =>
            {
                f.Property(x => x.Name).IsRequired();
                f.Property(x => x.Type).IsRequired();
                f.Property(x => x.Modifiers)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            });
            entity.HasMany(e => e.Methods)
                .WithOne()
                .HasForeignKey(f => f.ParentClass)
                .HasPrincipalKey(c => c.Name);
            entity.HasOne(e => e.LanguageInfo)
                .WithMany()
                .HasForeignKey(e => e.Language)
                .HasPrincipalKey(l => l.Name);
            entity.HasIndex(e => new { e.Name, e.FilePath });
            entity.HasIndex(e => e.Language);
        });
    }
}

// Database/CKGDatabase.cs
public class CKGDatabase
{
    private readonly CKGDbContext _context;
    private readonly ILogger<CKGDatabase> _logger;
    
    public CKGDatabase(CKGDbContext context, ILogger<CKGDatabase> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        await SeedLanguagesAsync();
    }
    
    private async Task SeedLanguagesAsync()
    {
        if (await _context.Languages.AnyAsync())
            return;
            
        var languages = new[]
        {
            new CSharpLanguageProvider().GetLanguageInfo(),
            new PythonLanguageProvider().GetLanguageInfo(),
            new JavaLanguageProvider().GetLanguageInfo(),
            new JavaScriptLanguageProvider().GetLanguageInfo(),
            new TypeScriptLanguageProvider().GetLanguageInfo(),
            new CLanguageProvider().GetLanguageInfo(),
            new CppLanguageProvider().GetLanguageInfo(),
            new GoLanguageProvider().GetLanguageInfo(),
            new RustLanguageProvider().GetLanguageInfo(),
            new LuaLanguageProvider().GetLanguageInfo()
        };
        
        _context.Languages.AddRange(languages);
        await _context.SaveChangesAsync();
    }
    
    public async Task<int> SaveParseResultAsync(ParseResult result)
    {
        if (!result.Success)
            return 0;
            
        var saved = 0;
        
        // 保存函数
        foreach (var function in result.Functions)
        {
            var existing = await _context.Functions
                .FirstOrDefaultAsync(f => f.Name == function.Name && 
                                        f.FilePath == function.FilePath &&
                                        f.StartLine == function.StartLine);
            
            if (existing == null)
            {
                _context.Functions.Add(function);
                saved++;
            }
            else
            {
                // 更新现有记录
                existing.Body = function.Body;
                existing.UpdatedAt = DateTime.UtcNow;
                saved++;
            }
        }
        
        // 保存类
        foreach (var classEntry in result.Classes)
        {
            var existing = await _context.Classes
                .FirstOrDefaultAsync(c => c.Name == classEntry.Name && 
                                        c.FilePath == classEntry.FilePath &&
                                        c.StartLine == classEntry.StartLine);
            
            if (existing == null)
            {
                _context.Classes.Add(classEntry);
                saved++;
            }
            else
            {
                // 更新现有记录
                existing.Body = classEntry.Body;
                existing.UpdatedAt = DateTime.UtcNow;
                saved++;
            }
        }
        
        await _context.SaveChangesAsync();
        return saved;
    }
    
    public async Task<List<FunctionEntry>> SearchFunctionsAsync(
        string? name = null, 
        string? language = null, 
        string? filePath = null,
        int skip = 0, 
        int take = 100)
    {
        var query = _context.Functions.AsQueryable();
        
        if (!string.IsNullOrEmpty(name))
            query = query.Where(f => f.Name.Contains(name));
            
        if (!string.IsNullOrEmpty(language))
            query = query.Where(f => f.Language == language);
            
        if (!string.IsNullOrEmpty(filePath))
            query = query.Where(f => f.FilePath.Contains(filePath));
            
        return await query
            .OrderBy(f => f.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
    
    public async Task<List<ClassEntry>> SearchClassesAsync(
        string? name = null, 
        string? language = null, 
        string? filePath = null,
        int skip = 0, 
        int take = 100)
    {
        var query = _context.Classes.AsQueryable();
        
        if (!string.IsNullOrEmpty(name))
            query = query.Where(c => c.Name.Contains(name));
            
        if (!string.IsNullOrEmpty(language))
            query = query.Where(c => c.Language == language);
            
        if (!string.IsNullOrEmpty(filePath))
            query = query.Where(c => c.FilePath.Contains(filePath));
            
        return await query
            .OrderBy(c => c.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}
```

## 主要服务实现

### 1. Git 服务

```csharp
// Services/GitService.cs
public class GitService
{
    private readonly ILogger<GitService> _logger;
    
    public GitService(ILogger<GitService> logger)
    {
        _logger = logger;
    }
    
    public string? GetRepositoryRoot(string path)
    {
        try
        {
            return Repository.Discover(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to discover git repository for path: {Path}", path);
            return null;
        }
    }
    
    public string? GetCurrentCommitHash(string repositoryPath)
    {
        try
        {
            using var repo = new Repository(repositoryPath);
            return repo.Head.Tip?.Sha;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get commit hash for repository: {Path}", repositoryPath);
            return null;
        }
    }
    
    public List<string> GetModifiedFiles(string repositoryPath)
    {
        try
        {
            using var repo = new Repository(repositoryPath);
            var status = repo.RetrieveStatus();
            
            return status
                .Where(s => s.State != FileStatus.Unaltered && s.State != FileStatus.Ignored)
                .Select(s => Path.Combine(repo.Info.WorkingDirectory, s.FilePath))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get modified files for repository: {Path}", repositoryPath);
            return new List<string>();
        }
    }
}
```

### 2. 缓存服务

```csharp
// Services/CacheService.cs
public class CacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);
    
    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }
    
    public async Task<T?> GetOrSetAsync<T>(
        string key, 
        Func<Task<T>> factory, 
        TimeSpan? expiration = null) where T : class
    {
        if (_memoryCache.TryGetValue(key, out T? cached))
        {
            return cached;
        }
        
        var value = await factory();
        if (value != null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
            };
            
            _memoryCache.Set(key, value, options);
        }
        
        return value;
    }
    
    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }
    
    public void RemoveByPattern(string pattern)
    {
        // 简单的模式匹配实现
        if (_memoryCache is MemoryCache mc)
        {
            var field = typeof(MemoryCache).GetField("_coherentState", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field?.GetValue(mc) is IDictionary coherentState)
            {
                var keysToRemove = new List<object>();
                foreach (DictionaryEntry entry in coherentState)
                {
                    if (entry.Key.ToString()?.Contains(pattern) == true)
                    {
                        keysToRemove.Add(entry.Key);
                    }
                }
                
                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                }
            }
        }
    }
}
```

## 主工具类实现

```csharp
// CKGTool.cs
public class CKGTool
{
    private readonly ParseResultAggregator _aggregator;
    private readonly CKGDatabase _database;
    private readonly GitService _gitService;
    private readonly CacheService _cacheService;
    private readonly ILogger<CKGTool> _logger;
    
    public CKGTool(
        ParseResultAggregator aggregator,
        CKGDatabase database,
        GitService gitService,
        CacheService cacheService,
        ILogger<CKGTool> logger)
    {
        _aggregator = aggregator;
        _database = database;
        _gitService = gitService;
        _cacheService = cacheService;
        _logger = logger;
    }
    
    public async Task<CKGAnalysisResult> AnalyzeProjectAsync(
        string projectPath,
        CKGAnalysisOptions? options = null,
        IProgress<CKGProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new CKGAnalysisOptions();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // 初始化数据库
            await _database.InitializeAsync();
            
            // 获取 Git 信息
            var gitRoot = _gitService.GetRepositoryRoot(projectPath);
            var commitHash = gitRoot != null ? _gitService.GetCurrentCommitHash(gitRoot) : null;
            
            progress?.Report(new CKGProgress { Stage = "Scanning files", Percentage = 10 });
            
            // 解析项目文件
            var parseProgress = new Progress<(int processed, int total)>(p =>
            {
                var percentage = 10 + (int)(70.0 * p.processed / p.total);
                progress?.Report(new CKGProgress 
                { 
                    Stage = $"Parsing files ({p.processed}/{p.total})", 
                    Percentage = percentage 
                });
            });
            
            var results = await _aggregator.ParseDirectoryAsync(
                projectPath,
                options.IncludePatterns,
                options.ExcludePatterns,
                parseProgress,
                cancellationToken);
            
            progress?.Report(new CKGProgress { Stage = "Saving to database", Percentage = 80 });
            
            // 保存到数据库
            var totalSaved = 0;
            foreach (var result in results.Where(r => r.Success))
            {
                totalSaved += await _database.SaveParseResultAsync(result);
            }
            
            progress?.Report(new CKGProgress { Stage = "Generating statistics", Percentage = 90 });
            
            // 生成统计信息
            var statistics = GenerateStatistics(results);
            
            stopwatch.Stop();
            
            progress?.Report(new CKGProgress { Stage = "Completed", Percentage = 100 });
            
            return new CKGAnalysisResult
            {
                ProjectPath = projectPath,
                CommitHash = commitHash,
                TotalFiles = results.Count,
                SuccessfulFiles = results.Count(r => r.Success),
                FailedFiles = results.Count(r => !r.Success),
                TotalFunctions = results.Sum(r => r.Functions.Count),
                TotalClasses = results.Sum(r => r.Classes.Count),
                TotalSaved = totalSaved,
                Duration = stopwatch.Elapsed,
                Statistics = statistics,
                Results = results
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing project: {ProjectPath}", projectPath);
            throw;
        }
    }
    
    public async Task<List<FunctionEntry>> SearchFunctionsAsync(
        string query,
        string? language = null,
        string? filePath = null)
    {
        var cacheKey = $"search_functions_{query}_{language}_{filePath}";
        
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            return await _database.SearchFunctionsAsync(query, language, filePath);
        }, TimeSpan.FromMinutes(10)) ?? new List<FunctionEntry>();
    }
    
    public async Task<List<ClassEntry>> SearchClassesAsync(
        string query,
        string? language = null,
        string? filePath = null)
    {
        var cacheKey = $"search_classes_{query}_{language}_{filePath}";
        
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            return await _database.SearchClassesAsync(query, language, filePath);
        }, TimeSpan.FromMinutes(10)) ?? new List<ClassEntry>();
    }
    
    private Dictionary<string, object> GenerateStatistics(List<ParseResult> results)
    {
        var successfulResults = results.Where(r => r.Success).ToList();
        
        return new Dictionary<string, object>
        {
            ["LanguageDistribution"] = successfulResults
                .GroupBy(r => r.Language)
                .ToDictionary(g => g.Key, g => g.Count()),
            ["FunctionsByLanguage"] = successfulResults
                .GroupBy(r => r.Language)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Functions.Count)),
            ["ClassesByLanguage"] = successfulResults
                .GroupBy(r => r.Language)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Classes.Count)),
            ["AverageParseTime"] = successfulResults.Average(r => r.ParseDuration.TotalMilliseconds),
            ["TotalLinesOfCode"] = successfulResults.Sum(r => r.LinesOfCode),
            ["LargestFile"] = successfulResults.OrderByDescending(r => r.LinesOfCode).FirstOrDefault()?.FilePath,
            ["MostComplexFile"] = successfulResults
                .OrderByDescending(r => r.Functions.Count + r.Classes.Count)
                .FirstOrDefault()?.FilePath
        };
    }
}

// 辅助类
public class CKGAnalysisOptions
{
    public string[]? IncludePatterns { get; set; }
    public string[]? ExcludePatterns { get; set; } = new[] { "node_modules", ".git", "bin", "obj", "target" };
    public bool UseCache { get; set; } = true;
    public int MaxConcurrency { get; set; } = Environment.ProcessorCount;
}

public class CKGAnalysisResult
{
    public string ProjectPath { get; set; } = string.Empty;
    public string? CommitHash { get; set; }
    public int TotalFiles { get; set; }
    public int SuccessfulFiles { get; set; }
    public int FailedFiles { get; set; }
    public int TotalFunctions { get; set; }
    public int TotalClasses { get; set; }
    public int TotalSaved { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> Statistics { get; set; } = new();
    public List<ParseResult> Results { get; set; } = new();
}

public class CKGProgress
{
    public string Stage { get; set; } = string.Empty;
    public int Percentage { get; set; }
    public string? CurrentFile { get; set; }
 }
 ```

## 使用示例

### 1. 基本使用

```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// 配置服务
builder.Services.AddDbContext<CKGDbContext>(options =>
    options.UseSqlite("Data Source=ckg.db"));

builder.Services.AddMemoryCache();
builder.Services.AddLogging();

// 注册服务
builder.Services.AddSingleton<LanguageDetector>();
builder.Services.AddSingleton<ParserFactory>();
builder.Services.AddSingleton<ParseResultAggregator>();
builder.Services.AddScoped<CKGDatabase>();
builder.Services.AddScoped<GitService>();
builder.Services.AddScoped<CacheService>();
builder.Services.AddScoped<CKGTool>();

// 注册语言提供者
builder.Services.AddSingleton<ILanguageProvider, CSharpLanguageProvider>();
builder.Services.AddSingleton<ILanguageProvider, PythonLanguageProvider>();
builder.Services.AddSingleton<ILanguageProvider, JavaLanguageProvider>();
builder.Services.AddSingleton<ILanguageProvider, JavaScriptLanguageProvider>();
builder.Services.AddSingleton<ILanguageProvider, TypeScriptLanguageProvider>();
builder.Services.AddSingleton<ILanguageProvider, CLanguageProvider>();
builder.Services.AddSingleton<ILanguageProvider, CppLanguageProvider>();
builder.Services.AddSingleton<ILanguageProvider, GoLanguageProvider>();
builder.Services.AddSingleton<ILanguageProvider, RustLanguageProvider>();
builder.Services.AddSingleton<ILanguageProvider, LuaLanguageProvider>();

var app = builder.Build();

// 使用示例
var ckgTool = app.Services.GetRequiredService<CKGTool>();

// 分析项目
var progress = new Progress<CKGProgress>(p => 
    Console.WriteLine($"[{p.Percentage}%] {p.Stage}"));

var result = await ckgTool.AnalyzeProjectAsync(
    "/path/to/your/project",
    new CKGAnalysisOptions
    {
        IncludePatterns = new[] { "*.cs", "*.py", "*.java", "*.js", "*.ts" },
        ExcludePatterns = new[] { "node_modules", ".git", "bin", "obj" }
    },
    progress);

Console.WriteLine($"分析完成: {result.TotalFiles} 个文件, {result.TotalFunctions} 个函数, {result.TotalClasses} 个类");

// 搜索函数
var functions = await ckgTool.SearchFunctionsAsync("Calculate", "C#");
foreach (var func in functions)
{
    Console.WriteLine($"函数: {func.Name} 在 {func.FilePath}:{func.StartLine}");
}

// 搜索类
var classes = await ckgTool.SearchClassesAsync("Service", "C#");
foreach (var cls in classes)
{
    Console.WriteLine($"类: {cls.Name} 在 {cls.FilePath}:{cls.StartLine}");
}
```

### 2. Web API 集成

```csharp
// Controllers/CKGController.cs
[ApiController]
[Route("api/[controller]")]
public class CKGController : ControllerBase
{
    private readonly CKGTool _ckgTool;
    private readonly ILogger<CKGController> _logger;
    
    public CKGController(CKGTool ckgTool, ILogger<CKGController> logger)
    {
        _ckgTool = ckgTool;
        _logger = logger;
    }
    
    [HttpPost("analyze")]
    public async Task<ActionResult<CKGAnalysisResult>> AnalyzeProject(
        [FromBody] AnalyzeProjectRequest request)
    {
        try
        {
            var result = await _ckgTool.AnalyzeProjectAsync(
                request.ProjectPath,
                new CKGAnalysisOptions
                {
                    IncludePatterns = request.IncludePatterns,
                    ExcludePatterns = request.ExcludePatterns
                });
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing project: {ProjectPath}", request.ProjectPath);
            return BadRequest($"分析失败: {ex.Message}");
        }
    }
    
    [HttpGet("functions/search")]
    public async Task<ActionResult<List<FunctionEntry>>> SearchFunctions(
        [FromQuery] string query,
        [FromQuery] string? language = null,
        [FromQuery] string? filePath = null)
    {
        var functions = await _ckgTool.SearchFunctionsAsync(query, language, filePath);
        return Ok(functions);
    }
    
    [HttpGet("classes/search")]
    public async Task<ActionResult<List<ClassEntry>>> SearchClasses(
        [FromQuery] string query,
        [FromQuery] string? language = null,
        [FromQuery] string? filePath = null)
    {
        var classes = await _ckgTool.SearchClassesAsync(query, language, filePath);
        return Ok(classes);
    }
}

public class AnalyzeProjectRequest
{
    public string ProjectPath { get; set; } = string.Empty;
    public string[]? IncludePatterns { get; set; }
    public string[]? ExcludePatterns { get; set; }
}
```

## 项目配置

### 1. 项目文件 (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>

  <ItemGroup>
    <!-- Tree-sitter 官方 C# 绑定 -->
    <PackageReference Include="TreeSitter" Version="0.21.0" />
    
    <!-- 数据库相关 -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
    
    <!-- Git 集成 -->
    <PackageReference Include="LibGit2Sharp" Version="0.29.0" />
    
    <!-- C# 语法分析 -->
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" />
    
    <!-- 日志和依赖注入 -->
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="8.0.0" />
    
    <!-- Web API (可选) -->
    <PackageReference Include="Microsoft.AspNetCore.App" Version="8.0.0" />
  </ItemGroup>

  <!-- Tree-sitter 原生库 -->
  <ItemGroup>
    <Content Include="runtimes/win-x64/native/tree-sitter.dll" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="runtimes/linux-x64/native/libtree-sitter.so" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="runtimes/osx-x64/native/libtree-sitter.dylib" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="runtimes/osx-arm64/native/libtree-sitter.dylib" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

  <!-- 语言语法库 -->
  <ItemGroup>
    <Content Include="grammars/tree-sitter-c.dll" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="grammars/tree-sitter-cpp.dll" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="grammars/tree-sitter-csharp.dll" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="grammars/tree-sitter-java.dll" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="grammars/tree-sitter-javascript.dll" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="grammars/tree-sitter-typescript.dll" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="grammars/tree-sitter-python.dll" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="grammars/tree-sitter-go.dll" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="grammars/tree-sitter-rust.dll" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="grammars/tree-sitter-lua.dll" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

</Project>
```

### 2. 构建脚本 (build.ps1)

```powershell
#!/usr/bin/env pwsh

# CKG 工具构建脚本
param(
    [string]$Configuration = "Release",
    [string]$Platform = "Any CPU",
    [switch]$SkipTests,
    [switch]$BuildNative
)

Write-Host "开始构建 CKG 工具..." -ForegroundColor Green

# 清理输出目录
if (Test-Path "bin") {
    Remove-Item -Recurse -Force "bin"
}
if (Test-Path "obj") {
    Remove-Item -Recurse -Force "obj"
}

# 构建原生库 (如果需要)
if ($BuildNative) {
    Write-Host "构建 Tree-sitter 原生库..." -ForegroundColor Yellow
    
    # 下载并编译 Tree-sitter 核心库
    if (!(Test-Path "tree-sitter")) {
        git clone https://github.com/tree-sitter/tree-sitter.git
    }
    
    Push-Location "tree-sitter"
    make
    Pop-Location
    
    # 编译各语言语法库
    $languages = @("c", "cpp", "csharp", "java", "javascript", "typescript", "python", "go", "rust", "lua")
    
    foreach ($lang in $languages) {
        Write-Host "编译 $lang 语法库..." -ForegroundColor Yellow
        
        if (!(Test-Path "tree-sitter-$lang")) {
            git clone "https://github.com/tree-sitter/tree-sitter-$lang.git"
        }
        
        Push-Location "tree-sitter-$lang"
        
        # 编译为动态库
        if ($IsWindows) {
            gcc -shared -fPIC -o "../grammars/tree-sitter-$lang.dll" src/parser.c
        } elseif ($IsLinux) {
            gcc -shared -fPIC -o "../grammars/libtree-sitter-$lang.so" src/parser.c
        } elseif ($IsMacOS) {
            gcc -shared -fPIC -o "../grammars/libtree-sitter-$lang.dylib" src/parser.c
        }
        
        Pop-Location
    }
}

# 恢复 NuGet 包
Write-Host "恢复 NuGet 包..." -ForegroundColor Yellow
dotnet restore

# 构建项目
Write-Host "构建项目..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "构建失败!"
    exit 1
}

# 运行测试 (如果不跳过)
if (!$SkipTests) {
    Write-Host "运行测试..." -ForegroundColor Yellow
    dotnet test --configuration $Configuration --no-build --verbosity normal
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "测试失败!"
        exit 1
    }
}

# 打包
Write-Host "创建 NuGet 包..." -ForegroundColor Yellow
dotnet pack --configuration $Configuration --no-build --output "./artifacts"

Write-Host "构建完成!" -ForegroundColor Green
Write-Host "输出目录: ./artifacts" -ForegroundColor Cyan
```

## 性能优化策略

### 1. 并行处理

```csharp
// Services/ParallelParseService.cs
public class ParallelParseService
{
    private readonly ParserFactory _parserFactory;
    private readonly ILogger<ParallelParseService> _logger;
    private readonly SemaphoreSlim _semaphore;
    
    public ParallelParseService(
        ParserFactory parserFactory, 
        ILogger<ParallelParseService> logger,
        int maxConcurrency = 0)
    {
        _parserFactory = parserFactory;
        _logger = logger;
        _semaphore = new SemaphoreSlim(maxConcurrency > 0 ? maxConcurrency : Environment.ProcessorCount);
    }
    
    public async Task<List<ParseResult>> ParseFilesAsync(
        IEnumerable<string> filePaths,
        IProgress<(int processed, int total)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var files = filePaths.ToList();
        var results = new ConcurrentBag<ParseResult>();
        var processed = 0;
        
        var tasks = files.Select(async filePath =>
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await ParseFileAsync(filePath, cancellationToken);
                results.Add(result);
                
                var currentProcessed = Interlocked.Increment(ref processed);
                progress?.Report((currentProcessed, files.Count));
            }
            finally
            {
                _semaphore.Release();
            }
        });
        
        await Task.WhenAll(tasks);
        return results.ToList();
    }
    
    private async Task<ParseResult> ParseFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var parser = await _parserFactory.CreateParserAsync(filePath);
            if (parser == null)
            {
                return ParseResult.Failed(filePath, "不支持的文件类型");
            }
            
            return await parser.ParseFileAsync(filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析文件失败: {FilePath}", filePath);
            return ParseResult.Failed(filePath, ex.Message);
        }
    }
}
```

### 2. 增量更新

```csharp
// Services/IncrementalUpdateService.cs
public class IncrementalUpdateService
{
    private readonly CKGDatabase _database;
    private readonly GitService _gitService;
    private readonly ParallelParseService _parseService;
    private readonly ILogger<IncrementalUpdateService> _logger;
    
    public IncrementalUpdateService(
        CKGDatabase database,
        GitService gitService,
        ParallelParseService parseService,
        ILogger<IncrementalUpdateService> logger)
    {
        _database = database;
        _gitService = gitService;
        _parseService = parseService;
        _logger = logger;
    }
    
    public async Task<IncrementalUpdateResult> UpdateProjectAsync(
        string projectPath,
        string? lastCommitHash = null,
        IProgress<CKGProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var gitRoot = _gitService.GetRepositoryRoot(projectPath);
        if (gitRoot == null)
        {
            throw new InvalidOperationException("项目不在 Git 仓库中");
        }
        
        var currentCommit = _gitService.GetCurrentCommitHash(gitRoot);
        if (currentCommit == lastCommitHash)
        {
            return new IncrementalUpdateResult
            {
                IsUpToDate = true,
                CommitHash = currentCommit
            };
        }
        
        progress?.Report(new CKGProgress { Stage = "检测修改的文件", Percentage = 10 });
        
        // 获取修改的文件
        var modifiedFiles = _gitService.GetModifiedFiles(gitRoot)
            .Where(f => _parseService.IsSupported(f))
            .ToList();
        
        if (!modifiedFiles.Any())
        {
            return new IncrementalUpdateResult
            {
                IsUpToDate = true,
                CommitHash = currentCommit
            };
        }
        
        progress?.Report(new CKGProgress { Stage = "解析修改的文件", Percentage = 30 });
        
        // 解析修改的文件
        var parseProgress = new Progress<(int processed, int total)>(p =>
        {
            var percentage = 30 + (int)(50.0 * p.processed / p.total);
            progress?.Report(new CKGProgress
            {
                Stage = $"解析文件 ({p.processed}/{p.total})",
                Percentage = percentage
            });
        });
        
        var results = await _parseService.ParseFilesAsync(
            modifiedFiles,
            parseProgress,
            cancellationToken);
        
        progress?.Report(new CKGProgress { Stage = "更新数据库", Percentage = 80 });
        
        // 删除旧记录并插入新记录
        var totalUpdated = 0;
        foreach (var result in results.Where(r => r.Success))
        {
            await _database.RemoveFileEntriesAsync(result.FilePath);
            totalUpdated += await _database.SaveParseResultAsync(result);
        }
        
        progress?.Report(new CKGProgress { Stage = "完成", Percentage = 100 });
        
        return new IncrementalUpdateResult
        {
            IsUpToDate = false,
            CommitHash = currentCommit,
            ModifiedFiles = modifiedFiles.Count,
            UpdatedEntries = totalUpdated
        };
    }
}

public class IncrementalUpdateResult
{
    public bool IsUpToDate { get; set; }
    public string? CommitHash { get; set; }
    public int ModifiedFiles { get; set; }
    public int UpdatedEntries { get; set; }
}
```

## 实施计划

### 阶段 1: 核心架构 (2-3 周)

1. **Week 1**: 基础架构设计
   - 实现核心接口和抽象类
   - 集成 `csharp-tree-sitter` 库
   - 实现语言检测器和解析器工厂
   - 基础数据模型设计

2. **Week 2**: 数据库集成
   - EF Core DbContext 实现
   - 数据库迁移和种子数据
   - 基础 CRUD 操作
   - 缓存服务实现

3. **Week 3**: Git 集成和测试
   - LibGit2Sharp 集成
   - 版本控制功能
   - 单元测试框架搭建
   - 集成测试

### 阶段 2: 基础语言支持 (3-4 周)

1. **Week 4**: C# 和 Java 解析器
   - C# 解析器实现 (使用 Roslyn + Tree-sitter)
   - Java 解析器实现
   - Tree-sitter 查询优化
   - 性能测试

2. **Week 5**: C/C++ 解析器
   - C 语言解析器实现
   - C++ 解析器实现
   - 复杂语法处理 (模板、宏等)
   - 错误处理优化

3. **Week 6**: 脚本语言支持
   - Python 解析器实现
   - JavaScript/TypeScript 解析器
   - 动态特性处理
   - 模块系统支持

4. **Week 7**: 现代语言支持
   - Go 解析器实现
   - Rust 解析器实现
   - Lua 解析器实现
   - 语言特性完善

### 阶段 3: 高级功能 (2-3 周)

1. **Week 8**: 性能优化
   - 并行处理实现
   - 内存优化
   - 缓存策略优化
   - 性能基准测试

2. **Week 9**: 增量更新
   - Git 差异检测
   - 增量解析逻辑
   - 数据库增量更新
   - 冲突解决机制

3. **Week 10**: Web API 和工具
   - RESTful API 实现
   - 命令行工具
   - 配置管理
   - 文档生成

### 阶段 4: 测试和部署 (1-2 周)

1. **Week 11**: 全面测试
   - 单元测试覆盖率 > 90%
   - 集成测试
   - 性能测试
   - 压力测试

2. **Week 12**: 部署和文档
   - Docker 容器化
   - CI/CD 流水线
   - 用户文档
   - API 文档

## 技术风险和缓解策略

### 1. Tree-sitter 语法更新风险

**风险**: 各语言的 Tree-sitter 语法可能更新，导致查询失效

**缓解策略**:
- 版本锁定: 固定使用稳定版本的语法库
- 自动化测试: 建立语法兼容性测试套件
- 降级机制: 支持多版本语法库并行
- 监控告警: 监控语法库更新并及时适配

### 2. 原生库依赖风险

**风险**: 跨平台原生库编译和分发复杂

**缓解策略**:
- 预编译库: 提供主流平台的预编译库
- 容器化部署: 使用 Docker 统一运行环境
- 云端编译: 建立自动化编译流水线
- 备选方案: 准备纯 C# 实现的备选解析器

### 3. 性能瓶颈风险

**风险**: 大型项目解析性能不足

**缓解策略**:
- 并行处理: 多线程并行解析
- 增量更新: 只解析修改的文件
- 智能缓存: 多层缓存策略
- 资源限制: 可配置的资源使用限制

### 4. 内存泄漏风险

**风险**: Tree-sitter 原生库可能存在内存泄漏

**缓解策略**:
- 资源管理: 严格的 IDisposable 实现
- 内存监控: 实时内存使用监控
- 定期重启: 长时间运行时的定期重启机制
- 内存池: 对象池和内存池优化

## 预期收益

### 技术收益

1. **多语言统一**: 支持 9 种主流编程语言的统一解析
2. **高性能**: 基于 Tree-sitter 的高效解析引擎
3. **可扩展**: 模块化设计，易于添加新语言支持
4. **跨平台**: 支持 Windows、Linux、macOS
5. **现代化**: 基于 .NET 8 和最新技术栈

### 业务收益

1. **市场竞争力**: 相比现有工具支持更多语言
2. **用户体验**: 统一的 API 和一致的使用体验
3. **开发效率**: 显著提升代码分析和搜索效率
4. **成本节约**: 减少多工具维护成本
5. **生态建设**: 为代码智能分析生态奠定基础

### 长期价值

1. **技术积累**: 建立代码解析和分析的核心技术能力
2. **平台基础**: 为 AI 辅助编程提供数据基础
3. **商业化**: 具备商业化产品的技术基础
4. **开源贡献**: 可以回馈开源社区，提升技术影响力

---

**总结**: 本方案基于官方 `csharp-tree-sitter` 库，整合了多语言代码解析、知识图谱构建和智能搜索功能，支持 9 种主流编程语言，采用现代化的 .NET 8 技术栈，具有高性能、可扩展、跨平台的特点。通过 4 个阶段 12 周的实施计划，可以构建出一个功能完整、性能优异的代码知识图谱工具，为代码分析和智能编程提供强大的技术支撑。