open Microsoft.Data.SqlClient
open System
open System.Data

let getSqlSchema connectionString (tableName: string) =
    use cn = new SqlConnection(connectionString)
    use cmd = new SqlCommand($"SELECT * FROM {tableName}", cn)

    cn.Open()
    use reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly)
    let schemaColumns = reader.GetColumnSchema()
    schemaColumns

let schemaColumns = getSqlSchema "Integrated Security=SSPI;Initial Catalog=FSharpForActuaries;Data Source=RODI\SQLEXPRESS;Trust Server Certificate=True;" "TestTable"

let mapTypeToFSharpType (dataType: Type) =
    if dataType = typeof<string> then
        "string"
    else
        "?"

for column in schemaColumns do
    printfn $"{column.ColumnName}, {column.ColumnSize}, {column.DataTypeName}, {column.DataType}, {column.AllowDBNull}"

