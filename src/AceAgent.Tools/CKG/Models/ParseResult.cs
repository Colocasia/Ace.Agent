using AceAgent.Tools.CKG.Models;

namespace AceAgent.Tools.CKG.Models;

public class ParseResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public List<Function> Functions { get; set; } = new();
    public List<Class> Classes { get; set; } = new();
    public List<Property> Properties { get; set; } = new();
    public List<Field> Fields { get; set; } = new();
    public List<Variable> Variables { get; set; } = new();
    public DateTime ParsedAt { get; set; } = DateTime.UtcNow;

    public static ParseResult Success(string filePath, string language)
    {
        return new ParseResult
        {
            IsSuccess = true,
            FilePath = filePath,
            Language = language
        };
    }

    public static ParseResult Failure(string filePath, string language, string errorMessage)
    {
        return new ParseResult
        {
            IsSuccess = false,
            FilePath = filePath,
            Language = language,
            ErrorMessage = errorMessage
        };
    }
}