# CslAppDatabase

## Visão Geral

CslAppDatabase é um projeto de demonstração que apresenta três abordagens diferentes para conectividade de banco de dados em aplicações C# .NET. O projeto implementa uma arquitetura em camadas com três camadas distintas de acesso a dados:

1. **Camada API (DAL)** - ADO.NET bruto usando Microsoft.Data.SqlClient
2. **Camada Dapper** - ORM leve usando Dapper
3. **Camada Entity Framework** - ORM completo usando Entity Framework Core

O projeto demonstra operações CRUD (Create, Read, Update, Delete) para uma entidade simples `People` em todas as três camadas, permitindo aos desenvolvedores comparar desempenho, complexidade e verbosidade de código entre diferentes abordagens de acesso a banco de dados.

## Arquitetura

```
┌─────────────────┐
│   Aplicação     │
└─────────────────┘
         │
    ┌────┴────┐
    │ Camadas │
    └────┴────┘
         │
    ┌────┼────┐
    │ API │ Dapper │ EF │
    └────┼────┘
         │
┌───────────────┐
│   Banco de    │
│   Dados       │
│   (SQL Server)│
└───────────────┘
```

## Estrutura do Projeto

```
CslAppDatabase/
├── Program.cs              # Aplicação principal com métodos de teste
├── Models/
│   └── People.cs           # Modelo de entidade e mapeamento EF
├── DAL/
│   └── DalPeople.cs        # Implementação ADO.NET bruto
├── Dapper/
│   └── DapperPeople.cs     # Implementação ORM Dapper
├── EF/
│   └── EFDatabase.cs       # DbContext do Entity Framework
└── CslAppDatabase.csproj   # Configuração do projeto
```

## Pré-requisitos

- **.NET 6.0 ou posterior**
- **SQL Server** (LocalDB, Express ou instância completa)
- **Pacotes NuGet:**
  - `Microsoft.Data.SqlClient`
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Dapper`
  - `Bogus` (para geração de dados de teste)

## Configuração

1. **Clone o repositório:**
   ```bash
   git clone <url-do-repositório>
   cd CslAppDatabase
   ```

2. **Restaure as dependências:**
   ```bash
   dotnet restore
   ```

3. **Configuração do Banco de Dados:**
   - Atualize a string de conexão em `Program.cs` se necessário
   - Conexão padrão: `Server=127.0.0.1;Database=Test;MultipleActiveResultSets=true;User Id=sa;Password=senha;Encrypt=False;`

4. **Execute a aplicação:**
   ```bash
   dotnet run
   ```

## Esquema do Banco de Dados

O projeto usa uma tabela simples `People`:

```sql
CREATE TABLE [dbo].[People](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Name] [varchar](50) NOT NULL,
    [Active] [bit] NOT NULL,
    CONSTRAINT [PK_People] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
```

## Camadas de Acesso a Dados

### 1. Camada API (ADO.NET Bruto)

A camada API usa ADO.NET bruto com `Microsoft.Data.SqlClient` para operações diretas no banco de dados. Esta abordagem fornece controle máximo, mas requer escrita manual de SQL e manipulação de parâmetros.

**Características Principais:**
- Nível mais baixo de abstração
- Máximo desempenho e controle
- Gerenciamento manual de conexões
- Código verboso para operações CRUD

**Operações CRUD:**

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
    command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = model.Id; // Corrigido: era model.Name
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

### 2. Camada Dapper

A camada Dapper usa o micro-ORM Dapper, que estende `IDbConnection` com métodos de execução SQL dinâmica. Ele fornece mapeamento de objetos com sobrecarga mínima.

**Características Principais:**
- ORM leve
- Abordagem SQL-first
- Melhor desempenho que ORMs completos
- Menos boilerplate que ADO.NET bruto
- Ainda requer escrita de SQL

**Operações CRUD:**

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

### 3. Camada Entity Framework

A camada Entity Framework usa EF Core, o ORM completo da Microsoft. Ele fornece abstrações de alto nível, rastreamento de mudanças e suporte a LINQ.

**Características Principais:**
- ORM completo
- Suporte a LINQ
- Rastreamento automático de mudanças
- Abordagens code-first ou database-first
- Camada mais abstrata
- Ecossistema e ferramentas ricos

**Operações CRUD:**

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
// Operação Edit do EF (não mostrada no código atual, mas tipicamente:)
using (var context = new EFDatabase(options))
{
    var person = context.People.Find(id);
    if (person != null)
    {
        person.Name = "Nome Atualizado";
        person.Active = true;
        context.SaveChanges();
    }
}
```

#### Find
```csharp
// Operação Find do EF (não mostrada no código atual, mas tipicamente:)
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
// Operação Delete do EF (não mostrada no código atual, mas tipicamente:)
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

## Uso

O arquivo `Program.cs` contém métodos de teste para cada camada. Descomente as chamadas de método de teste desejadas no final do arquivo:

```csharp
// Descomente para executar testes da camada API
// TesteByAPIMicrosoftDataSqlClientInsert();
// TesteByAPIMicrosoftDataSqlClientFindAll();

// Descomente para executar testes da camada Dapper
// TesteByDapperClientInsert();
// TesteByDapperClientFindAll();

// Descomente para executar testes da camada EF
// TesteByEFClientInsert();
// TesteByEFClientFindAll();
```

### Executando Testes

1. **Crie a tabela do banco de dados:**
   O método `CreateTableTest()` criará automaticamente a tabela `People` se ela não existir.

2. **Execute testes específicos:**
   - Camada API: `TesteByAPIMicrosoftDataSqlClientInsert()` / `TesteByAPIMicrosoftDataSqlClientFindAll()`
   - Camada Dapper: `TesteByDapperClientInsert()` / `TesteByDapperClientFindAll()`
   - Camada EF: `TesteByEFClientInsert()` / `TesteByEFClientFindAll()`

## Comparação de Desempenho

- **API (ADO.NET Bruto)**: Desempenho mais rápido, mais controle, maior verbosidade de código
- **Dapper**: Excelente desempenho com sobrecarga mínima, complexidade de código equilibrada
- **Entity Framework**: Recursos ricos e abstrações, ligeiramente mais lento devido ao rastreamento de mudanças

Escolha a camada apropriada baseada nos requisitos do seu projeto:
- Use **API** para máximo desempenho e controle
- Use **Dapper** para um bom equilíbrio entre desempenho e produtividade do desenvolvedor
- Use **EF** para aplicações complexas que requerem recursos avançados de ORM

## Dependências

- `Microsoft.Data.SqlClient` - Provedor de dados ADO.NET para SQL Server
- `Microsoft.EntityFrameworkCore.SqlServer` - Provedor SQL Server do EF Core
- `Dapper` - Micro-ORM para .NET
- `Bogus` - Gerador de dados falsos para testes