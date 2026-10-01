namespace BridgeTask.Api.Common.Pagination;

public static class SearchPattern
{
    public const string EscapeCharacter = "\\";

    public static string Contains(string term)
    {
        var escaped = term.Trim()
            .Replace(EscapeCharacter, EscapeCharacter + EscapeCharacter)
            .Replace("%", EscapeCharacter + "%")
            .Replace("_", EscapeCharacter + "_");
        return $"%{escaped}%";
    }
}
