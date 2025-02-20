using Microsoft.Data.Sqlite;

public abstract class QueryMaker : IDisposable
{
    public interface query_builder_nonquery_end
    {
        public abstract string ExecuteNonQuery();
    }

    public interface table_name_builder<NextT>
    {
        public NextT With_Name(string name);
    }

    public interface column_name_type_builder<NextT>
    {
        public NextT Add_Column(string name, string type);
    }
    
    public interface looping_column_name_type_builder 
        : query_builder_nonquery_end
    {
        public looping_column_name_type_builder Add_Column(string name, string type);
    }

    public interface row_values_builder : 
        query_builder_nonquery_end
    {
        public row_values_builder Add_Values(params object[] type);
    }

    public interface column_names_builder<NextT>
    {
        public NextT To_Columns(params string[] columns);
    }

    public interface looping_table_column_alter_builder :
        query_builder_nonquery_end
    {
        public looping_table_column_alter_builder Rename_Table(string newname);

        public looping_table_column_alter_builder Add_Column(string name, string type);

        public looping_table_column_alter_builder Drop_Column(string name);

        public looping_table_column_alter_builder Rename_Column(string oldname, string newname);
    }

    public interface where_builder<NextT> : query_builder_nonquery_end
    {
        public NextT Where(string condition);
    }

    private class create_table :
        table_name_builder<looping_column_name_type_builder>,
        looping_column_name_type_builder
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

        public looping_column_name_type_builder Add_Column(string name, string type)
        {
            this.columns.Add($"{name} {type}");
            return this;
        }

        public looping_column_name_type_builder With_Name(string name)
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

    private class insert_into :
        table_name_builder<column_names_builder<row_values_builder>>,
        column_names_builder<row_values_builder>,
        row_values_builder
    {
        private string name;

        private List<string> columns;

        private List<string> rows;

        private SqliteCommand command;

        public insert_into(SqliteCommand command)
        {
            this.name = "";
            this.columns = new List<string>();
            this.rows = new List<string>();
            this.command = command;
        }

        public column_names_builder<row_values_builder> With_Name(string name)
        {
            this.name = name;
            return this;
        }

        public row_values_builder To_Columns(params string[] columns)
        {
            foreach (string column in columns)
            {
                this.columns.Add(column);
            }

            return this;
        }

        public row_values_builder Add_Values(params object[] type)
        {
            int row_index = this.rows.Count;
            this.rows.Add($"@{string.Join($"_{row_index}, @", this.columns)}_{row_index})");

            for (int i = 0; i < Math.Min(type.Length, columns.Count); i++)
            {
                // TODO
            }

            return this;
        }

        public string ExecuteNonQuery()
        {
            string query = $"INSERT INTO {name} ({string.Join(", ", this.columns)}) VALUES\n\t{string.Join(",\n\t", this.rows)};";

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
        table_name_builder<query_builder_nonquery_end>,
        query_builder_nonquery_end
    {
        private string name;

        private SqliteCommand command;

        public drop_table(SqliteCommand command)
        {
            this.name = "";
            this.command = command;
        }

        public query_builder_nonquery_end With_Name(string name)
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

    private class alter_table :
        table_name_builder<looping_table_column_alter_builder>,
        looping_table_column_alter_builder
    {
        private string name;

        private List<string> alters;

        private SqliteCommand command;

        public alter_table(SqliteCommand command)
        {
            this.name = "";
            this.alters = new List<string>();
            this.command = command;
        }

        public looping_table_column_alter_builder With_Name(string name)
        {
            this.name = name;
            return this;
        }

        public looping_table_column_alter_builder Add_Column(string name, string type)
        {
            this.alters.Add($"ADD COLUMN {name} {type}");
            return this;
        }
        public looping_table_column_alter_builder Drop_Column(string name)
        {
            this.alters.Add($"DROP COLUMN {name}");
            return this;
        }

        public looping_table_column_alter_builder Rename_Column(string oldname, string newname)
        {
            this.alters.Add($"RENAME COLUMN {oldname} TO {newname}");
            return this;
        }

        public looping_table_column_alter_builder Rename_Table(string newname)
        {
            this.alters.Add($"RENAME TO {newname}");
            return this;
        }

        public string ExecuteNonQuery()
        {
            string query = $"ALTER TABLE {name} {string.Join($";\nALTER TABLE {name} ", this.alters)};"; 
            return query;
        }
    }

    private class delete_from :
        table_name_builder<where_builder<query_builder_nonquery_end>>,
        where_builder<query_builder_nonquery_end>,
        query_builder_nonquery_end
    {
        private string name;
        private string condition;
        private SqliteCommand command;

        public delete_from(SqliteCommand command)
        {
            this.name = "";
            this.condition = "";
            this.command = command;
        }

        public where_builder<query_builder_nonquery_end> With_Name(string name)
        {
            this.name = name;
            return this;
        }

        public query_builder_nonquery_end Where(string condition)
        {
            this.condition = $"WHERE {condition}";
            return this;
        }

        public string ExecuteNonQuery()
        {
            string query = $"DELETE FROM {name}{(string.IsNullOrEmpty(condition) ? "" : "\n" + condition)};";

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

    private SqliteConnection? connection;

    protected void SetConnection(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public abstract void Dispose();

    public table_name_builder<looping_column_name_type_builder> Create_table =>
        new create_table(connection!.CreateCommand());

    public table_name_builder<column_names_builder<row_values_builder>> Insert_Into_Table =>
        new insert_into(connection!.CreateCommand());

    public table_name_builder<query_builder_nonquery_end> Drop_Table =>
        new drop_table(connection!.CreateCommand());

    public table_name_builder<looping_table_column_alter_builder> Alter_Talbe =>
        new alter_table(connection!.CreateCommand());

    public table_name_builder<where_builder<query_builder_nonquery_end>> Delete_From_Table =>
        new delete_from(connection!.CreateCommand());
}

