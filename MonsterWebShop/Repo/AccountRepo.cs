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
                SELECT
                    AccountID,
                    Username,
                    Email,
                    PasswordHash,
                    Role,
                    AdminID,
                    CustomerID,
                    PasswordResetToken,
                    PasswordResetTokenExpires
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
                SELECT
                    AccountID,
                    Username,
                    Email,
                    PasswordHash,
                    Role,
                    AdminID,
                    CustomerID,
                    PasswordResetToken,
                    PasswordResetTokenExpires
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

        public Account? GetAccountByEmail(string email)
        {
            using SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();

            string sql = @"
                SELECT
                    AccountID,
                    Username,
                    Email,
                    PasswordHash,
                    Role,
                    AdminID,
                    CustomerID,
                    PasswordResetToken,
                    PasswordResetTokenExpires
                FROM Account
                WHERE Email = @Email";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Email", email);

            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return CreateAccountFromReader(reader);
        }

        public Account? GetAccountByResetToken(string token)
        {
            using SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();

            string sql = @"
                SELECT
                    AccountID,
                    Username,
                    Email,
                    PasswordHash,
                    Role,
                    AdminID,
                    CustomerID,
                    PasswordResetToken,
                    PasswordResetTokenExpires
                FROM Account
                WHERE PasswordResetToken = @Token";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Token", token);

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
                    Username,
                    Email,
                    PasswordHash,
                    Role,
                    AdminID,
                    CustomerID,
                    PasswordResetToken,
                    PasswordResetTokenExpires
                )
                VALUES
                (
                    @Username,
                    @Email,
                    @PasswordHash,
                    @Role,
                    @AdminID,
                    @CustomerID,
                    @PasswordResetToken,
                    @PasswordResetTokenExpires
                )";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@Username",
                account.Username ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@Email",
                account.Email ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@PasswordHash",
                account.PasswordHash ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@Role",
                account.Role ?? (object)DBNull.Value
            );

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

            command.Parameters.AddWithValue(
                "@PasswordResetToken",
                account.PasswordResetToken ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@PasswordResetTokenExpires",
                account.PasswordResetTokenExpires.HasValue
                    ? account.PasswordResetTokenExpires.Value
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
                    Email = @Email,
                    PasswordHash = @PasswordHash,
                    Role = @Role,
                    AdminID = @AdminID,
                    CustomerID = @CustomerID,
                    PasswordResetToken = @PasswordResetToken,
                    PasswordResetTokenExpires = @PasswordResetTokenExpires
                WHERE AccountID = @AccountID";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@AccountID", account.Id);

            command.Parameters.AddWithValue(
                "@Username",
                account.Username ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@Email",
                account.Email ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@PasswordHash",
                account.PasswordHash ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@Role",
                account.Role ?? (object)DBNull.Value
            );

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

            command.Parameters.AddWithValue(
                "@PasswordResetToken",
                account.PasswordResetToken ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@PasswordResetTokenExpires",
                account.PasswordResetTokenExpires.HasValue
                    ? account.PasswordResetTokenExpires.Value
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
            string role = reader["Role"].ToString()!;

            Account account;

            if (role == "Admin")
            {
                account = new AdminAccount(
                    (int)reader["AccountID"],
                    reader["Username"].ToString()!,
                    reader["Email"] == DBNull.Value
                        ? ""
                        : reader["Email"].ToString()!,
                    reader["PasswordHash"].ToString()!
                );
            }
            else
            {
                account = new CustomerAccount(
                    (int)reader["AccountID"],
                    reader["Username"].ToString()!,
                    reader["Email"] == DBNull.Value
                        ? ""
                        : reader["Email"].ToString()!,
                    reader["PasswordHash"].ToString()!
                );
            }

            account.AdminID =
                reader["AdminID"] == DBNull.Value
                    ? null
                    : (int)reader["AdminID"];

            account.CustomerID =
                reader["CustomerID"] == DBNull.Value
                    ? null
                    : (int)reader["CustomerID"];

            account.PasswordResetToken =
                reader["PasswordResetToken"] == DBNull.Value
                    ? null
                    : reader["PasswordResetToken"].ToString();

            account.PasswordResetTokenExpires =
                reader["PasswordResetTokenExpires"] == DBNull.Value
                    ? null
                    : (DateTime)reader["PasswordResetTokenExpires"];

            return account;
        }
    }
}