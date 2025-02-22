using System.Dynamic;
using System.Net;
using System.Text;

class Program
{
    class User 
    {
        [QueryableField]
        private int id;

        [QueryableField]
        public string? name;

        [QueryableField]
        private readonly string? surname;

        public int ignored;

        public override string ToString()
        {
            return $"{id}:{name} {surname}";
        }
    }

    public static void Main()
    {
        Database db = new Database("Database");

        var users = db.Select<User>().From_Table("Users").ExecuteQuery();

        foreach(var user in users)
        {
            System.Console.WriteLine(user);
        }
    }
}
