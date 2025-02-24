using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

public abstract partial class QueryMaker : IDisposable
{
    #region RECURRING

    public interface query_end<T>
    {
        public T[] ExecuteQuery();
    }

    public interface non_query_end
    {
        public abstract string ExecuteNonQuery();
    }

    public interface with_name<NextT>
    {
        public NextT With_Name(string name);
    }

    #endregion


    #region CREATE TABLE

    public interface add_column
    {
        public add_columns Add_Column(string name, string type);

        public non_query_end For<T>();
    }

    public interface add_columns : add_column, non_query_end
    {

    }


    private class create_table : with_name<add_column>, add_column, add_columns
    {
        private string name;

        private List<string> columns;

        private SqliteCommand command;

        private Type? tableType;

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

        public non_query_end For<T>()
        {
            this.tableType = typeof(T);
            return this;
        }

        public add_column With_Name(string name)
        {
            this.name = name;
            return this;
        }

        public string ExecuteNonQuery()
        {
            string query = "";
            if (tableType != null)
            {
                var fieldsWithAttribute = tableType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(field => field.IsDefined(typeof(QueryableField), false));

                var columns = from field in fieldsWithAttribute select $"{field.Name} {Utils.MapToSQLiteType(field.FieldType)}";

                query = $"CREATE TABLE {name} {(columns.Count() == 0 ? "" : $"(\n\t{string.Join(",\n\t", columns)}\n)")};";

            }
            else
            {
                query = $"CREATE TABLE {name} {(columns.Count == 0 ? "" : $"(\n\t{string.Join(",\n\t", this.columns)}\n)")};";
            }

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

    #endregion


    #region DROP TABLE

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

    #endregion


    #region ALTER TABLE

    public interface alter_table_parts : non_query_end
    {
        public alter_table_parts Rename_Table(string newname);

        public alter_table_parts Add_Column(string name, string type);

        public alter_table_parts Drop_Column(string name);

        public alter_table_parts Rename_Column(string oldname, string newname);
    }

    private class alter_table :
        with_name<alter_table_parts>,
        alter_table_parts
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

        public alter_table_parts With_Name(string name)
        {
            this.name = name;
            return this;
        }

        public alter_table_parts Add_Column(string name, string type)
        {
            this.alters.Add($"ADD COLUMN {name} {type}");
            return this;
        }
        public alter_table_parts Drop_Column(string name)
        {
            this.alters.Add($"DROP COLUMN {name}");
            return this;
        }

        public alter_table_parts Rename_Column(string oldname, string newname)
        {
            this.alters.Add($"RENAME COLUMN {oldname} TO {newname}");
            return this;
        }

        public alter_table_parts Rename_Table(string newname)
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

    #endregion


    #region INSERT INTO

    public interface to_columns_or_type
    {
        public add_values To_Columns(params string[] columns);

        public non_query_end Values<T>(ICollection<T> value);
    }

    public interface add_values : non_query_end
    {
        public add_values Add_Values(params object[] type);
    }

    private class insert_into : with_name<to_columns_or_type>, to_columns_or_type, add_values
    {
        private string name;

        private List<string> columns;

        private List<string> rows;

        private Type? collectionType;

        private object? valuesCollection;

        private SqliteCommand command;

        public insert_into(SqliteCommand command)
        {
            this.name = "";
            this.columns = new List<string>();
            this.rows = new List<string>();
            this.command = command;
        }

        public to_columns_or_type With_Name(string name)
        {
            this.name = name;
            return this;
        }

        public add_values To_Columns(params string[] columns)
        {
            foreach (string column in columns)
            {
                this.columns.Add(column);
            }

            return this;
        }

        public non_query_end Values<T>(ICollection<T> value)
        {
            collectionType = typeof(T);
            valuesCollection = value;
            return this;
        }

        public add_values Add_Values(params object[] type)
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
            string query = "";

            if (collectionType != null)
            {
                var fieldsWithAttribute = collectionType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(field => field.IsDefined(typeof(QueryableField), false));

                string[] columns = (from field in fieldsWithAttribute select field.Name).ToArray();

                ICollection collection = (ICollection)this.valuesCollection!;
                List<string> values = new();

                int i = 0;
                foreach (var c in collection)
                {
                    values.Add(string.Join(", ", from field in fieldsWithAttribute select Utils.FormatValue(field.GetValue(c)) ?? "NULL"));
                    i++;
                }

                query = $"INSERT INTO {name} ({string.Join(", ", columns)}) VALUES\n\t{string.Join(",\n\t", values.Select(v => $"({v})"))};";

            }
            else
            {
                query = $"INSERT INTO {name} ({string.Join(", ", this.columns)}) VALUES\n\t{string.Join(",\n\t", this.rows)};";
            }

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

    #endregion


    #region DELETE FROM 

    public interface where_builder : non_query_end
    {
        public non_query_end Where(string condition);
    }

    private class delete_from :
        with_name<where_builder>,
        where_builder,
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

        public where_builder With_Name(string name)
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

    #endregion


    #region SELECT

    public interface type_selector
    {
        public from_table<T> Type<T>();
    }

    public interface from_table<T>
    {
        public query_end<T> From_Table(string name);
    }

    private class select_ : type_selector
    {
        private class select_return<T> : from_table<T>, query_end<T>
        {
            private string name;

            private SqliteCommand command;

            public select_return(SqliteCommand command)
            {
                this.name = "";
                this.command = command;
            }

            public query_end<T> From_Table(string name)
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

                                object? convertedValue = null;
                                if (fieldValue != DBNull.Value)
                                {
                                    if (Nullable.GetUnderlyingType(field.FieldType) != null)
                                    {
                                        convertedValue = Convert.ChangeType(fieldValue, Nullable.GetUnderlyingType(field.FieldType)!);
                                    }
                                    else
                                    {
                                        convertedValue = Convert.ChangeType(fieldValue, field.FieldType);
                                    }
                                }
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

        private SqliteCommand command;

        public select_(SqliteCommand command)
        {
            this.command = command;
        }

        public from_table<T> Type<T>()
        {
            return new select_return<T>(command);
        }
    }

    #endregion

    private SqliteConnection? connection;

    protected void SetConnection(SqliteConnection connection) => this.connection = connection;

    public abstract void Dispose();

    public with_name<add_column> Create_table =>
        new create_table(connection!.CreateCommand());

    public with_name<non_query_end> Drop_Table =>
        new drop_table(connection!.CreateCommand());

    public with_name<alter_table_parts> Alter_Talbe =>
        new alter_table(connection!.CreateCommand());

    public with_name<to_columns_or_type> Insert_Into_Table =>
        new insert_into(connection!.CreateCommand());

    public with_name<where_builder> Delete_From_Table =>
        new delete_from(connection!.CreateCommand());

    public type_selector Select =>
        new select_(connection!.CreateCommand());
}