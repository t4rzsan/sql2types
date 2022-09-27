open Microsoft.Data.SqlClient
open System
open System.Data
open System.IO

type SchemaColumn =
    { Name: string
      DataType: Type
      AllowNull: bool option }

let getSqlSchema connectionString (tableName: string) =
    use cn = new SqlConnection(connectionString)
    use cmd = new SqlCommand($"SELECT * FROM {tableName}", cn)

    cn.Open()
    use reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly)
    let schemaColumns = reader.GetColumnSchema()

    schemaColumns
    |> Seq.map (fun col ->
        { Name = col.ColumnName
          DataType = col.DataType
          AllowNull = col.AllowDBNull |> Option.ofNullable })
    |> Array.ofSeq

let writeIndent (writer: TextWriter) = writer.Write("    ")

let writeIndentForProperty (writer: TextWriter) =
    writeIndent writer
    writer.Write("  ")
    writer

let writeTypeOpening typeName (writer: TextWriter) =
    writer.WriteLine($"type {typeName} =")
    writer |> writeIndent
    writer.Write("{ ")
    writer

let writeClosingCurlyBrace (writer: TextWriter) = writer.WriteLine(" }")

let mapTypeToFSharpType dataType =
    match dataType with
    | x when x = typeof<string> -> "string"
    | x when x = typeof<int> -> "int"
    | x when x = typeof<int64> -> "bigint"
    | x when x = typeof<double> -> "double"
    | x when x = typeof<float> -> "float"
    | x when x = typeof<decimal> -> "decimal"
    | x when x = typeof<bool> -> "bool"
    | _ -> dataType.ToString().Replace("System.", "")

let writeProperty schemaColumn (writer: TextWriter) =
    writer.Write($"{schemaColumn.Name}: {schemaColumn.DataType |> mapTypeToFSharpType}")
    writer

let writeLine (writer: TextWriter) = 
    writer.WriteLine()
    writer

let outputFSharp (schema: string) (table: string) (writer: TextWriter) =
    let schemaColumns =
        getSqlSchema
            "Integrated Security=SSPI;Initial Catalog=SrgRegnskab_01;Data Source=PIN-SQL03;Trust Server Certificate=True;"
            $"{schema}.{table}"

    let schemaColumnsWithoutLastElement = schemaColumns[..schemaColumns.Length - 2]

    let writeProperties =
        schemaColumnsWithoutLastElement
        |> Seq.fold
            (fun composedWriter schemaColumn ->
                composedWriter
                >> (writeProperty schemaColumn)
                >> writeLine
                >> writeIndentForProperty)
            (writeTypeOpening table)

    writer
    |> writeProperties
    |> writeProperty (Array.last schemaColumns)
    |> writeClosingCurlyBrace

[<EntryPoint>]
let main args =
    printfn "%A" args
    use writer = new StringWriter()
    outputFSharp "Data" "Skade" writer
    printfn "%s" (writer.ToString())

    0
