namespace AceAgent.Tools.CKG.Models;

public abstract class CodeElement
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public string ProjectPath { get; set; } = string.Empty;
    public string CommitHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Function : CodeElement
{
    public string ReturnType { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
    public string Modifiers { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public string? Namespace { get; set; }
    public bool IsStatic { get; set; }
    public bool IsPublic { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsProtected { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }
    public bool IsAbstract { get; set; }
    public string? Documentation { get; set; }
}

public class Class : CodeElement
{
    public string? Namespace { get; set; }
    public string Modifiers { get; set; } = string.Empty;
    public string? BaseClass { get; set; }
    public string Interfaces { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsSealed { get; set; }
    public bool IsPartial { get; set; }
    public bool IsPublic { get; set; }
    public bool IsInternal { get; set; }
    public string? Documentation { get; set; }
}

public class Property : CodeElement
{
    public string Type { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public string? Namespace { get; set; }
    public string Modifiers { get; set; } = string.Empty;
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }
    public bool IsStatic { get; set; }
    public bool IsPublic { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsProtected { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }
    public bool IsAbstract { get; set; }
    public string? Documentation { get; set; }
}

public class Field : CodeElement
{
    public string Type { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public string? Namespace { get; set; }
    public string Modifiers { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    public bool IsReadonly { get; set; }
    public bool IsConst { get; set; }
    public bool IsPublic { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsProtected { get; set; }
    public string? DefaultValue { get; set; }
    public string? Documentation { get; set; }
}

public class Variable : CodeElement
{
    public string Type { get; set; } = string.Empty;
    public string? FunctionName { get; set; }
    public string? ClassName { get; set; }
    public string? Namespace { get; set; }
    public string Scope { get; set; } = string.Empty; // local, parameter, field
    public bool IsParameter { get; set; }
    public bool IsLocal { get; set; }
    public string? DefaultValue { get; set; }
}