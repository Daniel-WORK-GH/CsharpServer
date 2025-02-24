class User
{
    [QueryableField]
    public int? id; // Will be used regardless on 'private'.

    [QueryableField]
    public string? name; // Will be used.

    [QueryableField]
    private readonly string? surname;  // Will be used regardless on 'private'.
                                       // Data will be forced written into it.

    public int ignored; // Will be ignored in queries.
}

class Program
{
    public static void Main(string[] args)
    {
        Database db = new Database("Database");
    }
}