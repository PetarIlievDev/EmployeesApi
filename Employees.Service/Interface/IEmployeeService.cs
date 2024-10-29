namespace Employees.Service.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Employees.Service.Models;

    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct);
        Task<Employee> GetByIdAsync(int employeeId, CancellationToken ct);
        Task<bool> AddOrUpdateAsync(EmployeeData employeeData, CancellationToken ct);
        Task<bool> DeleteAsync(int employeeId, CancellationToken ct);
    }
}
