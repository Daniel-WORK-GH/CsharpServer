using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Data.Sqlite;

partial class QueryMaker
{
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

            if(collectionType != null)
            {
                var fieldsWithAttribute = collectionType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(field => field.IsDefined(typeof(QueryableField), false));

                string[] columns = (from field in fieldsWithAttribute select field.Name).ToArray();

                Type iCollectionType = typeof(ICollection<>).MakeGenericType(collectionType);

                if (iCollectionType.IsInstanceOfType(valuesCollection))
                {
                    dynamic collectionDynamic = Convert.ChangeType(valuesCollection, iCollectionType);

                    string[] values = new string[collectionDynamic.Count];  
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = string.Join(", ", from field in fieldsWithAttribute select Utils.FormatValue(field.GetValue(collectionDynamic[i])) ?? "NULL");
                    }

                    query = $"INSERT INTO {name} ({string.Join(", ", columns)}) VALUES\n\t{string.Join(",\n\t", values.Select(v => $"({v})"))};";
                }
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

    public with_name<to_columns_or_type> Insert_Into_Table =>
        new insert_into(connection!.CreateCommand());
}