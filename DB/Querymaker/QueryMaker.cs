using System.Reflection;
using Microsoft.Data.Sqlite;

public abstract partial class QueryMaker : IDisposable
{
    public interface non_query_end
    {
        public abstract string ExecuteNonQuery();
    }

    public interface looping_table_column_alter_builder :
        non_query_end
    {
        public looping_table_column_alter_builder Rename_Table(string newname);

        public looping_table_column_alter_builder Add_Column(string name, string type);

        public looping_table_column_alter_builder Drop_Column(string name);

        public looping_table_column_alter_builder Rename_Column(string oldname, string newname);
    }

    public interface where_builder<NextT> : non_query_end
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

    private class alter_table :
        with_name<looping_table_column_alter_builder>,
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
        with_name<where_builder<non_query_end>>,
        where_builder<non_query_end>,
        non_query_end
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

        public where_builder<non_query_end> With_Name(string name)
        {
            this.name = name;
            return this;
        }

        public non_query_end Where(string condition)
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


    public with_name<looping_table_column_alter_builder> Alter_Talbe =>
        new alter_table(connection!.CreateCommand());

    public with_name<where_builder<non_query_end>> Delete_From_Table =>
        new delete_from(connection!.CreateCommand());

    public from_name_builder<query_builder_return_end<T>> Select<T>() =>
        new select_<T>(connection!.CreateCommand());
}