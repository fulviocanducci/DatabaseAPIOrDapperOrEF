using CslAppDatabase.DAL;
using CslAppDatabase.Dapper;
using CslAppDatabase.EF;
using CslAppDatabase.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

static bool CreateTableTest()
{
    bool create = false;
    using (var conn = new SqlConnection(GetConnectString()))
    {
        conn.Open();
        using (var cmd = new SqlCommand("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'People'", conn))
        {
            if (cmd.ExecuteScalar() == null)
            {
                cmd.CommandText = @"                    
                    CREATE TABLE [dbo].[People](
	                    [Id] [int] IDENTITY(1,1) NOT NULL,
	                    [Name] [varchar](50) NOT NULL,
	                    [Active] [bit] NOT NULL,
                     CONSTRAINT [PK_People] PRIMARY KEY CLUSTERED 
                    ([Id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
                    ) ON [PRIMARY]
                ";
            }
            create = cmd.ExecuteNonQuery() > 0;
        }
        conn.Close();
    }
    return create;
}
static string GetConnectString()
{
    return "Server=127.0.0.1;Database=Test;MultipleActiveResultSets=true;User Id=sa;Password=senha;Encrypt=False;";
}

static SqlConnection GetSqlConnection()
{
    return new SqlConnection(GetConnectString());
}

static People NewPeople()
{
    return new People
    {
        Id = 0,
        Name = Faker.Name.FullName(),
        Active = Faker.Boolean.Random()
    };
}

#region API
static void TesteByAPIMicrosoftDataSqlClientInsert()
{
    SqlConnection connection = GetSqlConnection();
    DalPeople dal = new DalPeople(connection);
    for (int i = 0; i < 10; i++)
    {
        People p = NewPeople();
        dal.Insert(p);
        Console.WriteLine(p.Id);
    }
}

static void TesteByAPIMicrosoftDataSqlClientFindAll()
{
    SqlConnection connection = GetSqlConnection();
    DalPeople dal = new DalPeople(connection);
    foreach (People p in dal.FindAll())
    {
        Console.WriteLine("{0} {1} {2}", p.Id, p.Name, p.Active ? "true" : "false");
    }
}
#endregion API

#region DAPPER
static void TesteByDapperClientInsert()
{
    SqlConnection connection = GetSqlConnection();
    DapperPeople dapperPeople = new DapperPeople(connection);
    for (int i = 0; i < 10000; i++)
    {
        People p = NewPeople();
        dapperPeople.Insert(p);
        Console.WriteLine(p.Id);
    }
}
static void TesteByDapperClientFindAll()
{
    SqlConnection connection = GetSqlConnection();
    DapperPeople dapperPeople = new DapperPeople(connection);
    foreach (People p in dapperPeople.FindAll())
    {
        Console.WriteLine("{0} {1} {2}", p.Id, p.Name, p.Active ? "true" : "false");
    }
}
#endregion DAPPER

#region EF
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
#endregion EF
//TesteByEFClientInsert();
CreateTableTest();
TesteByAPIMicrosoftDataSqlClientInsert();
TesteByEFClientFindAll();
