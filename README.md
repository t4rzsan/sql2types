# sql2types
Generate F# record types from SQL tables.

## Introduction
This is a simple tool to generate F# record type from SQL databases.  All it does is create a record type with properties according to the fields in the database table.

There is no synchronization and there is no annoying references to Entity Framework or anything.  All you get is a type that you can use as a starting point when working with Microsoft SQL Server, for example if you use [Dapper](https://github.com/DapperLib/Dapper).

## How to install
So far there is no cool dotnet tool or other means of installing sql2types.  Your only option is to get a copy of the code and compile it.

# Usage
Run sql2types from a command prompt, for example like so:
```cmd
sql2types.exe --connectionstring "Integrated Security=SSPI;Initial Catalog=MyDatabase;Data Source=MySQLServer;Trust Server Certificate=True;" --schemaname "Data" --tablename "User" --outputfolder "c:\temp"
```
In this example, the tool will generate a record type named `User` in a file name `c:\temp\User.fs` from the database table `Data.User`.

Currently, the tool only works for on-premise Microsoft SQL Server (i.e. not Azure).  

# TODO
* Add possibility for Azure SQL databases (also with MFA).
* Add a way to run the tool for several tables in one go.
* Publish as dotnet tool.
* Add some tests.
* Add C# generator for C# classes and/or records.
* 