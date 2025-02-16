using Microsoft.Data.Sqlite;

public class QueryMaker
{
    public abstract class slq_query
    {
        public abstract string Execute();
    }

    public class create_table : slq_query
    {
        private string? table_name;

        private SqliteCommand command;

        private List<string> values;

        public create_table(SqliteCommand command)
        {
            this.command = command;

            this.values = new List<string>();
        }
        
        public create_table With_Name(string name)
        { 
            this.table_name = name;
            return this;
        }

        public create_table Add_Column(string column_name, string data_type)
        {
            command.Parameters.AddWithValue($"@name{values.Count}", column_name);
            command.Parameters.AddWithValue($"@type{values.Count}", data_type);
            values.Add($"@name{values.Count} @type{values.Count}");
            return this;
        }

        public override string Execute()
        {
            if(string.IsNullOrEmpty(table_name))
            {
                throw new Exception("Table name wasn't set.");
            }

            string query = $"CREATE TABLE {table_name} ({string.Join(",", values)});";

            System.Console.WriteLine(command.Parameters[0].ParameterName);
            System.Console.WriteLine(query);

            command.CommandText = query;
            command.ExecuteNonQuery();
            
            return query;
        }
    }

    private SqliteCommand command;

    public QueryMaker(SqliteCommand command)
    {
        this.command = command;
    }

    public create_table Create_Table => new create_table(command);
}

