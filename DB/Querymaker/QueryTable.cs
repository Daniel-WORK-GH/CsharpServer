using System.Reflection;
using Microsoft.Data.Sqlite;

partial class QueryMaker
{    
    public interface with_name<NextT>
    {
        public NextT With_Name(string name);
    }

    public interface add_column
    {
        public add_columns Add_Column(string name, string type);
    }

    public interface add_columns : add_column,  non_query_end
    {
        
    }

    
    private class create_table : with_name<add_column>, add_column, add_columns
    {
        private string name;

        private List<string> columns;

        private SqliteCommand command;

        public create_table(SqliteCommand command)
        {
            this.name = "";
            this.columns = new List<string>();
            this.command = command;
        }

        public add_columns Add_Column(string name, string type)
        {
            this.columns.Add($"{name} {type}");
            return this;
        }

        public add_column With_Name(string name)
        {
            this.name = name;
            return this;
        }

        public string ExecuteNonQuery()
        {
            string query = $"CREATE TABLE {name} {(columns.Count == 0 ? "" : $"(\n\t{string.Join(",\n\t", this.columns)}\n)")};";

            try
            {
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }

            return query;
        }
    }

    private class create_table_for<T> :
        with_name<non_query_end>,
        non_query_end
    {
        private string name;

        private SqliteCommand command;

        public create_table_for(SqliteCommand command)
        {
            this.name = "";
            this.command = command;
        }

        public non_query_end With_Name(string name)
        {
            this.name = name;
            return this;
        }

        public string ExecuteNonQuery()
        {
            var fieldsWithAttribute = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.IsDefined(typeof(QueryableField), false));

            var columns = from field in fieldsWithAttribute select $"{field.Name} {Utils.MapToSQLiteType(field.FieldType)}";

            string query = $"CREATE TABLE {name} {(columns.Count() == 0 ? "" : $"(\n\t{string.Join(",\n\t", columns)}\n)")};";

            try
            {
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }

            return query;
        }
    }

    private class drop_table :
        with_name<non_query_end>,
        non_query_end
    {
        private string name;

        private SqliteCommand command;

        public drop_table(SqliteCommand command)
        {
            this.name = "";
            this.command = command;
        }

        public non_query_end With_Name(string name)
        {
            this.name = name;
            return this;
        }

        public string ExecuteNonQuery()
        {
            string query = $"DROP TABLE {name};";

            try
            {
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }

            return query;
        }
    }


    public with_name<add_column> Create_table =>
        new create_table(connection!.CreateCommand());

    public with_name<non_query_end> Create_Table_For<T>() =>
        new create_table_for<T>(connection!.CreateCommand());

    public with_name<non_query_end> Drop_Table =>
        new drop_table(connection!.CreateCommand());
}