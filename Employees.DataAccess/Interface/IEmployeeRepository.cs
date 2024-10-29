namespace Employees.DataAccess.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Employees.DataAccess.Models;

    public interface IEmployeeRepository
    {
        Task<IEnumerable<EmployeeDbModel>> GetAllAsync(CancellationToken ct);
        Task<IEnumerable<EmployeeDbModel>> GetByIdAsync(int employeeId, CancellationToken ct);
        Task<bool> CreateAsync(EmployeeDbModel employee, CancellationToken ct);
        Task<bool> UpdateAsync(EmployeeDbModel employee, CancellationToken ct);
        Task<bool> UpdateEmployeeManagerAsync(int employeeId, int? managerId, CancellationToken ct);
        Task<bool> DeleteAsync(int employeeId, CancellationToken ct);
    }
}
