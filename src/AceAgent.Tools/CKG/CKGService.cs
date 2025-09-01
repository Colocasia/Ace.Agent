using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AceAgent.Tools.CKG.Data;
using AceAgent.Tools.CKG.Models;
using AceAgent.Tools.CKG.Services;

namespace AceAgent.Tools.CKG;

public class CKGService
{
    private readonly TreeSitterService _treeSitterService;
    private readonly CKGDbContext _dbContext;
    private readonly ILogger<CKGService> _logger;
    
    private static readonly Dictionary<string, string> FileExtensionToLanguage = new()
    {
        { ".c", "c" },
        { ".h", "c" },
        { ".cpp", "cpp" },
        { ".cxx", "cpp" },
        { ".cc", "cpp" },
        { ".hpp", "cpp" },
        { ".hxx", "cpp" },
        { ".cs", "csharp" },
        { ".java", "java" },
        { ".js", "javascript" },
        { ".jsx", "javascript" },
        { ".ts", "typescript" },
        { ".tsx", "typescript" },
        { ".py", "python" },
        { ".pyw", "python" },
        { ".go", "go" },
        { ".rs", "rust" },
        { ".lua", "lua" },
        { ".php", "php" }
    };

    public CKGService(
        TreeSitterService treeSitterService,
        CKGDbContext dbContext,
        ILogger<CKGService> logger)
    {
        _treeSitterService = treeSitterService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> AnalyzeRepositoryAsync(string repositoryPath, string[]? languages = null, string? databasePath = null, bool verbose = false)
    {
        try
        {
            if (!Directory.Exists(repositoryPath))
            {
                _logger.LogError("Repository path does not exist: {RepositoryPath}", repositoryPath);
                return false;
            }

            // Ensure database is created
            await _dbContext.Database.EnsureCreatedAsync();

            // Update database connection if specified
            if (!string.IsNullOrEmpty(databasePath))
            {
                await UpdateDatabaseConnectionAsync(databasePath);
            }

            var supportedLanguages = (languages?.Length > 0 ? languages.ToList() : null) ?? GetSupportedLanguages();
            
            if (verbose)
            {
                _logger.LogInformation("Starting analysis of repository: {RepositoryPath}", repositoryPath);
                _logger.LogInformation("Supported languages: {Languages}", string.Join(", ", supportedLanguages));
            }

            var codeFiles = GetCodeFiles(repositoryPath, supportedLanguages);
            
            if (verbose)
            {
                _logger.LogInformation("Found {FileCount} files, {CodeFileCount} are code files", 
                    Directory.GetFiles(repositoryPath, "*", SearchOption.AllDirectories).Length,
                    codeFiles.Count);
            }

            var processedFiles = 0;
            foreach (var filePath in codeFiles)
            {
                var result = await AnalyzeFileAsync(filePath, repositoryPath);
                if (result != null && result.IsSuccess)
                {
                    await SaveParseResultAsync(result);
                    processedFiles++;
                }
            }

            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Repository analysis completed. Processed {ProcessedFiles} files. Data saved to database.", processedFiles);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during repository analysis: {RepositoryPath}", repositoryPath);
            return false;
        }
    }

    public async Task<ParseResult?> AnalyzeFileAsync(string filePath, string? projectPath = null, string? commitHash = null)
    {
        var result = await AnalyzeFileInternalAsync(filePath, projectPath, commitHash);
        return result;
    }

    public async Task<ParseResult?> AnalyzeFileAndSaveAsync(string filePath, string? projectPath = null, string? commitHash = null)
    {
        try
        {
            // Ensure database is created
            await _dbContext.Database.EnsureCreatedAsync();
            
            var result = await AnalyzeFileInternalAsync(filePath, projectPath, commitHash);
            if (result != null && result.IsSuccess)
            {
                await SaveParseResultAsync(result);
                await _dbContext.SaveChangesAsync();
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing and saving file: {FilePath}", filePath);
            return null;
        }
    }

    private async Task<ParseResult?> AnalyzeFileInternalAsync(string filePath, string? projectPath = null, string? commitHash = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File does not exist: {FilePath}", filePath);
                return null;
            }

            var language = GetLanguageFromExtension(Path.GetExtension(filePath));
            if (string.IsNullOrEmpty(language))
            {
                _logger.LogDebug("Unsupported file type: {FilePath}", filePath);
                return null;
            }

            var sourceCode = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                _logger.LogDebug("Empty file: {FilePath}", filePath);
                return null;
            }

            var result = await _treeSitterService.ParseCodeAsync(sourceCode, language, filePath);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Parse failed for {FilePath}: {Error}", filePath, result.ErrorMessage);
                return result;
            }

            // Set additional properties
            var normalizedProjectPath = projectPath ?? Path.GetDirectoryName(filePath) ?? "";
            var normalizedCommitHash = commitHash ?? "";
            
            foreach (var func in result.Functions)
            {
                func.ProjectPath = normalizedProjectPath;
                func.CommitHash = normalizedCommitHash;
            }
            
            foreach (var cls in result.Classes)
            {
                cls.ProjectPath = normalizedProjectPath;
                cls.CommitHash = normalizedCommitHash;
            }
            
            foreach (var prop in result.Properties)
            {
                prop.ProjectPath = normalizedProjectPath;
                prop.CommitHash = normalizedCommitHash;
            }
            
            foreach (var field in result.Fields)
            {
                field.ProjectPath = normalizedProjectPath;
                field.CommitHash = normalizedCommitHash;
            }
            
            foreach (var variable in result.Variables)
            {
                variable.ProjectPath = normalizedProjectPath;
                variable.CommitHash = normalizedCommitHash;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing file: {FilePath}", filePath);
            return null;
        }
    }

    public async Task<List<object>> ExecuteQueryAsync(string query, string? databasePath = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(databasePath))
            {
                await UpdateDatabaseConnectionAsync(databasePath);
            }

            _logger.LogInformation("执行查询: {Query}", query);
            
            // TODO: Implement actual SQL query execution
            _logger.LogInformation("查询功能正在开发中...");
            
            return new List<object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query: {Query}", query);
            throw;
        }
    }

    public async Task<bool> ExportDataAsync(string outputPath, string format = "json", string? databasePath = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(databasePath))
            {
                await UpdateDatabaseConnectionAsync(databasePath);
            }

            _logger.LogInformation("导出知识图谱到: {OutputPath}, 格式: {Format}", outputPath, format);
            
            // TODO: Implement actual export functionality
            _logger.LogInformation("导出功能正在开发中...");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting data to: {OutputPath}", outputPath);
            return false;
        }
    }

    public async Task<bool> ImportDataAsync(string inputPath, string? databasePath = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(databasePath))
            {
                await UpdateDatabaseConnectionAsync(databasePath);
            }

            _logger.LogInformation("导入知识图谱从: {InputPath}", inputPath);
            
            // TODO: Implement actual import functionality
            _logger.LogInformation("导入功能正在开发中...");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing data from: {InputPath}", inputPath);
            return false;
        }
    }

    private async Task UpdateDatabaseConnectionAsync(string databasePath)
    {
        // TODO: Implement database connection update
        await Task.CompletedTask;
    }

    private async Task SaveParseResultAsync(ParseResult result)
    {
        // Remove existing entries for this file
        var existingFunctions = await _dbContext.Functions
            .Where(f => f.FilePath == result.FilePath)
            .ToListAsync();
        _dbContext.Functions.RemoveRange(existingFunctions);
        
        var existingClasses = await _dbContext.Classes
            .Where(c => c.FilePath == result.FilePath)
            .ToListAsync();
        _dbContext.Classes.RemoveRange(existingClasses);
        
        var existingProperties = await _dbContext.Properties
            .Where(p => p.FilePath == result.FilePath)
            .ToListAsync();
        _dbContext.Properties.RemoveRange(existingProperties);
        
        var existingFields = await _dbContext.Fields
            .Where(f => f.FilePath == result.FilePath)
            .ToListAsync();
        _dbContext.Fields.RemoveRange(existingFields);
        
        var existingVariables = await _dbContext.Variables
            .Where(v => v.FilePath == result.FilePath)
            .ToListAsync();
        _dbContext.Variables.RemoveRange(existingVariables);

        // Add new entries
        await _dbContext.Functions.AddRangeAsync(result.Functions);
        await _dbContext.Classes.AddRangeAsync(result.Classes);
        await _dbContext.Properties.AddRangeAsync(result.Properties);
        await _dbContext.Fields.AddRangeAsync(result.Fields);
        await _dbContext.Variables.AddRangeAsync(result.Variables);
    }

    private List<string> GetCodeFiles(string directoryPath, List<string> supportedLanguages)
    {
        var allFiles = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
        return allFiles.Where(file => IsCodeFile(file, supportedLanguages)).ToList();
    }

    private bool IsCodeFile(string filePath, List<string> supportedLanguages)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        if (!FileExtensionToLanguage.TryGetValue(extension, out var language))
        {
            return false;
        }
        
        return supportedLanguages.Contains(language);
    }

    private string? GetLanguageFromExtension(string extension)
    {
        return FileExtensionToLanguage.TryGetValue(extension.ToLowerInvariant(), out var language) ? language : null;
    }

    private List<string> GetSupportedLanguages()
    {
        return new List<string> { "c", "cpp", "csharp", "go", "java", "javascript", "python", "rust", "typescript" };
    }

    public async Task<string> QueryKnowledgeGraphAsync(string query, string? database, string format)
    {
        try
        {
            _logger.LogInformation("执行查询: {Query}", query);
            _logger.LogInformation("查询功能正在开发中...");
            await Task.CompletedTask;
            return "查询功能正在开发中...";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询知识图谱时发生错误");
            throw;
        }
    }

    public async Task<bool> ExportKnowledgeGraphAsync(string output, string? database, string format)
    {
        try
        {
            _logger.LogInformation("导出知识图谱到: {Output}, 格式: {Format}", output, format);
            _logger.LogInformation("导出功能正在开发中...");
            await Task.CompletedTask;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出知识图谱时发生错误");
            throw;
        }
    }

    public async Task<bool> ImportKnowledgeGraphAsync(string input, string? database, bool merge)
    {
        try
        {
            _logger.LogInformation("导入知识图谱从: {Input}, 合并: {Merge}", input, merge);
            _logger.LogInformation("导入功能正在开发中...");
            await Task.CompletedTask;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导入知识图谱时发生错误");
            throw;
        }
    }
}