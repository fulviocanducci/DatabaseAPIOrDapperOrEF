# CslAppDatabase

## Overview

CslAppDatabase is a demonstration project showcasing three different approaches to database connectivity in C# .NET applications. The project implements a layered architecture with three distinct data access layers:

1. **API Layer (DAL)** - Raw ADO.NET using Microsoft.Data.SqlClient
2. **Dapper Layer** - Lightweight ORM using Dapper
3. **Entity Framework Layer** - Full-featured ORM using Entity Framework Core

The project demonstrates CRUD operations (Create, Read, Update, Delete) for a simple `People` entity across all three layers, allowing developers to compare performance, complexity, and code verbosity between different database access approaches.

## Architecture

```
┌─────────────────┐
│   Application   │
└─────────────────┘
         │
    ┌────┴────┐
    │  Layers │
    └────┴────┘
         │
    ┌────┼────┐
    │ API │ Dapper │ EF │
    └────┼────┘
         │
┌───────────────┐
│   Database    │
│   (SQL Server)│
└───────────────┘
```

## Project Structure

```
CslAppDatabase/
├── Program.cs              # Main application with test methods
├── Models/
│   └── People.cs           # Entity model and EF mapping
├── DAL/
│   └── DalPeople.cs        # Raw ADO.NET implementation
├── Dapper/
│   └── DapperPeople.cs     # Dapper ORM implementation
├── EF/
│   └── EFDatabase.cs       # Entity Framework DbContext
└── CslAppDatabase.csproj   # Project configuration
```

## Prerequisites

- **.NET 6.0 or later**
- **SQL Server** (LocalDB, Express, or full instance)
- **NuGet Packages:**
  - `Microsoft.Data.SqlClient`
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Dapper`
  - `Bogus` (for test data generation)

## Setup

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd CslAppDatabase
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Database Configuration:**
   - Update the connection string in `Program.cs` if needed
   - Default connection: `Server=127.0.0.1;Database=Test;MultipleActiveResultSets=true;User Id=sa;Password=senha;Encrypt=False;`

4. **Run the application:**
   ```bash
   dotnet run
   ```

## Database Schema

The project uses a simple `People` table:

```sql
CREATE TABLE [dbo].[People](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Name] [varchar](50) NOT NULL,
    [Active] [bit] NOT NULL,
    CONSTRAINT [PK_People] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
```

## Data Access Layers

### 1. API Layer (Raw ADO.NET)

The API layer uses raw ADO.NET with `Microsoft.Data.SqlClient` for direct database operations. This approach provides maximum control but requires manual SQL writing and parameter handling.

**Key Characteristics:**
- Lowest level of abstraction
- Maximum performance and control
- Manual connection management
- Verbose code for CRUD operations

**CRUD Operations:**

#### Insert
```csharp
public People Insert(People model)
{
    using SqlCommand command = Connection.CreateCommand();
    StringBuilder strSql = new();
    strSql.Append("INSERT INTO People(Name, Active) VALUES(@Name, @Active);");
    strSql.Append("SELECT CAST(SCOPE_IDENTITY() AS INT);");
    command.CommandType = System.Data.CommandType.Text;
    command.CommandText = strSql.ToString();
    command.Parameters.Add("@Name", System.Data.SqlDbType.VarChar, 50).Value = model.Name;
    command.Parameters.Add("@Active", System.Data.SqlDbType.Bit).Value = model.Active;
    Connection.Open();
    model.Id = (int)command.ExecuteScalar();
    Connection.Close();
    return model;
}
```

#### Edit
```csharp
public bool Edit(People model)
{
    using SqlCommand command = Connection.CreateCommand();
    StringBuilder strSql = new();
    strSql.Append("UPDATE People SET Name=@Name, Active=@Active WHERE Id=@Id");
    command.CommandType = System.Data.CommandType.Text;
    command.CommandText = strSql.ToString();
    command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = model.Id; // Fixed: was model.Name
    command.Parameters.Add("@Name", System.Data.SqlDbType.VarChar, 50).Value = model.Name;
    command.Parameters.Add("@Active", System.Data.SqlDbType.Bit).Value = model.Active;
    Connection.Open();
    bool result = command.ExecuteNonQuery() > 0;
    Connection.Close();
    return result;
}
```

#### Find
```csharp
public People? Find(int id)
{
    using SqlCommand command = Connection.CreateCommand();
    StringBuilder strSql = new();
    strSql.Append("SELECT Id, Name, Active FROM People WHERE Id=@Id");
    command.CommandType = System.Data.CommandType.Text;
    command.CommandText = strSql.ToString();
    command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
    Connection.Open();
    SqlDataReader reader = command.ExecuteReader();
    if (reader.HasRows)
    {
        reader.Read();
        return new People(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2));
    }
    Connection.Close();
    return null;
}
```

#### FindAll
```csharp
public IEnumerable<People> FindAll()
{
    using SqlCommand command = Connection.CreateCommand();
    StringBuilder strSql = new();
    strSql.Append("SELECT Id, Name, Active FROM People ORDER BY Name, Id");
    command.CommandType = System.Data.CommandType.Text;
    command.CommandText = strSql.ToString();
    Connection.Open();
    SqlDataReader reader = command.ExecuteReader();
    if (reader.HasRows)
    {
        while (reader.Read())
        {
            yield return new People(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2));
        }
    }
    Connection.Close();
}
```

#### Delete
```csharp
public bool Delete(int id)
{
    using SqlCommand command = Connection.CreateCommand();
    StringBuilder strSql = new();
    strSql.Append("DELETE FROM People WHERE Id=@Id");
    command.CommandType = System.Data.CommandType.Text;
    command.CommandText = strSql.ToString();
    command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
    Connection.Open();
    bool result = command.ExecuteNonQuery() > 0;
    Connection.Close();
    return result;
}
```

### 2. Dapper Layer

The Dapper layer uses the Dapper micro-ORM, which extends `IDbConnection` with dynamic SQL execution methods. It provides object mapping with minimal overhead.

**Key Characteristics:**
- Lightweight ORM
- SQL-first approach
- Better performance than full ORMs
- Less boilerplate than raw ADO.NET
- Still requires SQL writing

**CRUD Operations:**

#### Insert
```csharp
public People Insert(People model)
{
    StringBuilder strSql = new();
    strSql.Append("INSERT INTO People(Name, Active) VALUES(@Name, @Active);");
    strSql.Append("SELECT CAST(SCOPE_IDENTITY() AS INT);");
    model.Id = Connection.QueryFirst<int>(strSql.ToString(), model);
    return model;
}
```

#### Edit
```csharp
public bool Edit(People model)
{
    StringBuilder strSql = new();
    strSql.Append("UPDATE People SET Name=@Name, Active=@Active WHERE Id=@Id");
    return Connection.Execute(strSql.ToString(), model) > 0;
}
```

#### Find
```csharp
public People? Find(int id)
{
    StringBuilder strSql = new();
    strSql.Append("SELECT Id, Name, Active FROM People WHERE Id=@Id");
    return Connection.QueryFirst<People>(strSql.ToString(), new { Id = id });
}
```

#### FindAll
```csharp
public IEnumerable<People> FindAll()
{
    StringBuilder strSql = new();
    strSql.Append("SELECT Id, Name, Active FROM People ORDER BY Name, Id");
    return Connection.Query<People>(strSql.ToString());
}
```

#### Delete
```csharp
public bool Delete(int id)
{
    StringBuilder strSql = new();
    strSql.Append("DELETE FROM People WHERE Id=@Id");
    return Connection.Execute(strSql.ToString(), new { Id = id }) > 0;
}
```

### 3. Entity Framework Layer

The Entity Framework layer uses EF Core, Microsoft's full-featured ORM. It provides high-level abstractions, change tracking, and LINQ support.

**Key Characteristics:**
- Full-featured ORM
- LINQ support
- Automatic change tracking
- Code-first or database-first approaches
- Most abstracted layer
- Rich ecosystem and tooling

**CRUD Operations:**

#### Insert
```csharp
static void TesteByEFClientInsert()
{
    DbContextOptionsBuilder<EFDatabase> dbOptionsBuilder = new DbContextOptionsBuilder<EFDatabase>();
    dbOptionsBuilder.UseSqlServer(GetConnectString());
    EFDatabase efDatabase = new EFDatabase(dbOptionsBuilder.Options);
    People people = NewPeople();
    efDatabase.People.Add(people);
    efDatabase.SaveChanges();
    Console.WriteLine(people.Id);
}
```

#### Edit
```csharp
// EF Edit operation (not shown in current code, but typically:)
using (var context = new EFDatabase(options))
{
    var person = context.People.Find(id);
    if (person != null)
    {
        person.Name = "Updated Name";
        person.Active = true;
        context.SaveChanges();
    }
}
```

#### Find
```csharp
// EF Find operation (not shown in current code, but typically:)
using (var context = new EFDatabase(options))
{
    var person = context.People.Find(id);
    return person;
}
```

#### FindAll
```csharp
static void TesteByEFClientFindAll()
{
    DbContextOptionsBuilder<EFDatabase> dbOptionsBuilder = new DbContextOptionsBuilder<EFDatabase>();
    dbOptionsBuilder.UseSqlServer(GetConnectString());
    EFDatabase efDatabase = new EFDatabase(dbOptionsBuilder.Options);
    foreach (People p in efDatabase.People)
    {
        Console.WriteLine("{0} {1} {2}", p.Id, p.Name, p.Active ? "true" : "false");
    }
}
```

#### Delete
```csharp
// EF Delete operation (not shown in current code, but typically:)
using (var context = new EFDatabase(options))
{
    var person = context.People.Find(id);
    if (person != null)
    {
        context.People.Remove(person);
        context.SaveChanges();
    }
}
```

## Usage

The `Program.cs` file contains test methods for each layer. Uncomment the desired test method calls at the bottom of the file:

```csharp
// Uncomment to run API layer tests
// TesteByAPIMicrosoftDataSqlClientInsert();
// TesteByAPIMicrosoftDataSqlClientFindAll();

// Uncomment to run Dapper layer tests
// TesteByDapperClientInsert();
// TesteByDapperClientFindAll();

// Uncomment to run EF layer tests
// TesteByEFClientInsert();
// TesteByEFClientFindAll();
```

### Running Tests

1. **Create the database table:**
   The `CreateTableTest()` method will automatically create the `People` table if it doesn't exist.

2. **Run specific tests:**
   - API Layer: `TesteByAPIMicrosoftDataSqlClientInsert()` / `TesteByAPIMicrosoftDataSqlClientFindAll()`
   - Dapper Layer: `TesteByDapperClientInsert()` / `TesteByDapperClientFindAll()`
   - EF Layer: `TesteByEFClientInsert()` / `TesteByEFClientFindAll()`

## Performance Comparison

- **API (Raw ADO.NET)**: Fastest performance, most control, highest code verbosity
- **Dapper**: Excellent performance with minimal overhead, balanced code complexity
- **Entity Framework**: Rich features and abstractions, slightly slower due to change tracking

Choose the appropriate layer based on your project requirements:
- Use **API** for maximum performance and control
- Use **Dapper** for a good balance of performance and developer productivity
- Use **EF** for complex applications requiring advanced ORM features

## Dependencies

- `Microsoft.Data.SqlClient` - ADO.NET data provider for SQL Server
- `Microsoft.EntityFrameworkCore.SqlServer` - EF Core SQL Server provider
- `Dapper` - Micro-ORM for .NET
- `Bogus` - Fake data generator for testing