
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Microsoft.Identity.Client;

public class QueryMaker
{
    public interface query_builder_nonquery_end 
    {
        public string ExecuteNonQuery();
    }

    public interface table_name_builder<NextT>
    {
        public NextT With_Name(string name);
    }

    public interface column_name_type_builder<NextT>
    {
        public NextT Add_Column(string name, string type);
    }
    
    public interface looping_column_name_type_builder : query_builder_nonquery_end
    {
        public looping_column_name_type_builder Add_Column(string name, string type);
    }

    public interface row_values_builder : query_builder_nonquery_end
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
        public looping_table_column_alter_builder Add_Column(string name, string type);

        public looping_table_column_alter_builder Drop_Column(string name);

        public looping_table_column_alter_builder Rename_Column(string oldname, string newname);

        public looping_table_column_alter_builder Rename_Table(string newname);
    }

    private class create_table :
        table_name_builder<looping_column_name_type_builder>, 
        looping_column_name_type_builder
    {
        private string name;
        private List<string> columns;

        public create_table()
        {
            this.name = "";
            this.columns = new List<string>();
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
            string query = $"CREATE TABLE {name} (\n\t{string.Join(",\n\t", this.columns)}\n);";

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

        public insert_into()
        {
            this.name = "";
            this.columns = new List<string>();
            this.rows = new List<string>();
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

            for(int i = 0 ; i < Math.Min(type.Length, columns.Count); i++)
            {
                // TODO
            }

            return this;
        }

        public string ExecuteNonQuery()
        {
            string query = $"INSERT INTO {name} ({string.Join(", ", this.columns)}) VALUES\n\t{string.Join(",\n\t", this.rows)};";        
            return query;
        }
    }

    private class drop_table :
        table_name_builder<query_builder_nonquery_end>,
        query_builder_nonquery_end
    {
        private string name;

        public drop_table()
        {
            this.name = "";
        }

        public query_builder_nonquery_end With_Name(string name)
        {
            this.name = name;
            return this;
        }

        public string ExecuteNonQuery()
        {
            string query = $"DROP TABLE {name};";

            return query;
        }
    }

    private class alter_table :
        table_name_builder<looping_table_column_alter_builder>,
        looping_table_column_alter_builder
    {
        private string name;

        private List<string> alters;

        public alter_table()
        {
            this.name = "";
            this.alters = new List<string>();
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

    public static table_name_builder<looping_column_name_type_builder> Create_table => new create_table();

    public static table_name_builder<column_names_builder<row_values_builder>> Insert_Into_Table => new insert_into();

    public static table_name_builder<query_builder_nonquery_end> Drop_Table => new drop_table();

    public static table_name_builder<looping_table_column_alter_builder> Alter_Talbe => new alter_table();
}

