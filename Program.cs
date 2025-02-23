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

        db.Insert_Into_Table.With_Name("Users")
            .Values([new User() { name = "dan" }, new User() { name = "daniel" }])
            .ExecuteNonQuery();
        System.Console.WriteLine("1");
        var users = db.Select.Type<User>().From_Table("Users").ExecuteQuery();
        System.Console.WriteLine("2");

        foreach (var u in users)
        {
            System.Console.WriteLine(u.name);
        }
    }
}