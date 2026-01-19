namespace Automation.Validator.Models;

/// <summary>
/// Resultado de validação com lista de erros e avisos.
/// </summary>
public record ValidationResult(
    bool IsValid,
    List<ValidationError> Errors,
    List<ValidationWarning> Warnings
)
{
    public static ValidationResult Success() => new(true, [], []);
    
    public static ValidationResult WithErrors(params ValidationError[] errors) => 
        new(false, errors.ToList(), []);
    
    public void AddError(ValidationError error) => Errors.Add(error);
    public void AddWarning(ValidationWarning warning) => Warnings.Add(warning);
}

/// <summary>
/// Erro de validação com localização e mensagem.
/// </summary>
public record ValidationError(
    string Code,
    string Message,
    string? File = null,
    int? Line = null
);

/// <summary>
/// Aviso de validação (não bloqueia execução).
/// </summary>
public record ValidationWarning(
    string Code,
    string Message,
    string? File = null
);

/// <summary>
/// Modelo de UiMap carregado do YAML.
/// </summary>
public class UiMapModel
{
    public Dictionary<string, UiPage> Pages { get; set; } = [];
}

/// <summary>
/// Página no UiMap com metadados e elementos.
/// </summary>
public class UiPage
{
    public string? Route { get; set; }
    public string? Anchor { get; set; }
    public Dictionary<string, UiElement> Elements { get; set; } = [];
}

/// <summary>
/// Elemento de UI com testId.
/// </summary>
public class UiElement
{
    public string? TestId { get; set; }
}

/// <summary>
/// Modelo de DataMap carregado do YAML.
/// </summary>
public class DataMapModel
{
    public Dictionary<string, object> Contexts { get; set; } = [];
    public Dictionary<string, DataSet> Datasets { get; set; } = [];
}

/// <summary>
/// Dataset com estratégia de seleção.
/// </summary>
public class DataSet
{
    public string Strategy { get; set; } = "sequential";
    public List<string> Items { get; set; } = [];
}

/// <summary>
/// Resultado de análise de cobertura.
/// </summary>
public record CoverageReport(
    int TotalPages,
    int TotalElements,
    int TestedPages,
    int TestedElements,
    double CoveragePercentage,
    List<string> UntestPages
);
