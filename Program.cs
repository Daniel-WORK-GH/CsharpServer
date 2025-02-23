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

        

        db.Insert_Into_Table.With_Name("Users").Values([new User(){id = 100}]).ExecuteNonQuery();
    }
}
