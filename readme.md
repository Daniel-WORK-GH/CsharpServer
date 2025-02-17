# Implemented Commands 

### DROP TABLE {name};
Usage: 
```cs
Querymaker.Drop_Table.With_Name("name");
```

### CREATE TABLE {name} ({columns});
Usage:
```cs
Querymaker.Create_Table.With_Name("name").Add_Column("col1", "value").Add_Column("col2", "value") ... ;
```

### INSERT INTO {name} ({columns}) VALUES ({values});
Usage:
```cs
Querymaker.Insert_Into_Table.With_Name("name").To_Columns("col1", "value")
    .Add_Value(val1, val2, val3, ... )
    .Add_Value(val1, val2, val3, ... ) 
    .Add_Value(val1, val2, val3, ... ) 
    ... ;
```

