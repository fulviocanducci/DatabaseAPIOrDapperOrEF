using CslAppDatabase.Models;
using Microsoft.Data.SqlClient;
using System.Text;
namespace CslAppDatabase.DAL
{
    public class DalPeople
    {
        public SqlConnection Connection { get; }
        public DalPeople(SqlConnection connection)
        {
            Connection = connection;
        }

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

        public bool Edit(People model)
        {
            using SqlCommand command = Connection.CreateCommand();
            StringBuilder strSql = new();
            strSql.Append("UPDATE People SET Name=@Name, Active=@Active WHERE Id=@Id");
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = strSql.ToString();
            command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = model.Name;
            command.Parameters.Add("@Name", System.Data.SqlDbType.VarChar, 50).Value = model.Name;
            command.Parameters.Add("@Active", System.Data.SqlDbType.Bit).Value = model.Active;
            Connection.Open();
            bool result = command.ExecuteNonQuery() > 0;
            Connection.Close();
            return result;
        }

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
                return new People
                    (
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetBoolean(2)
                    );
            }
            Connection.Close();
            return null;
        }

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
                    yield return new People
                        (
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetBoolean(2)
                        );
                }
            }
            Connection.Close();
        }

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
    }
}
