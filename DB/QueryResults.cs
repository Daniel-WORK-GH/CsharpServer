public struct NonQueryResult(bool success, string result, string error)
{
    public readonly bool Success = success;
    public readonly string Result = result;
    public readonly string Error = error;
}

