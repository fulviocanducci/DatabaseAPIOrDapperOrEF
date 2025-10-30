using CslAppDatabase.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace CslAppDatabase.Dapper
{
    public class DapperPeople
    {
        public SqlConnection Connection { get; }
        public DapperPeople(SqlConnection connection)
        {
            Connection = connection;
        }

        public People Insert(People model)
        {
            StringBuilder strSql = new();
            strSql.Append("INSERT INTO People(Name, Active) VALUES(@Name, @Active);");
            strSql.Append("SELECT CAST(SCOPE_IDENTITY() AS INT);");
            model.Id = Connection.QueryFirst<int>(strSql.ToString(), model);
            return model;
        }

        public bool Edit(People model)
        {
            StringBuilder strSql = new();
            strSql.Append("UPDATE People SET Name=@Name, Active=@Active WHERE Id=@Id");
            return Connection.Execute(strSql.ToString(), model) > 0;
        }

        public People? Find(int id)
        {
            StringBuilder strSql = new();
            strSql.Append("SELECT Id, Name, Active FROM People WHERE Id=@Id");
            return Connection.QueryFirst<People>(strSql.ToString(), new { Id = id });
        }

        public IEnumerable<People> FindAll()
        {
            StringBuilder strSql = new();
            strSql.Append("SELECT Id, Name, Active FROM People ORDER BY Name, Id");
            return Connection.Query<People>(strSql.ToString());
        }

        public bool Delete(int id)
        {
            StringBuilder strSql = new();
            strSql.Append("DELETE FROM People WHERE Id=@Id");
            return Connection.Execute(strSql.ToString(), new { Id = id }) > 0;
        }
    }
}
