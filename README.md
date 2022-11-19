# EntityFramework.Extensions.Procedure

[![NuGet](https://img.shields.io/nuget/v/Abp.svg?style=flat-square)](https://www.nuget.org/packages/Abp)
[![NuGet Download](https://img.shields.io/nuget/dt/Abp.svg?style=flat-square)](https://www.nuget.org/packages/Abp)

## What is EntityFramework.Extensions.Procedure ?
This is an extension method which helps to call stored procedures with DbContext class of Microsoft.EntityFrameworkCore. 

## How it works
![image](https://user-images.githubusercontent.com/25504137/202475041-67fff0f5-d066-4280-a170-8d03769ece50.png)

The moest important parameater among these is commandBehavior, it will allow to define results of the query and its effect on the database.
1. **Default** - The query may return multiple result sets. Execution of the query may affect the database state.
2. **SingleResult** - The query returns a single result set.
3. **SchemaOnly** - The query returns column and primary key information.
4. **KeyInfo** - The query returns column and primary key information. The provider appends extra columns to the result set for existing primary key and timestamp columns.
5. **SingleRow** - The query is expected to return a single row of the first result set. Execution of the query may affect the database state.
6. **SequentialAccess** - Provides a way for the DataReader to handle rows that contain columns with large binary values.
7. **CloseConnection** - When the command is executed, the associated Connection object is closed when the associated DataReader object is closed.

Further, as if it is expected to return multiple sets of data, it is required to define a result set(as a list of objects) and the expected output types(ex - Dtos or data modeles) with a default constructor(zero parameters). 



## Nuget Packages

EntityFramework.Extensions.Procedure is distributed as NuGet packages.

|Package|Status|
|:------|:-----:|
|EntityFramework.Extensions.Procedure|[![NuGet version](https://avatars.githubusercontent.com/u/7880472?s=200&v=4)](https://badge.fury.io/nu/Abp)|

