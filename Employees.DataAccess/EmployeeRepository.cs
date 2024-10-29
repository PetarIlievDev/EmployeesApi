namespace Employees.DataAccess
{
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using Employees.DataAccess.Interface;
    using Employees.DataAccess.Models;
    using Npgsql;

    [ExcludeFromCodeCoverage]
    public class EmployeeRepository(NpgsqlConnection connection) : IEmployeeRepository, IDisposable
    {
        public async Task<IEnumerable<EmployeeDbModel>> GetAllAsync(CancellationToken ct)
        {
            var employees = new List<EmployeeDbModel>();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                @"WITH RECURSIVE subordinates AS (
                    SELECT
                    employeeid,
                    manageremployeeid,
                    fullname,
                    title
                    FROM
                    employees
                    UNION
                    SELECT
                    e.employeeid,
                    e.manageremployeeid,
                    e.fullname,
                    e.title
                    FROM
                    employees e
                    INNER JOIN subordinates s ON s.employeeid = e.manageremployeeid
                )
                SELECT * FROM subordinates;";
            await connection.OpenAsync(ct);
            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (reader is not null)
            {
                while (await reader.ReadAsync())
                {
                    employees.Add(new EmployeeDbModel()
                    {
                        EmployeeId = Convert.ToInt32(reader["employeeid"]),
                        FullName = reader["fullname"].ToString(),
                        Title = reader["title"].ToString(),
                        ManagerEmployeeId = reader["manageremployeeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["manageremployeeid"])
                    });
                }
            }
            await connection.CloseAsync();
            return employees;
        }

        public async Task<IEnumerable<EmployeeDbModel>> GetByIdAsync(int employeeId, CancellationToken ct)
        {
            var employees = new List<EmployeeDbModel>();
            using var cmd = connection.CreateCommand();
            AddRecursiveCommandText(cmd);
                //@"SELECT employeeid, fullname, title, manageremployeeid 
                //FROM employees WHERE employeeid = :employeeid 
                //OR manageremployeeid = :employeeid;";
                //cmd.CommandText = "SELECT employeeid, fullname, title, manageremployeeid FROM employees where employeeid = :employeeid;";
            cmd.Parameters.AddWithValue(":employeeid", employeeId);
            await connection.OpenAsync(ct);
            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (reader is not null)
            {
                while (await reader.ReadAsync())
                {
                    employees.Add(new EmployeeDbModel()
                    {
                        EmployeeId = Convert.ToInt32(reader["employeeid"]),
                        FullName = reader["fullname"].ToString(),
                        Title = reader["title"].ToString(),
                        ManagerEmployeeId = reader["manageremployeeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["manageremployeeid"])
                    });
                }
            }
            await connection.CloseAsync();
            return employees;
        }

        public async Task<bool> CreateAsync(EmployeeDbModel employee, CancellationToken ct)
        {
            const string insertQuery =
                "INSERT INTO employees (fullname, title, manageremployeeid) " +
                "VALUES (:fullname, :title, :manageremployeeid);";

            using var cmd = connection.CreateCommand();
            cmd.CommandText = insertQuery;
            AddParameters(cmd, employee);
            await connection.OpenAsync(ct);
            var rowAffected = await cmd.ExecuteNonQueryAsync(ct);
            await connection.CloseAsync();
            return rowAffected > 0;

        }

        public async Task<bool> UpdateAsync(EmployeeDbModel employee, CancellationToken ct)
        {
            const string updateQuery =
            "UPDATE employees SET fullname = :fullname, title = :title, manageremployeeid = :manageremployeeid WHERE employeeid = :employeeid;";

            using var cmd = connection.CreateCommand();
            cmd.CommandText = updateQuery;
            AddParameters(cmd, employee);
            await connection.OpenAsync(ct);
            var rowAffected = await cmd.ExecuteNonQueryAsync(ct);
            await connection.CloseAsync();
            return rowAffected > 0;
        }

        public async Task<bool> UpdateEmployeeManagerAsync(int employeeId, int? managerId, CancellationToken ct)
        {
            const string updateQuery =
            "UPDATE employees SET manageremployeeid = :manageremployeeid WHERE employeeid = :employeeid;";

            using var cmd = connection.CreateCommand();
            cmd.CommandText = updateQuery;
            cmd.Parameters.AddWithValue(":employeeId", employeeId);
            cmd.Parameters.AddWithValue(":manageremployeeid", (managerId != null && managerId != 0) ? managerId : DBNull.Value);
            await connection.OpenAsync(ct);
            var rowAffected = await cmd.ExecuteNonQueryAsync(ct);
            await connection.CloseAsync();
            return rowAffected > 0;
        }

        public async Task<bool> DeleteAsync(int employeeId, CancellationToken ct)
        {
            const string deleteQuery = "DELETE FROM employees WHERE employeeid = :employeeid;";
            using var cmd = connection.CreateCommand();
            cmd.CommandText = deleteQuery;
            cmd.Parameters.AddWithValue(":employeeid", employeeId);
            await connection.OpenAsync();
            var rowAffected = await cmd.ExecuteNonQueryAsync();
            await connection.CloseAsync();
            return rowAffected > 0;

        }

        private static void AddRecursiveCommandText(NpgsqlCommand cmd)
        {
            cmd.CommandText =
                @"WITH RECURSIVE subordinates AS (
                    SELECT
                    employeeid,
                    manageremployeeid,
                    fullname,
                    title
                    FROM
                    employees
                    WHERE employeeid = :employeeid
                    UNION
                    SELECT
                    e.employeeid,
                    e.manageremployeeid,
                    e.fullname,
                    e.title
                    FROM
                    employees e
                    INNER JOIN subordinates s ON s.employeeid = e.manageremployeeid
                )
                SELECT * FROM subordinates;";
        }

        private static void AddParameters(NpgsqlCommand command, EmployeeDbModel employee)
        {
            var parameters = command.Parameters;

            parameters.AddWithValue(":employeeId", employee.EmployeeId);
            parameters.AddWithValue(":fullname", employee.FullName);
            parameters.AddWithValue(":title", employee.Title);
            parameters.AddWithValue(":manageremployeeid", employee.ManagerEmployeeId != null ? employee.ManagerEmployeeId : DBNull.Value);
        }

        public void Dispose()
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
            GC.SuppressFinalize(this);
        }
    }
}
