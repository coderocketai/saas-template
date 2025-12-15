namespace CodeRocket.Common.Helpers;

public static class PgSqlHelper
{
    /// <summary>
    /// Parse SQL statements handling PostgreSQL-specific syntax like functions and triggers
    /// </summary>
    public static List<string> ParseSqlStatements(string sqlContent)
    {
        var statements = new List<string>();
        var currentStatement = new System.Text.StringBuilder();
        var inString = false;
        var inComment = false;
        var stringChar = '\0';
        var dollarQuoteStart = -1;
        var dollarQuoteName = "";

        for (int i = 0; i < sqlContent.Length; i++)
        {
            char current = sqlContent[i];
            char next = i + 1 < sqlContent.Length ? sqlContent[i + 1] : '\0';

            // Handle dollar-quoted strings (PostgreSQL feature for functions/triggers)
            if (!inString && !inComment && current == '$')
            {
                // Try to match a dollar quote like $$ or $tag$
                int tagEnd = i + 1;
                while (tagEnd < sqlContent.Length && sqlContent[tagEnd] != '$')
                    tagEnd++;

                if (tagEnd < sqlContent.Length && sqlContent[tagEnd] == '$')
                {
                    string tag = sqlContent.Substring(i, tagEnd - i + 1);
                    
                    if (dollarQuoteStart == -1)
                    {
                        // Entering dollar quote
                        dollarQuoteStart = i;
                        dollarQuoteName = tag;
                        currentStatement.Append(sqlContent.Substring(i, tag.Length));
                        i = tagEnd;
                        continue;
                    }
                    else if (tag == dollarQuoteName)
                    {
                        // Exiting dollar quote
                        currentStatement.Append(tag);
                        dollarQuoteStart = -1;
                        dollarQuoteName = "";
                        i = tagEnd;
                        continue;
                    }
                }
            }

            // If inside dollar quote, just append
            if (dollarQuoteStart != -1)
            {
                currentStatement.Append(current);
                continue;
            }

            // Handle line comments
            if (!inString && current == '-' && next == '-')
            {
                inComment = true;
                currentStatement.Append("--");
                i++; // Skip next dash
                continue;
            }

            if (inComment && current == '\n')
            {
                inComment = false;
                currentStatement.Append(current);
                continue;
            }

            if (inComment)
            {
                currentStatement.Append(current);
                continue;
            }

            // Handle string literals
            if (!inString && (current == '\'' || current == '"'))
            {
                inString = true;
                stringChar = current;
                currentStatement.Append(current);
                continue;
            }

            if (inString && current == stringChar)
            {
                // Check for escaped quote (double quote)
                if (next == stringChar)
                {
                    currentStatement.Append(current);
                    currentStatement.Append(next);
                    i++; // Skip next quote
                    continue;
                }
                else
                {
                    inString = false;
                    currentStatement.Append(current);
                    continue;
                }
            }

            if (inString)
            {
                currentStatement.Append(current);
                continue;
            }

            // Handle statement terminator
            if (current == ';')
            {
                currentStatement.Append(current);
                var trimmed = currentStatement.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    statements.Add(trimmed);
                }
                currentStatement.Clear();
                continue;
            }

            currentStatement.Append(current);
        }

        // Add any remaining statement
        var remaining = currentStatement.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(remaining))
        {
            statements.Add(remaining);
        }

        return statements;
    }
}