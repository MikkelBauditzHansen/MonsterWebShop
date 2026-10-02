using MonsterWebShop.Models;
using Microsoft.Data.SqlClient;

namespace MonsterWebShop.Repo
{
    public class MonsterRepoDB : IMonsterRepo
    {
        private readonly string _connectionString;

        public MonsterRepoDB(string connectionString)
        {
            _connectionString = connectionString;
        }


        // ---------------------------------
        // HENT ALLE MONSTRE
        // ---------------------------------

        public List<Monster> GetAllMonsters()
        {
            List<Monster> monsters = new List<Monster>();

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = @"
                SELECT
                    ID,
                    Name,
                    Color,
                    ImagePath,
                    Age,
                    Price,
                    HumanoidID,
                    UndeadID,
                    DragonID
                FROM Monster";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            using SqlDataReader reader =
                command.ExecuteReader();

            List<(int Id, string Name, string Color, string ImagePath,
                int Age, double Price, int? HumanoidId,
                int? UndeadId, int? DragonId)> rows = new();

            while (reader.Read())
            {
                rows.Add((
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetDouble(5),

                    reader.IsDBNull(6)
                        ? null
                        : reader.GetInt32(6),

                    reader.IsDBNull(7)
                        ? null
                        : reader.GetInt32(7),

                    reader.IsDBNull(8)
                        ? null
                        : reader.GetInt32(8)
                ));
            }

            reader.Close();


            foreach (var row in rows)
            {
                Monster monster;

                if (row.DragonId != null)
                {
                    monster = GetDragon(
                        connection,
                        row.Id,
                        row.Name,
                        row.Color,
                        row.ImagePath,
                        row.Age,
                        row.Price,
                        row.DragonId.Value);
                }
                else if (row.HumanoidId != null)
                {
                    monster = GetHumanoid(
                        connection,
                        row.Id,
                        row.Name,
                        row.Color,
                        row.ImagePath,
                        row.Age,
                        row.Price,
                        row.HumanoidId.Value);
                }
                else if (row.UndeadId != null)
                {
                    monster = GetUndead(
                        connection,
                        row.Id,
                        row.Name,
                        row.Color,
                        row.ImagePath,
                        row.Age,
                        row.Price,
                        row.UndeadId.Value);
                }
                else
                {
                    monster = new Monster(
                        row.Name,
                        row.Color,
                        row.ImagePath,
                        row.Age,
                        row.Price)
                    {
                        Id = row.Id
                    };
                }

                monsters.Add(monster);
            }

            return monsters;
        }


        // ---------------------------------
        // HENT MONSTER MED ID
        // ---------------------------------

        public Monster? GetMonsterById(int id)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = @"
                SELECT
                    ID,
                    Name,
                    Color,
                    ImagePath,
                    Age,
                    Price,
                    HumanoidID,
                    UndeadID,
                    DragonID
                FROM Monster
                WHERE ID = @Id";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);

            using SqlDataReader reader =
                command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            string name = reader.GetString(1);
            string color = reader.GetString(2);
            string imagePath = reader.GetString(3);
            int age = reader.GetInt32(4);
            double price = reader.GetDouble(5);

            int? humanoidId =
                reader.IsDBNull(6)
                    ? null
                    : reader.GetInt32(6);

            int? undeadId =
                reader.IsDBNull(7)
                    ? null
                    : reader.GetInt32(7);

            int? dragonId =
                reader.IsDBNull(8)
                    ? null
                    : reader.GetInt32(8);

            reader.Close();


            if (dragonId != null)
            {
                return GetDragon(
                    connection,
                    id,
                    name,
                    color,
                    imagePath,
                    age,
                    price,
                    dragonId.Value);
            }

            if (humanoidId != null)
            {
                return GetHumanoid(
                    connection,
                    id,
                    name,
                    color,
                    imagePath,
                    age,
                    price,
                    humanoidId.Value);
            }

            if (undeadId != null)
            {
                return GetUndead(
                    connection,
                    id,
                    name,
                    color,
                    imagePath,
                    age,
                    price,
                    undeadId.Value);
            }

            return new Monster(
                name,
                color,
                imagePath,
                age,
                price)
            {
                Id = id
            };
        }


        // ---------------------------------
        // ADD MONSTER
        // ---------------------------------

        public Monster AddMonster(Monster monster)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            int? humanoidId = null;
            int? undeadId = null;
            int? dragonId = null;


            // Find ID til subtype
            if (monster is Dragon)
            {
                dragonId =
                    GetNextId(connection, "Dragon", "DragonID");
            }
            else if (monster is Humanoid)
            {
                humanoidId =
                    GetNextId(connection, "Humanoid", "HumanoidID");
            }
            else if (monster is Undead)
            {
                undeadId =
                    GetNextId(connection, "Undead", "UndeadID");
            }


            // Find ID til Monster
            int monsterId =
                GetNextId(connection, "Monster", "ID");


            // ---------------------------------
            // DRAGON
            // ---------------------------------

            if (monster is Dragon dragon)
            {
                string sql = @"
                    INSERT INTO Dragon
                    (
                        DragonID,
                        WingSpan,
                        DragonType
                    )
                    VALUES
                    (
                        @Id,
                        @WingSpan,
                        @Type
                    )";

                using SqlCommand command =
                    new SqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@Id",
                    dragonId);

                command.Parameters.AddWithValue(
                    "@WingSpan",
                    dragon.WingSpan);

                command.Parameters.AddWithValue(
                    "@Type",
                    (int)dragon.Type);

                command.ExecuteNonQuery();
            }


            // ---------------------------------
            // HUMANOID
            // ---------------------------------

            else if (monster is Humanoid humanoid)
            {
                string sql = @"
                    INSERT INTO Humanoid
                    (
                        HumanoidID,
                        HumanoidType
                    )
                    VALUES
                    (
                        @Id,
                        @Type
                    )";

                using SqlCommand command =
                    new SqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@Id",
                    humanoidId);

                command.Parameters.AddWithValue(
                    "@Type",
                    (int)humanoid.Type);

                command.ExecuteNonQuery();
            }


            // ---------------------------------
            // UNDEAD
            // ---------------------------------

            else if (monster is Undead undead)
            {
                string sql = @"
                    INSERT INTO Undead
                    (
                        UndeadID,
                        UndeadType
                    )
                    VALUES
                    (
                        @Id,
                        @Type
                    )";

                using SqlCommand command =
                    new SqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@Id",
                    undeadId);

                command.Parameters.AddWithValue(
                    "@Type",
                    (int)undead.Type);

                command.ExecuteNonQuery();
            }


            // ---------------------------------
            // MONSTER
            // ---------------------------------

            string monsterSql = @"
                INSERT INTO Monster
                (
                    ID,
                    Name,
                    Color,
                    ImagePath,
                    Age,
                    Price,
                    HumanoidID,
                    UndeadID,
                    DragonID
                )
                VALUES
                (
                    @Id,
                    @Name,
                    @Color,
                    @ImagePath,
                    @Age,
                    @Price,
                    @HumanoidID,
                    @UndeadID,
                    @DragonID
                )";

            using SqlCommand monsterCommand =
                new SqlCommand(monsterSql, connection);

            monsterCommand.Parameters.AddWithValue(
                "@Id",
                monsterId);

            monsterCommand.Parameters.AddWithValue(
                "@Name",
                monster.Name ?? "");

            monsterCommand.Parameters.AddWithValue(
                "@Color",
                monster.Color ?? "");

            monsterCommand.Parameters.AddWithValue(
                "@ImagePath",
                monster.ImagePath ?? "");

            monsterCommand.Parameters.AddWithValue(
                "@Age",
                monster.Age);

            monsterCommand.Parameters.AddWithValue(
                "@Price",
                monster.Price);

            monsterCommand.Parameters.AddWithValue(
                "@HumanoidID",
                (object?)humanoidId ?? DBNull.Value);

            monsterCommand.Parameters.AddWithValue(
                "@UndeadID",
                (object?)undeadId ?? DBNull.Value);

            monsterCommand.Parameters.AddWithValue(
                "@DragonID",
                (object?)dragonId ?? DBNull.Value);

            monsterCommand.ExecuteNonQuery();


            monster.Id = monsterId;

            return monster;
        }


        // ---------------------------------
        // SLET MONSTER
        // ---------------------------------

        public Monster? RemoveMonster(int id)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            Monster? monster =
                GetMonsterById(id);

            if (monster == null)
            {
                return null;
            }


            string sql = @"
                SELECT
                    HumanoidID,
                    UndeadID,
                    DragonID
                FROM Monster
                WHERE ID = @Id";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@Id",
                id);

            int? humanoidId = null;
            int? undeadId = null;
            int? dragonId = null;

            using (SqlDataReader reader =
                   command.ExecuteReader())
            {
                if (reader.Read())
                {
                    humanoidId =
                        reader.IsDBNull(0)
                            ? null
                            : reader.GetInt32(0);

                    undeadId =
                        reader.IsDBNull(1)
                            ? null
                            : reader.GetInt32(1);

                    dragonId =
                        reader.IsDBNull(2)
                            ? null
                            : reader.GetInt32(2);
                }
            }


            // Slet Monster først
            string deleteMonsterSql =
                "DELETE FROM Monster WHERE ID = @Id";

            using SqlCommand deleteMonster =
                new SqlCommand(deleteMonsterSql, connection);

            deleteMonster.Parameters.AddWithValue(
                "@Id",
                id);

            deleteMonster.ExecuteNonQuery();


            // Slet subtype
            if (dragonId != null)
            {
                DeleteSubtype(
                    connection,
                    "Dragon",
                    "DragonID",
                    dragonId.Value);
            }

            if (humanoidId != null)
            {
                DeleteSubtype(
                    connection,
                    "Humanoid",
                    "HumanoidID",
                    humanoidId.Value);
            }

            if (undeadId != null)
            {
                DeleteSubtype(
                    connection,
                    "Undead",
                    "UndeadID",
                    undeadId.Value);
            }

            return monster;
        }

        //searchmonster
        public List<Monster> SearchMonsters(string searchText)
        {
            List<Monster> monsters = new List<Monster>();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return GetAllMonsters();
            }

            searchText = searchText.Trim();

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string sql = @"
        SELECT
            ID,
            Name,
            Color,
            ImagePath,
            Age,
            Price,
            HumanoidID,
            UndeadID,
            DragonID
        FROM Monster
        WHERE Name LIKE @Search
        OR Color LIKE @Search";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@Search",
                "%" + searchText + "%"
            );

            using SqlDataReader reader =
                command.ExecuteReader();

            List<(int Id,
                  string Name,
                  string Color,
                  string ImagePath,
                  int Age,
                  double Price,
                  int? HumanoidId,
                  int? UndeadId,
                  int? DragonId)> rows = new();

            while (reader.Read())
            {
                rows.Add((
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetDouble(5),

                    reader.IsDBNull(6)
                        ? null
                        : reader.GetInt32(6),

                    reader.IsDBNull(7)
                        ? null
                        : reader.GetInt32(7),

                    reader.IsDBNull(8)
                        ? null
                        : reader.GetInt32(8)
                ));
            }

            reader.Close();

            foreach (var row in rows)
            {
                Monster monster;

                if (row.DragonId != null)
                {
                    monster = GetDragon(
                        connection,
                        row.Id,
                        row.Name,
                        row.Color,
                        row.ImagePath,
                        row.Age,
                        row.Price,
                        row.DragonId.Value
                    );
                }
                else if (row.HumanoidId != null)
                {
                    monster = GetHumanoid(
                        connection,
                        row.Id,
                        row.Name,
                        row.Color,
                        row.ImagePath,
                        row.Age,
                        row.Price,
                        row.HumanoidId.Value
                    );
                }
                else if (row.UndeadId != null)
                {
                    monster = GetUndead(
                        connection,
                        row.Id,
                        row.Name,
                        row.Color,
                        row.ImagePath,
                        row.Age,
                        row.Price,
                        row.UndeadId.Value
                    );
                }
                else
                {
                    monster = new Monster(
                        row.Name,
                        row.Color,
                        row.ImagePath,
                        row.Age,
                        row.Price
                    )
                    {
                        Id = row.Id
                    };
                }

                monsters.Add(monster);
            }

            return monsters;
        }

        // ---------------------------------
        // OPDATER MONSTER
        // ---------------------------------

        public Monster? UpdateMonster(
            int id,
            Monster updatedMonster)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            Monster? monster =
                GetMonsterById(id);

            if (monster == null)
            {
                return null;
            }


            string sql = @"
                UPDATE Monster
                SET
                    Name = @Name,
                    Color = @Color,
                    ImagePath = @ImagePath,
                    Age = @Age,
                    Price = @Price
                WHERE ID = @Id";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@Id",
                id);

            command.Parameters.AddWithValue(
                "@Name",
                updatedMonster.Name ?? "");

            command.Parameters.AddWithValue(
                "@Color",
                updatedMonster.Color ?? "");

            command.Parameters.AddWithValue(
                "@ImagePath",
                updatedMonster.ImagePath ?? "");

            command.Parameters.AddWithValue(
                "@Age",
                updatedMonster.Age);

            command.Parameters.AddWithValue(
                "@Price",
                updatedMonster.Price);

            command.ExecuteNonQuery();


            // Dragon
            if (updatedMonster is Dragon dragon)
            {
                string dragonSql = @"
                    UPDATE Dragon
                    SET
                        WingSpan = @WingSpan,
                        DragonType = @Type
                    WHERE DragonID =
                    (
                        SELECT DragonID
                        FROM Monster
                        WHERE ID = @Id
                    )";

                using SqlCommand dragonCommand =
                    new SqlCommand(
                        dragonSql,
                        connection);

                dragonCommand.Parameters.AddWithValue(
                    "@Id",
                    id);

                dragonCommand.Parameters.AddWithValue(
                    "@WingSpan",
                    dragon.WingSpan);

                dragonCommand.Parameters.AddWithValue(
                    "@Type",
                    (int)dragon.Type);

                dragonCommand.ExecuteNonQuery();
            }


            // Humanoid
            if (updatedMonster is Humanoid humanoid)
            {
                string humanoidSql = @"
                    UPDATE Humanoid
                    SET
                        HumanoidType = @Type
                    WHERE HumanoidID =
                    (
                        SELECT HumanoidID
                        FROM Monster
                        WHERE ID = @Id
                    )";

                using SqlCommand humanoidCommand =
                    new SqlCommand(
                        humanoidSql,
                        connection);

                humanoidCommand.Parameters.AddWithValue(
                    "@Id",
                    id);

                humanoidCommand.Parameters.AddWithValue(
                    "@Type",
                    (int)humanoid.Type);

                humanoidCommand.ExecuteNonQuery();
            }


            // Undead
            if (updatedMonster is Undead undead)
            {
                string undeadSql = @"
                    UPDATE Undead
                    SET
                        UndeadType = @Type
                    WHERE UndeadID =
                    (
                        SELECT UndeadID
                        FROM Monster
                        WHERE ID = @Id
                    )";

                using SqlCommand undeadCommand =
                    new SqlCommand(
                        undeadSql,
                        connection);

                undeadCommand.Parameters.AddWithValue(
                    "@Id",
                    id);

                undeadCommand.Parameters.AddWithValue(
                    "@Type",
                    (int)undead.Type);

                undeadCommand.ExecuteNonQuery();
            }

            return updatedMonster;
        }


        // ---------------------------------
        // HJÆLPEMETODE - NÆSTE ID
        // ---------------------------------

        private int GetNextId(
            SqlConnection connection,
            string table,
            string column)
        {
            string sql =
                $"SELECT ISNULL(MAX({column}), 0) + 1 FROM {table}";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            return (int)command.ExecuteScalar();
        }


        // ---------------------------------
        // HENT DRAGON
        // ---------------------------------

        private Dragon GetDragon(
            SqlConnection connection,
            int id,
            string name,
            string color,
            string imagePath,
            int age,
            double price,
            int dragonId)
        {
            string sql = @"
                SELECT
                    WingSpan,
                    DragonType
                FROM Dragon
                WHERE DragonID = @Id";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@Id",
                dragonId);

            double wingSpan = 0;
            int type = 0;

            using (SqlDataReader reader =
                   command.ExecuteReader())
            {
                if (reader.Read())
                {
                    wingSpan = reader.GetDouble(0);
                    type = reader.GetInt32(1);
                }
            }

            return new Dragon(
                name,
                color,
                imagePath,
                age,
                price,
                wingSpan,
                (DragonType)type)
            {
                Id = id
            };
        }


        // ---------------------------------
        // HENT HUMANOID
        // ---------------------------------

        private Humanoid GetHumanoid(
            SqlConnection connection,
            int id,
            string name,
            string color,
            string imagePath,
            int age,
            double price,
            int humanoidId)
        {
            string sql = @"
                SELECT HumanoidType
                FROM Humanoid
                WHERE HumanoidID = @Id";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@Id",
                humanoidId);

            int type = 0;

            using (SqlDataReader reader =
                   command.ExecuteReader())
            {
                if (reader.Read())
                {
                    type = reader.GetInt32(0);
                }
            }

            return new Humanoid(
                name,
                color,
                imagePath,
                age,
                price,
                (HumanoidType)type)
            {
                Id = id
            };
        }


        // ---------------------------------
        // HENT UNDEAD
        // ---------------------------------

        private Undead GetUndead(
            SqlConnection connection,
            int id,
            string name,
            string color,
            string imagePath,
            int age,
            double price,
            int undeadId)
        {
            string sql = @"
                SELECT UndeadType
                FROM Undead
                WHERE UndeadID = @Id";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@Id",
                undeadId);

            int type = 0;

            using (SqlDataReader reader =
                   command.ExecuteReader())
            {
                if (reader.Read())
                {
                    type = reader.GetInt32(0);
                }
            }

            return new Undead(
                name,
                color,
                imagePath,
                age,
                price,
                (UndeadType)type)
            {
                Id = id
            };
        }


        // ---------------------------------
        // SLET SUBTYPE
        // ---------------------------------

        private void DeleteSubtype(
            SqlConnection connection,
            string table,
            string column,
            int id)
        {
            string sql =
                $"DELETE FROM {table} WHERE {column} = @Id";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@Id",
                id);

            command.ExecuteNonQuery();
        }
    }
}