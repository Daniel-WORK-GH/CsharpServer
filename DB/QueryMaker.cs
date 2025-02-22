using System.Collections;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
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
        public looping_column_name_type_builder Add_Column(string name, string type);
    }

    public interface looping_column_name_type_builder : column_name_type_builder<looping_column_name_type_builder>,  query_builder_nonquery_end
    {
        
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

    public interface from_name_builder<NextT>
    {
        public NextT From_Table(string name);
    }

    public interface query_builder_return_end<T>
    {
        public T[] ExecuteQuery();
    }

    public interface insert_table_name<NextT>
    {
        public NextT To_Table(string name);
    }

    public interface insert_table_values<NextT, T>
    {
        public NextT Values(T[] value);
    }

    private class create_table :
        table_name_builder<column_name_type_builder<looping_column_name_type_builder>>,
        column_name_type_builder<looping_column_name_type_builder>,
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

        public column_name_type_builder<looping_column_name_type_builder> With_Name(string name)
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
        table_name_builder<query_builder_nonquery_end>,
        query_builder_nonquery_end
    {
        private string name;

        private SqliteCommand command;

        public create_table_for(SqliteCommand command)
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

    private class insert_<T> :
        insert_table_name<insert_table_values<query_builder_nonquery_end, T>>,
        insert_table_values<query_builder_nonquery_end, T>,
        query_builder_nonquery_end
    {
        private string name;

        private SqliteCommand command;

        private T[] values;

        public insert_(SqliteCommand command)
        {
            this.name = "";
            this.command = command;
            this.values = [];
        }

        public insert_table_values<query_builder_nonquery_end, T> To_Table(string name)
        {
            this.name = name;
            return this;
        }
    
        public query_builder_nonquery_end Values(T[] value)
        {
            this.values = value;
            return this;
        }

        public string ExecuteNonQuery()
        {
            var fieldsWithAttribute = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.IsDefined(typeof(QueryableField), false));

            string[] columns = (from field in fieldsWithAttribute select field.Name).ToArray();

            string[] values = new string[this.values.Length];  
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = string.Join(", ", from field in fieldsWithAttribute select Utils.FormatValue(field.GetValue(this.values[i])) ?? "NULL");
            }

            string query = $"INSERT INTO {name} ({string.Join(", ", columns)}) VALUES\n\t{string.Join(",\n\t", values.Select(v => $"({v})"))};";

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

    private class select_<T> :
        from_name_builder<query_builder_return_end<T>>,
        query_builder_return_end<T>
    {
        private string name;

        private SqliteCommand command;

        public select_(SqliteCommand command)
        {
            this.name = "";
            this.command = command;
        }

        public query_builder_return_end<T> From_Table(string name)
        {
            this.name = name;
            return this;
        }

        public T[] ExecuteQuery()
        {
            var fieldsWithAttribute = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.IsDefined(typeof(QueryableField), false));

            var columns = from field in fieldsWithAttribute select field.Name;
            string query = $"SELECT {string.Join(", ", columns)} FROM {this.name};";

            List<T> result = new List<T>();

            try
            {
                command.CommandText = query;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var temp = Activator.CreateInstance(typeof(T));

                        foreach (var field in fieldsWithAttribute)
                        {
                            int fieldIndex = reader.GetOrdinal(field.Name);
                            object fieldValue = reader.GetValue(fieldIndex);

                            object convertedValue = Convert.ChangeType(fieldValue, field.FieldType);

                            field.SetValue(temp, convertedValue);
                        }

                        result.Add((T)temp!);
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }

            return result.ToArray();
        }
    }

    private SqliteConnection? connection;

    protected void SetConnection(SqliteConnection connection)
    {
        this.connection = connection;
    }

    public abstract void Dispose();

    public table_name_builder<column_name_type_builder<looping_column_name_type_builder>> Create_table =>
        new create_table(connection!.CreateCommand());

    public table_name_builder<query_builder_nonquery_end> Create_Table_For<T>() =>
        new create_table_for<T>(connection!.CreateCommand());

    public table_name_builder<column_names_builder<row_values_builder>> Insert_Into_Table =>
        new insert_into(connection!.CreateCommand());

    public insert_table_name<insert_table_values<query_builder_nonquery_end, T>> Insert<T>() =>
        new insert_<T>(connection!.CreateCommand());

    public table_name_builder<query_builder_nonquery_end> Drop_Table =>
        new drop_table(connection!.CreateCommand());

    public table_name_builder<looping_table_column_alter_builder> Alter_Talbe =>
        new alter_table(connection!.CreateCommand());

    public table_name_builder<where_builder<query_builder_nonquery_end>> Delete_From_Table =>
        new delete_from(connection!.CreateCommand());

    public from_name_builder<query_builder_return_end<T>> Select<T>() =>
        new select_<T>(connection!.CreateCommand());
}