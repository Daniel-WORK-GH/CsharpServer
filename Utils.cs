public static class Utils
{
    public static string CurrentDir => Directory.GetCurrentDirectory();

    public static bool IsValidFileName(string file_name)
    {
        if (string.IsNullOrWhiteSpace(file_name))
            return false;

        // Check if the filename contains an extension
        if (!string.IsNullOrEmpty(Path.GetExtension(file_name)))
            return false;

        // Check for invalid characters
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return file_name.IndexOfAny(invalidChars) == -1;
    }

    public static string MapToSQLiteType(Type fieldType)
    {
        if (fieldType == typeof(int) ||
            fieldType == typeof(long) ||
            fieldType == typeof(short) ||
            fieldType == typeof(byte) ||
            fieldType == typeof(bool))
        {
            return "INTEGER";
        }
        else if (fieldType == typeof(float) ||
                    fieldType == typeof(double) ||
                    fieldType == typeof(decimal))
        {
            return "REAL";
        }
        else if (fieldType == typeof(string) ||
                    fieldType == typeof(char) ||
                    fieldType == typeof(DateTime))
        {
            return "TEXT";
        }
        else if (fieldType == typeof(byte[]))
        {
            return "BLOB";
        }
        else
        {
            // Default mapping if the type is not explicitly handled.
            return "TEXT";
        }
    }

    public static string FormatValue(object? value)
    {
        if (value is string str)
        {
            return $"'{str}'";
        }
        else
        {
            return value?.ToString() ?? "NULL";
        }
    }
}