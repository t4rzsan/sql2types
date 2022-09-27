open CommandLine
open Microsoft.Data.SqlClient
open System
open System.Data
open System.IO
open System.Text

type CommandLineOptions =
    { [<Option(Required = true, HelpText = "The connection to your SQL Server database.")>]
      connectionString: string
      [<Option(Default = "dbo", HelpText = "The name of the database table schema for which go generate the code.")>]
      schemaName: string
      [<Option(Required = true,
               HelpText = "The name of the database table for which go generate the code.  The application will use the table named as '[schemaName].[tableName]'.")>]
      tableName: string
      [<Option(Required = true,
               HelpText = "The output folder where the program will put the generated code.  The output file will be named '[tableName].fs'")>]
      outputFolder: string }

type SchemaColumn =
    { Name: string
      DataType: Type
      AllowNull: bool option }

let getSqlSchema connectionString (schemaName: string) (tableName: string) =
    use cn = new SqlConnection(connectionString)
    use cmd = new SqlCommand($"SELECT * FROM [{schemaName}].[{tableName}]", cn)

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

let writeTypeClosing (writer: TextWriter) = writer.WriteLine(" }")

let mapTypeToString dataType =
    match dataType with
    | x when x = typeof<string> -> "string"
    | x when x = typeof<int> -> "int"
    | x when x = typeof<int64> -> "bigint"
    | x when x = typeof<double> -> "double"
    | x when x = typeof<float> -> "float"
    | x when x = typeof<decimal> -> "decimal"
    | x when x = typeof<bool> -> "bool"
    | _ -> dataType.ToString()

let writeProperty schemaColumn (writer: TextWriter) =
    writer.Write($"{schemaColumn.Name}: {schemaColumn.DataType |> mapTypeToString}")
    if schemaColumn.AllowNull |> Option.defaultValue false then
        writer.Write(" option")
    writer

let writeLine (writer: TextWriter) =
    writer.WriteLine()
    writer

let outputFSharp connectionString (schemaName: string) (tableName: string) (writer: TextWriter) =
    let schemaColumns = getSqlSchema connectionString schemaName tableName

    let schemaColumnsWithoutLastElement = schemaColumns[.. schemaColumns.Length - 2]

    // Compose one big function to write all properties (except the last one)
    // which we handle separately below because of the closing curly brace.
    let writeProperties =
        schemaColumnsWithoutLastElement
        |> Seq.fold
            (fun composedWriter schemaColumn ->
                composedWriter
                >> (writeProperty schemaColumn)
                >> writeLine
                >> writeIndentForProperty)
            (writeTypeOpening tableName)

    writer
    |> writeProperties
    // Add last property with closing curly brace.
    |> writeProperty (Array.last schemaColumns)
    |> writeTypeClosing

[<EntryPoint>]
let main args =
    let result = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)

    match result with
    | :? Parsed<CommandLineOptions> as parsed ->
        use writer =
            new StreamWriter(
                Path.Combine(parsed.Value.outputFolder, parsed.Value.tableName)
                + ".fs",
                false,
                Encoding.UTF8
            )

        outputFSharp parsed.Value.connectionString parsed.Value.schemaName parsed.Value.tableName writer

        0
    | :? NotParsed<CommandLineOptions> as notParsed -> 1
    | _ -> 0
