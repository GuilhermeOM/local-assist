using Local.Assist.Application.CustomTools.GetCurrentDateTime;
using OllamaSharp;

namespace Local.Assist.Application.CustomTools;

public sealed class Tools
{
    /// <summary>
    /// Obtem a data e hora atual do sistema.
    /// </summary>
    [OllamaTool]
    public static GetCurrentDateTimeCustomTool GetCurrentDateTime() => new();
}
