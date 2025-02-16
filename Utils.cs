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
}