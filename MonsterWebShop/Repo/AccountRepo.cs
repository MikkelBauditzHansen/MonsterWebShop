using Microsoft.Data.SqlClient;
using MonsterWebShop.Models;

namespace MonsterWebShop.Repo
{
    public class AccountRepo : IAccountRepo
    {
        private readonly string connectionString;

        public AccountRepo(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public Account? GetAccount(int accountId)
        {
            using SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();

            string sql = @"
                SELECT AccountID, Username, PasswordHash, Role, AdminID, CustomerID
                FROM Account
                WHERE AccountID = @AccountID";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@AccountID", accountId);

            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return CreateAccountFromReader(reader);
        }

        public Account? GetAccountByUsername(string username)
        {
            using SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();

            string sql = @"
                SELECT AccountID, Username, PasswordHash, Role, AdminID, CustomerID
                FROM Account
                WHERE Username = @Username";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Username", username);

            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return CreateAccountFromReader(reader);
        }

        public void AddAccount(Account account)
        {
            using SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();

            string sql = @"
                INSERT INTO Account
                (
                    AccountID,
                    Username,
                    PasswordHash,
                    Role,
                    AdminID,
                    CustomerID
                )
                VALUES
                (
                    @AccountID,
                    @Username,
                    @PasswordHash,
                    @Role,
                    @AdminID,
                    @CustomerID
                )";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@AccountID", account.Id);
            command.Parameters.AddWithValue("@Username", account.Username);
            command.Parameters.AddWithValue("@PasswordHash", account.PasswordHash);
            command.Parameters.AddWithValue("@Role", account.Role);

            command.Parameters.AddWithValue(
                "@AdminID",
                account.AdminID.HasValue
                    ? account.AdminID.Value
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@CustomerID",
                account.CustomerID.HasValue
                    ? account.CustomerID.Value
                    : DBNull.Value
            );

            command.ExecuteNonQuery();
        }

        public void UpdateAccount(Account account)
        {
            using SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();

            string sql = @"
                UPDATE Account
                SET
                    Username = @Username,
                    PasswordHash = @PasswordHash,
                    Role = @Role,
                    AdminID = @AdminID,
                    CustomerID = @CustomerID
                WHERE AccountID = @AccountID";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@AccountID", account.Id);
            command.Parameters.AddWithValue("@Username", account.Username);
            command.Parameters.AddWithValue("@PasswordHash", account.PasswordHash);
            command.Parameters.AddWithValue("@Role", account.Role);

            command.Parameters.AddWithValue(
                "@AdminID",
                account.AdminID.HasValue
                    ? account.AdminID.Value
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@CustomerID",
                account.CustomerID.HasValue
                    ? account.CustomerID.Value
                    : DBNull.Value
            );

            command.ExecuteNonQuery();
        }

        public void DeleteAccount(int accountId)
        {
            using SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();

            string sql = @"
                DELETE FROM Account
                WHERE AccountID = @AccountID";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@AccountID", accountId);

            command.ExecuteNonQuery();
        }

        private Account CreateAccountFromReader(SqlDataReader reader)
        {
            return new Account(
                (int)reader["AccountID"],
                reader["Username"].ToString()!,
                reader["PasswordHash"].ToString()!,
                reader["Role"].ToString()!
            )
            {
                AdminID = reader["AdminID"] == DBNull.Value
                    ? null
                    : (int)reader["AdminID"],

                CustomerID = reader["CustomerID"] == DBNull.Value
                    ? null
                    : (int)reader["CustomerID"]
            };
        }
    }
}