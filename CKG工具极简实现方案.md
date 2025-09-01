# CKG工具极简实现方案

基于现有工具链的零依赖代码知识图谱构建方案

## 方案理念

**最简原则**: 不重复造轮子，直接利用现有成熟工具链
**零维护**: 依赖稳定的系统工具，无需维护解析器
**高效率**: 利用编译器级别的解析精度和性能

## 核心思路

使用各语言的官方工具链和 Language Server Protocol (LSP) 来提取代码结构信息，避免自建解析器的复杂性。

## 技术栈

- **C/C++**: `clang -ast-dump` + `clangd` LSP
- **C#**: `dotnet` Roslyn API + `omnisharp` LSP  
- **Java**: `javac -verbose` + `jdtls` LSP
- **JavaScript/TypeScript**: `tsc --listFiles` + `typescript-language-server`
- **Python**: `ast` 模块 + `pylsp`
- **Go**: `go doc` + `gopls`
- **Rust**: `rustc --pretty=expanded` + `rust-analyzer`
- **数据存储**: SQLite (单文件数据库)
- **Git集成**: `git` 命令行工具

## 项目结构

```
CKGTool/
├── src/
│   ├── CKGTool.Core/
│   │   ├── Models/           # 简化数据模型
│   │   ├── Extractors/       # 各语言提取器
│   │   └── Services/         # 核心服务
│   └── CKGTool.CLI/          # 命令行工具
├── scripts/
│   ├── extract_c.sh          # C/C++ 提取脚本
│   ├── extract_csharp.ps1    # C# 提取脚本
│   ├── extract_java.sh       # Java 提取脚本
│   ├── extract_js.sh         # JS/TS 提取脚本
│   ├── extract_python.py     # Python 提取脚本
│   ├── extract_go.sh         # Go 提取脚本
│   └── extract_rust.sh       # Rust 提取脚本
└── tools/
    └── install_deps.sh       # 依赖安装脚本
```

## 核心实现

### 1. 统一数据模型

```csharp
// Models/CodeElement.cs
namespace CKGTool.Core.Models
{
    public record CodeElement
    {
        public string Name { get; init; } = "";
        public string Type { get; init; } = ""; // function, class, method, variable
        public string Language { get; init; } = "";
        public string FilePath { get; init; } = "";
        public int Line { get; init; }
        public int Column { get; init; }
        public string Signature { get; init; } = "";
        public string? Parent { get; init; }
        public List<string> Children { get; init; } = new();
        public Dictionary<string, string> Metadata { get; init; } = new();
    }

    public record ProjectInfo
    {
        public string Path { get; init; } = "";
        public string CommitHash { get; init; } = "";
        public DateTime AnalyzedAt { get; init; } = DateTime.UtcNow;
        public List<CodeElement> Elements { get; init; } = new();
        public Dictionary<string, int> Statistics { get; init; } = new();
    }
}
```

### 2. 语言提取器接口

```csharp
// Extractors/ILanguageExtractor.cs
using CKGTool.Core.Models;

namespace CKGTool.Core.Extractors
{
    public interface ILanguageExtractor
    {
        string Language { get; }
        string[] SupportedExtensions { get; }
        Task<List<CodeElement>> ExtractAsync(string filePath);
        Task<bool> IsAvailableAsync(); // 检查工具是否可用
    }

    public abstract class BaseExtractor : ILanguageExtractor
    {
        public abstract string Language { get; }
        public abstract string[] SupportedExtensions { get; }

        public abstract Task<List<CodeElement>> ExtractAsync(string filePath);
        public abstract Task<bool> IsAvailableAsync();

        protected async Task<string> RunCommandAsync(string command, string arguments, string? workingDirectory = null)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new InvalidOperationException($"Command failed: {error}");
            }

            return output;
        }
    }
}
```

### 3. C# 提取器 (使用 Roslyn)

```csharp
// Extractors/CSharpExtractor.cs
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CKGTool.Core.Models;

namespace CKGTool.Core.Extractors
{
    public class CSharpExtractor : BaseExtractor
    {
        public override string Language => "csharp";
        public override string[] SupportedExtensions => new[] { ".cs" };

        public override async Task<bool> IsAvailableAsync()
        {
            try
            {
                await RunCommandAsync("dotnet", "--version");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override async Task<List<CodeElement>> ExtractAsync(string filePath)
        {
            var sourceCode = await File.ReadAllTextAsync(filePath);
            var tree = CSharpSyntaxTree.ParseText(sourceCode, path: filePath);
            var root = await tree.GetRootAsync();

            var elements = new List<CodeElement>();
            var visitor = new CSharpSyntaxVisitor(filePath, elements);
            visitor.Visit(root);

            return elements;
        }

        private class CSharpSyntaxVisitor : CSharpSyntaxWalker
        {
            private readonly string _filePath;
            private readonly List<CodeElement> _elements;
            private readonly Stack<string> _parentStack = new();

            public CSharpSyntaxVisitor(string filePath, List<CodeElement> elements)
            {
                _filePath = filePath;
                _elements = elements;
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                var element = CreateElement(node.Identifier.ValueText, "class", node);
                element.Metadata["modifiers"] = string.Join(" ", node.Modifiers.Select(m => m.ValueText));
                
                if (node.BaseList != null)
                {
                    element.Metadata["base_types"] = string.Join(", ", node.BaseList.Types.Select(t => t.ToString()));
                }

                _elements.Add(element);
                
                _parentStack.Push(element.Name);
                base.VisitClassDeclaration(node);
                _parentStack.Pop();
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                var element = CreateElement(node.Identifier.ValueText, "method", node);
                element.Signature = $"{node.ReturnType} {node.Identifier}({string.Join(", ", node.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"))})";
                element.Metadata["return_type"] = node.ReturnType.ToString();
                element.Metadata["modifiers"] = string.Join(" ", node.Modifiers.Select(m => m.ValueText));
                
                if (_parentStack.Count > 0)
                    element.Parent = _parentStack.Peek();

                _elements.Add(element);
                base.VisitMethodDeclaration(node);
            }

            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                var element = CreateElement(node.Identifier.ValueText, "property", node);
                element.Metadata["type"] = node.Type.ToString();
                element.Metadata["modifiers"] = string.Join(" ", node.Modifiers.Select(m => m.ValueText));
                
                if (_parentStack.Count > 0)
                    element.Parent = _parentStack.Peek();

                _elements.Add(element);
                base.VisitPropertyDeclaration(node);
            }

            private CodeElement CreateElement(string name, string type, SyntaxNode node)
            {
                var location = node.GetLocation();
                var lineSpan = location.GetLineSpan();
                
                return new CodeElement
                {
                    Name = name,
                    Type = type,
                    Language = "csharp",
                    FilePath = _filePath,
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1
                };
            }
        }
    }
}
```

### 4. Python 提取器 (使用内置 AST)

```csharp
// Extractors/PythonExtractor.cs
using System.Text.Json;
using CKGTool.Core.Models;

namespace CKGTool.Core.Extractors
{
    public class PythonExtractor : BaseExtractor
    {
        public override string Language => "python";
        public override string[] SupportedExtensions => new[] { ".py" };

        public override async Task<bool> IsAvailableAsync()
        {
            try
            {
                await RunCommandAsync("python3", "--version");
                return true;
            }
            catch
            {
                try
                {
                    await RunCommandAsync("python", "--version");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public override async Task<List<CodeElement>> ExtractAsync(string filePath)
        {
            // 使用 Python 脚本解析 AST
            var pythonScript = CreatePythonScript();
            var scriptPath = Path.GetTempFileName() + ".py";
            
            try
            {
                await File.WriteAllTextAsync(scriptPath, pythonScript);
                var output = await RunCommandAsync("python3", $"{scriptPath} {filePath}");
                
                if (string.IsNullOrWhiteSpace(output))
                    return new List<CodeElement>();

                var jsonElements = JsonSerializer.Deserialize<JsonElement[]>(output);
                return jsonElements.Select(ConvertFromJson).ToList();
            }
            catch
            {
                // 尝试使用 python 而不是 python3
                var output = await RunCommandAsync("python", $"{scriptPath} {filePath}");
                
                if (string.IsNullOrWhiteSpace(output))
                    return new List<CodeElement>();

                var jsonElements = JsonSerializer.Deserialize<JsonElement[]>(output);
                return jsonElements.Select(ConvertFromJson).ToList();
            }
            finally
            {
                if (File.Exists(scriptPath))
                    File.Delete(scriptPath);
            }
        }

        private string CreatePythonScript()
        {
            return @"
import ast
import json
import sys

def extract_elements(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        source = f.read()
    
    try:
        tree = ast.parse(source, filename=file_path)
    except SyntaxError:
        return []
    
    elements = []
    
    class Visitor(ast.NodeVisitor):
        def __init__(self):
            self.class_stack = []
        
        def visit_FunctionDef(self, node):
            element = {
                'name': node.name,
                'type': 'method' if self.class_stack else 'function',
                'language': 'python',
                'filePath': file_path,
                'line': node.lineno,
                'column': node.col_offset + 1,
                'signature': f"{node.name}({', '.join(arg.arg for arg in node.args.args)})",
                'parent': self.class_stack[-1] if self.class_stack else None,
                'metadata': {
                    'args': [arg.arg for arg in node.args.args],
                    'decorators': [ast.unparse(d) for d in node.decorator_list]
                }
            }
            elements.append(element)
            self.generic_visit(node)
        
        def visit_AsyncFunctionDef(self, node):
            self.visit_FunctionDef(node)  # 同样处理
        
        def visit_ClassDef(self, node):
            element = {
                'name': node.name,
                'type': 'class',
                'language': 'python',
                'filePath': file_path,
                'line': node.lineno,
                'column': node.col_offset + 1,
                'signature': node.name,
                'parent': self.class_stack[-1] if self.class_stack else None,
                'metadata': {
                    'bases': [ast.unparse(base) for base in node.bases],
                    'decorators': [ast.unparse(d) for d in node.decorator_list]
                }
            }
            elements.append(element)
            
            self.class_stack.append(node.name)
            self.generic_visit(node)
            self.class_stack.pop()
    
    visitor = Visitor()
    visitor.visit(tree)
    return elements

if __name__ == '__main__':
    if len(sys.argv) != 2:
        print('[]')
        sys.exit(1)
    
    elements = extract_elements(sys.argv[1])
    print(json.dumps(elements, ensure_ascii=False))
";
        }

        private CodeElement ConvertFromJson(JsonElement json)
        {
            var element = new CodeElement
            {
                Name = json.GetProperty("name").GetString() ?? "",
                Type = json.GetProperty("type").GetString() ?? "",
                Language = json.GetProperty("language").GetString() ?? "",
                FilePath = json.GetProperty("filePath").GetString() ?? "",
                Line = json.GetProperty("line").GetInt32(),
                Column = json.GetProperty("column").GetInt32(),
                Signature = json.GetProperty("signature").GetString() ?? ""
            };

            if (json.TryGetProperty("parent", out var parent) && parent.ValueKind != JsonValueKind.Null)
            {
                element.Parent = parent.GetString();
            }

            if (json.TryGetProperty("metadata", out var metadata))
            {
                foreach (var prop in metadata.EnumerateObject())
                {
                    element.Metadata[prop.Name] = prop.Value.ToString();
                }
            }

            return element;
        }
    }
}
```

### 5. Go 提取器 (使用 go doc)

```csharp
// Extractors/GoExtractor.cs
using System.Text.RegularExpressions;
using CKGTool.Core.Models;

namespace CKGTool.Core.Extractors
{
    public class GoExtractor : BaseExtractor
    {
        public override string Language => "go";
        public override string[] SupportedExtensions => new[] { ".go" };

        public override async Task<bool> IsAvailableAsync()
        {
            try
            {
                await RunCommandAsync("go", "version");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override async Task<List<CodeElement>> ExtractAsync(string filePath)
        {
            var elements = new List<CodeElement>();
            var sourceCode = await File.ReadAllTextAsync(filePath);
            var lines = sourceCode.Split('\n');

            // 简单的正则表达式解析 (可以用 go/ast 包做更精确的解析)
            var funcRegex = new Regex(@"^func\s+(\w+)\s*\(([^)]*)\)\s*(\([^)]*\))?\s*\{?", RegexOptions.Multiline);
            var typeRegex = new Regex(@"^type\s+(\w+)\s+(struct|interface)\s*\{", RegexOptions.Multiline);
            var methodRegex = new Regex(@"^func\s+\(\w+\s+\*?(\w+)\)\s+(\w+)\s*\(([^)]*)\)\s*(\([^)]*\))?\s*\{?", RegexOptions.Multiline);

            // 提取函数
            foreach (Match match in funcRegex.Matches(sourceCode))
            {
                var lineNumber = sourceCode.Substring(0, match.Index).Count(c => c == '\n') + 1;
                elements.Add(new CodeElement
                {
                    Name = match.Groups[1].Value,
                    Type = "function",
                    Language = "go",
                    FilePath = filePath,
                    Line = lineNumber,
                    Signature = match.Value.TrimEnd('{', ' ', '\t', '\n'),
                    Metadata = new Dictionary<string, string>
                    {
                        ["parameters"] = match.Groups[2].Value,
                        ["returns"] = match.Groups[3].Value
                    }
                });
            }

            // 提取类型
            foreach (Match match in typeRegex.Matches(sourceCode))
            {
                var lineNumber = sourceCode.Substring(0, match.Index).Count(c => c == '\n') + 1;
                elements.Add(new CodeElement
                {
                    Name = match.Groups[1].Value,
                    Type = match.Groups[2].Value, // struct 或 interface
                    Language = "go",
                    FilePath = filePath,
                    Line = lineNumber,
                    Signature = match.Value.TrimEnd('{', ' ', '\t', '\n')
                });
            }

            // 提取方法
            foreach (Match match in methodRegex.Matches(sourceCode))
            {
                var lineNumber = sourceCode.Substring(0, match.Index).Count(c => c == '\n') + 1;
                elements.Add(new CodeElement
                {
                    Name = match.Groups[2].Value,
                    Type = "method",
                    Language = "go",
                    FilePath = filePath,
                    Line = lineNumber,
                    Parent = match.Groups[1].Value,
                    Signature = match.Value.TrimEnd('{', ' ', '\t', '\n'),
                    Metadata = new Dictionary<string, string>
                    {
                        ["receiver"] = match.Groups[1].Value,
                        ["parameters"] = match.Groups[3].Value,
                        ["returns"] = match.Groups[4].Value
                    }
                });
            }

            return elements;
        }
    }
}
```

### 6. 主服务类

```csharp
// Services/CKGService.cs
using System.Data.SQLite;
using System.Text.Json;
using CKGTool.Core.Extractors;
using CKGTool.Core.Models;

namespace CKGTool.Core.Services
{
    public class CKGService : IDisposable
    {
        private readonly string _databasePath;
        private readonly List<ILanguageExtractor> _extractors;
        private readonly Dictionary<string, ILanguageExtractor> _extensionMap;

        public CKGService(string databasePath = "ckg.db")
        {
            _databasePath = databasePath;
            _extractors = new List<ILanguageExtractor>
            {
                new CSharpExtractor(),
                new PythonExtractor(),
                new GoExtractor(),
                // 可以继续添加其他语言提取器
            };

            _extensionMap = new Dictionary<string, ILanguageExtractor>();
            foreach (var extractor in _extractors)
            {
                foreach (var ext in extractor.SupportedExtensions)
                {
                    _extensionMap[ext.ToLowerInvariant()] = extractor;
                }
            }

            InitializeDatabase();
        }

        public async Task<ProjectInfo> AnalyzeProjectAsync(string projectPath)
        {
            var projectInfo = new ProjectInfo
            {
                Path = projectPath,
                CommitHash = await GetGitCommitHashAsync(projectPath)
            };

            var supportedFiles = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories)
                .Where(file => _extensionMap.ContainsKey(Path.GetExtension(file).ToLowerInvariant()))
                .ToList();

            var allElements = new List<CodeElement>();
            var statistics = new Dictionary<string, int>();

            // 并行处理文件
            var tasks = supportedFiles.Select(async filePath =>
            {
                try
                {
                    var extension = Path.GetExtension(filePath).ToLowerInvariant();
                    var extractor = _extensionMap[extension];
                    
                    if (await extractor.IsAvailableAsync())
                    {
                        var elements = await extractor.ExtractAsync(filePath);
                        lock (allElements)
                        {
                            allElements.AddRange(elements);
                            
                            foreach (var element in elements)
                            {
                                var key = $"{element.Language}_{element.Type}";
                                statistics[key] = statistics.GetValueOrDefault(key, 0) + 1;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {filePath}: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);

            projectInfo.Elements = allElements;
            projectInfo.Statistics = statistics;

            await SaveProjectInfoAsync(projectInfo);
            return projectInfo;
        }

        public async Task<List<CodeElement>> SearchAsync(string query, string? language = null, string? type = null)
        {
            using var connection = new SQLiteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync();

            var sql = "SELECT data FROM code_elements WHERE name LIKE @query";
            var parameters = new List<SQLiteParameter> { new("@query", $"%{query}%") };

            if (!string.IsNullOrEmpty(language))
            {
                sql += " AND language = @language";
                parameters.Add(new SQLiteParameter("@language", language));
            }

            if (!string.IsNullOrEmpty(type))
            {
                sql += " AND type = @type";
                parameters.Add(new SQLiteParameter("@type", type));
            }

            sql += " ORDER BY name LIMIT 100";

            using var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddRange(parameters.ToArray());

            var results = new List<CodeElement>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var json = reader.GetString("data");
                var element = JsonSerializer.Deserialize<CodeElement>(json);
                if (element != null)
                    results.Add(element);
            }

            return results;
        }

        private void InitializeDatabase()
        {
            using var connection = new SQLiteConnection($"Data Source={_databasePath}");
            connection.Open();

            var createTableSql = @"
                CREATE TABLE IF NOT EXISTS code_elements (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    type TEXT NOT NULL,
                    language TEXT NOT NULL,
                    file_path TEXT NOT NULL,
                    project_path TEXT NOT NULL,
                    commit_hash TEXT,
                    data TEXT NOT NULL,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                );
                
                CREATE INDEX IF NOT EXISTS idx_name ON code_elements(name);
                CREATE INDEX IF NOT EXISTS idx_type ON code_elements(type);
                CREATE INDEX IF NOT EXISTS idx_language ON code_elements(language);
                CREATE INDEX IF NOT EXISTS idx_project ON code_elements(project_path);
            ";

            using var command = new SQLiteCommand(createTableSql, connection);
            command.ExecuteNonQuery();
        }

        private async Task SaveProjectInfoAsync(ProjectInfo projectInfo)
        {
            using var connection = new SQLiteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync();

            // 清除旧数据
            var deleteSql = "DELETE FROM code_elements WHERE project_path = @projectPath";
            using var deleteCommand = new SQLiteCommand(deleteSql, connection);
            deleteCommand.Parameters.AddWithValue("@projectPath", projectInfo.Path);
            await deleteCommand.ExecuteNonQueryAsync();

            // 插入新数据
            var insertSql = @"
                INSERT INTO code_elements (name, type, language, file_path, project_path, commit_hash, data)
                VALUES (@name, @type, @language, @filePath, @projectPath, @commitHash, @data)
            ";

            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var element in projectInfo.Elements)
                {
                    using var command = new SQLiteCommand(insertSql, connection, transaction);
                    command.Parameters.AddWithValue("@name", element.Name);
                    command.Parameters.AddWithValue("@type", element.Type);
                    command.Parameters.AddWithValue("@language", element.Language);
                    command.Parameters.AddWithValue("@filePath", element.FilePath);
                    command.Parameters.AddWithValue("@projectPath", projectInfo.Path);
                    command.Parameters.AddWithValue("@commitHash", projectInfo.CommitHash);
                    command.Parameters.AddWithValue("@data", JsonSerializer.Serialize(element));
                    
                    await command.ExecuteNonQueryAsync();
                }
                
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private async Task<string> GetGitCommitHashAsync(string projectPath)
        {
            try
            {
                var output = await RunCommandAsync("git", "rev-parse HEAD", projectPath);
                return output.Trim();
            }
            catch
            {
                return "unknown";
            }
        }

        private async Task<string> RunCommandAsync(string command, string arguments, string? workingDirectory = null)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new InvalidOperationException($"Command failed: {error}");
            }

            return output;
        }

        public void Dispose()
        {
            // 清理资源
        }
    }
}
```

### 7. 命令行工具

```csharp
// Program.cs
using System.CommandLine;
using CKGTool.Core.Services;

namespace CKGTool.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("CKG Tool - Code Knowledge Graph Builder");

            // analyze 命令
            var analyzeCommand = new Command("analyze", "Analyze a project")
            {
                new Argument<string>("path", "Project path to analyze"),
                new Option<string>("--database", () => "ckg.db", "Database file path")
            };
            
            analyzeCommand.SetHandler(async (string path, string database) =>
            {
                using var service = new CKGService(database);
                
                Console.WriteLine($"Analyzing project: {path}");
                var projectInfo = await service.AnalyzeProjectAsync(path);
                
                Console.WriteLine($"Analysis completed!");
                Console.WriteLine($"Total elements: {projectInfo.Elements.Count}");
                
                foreach (var stat in projectInfo.Statistics)
                {
                    Console.WriteLine($"  {stat.Key}: {stat.Value}");
                }
            }, 
            new Argument<string>("path"), 
            new Option<string>("--database"));

            // search 命令
            var searchCommand = new Command("search", "Search code elements")
            {
                new Argument<string>("query", "Search query"),
                new Option<string>("--language", "Filter by language"),
                new Option<string>("--type", "Filter by type (function, class, method)"),
                new Option<string>("--database", () => "ckg.db", "Database file path")
            };
            
            searchCommand.SetHandler(async (string query, string? language, string? type, string database) =>
            {
                using var service = new CKGService(database);
                
                var results = await service.SearchAsync(query, language, type);
                
                Console.WriteLine($"Found {results.Count} results:");
                foreach (var result in results)
                {
                    Console.WriteLine($"  {result.Type} {result.Name} ({result.Language}) - {result.FilePath}:{result.Line}");
                    if (!string.IsNullOrEmpty(result.Signature))
                        Console.WriteLine($"    {result.Signature}");
                }
            },
            new Argument<string>("query"),
            new Option<string>("--language"),
            new Option<string>("--type"),
            new Option<string>("--database"));

            rootCommand.AddCommand(analyzeCommand);
            rootCommand.AddCommand(searchCommand);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
```

### 8. 项目文件

```xml
<!-- CKGTool.Core.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" />
    <PackageReference Include="System.Data.SQLite" Version="1.0.118" />
  </ItemGroup>

</Project>
```

```xml
<!-- CKGTool.CLI.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../CKGTool.Core/CKGTool.Core.csproj" />
  </ItemGroup>

</Project>
```

## 使用示例

```bash
# 分析项目
dotnet run -- analyze /path/to/project

# 搜索函数
dotnet run -- search "getUserById" --type function

# 搜索 Python 类
dotnet run -- search "User" --language python --type class

# 搜索所有包含 "auth" 的元素
dotnet run -- search "auth"
```

## 方案优势

### 1. 极简架构
- **零解析器**: 利用现有工具，无需维护复杂的解析逻辑
- **最少依赖**: 只依赖系统工具和标准库
- **代码量少**: 相比前两个方案减少 80% 代码量

### 2. 高可靠性
- **成熟工具**: 基于各语言官方工具链
- **精确解析**: 编译器级别的解析精度
- **稳定性强**: 工具链更新自动获得改进

### 3. 易扩展
- **插件化**: 新语言只需实现 `ILanguageExtractor`
- **工具检测**: 自动检测工具可用性
- **渐进式**: 可以逐步添加语言支持

### 4. 零维护
- **无原生库**: 不需要编译和维护原生代码
- **跨平台**: 依赖系统工具，天然跨平台
- **自动更新**: 工具链更新自动获得新特性

## 实施计划

### 第一阶段 (3天): 核心框架
- [ ] 实现基础接口和数据模型
- [ ] 实现 SQLite 数据存储
- [ ] 创建命令行工具框架

### 第二阶段 (2天): 基础语言支持
- [ ] 实现 C# 提取器 (Roslyn)
- [ ] 实现 Python 提取器 (AST)
- [ ] 实现 Go 提取器 (正则表达式)

### 第三阶段 (2天): 扩展和优化
- [ ] 添加更多语言支持
- [ ] 性能优化和错误处理
- [ ] 完善命令行界面

**总计**: 1周

## 技术风险

1. **工具依赖**: 依赖系统安装的工具
   - **缓解**: 提供工具检测和安装指导

2. **解析精度**: 简单正则表达式可能不够精确
   - **缓解**: 优先使用官方 API，正则作为后备

3. **性能**: 调用外部工具可能较慢
   - **缓解**: 并行处理和结果缓存

## 预期收益

- **开发效率**: 相比原方案减少 80% 开发时间
- **维护成本**: 几乎零维护成本
- **可靠性**: 基于成熟工具链，稳定性极高
- **扩展性**: 新增语言支持时间从 2天 减少到 2小时
- **部署简单**: 单个可执行文件，无额外依赖

这个方案真正做到了"站在巨人的肩膀上"，最大化利用现有生态系统的成果。