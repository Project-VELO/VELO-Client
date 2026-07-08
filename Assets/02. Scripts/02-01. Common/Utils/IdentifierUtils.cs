public static class IdentifierUtils
{
    public static string SanitizeIdentifier(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return "_";
        }
        string sanitized = System.Text.RegularExpressions.Regex.Replace(input, @"[^a-zA-Z0-9_]", "_");
        if (char.IsDigit(sanitized[0]))
        {
            sanitized = "_" + sanitized;
        }
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"_+", "_");
        return sanitized;
    }
}
