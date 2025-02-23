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
        Database db = new Database("Database");

        db.Insert_Into_Table.With_Name("Users")
            .Values([new User() { name = "dan" }, new User() { name = "daniel" }])
            .ExecuteNonQuery();

        var users = db.Select.Type<User>().From_Table("Users").ExecuteQuery();

        foreach (var u in users)
        {
            System.Console.WriteLine(u.name);
        }
    }
}
```