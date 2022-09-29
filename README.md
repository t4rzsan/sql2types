# sql2types
Generate F# record types from SQL tables.

## Introduction
This is a simple tool to generate F# record types from database tables on Microsoft SQL Server databases.  All it does is create a record type with properties according to the fields in the database table.

There is no synchronization and there is no annoying references to Entity Framework or anything.  All you get is a type that you can use as a starting point when working with Microsoft SQL Server, for example if you use [Dapper](https://github.com/DapperLib/Dapper).

## How to install
So far there is no cool dotnet tool or other means of installing sql2types.  Your only option is to get a copy of the code and run it.  This is a .NET 6.0 project, så you can use `dotnet run`.

## Usage
Run sql2types from a command prompt, for example like so:
```cmd
dotnet run --connectionstring "Integrated Security=SSPI;Initial Catalog=MyDatabase;Data Source=MySQLServer;Trust Server Certificate=True;" --schemaname "Data" --tablename "User" --outputfolder "c:\temp"
```
For a table `Data.User` defined like this
```sql
CREATE TABLE Data.User
(
    UserId INT PRIMARY KEY NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NULL,
    LastSignin: DATETIME2(7)
)
```
you will get a file `c:\temp\User.fs` looking like this:
```fsharp
type User = 
    {  UserId: int
       Name: string
       Email: string option
       LastSignin: System.DateTime }       
```
The tool will convert basic types like `VARCHAR`, `INT`, `BIGINT` and a few others to F# intrinsic types.  Other types, like `DATETIME2` will show as .NET types, like `System.DateTime`.  As you can see, the tool converts nullable columns to `option`.

Currently, the tool only works for on-premise Microsoft SQL Server (i.e. not Azure).  

## Todo
* Add possibility for Azure SQL databases (also with MFA).
* Add a way to run the tool for several tables in one go.
* Publish as dotnet tool.
* Add some tests.
* Add C# generator for C# classes and/or records.
