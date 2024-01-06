# bamdb

The tool **bamdb** is used to manage, generate and run code related to data access objects.  Data access objects are used to store and read data from relational database management systems and data repositories.  Additionally, **bamdb** can host data access service api endpoints and it provides command line access to CRUD (Create, Retrieve, Update, Delete) functionality exposed by such services.

## Generator
The following describes options for generating data access objects.

### Required parameters
- **--from** - The source that data structures are derived from.  This can be one of the following:
  - *database connection string* - specify a connection string to an existing database for one of the supported relational database types, see [From an existing database](#from-an-existing-database).
  - *path to a .NET assembly* - specify the path to an existing .NET assembly that contains data types that are intended to be persisted.  See, *fromNamespace* in [Optional parameters](#optional-parameters).
  - *path to directory containing c# code files* - specify the path to an directory that contains c# code files.  To generate data access code for classes in a specific namespace specify the *fromNamespace* parameter.  See, *fromNamespace* in [Optional parameters](#optional-parameters).
  - *path to an open api specification* - specify the path to an open api specification.
- **--out** - The destination to write generated files to.  This can be one of the following:
  - *directory path* - specify the path to a directory where generated code files are written
  - *assembly path* - specify the path to an assembly and the generated code files are compiled to the specified file.

### Optional parameters

- --checkForIds:***[true | false]*** - Checks that source data types have an Id property and throws an exception if it is missing. 
- --fromNamespace:***[namespace]*** - The namespace to look for data types in.
- --schemaName:***[schemaName]*** - The name to give to the gnerated schema.
- --templatePath:***[/filesystem/path/to/templates]*** - Override the default templates with those found in the specified directory.
- --toNamespace:***[namespace]*** - The namespace to place generated classes into.

### Generate C# data access objects
There are a number of different options to generate C# classes for data access operations.

- From an existing database
- From C# source files
- From an existing assembly
- From an open api specification
- From a javascript literal file

These options are discussed further in the following sections.

### From an existing database
To extract a schema and generate related C# code, specify a database connection string as the `--from` parameter.  Also, specify the database type using the `--dbType` parameter.  Supported database types are:

- mssql - Microsoft Sql Server
- mysql - MySQL
- oracle - Oracle
- postgres - PostGreSQL

```
bamdb generate --from:"[database connection string]" --out:./Dao --dbType:[dbType]
```

### From C# source files
To generate data access objects from plain C# classes specify the directory where the source code is found.  Also, specify the namespace where existing data classes are defined in the existing source code.

```
bamdb generate --from:./srcDir --out:./Dao --fromNamespace:[existing.dataClass.namespace]
```

### From an existing assembly

```
bamdb generate --from:./assembly.dll --out:./Dao
```

## Relationship conventions
To ensure that appropriate foreign keys and cross reference tables are defined during generation the following conventions must be considered.

### Child collections (one to many)
To define a parent to children or one to many relationship be sure to define the enumerable property on the parent as `virtual` and specify a property of type `ulong` on the child whose name is in the form "<***parentTypeName***>Id", for example:

```
public virtual List<Child> Children { get; set; }
```

### Cross reference collections (many to many)
To define a cross reference or many to many relationship be sure t

## Generate C# data access object classes from a javascript literal file

```
bamdb generate --from:database.js --out:./Dao
```

## Generate a data access object assembly
To compile an assembly from the generated C# source code, specify the path to a .dll file as the `--out` parameter instead of a directory.

```
bamdb generate --from:<connectionString | srcDir | assembly.dll | database.js> --out:./path/to/<your-assembly-name>.dll
```

# Serve Data Access API

