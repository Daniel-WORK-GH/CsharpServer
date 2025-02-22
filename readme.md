# Implemented Commands 

### DROP TABLE {name};
Usage: 
```cs
Querymaker.Drop_Table.With_Name("name");
```

### CREATE TABLE {name} ({columns});
Usage:
```cs
Querymaker.Create_Table.With_Name("name")
    .Add_Column("col1", "value")
    .Add_Column("col2", "value") 
    ... ;
```

### INSERT INTO {name} ({columns}) VALUES ({values});
Usage:
```cs
Querymaker.Insert_Into_Table.With_Name("name").
    To_Columns("col1", "value")
    .Add_Value(val1, val2, val3, ... )
    .Add_Value(val1, val2, val3, ... ) 
    .Add_Value(val1, val2, val3, ... ) 
    ... ;
```

### ALTER TABLE {name} {alters};
Usage:
```cs
Querymaker.Alter_Talbe.With_Name("oldname")
    .Rename_Table("newname")
    .Add_Column("col5", "int")
    .Add_Column("col7", "int")
    .Rename_Column("col7", "col8")
    .Drop_Column("col1")
    ... ;
```

### DELETE FROM {name} WHERE {condition};
Usage:
```cs
Querymaker.Delete_From_Table.With_Name("tablename")
    .Where("age >= 50");
```

<br><br>

# Functions that use created classes:
Use the ```QueryableField``` attribute on fields to support reading/writing from the database.

Example:
```cs
class User 
{
    [QueryableField]
    private int id; // Will be used regardless on 'private'.

    [QueryableField]
    public string? name; // Will be used.

    [QueryableField]
    private readonly string? surname;  // Will be used regardless on 'private'.
                                       // Data will be forced written into it.

    public int ignored; // Will be ignored in queries.
}

class Program
{
    public static void Main(string[] args)
    {
        var users = db.Select<User>().From_Table("Users").ExecuteQuery();

        // Use database users here..
    }
}

```

### SELECT {type} FROM {name};
Usage:
```cs
Querymaker.Select<Type>()
    .From_Table("name")
    .ExecuteQuery();
```

### INSERT {type} TO {table} VALUES {values};
Usage:
```cs
Querymaker.Insert<Type>()
    .To_Table("table_name")
    .Values({values})
    .ExecuteNonQuery()
```

### CREATE TABLE {name} FOR {type};
Usage:
```cs
Querymaker.Create_Table_For<Type>().With_Name("name");
```