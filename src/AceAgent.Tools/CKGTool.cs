using System.Text.Json;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;
using AceAgent.Tools.CKG;
using Microsoft.Extensions.Logging;

namespace AceAgent.Tools;

public class CKGTool : ITool
{
    private readonly ILogger<CKGTool> _logger;
    private readonly CKGService _ckgService;

    public string Name => "ckg";
    public string Description => "代码知识图谱工具，用于分析、查询和管理代码库的结构信息";

    public CKGTool(ILogger<CKGTool> logger, CKGService ckgService)
    {
        _logger = logger;
        _ckgService = ckgService;
    }

    public async Task<ToolResult> ExecuteAsync(ToolInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            var args = ParseArguments(input.RawInput);
            
            if (args.Length == 0)
            {
                return ToolResult.CreateSuccess(GetHelpText());
            }

            var command = args[0].ToLower();
            var commandArgs = args.Skip(1).ToArray();

            var result = command switch
            {
                "analyze" => await ExecuteAnalyzeAsync(commandArgs),
                "query" => await ExecuteQueryAsync(commandArgs),
                "export" => await ExecuteExportAsync(commandArgs),
                "import" => await ExecuteImportAsync(commandArgs),
                "help" or "-h" or "--help" => GetHelpText(),
                _ => $"未知命令: {command}\n\n{GetHelpText()}"
            };
            
            return ToolResult.CreateSuccess(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行CKG命令时发生错误");
            return ToolResult.Failure($"错误: {ex.Message}");
        }
    }
    
    public async Task<bool> ValidateInputAsync(ToolInput input)
    {
        return await Task.FromResult(true); // 基本验证，所有输入都接受
    }
    
    private string[] ParseArguments(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
            return Array.Empty<string>();
            
        // 简单的参数解析，可以根据需要改进
        return arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private async Task<ToolResult> AnalyzeAsync(CKGArgs args)
    {
        if (string.IsNullOrEmpty(args.Path))
        {
            return new ToolResult
            {
                Success = false,
                Error = "Path is required for analyze command"
            };
        }

        var result = await _ckgService.AnalyzeRepositoryAsync(
            args.Path, 
            args.Languages?.ToArray(), 
            args.Database,
            args.Verbose);

        return new ToolResult
        {
            Success = result,
            Message = result ? "Analysis completed successfully" : "Analysis failed",
            Data = new { analyzed_path = args.Path }
        };
    }

    private async Task<ToolResult> QueryAsync(CKGArgs args)
    {
        if (string.IsNullOrEmpty(args.Query))
        {
            return new ToolResult
            {
                Success = false,
                Error = "Query is required for query command"
            };
        }

        var results = await _ckgService.ExecuteQueryAsync(args.Query, args.Database);
        
        return new ToolResult
        {
            Success = true,
            Message = $"Query executed successfully, found {results.Count} results",
            Data = results
        };
    }

    private async Task<ToolResult> ExportAsync(CKGArgs args)
    {
        if (string.IsNullOrEmpty(args.Output))
        {
            return new ToolResult
            {
                Success = false,
                Error = "Output path is required for export command"
            };
        }

        var result = await _ckgService.ExportDataAsync(args.Output, args.Format, args.Database);
        
        return new ToolResult
        {
            Success = result,
            Message = result ? $"Data exported to {args.Output}" : "Export failed",
            Data = new { output_path = args.Output, format = args.Format }
        };
    }

    private async Task<ToolResult> ImportAsync(CKGArgs args)
    {
        if (string.IsNullOrEmpty(args.Input))
        {
            return new ToolResult
            {
                Success = false,
                Error = "Input path is required for import command"
            };
        }

        var result = await _ckgService.ImportDataAsync(args.Input, args.Database);
        
        return new ToolResult
        {
            Success = result,
            Message = result ? $"Data imported from {args.Input}" : "Import failed",
            Data = new { input_path = args.Input }
        };
    }

    private async Task<string> ExecuteAnalyzeAsync(string[] args)
    {
        Console.WriteLine($"[DEBUG] ExecuteAnalyzeAsync called with {args.Length} args: [{string.Join(", ", args)}]");
        
        if (args.Length == 0)
        {
            return "错误: 请指定要分析的路径\n\n" + GetHelpText();
        }

        var path = args[0];
        var verbose = args.Contains("-v") || args.Contains("--verbose");
        
        try
         {
             // 检查路径是文件还是目录
              Console.WriteLine($"[DEBUG] 检查路径: {path}, 是文件: {File.Exists(path)}, 是目录: {Directory.Exists(path)}");
              _logger.LogInformation("检查路径: {Path}, 是文件: {IsFile}, 是目录: {IsDirectory}", path, File.Exists(path), Directory.Exists(path));
              
              if (File.Exists(path))
              {
                  // 分析单个文件
                  Console.WriteLine($"[DEBUG] 开始分析单个文件: {path}");
                  _logger.LogInformation("开始分析单个文件: {Path}", path);
                 var result = await _ckgService.AnalyzeFileAndSaveAsync(path);
                 if (result != null && result.IsSuccess)
                 {
                     return $"文件分析完成: {path}\n" +
                            $"- 函数: {result.Functions.Count}\n" +
                            $"- 类: {result.Classes.Count}\n" +
                            $"- 属性: {result.Properties.Count}\n" +
                            $"- 字段: {result.Fields.Count}\n" +
                            $"- 变量: {result.Variables.Count}";
                 }
                 else
                 {
                     return $"文件分析失败: {path}\n错误: {result?.ErrorMessage ?? "未知错误"}";
                 }
            }
            else if (Directory.Exists(path))
            {
                // 分析目录
                var success = await _ckgService.AnalyzeRepositoryAsync(path, verbose: verbose);
                return success ? $"目录分析完成: {path}" : $"目录分析失败: {path}";
            }
            else
            {
                return $"错误: 路径不存在: {path}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分析路径时发生错误: {Path}", path);
            return $"分析失败: {ex.Message}";
        }
    }
     
     private async Task<string> ExecuteQueryAsync(string[] args)
     {
         // 实现查询命令
         await Task.CompletedTask;
         return "查询命令执行完成";
     }
     
     private async Task<string> ExecuteExportAsync(string[] args)
     {
         // 实现导出命令
         await Task.CompletedTask;
         return "导出命令执行完成";
     }
     
     private async Task<string> ExecuteImportAsync(string[] args)
     {
         // 实现导入命令
         await Task.CompletedTask;
         return "导入命令执行完成";
     }
    
    private string GetHelpText()
    {
        return @"CKG (代码知识图谱) 工具

用法:
  analyze <path> [-v|--verbose]  - 分析代码文件或目录
                                   支持单个文件或整个目录分析
                                   -v, --verbose: 显示详细信息
  query <query>                  - 执行查询
  export <path>                  - 导出数据
  import <path>                  - 导入数据
  help                           - 显示帮助信息

示例:
  analyze /path/to/file.cs       - 分析单个C#文件
  analyze /path/to/project -v    - 分析整个项目目录（详细模式）";
    }
}

public class CKGArgs
{
    public string Command { get; set; } = "";
    public string Path { get; set; } = "";
    public string Query { get; set; } = "";
    public string Output { get; set; } = "";
    public string Input { get; set; } = "";
    public string Database { get; set; } = "ckg.db";
    public string Format { get; set; } = "json";
    public List<string>? Languages { get; set; }
    public bool Verbose { get; set; } = false;
}