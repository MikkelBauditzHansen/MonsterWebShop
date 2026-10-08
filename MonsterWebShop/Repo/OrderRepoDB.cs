
using Microsoft.Data.SqlClient;
using MonsterWebShop.Models;
using System.Data;

namespace MonsterWebShop.Repo
{
    public class OrderRepoDB : IOrderRepo
    {
        private readonly string _connectionString;

        public OrderRepoDB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Order CreateOrder(Order order)
        {
            if (order.AccountID <= 0)
                throw new ArgumentException("Ugyldig bruger.");

            if (order.CheckoutToken == Guid.Empty)
                throw new ArgumentException("CheckoutToken mangler.");

            if (order.OrderItems == null ||
                order.OrderItems.Count == 0)
                throw new ArgumentException("Ordren er tom.");

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            using SqlTransaction transaction =
                connection.BeginTransaction();

            try
            {
                // Beregn beløbet ud fra databasen,
                // ikke ud fra brugerens input.
                decimal total = 0;

                foreach (var item in order.OrderItems)
                {
                    if (item.MonsterID == null ||
                        item.Quantity <= 0)
                        throw new ArgumentException("Ugyldig ordrelinje.");

                    string productSql = @"
                        SELECT Name, Price
                        FROM Monster WITH (UPDLOCK, HOLDLOCK)
                        WHERE ID = @MonsterID";

                    using SqlCommand productCommand =
                        new SqlCommand(
                            productSql,
                            connection,
                            transaction);

                    productCommand.Parameters.Add(
                        "@MonsterID", SqlDbType.Int
                    ).Value = item.MonsterID.Value;

                    using SqlDataReader reader =
                        productCommand.ExecuteReader();

                    if (!reader.Read())
                        throw new InvalidOperationException(
                            "Et monster findes ikke længere.");

                    item.MonsterName = reader.GetString(0);

                    double databasePrice = reader.GetDouble(1);

                    item.UnitPrice = decimal.Round(
                        Convert.ToDecimal(databasePrice),
                        2,
                        MidpointRounding.AwayFromZero);

                    if (item.UnitPrice < 0)
                        throw new InvalidOperationException(
                            "Ugyldig produktpris.");

                    total = checked(
                        total + item.UnitPrice * item.Quantity);
                }

                order.TotalAmount = total;
                order.PaymentStatus = "Pending";

                // Opret ordre
                string orderSql = @"
                    INSERT INTO Orders
                    (
                        AccountID,
                        TotalAmount,
                        PaymentStatus,
                        CheckoutToken
                    )
                    OUTPUT INSERTED.OrderID
                    VALUES
                    (
                        @AccountID,
                        @TotalAmount,
                        @PaymentStatus,
                        @CheckoutToken
                    )";

                using SqlCommand orderCommand =
                    new SqlCommand(
                        orderSql,
                        connection,
                        transaction);

                orderCommand.Parameters.Add(
                    "@AccountID", SqlDbType.Int
                ).Value = order.AccountID;

                var totalParameter = orderCommand.Parameters.Add(
                    "@TotalAmount", SqlDbType.Decimal);

                totalParameter.Precision = 18;
                totalParameter.Scale = 2;
                totalParameter.Value = order.TotalAmount;

                orderCommand.Parameters.Add(
                    "@PaymentStatus", SqlDbType.VarChar, 20
                ).Value = order.PaymentStatus;

                orderCommand.Parameters.Add(
                    "@CheckoutToken", SqlDbType.UniqueIdentifier
                ).Value = order.CheckoutToken;

                order.OrderID =
                    (int)orderCommand.ExecuteScalar()!;

                // Opret ordrelinjer
                foreach (var item in order.OrderItems)
                {
                    string itemSql = @"
                        INSERT INTO OrderItem
                        (
                            OrderID,
                            MonsterID,
                            MonsterName,
                            Quantity,
                            UnitPrice
                        )
                        OUTPUT INSERTED.OrderItemID
                        VALUES
                        (
                            @OrderID,
                            @MonsterID,
                            @MonsterName,
                            @Quantity,
                            @UnitPrice
                        )";

                    using SqlCommand itemCommand =
                        new SqlCommand(
                            itemSql,
                            connection,
                            transaction);

                    itemCommand.Parameters.Add(
                        "@OrderID", SqlDbType.Int
                    ).Value = order.OrderID;

                    itemCommand.Parameters.Add(
                        "@MonsterID", SqlDbType.Int
                    ).Value = item.MonsterID.Value;

                    itemCommand.Parameters.Add(
                        "@MonsterName", SqlDbType.VarChar, 100
                    ).Value = item.MonsterName;

                    itemCommand.Parameters.Add(
                        "@Quantity", SqlDbType.Int
                    ).Value = item.Quantity;

                    var priceParameter = itemCommand.Parameters.Add(
                        "@UnitPrice", SqlDbType.Decimal);

                    priceParameter.Precision = 18;
                    priceParameter.Scale = 2;
                    priceParameter.Value = item.UnitPrice;

                    item.OrderID = order.OrderID;

                    item.OrderItemID =
                        (int)itemCommand.ExecuteScalar()!;
                }

                transaction.Commit();

                return order;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }


        public Order? GetOrderById(int orderId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = @"
        SELECT
            OrderID,
            AccountID,
            OrderDate,
            TotalAmount,
            PaymentStatus,
            CheckoutToken
        FROM Orders
        WHERE OrderID = @OrderID";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@OrderID", orderId);

            using SqlDataReader reader =
                command.ExecuteReader();

            if (!reader.Read())
                return null;

            return new Order
            {
                OrderID = reader.GetInt32(0),
                AccountID = reader.GetInt32(1),
                OrderDate = reader.GetDateTime(2),
                TotalAmount = reader.GetDecimal(3),
                PaymentStatus = reader.GetString(4),
                CheckoutToken = reader.GetGuid(5)
            };
        }



        public Order? GetOrderByCheckoutToken(Guid checkoutToken)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = @"
        SELECT
            OrderID,
            AccountID,
            OrderDate,
            TotalAmount,
            PaymentStatus,
            CheckoutToken
        FROM Orders
        WHERE CheckoutToken = @CheckoutToken";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@CheckoutToken",
                System.Data.SqlDbType.UniqueIdentifier
            ).Value = checkoutToken;

            using SqlDataReader reader =
                command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return new Order
            {
                OrderID = reader.GetInt32(0),
                AccountID = reader.GetInt32(1),
                OrderDate = reader.GetDateTime(2),
                TotalAmount = reader.GetDecimal(3),
                PaymentStatus = reader.GetString(4),
                CheckoutToken = reader.GetGuid(5)
            };
        }



        public List<Order> GetOrdersByAccountId(int accountId)
        {
            List<Order> orders = new();

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = @"
        SELECT
            OrderID,
            AccountID,
            OrderDate,
            TotalAmount,
            PaymentStatus,
            CheckoutToken
        FROM Orders
        WHERE AccountID = @AccountID
        ORDER BY OrderDate DESC, OrderID DESC";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@AccountID", accountId);

            using SqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                Order order = new Order
                {
                    OrderID = reader.GetInt32(0),
                    AccountID = reader.GetInt32(1),
                    OrderDate = reader.GetDateTime(2),
                    TotalAmount = reader.GetDecimal(3),
                    PaymentStatus = reader.GetString(4),
                    CheckoutToken = reader.GetGuid(5)
                };

                orders.Add(order);
            }

            return orders;
        }

        public bool CompletePayment(int orderId, int accountId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = @"
        UPDATE Orders
        SET PaymentStatus = 'Paid'
        WHERE OrderID = @OrderID
          AND AccountID = @AccountID
          AND PaymentStatus = 'Pending'";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@OrderID", orderId);
            command.Parameters.AddWithValue("@AccountID", accountId);

            int affectedRows = command.ExecuteNonQuery();

            return affectedRows == 1;
        }

    }
}
