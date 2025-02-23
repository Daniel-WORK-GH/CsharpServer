using System.Dynamic;
using System.Net;
using System.Text;

class Program
{
    class User
    {
        [QueryableField]
        public int id;
    }

    public static void Main()
    {
        Database db = new Database("Database");



        db.Delete_From_Table.With_Name("Users").ExecuteNonQuery();
    }
}
