namespace Employees.Service
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Employees.DataAccess.Interface;
    using Employees.DataAccess.Models;
    using Employees.Service.Interface;
    using Employees.Service.Models;

    public class EmployeeService(IEmployeeRepository employeeRepository, IMapper mapper) : IEmployeeService
    {
        public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct)
        {
            var employeesFromDb = await employeeRepository.GetAllAsync(ct);
            var result = new List<Employee>();
            var orderedEmployeesFromDb = employeesFromDb.OrderBy(x => x.ManagerEmployeeId).ToList();

            foreach (var emp in orderedEmployeesFromDb)
            {
                //Add root managers
                if (emp.ManagerEmployeeId == 0)
                {
                    result.Add(mapper.Map<Employee>(emp));
                    continue;
                }

                //Call recursion to nest employees
                NestEmploees(result, employeesFromDb, emp);
            }
            return result;
        }

        public async Task<Employee> GetByIdAsync(int employeeId, CancellationToken ct)
        {
            var employeesFromDb = await employeeRepository.GetByIdAsync(employeeId, ct);
            Employee result = mapper.Map<Employee>(employeesFromDb.Where(x => x.EmployeeId == employeeId).FirstOrDefault());
            var managerFirstLevelList = employeesFromDb.Where(x => x.EmployeeId != employeeId && x.ManagerEmployeeId == employeeId).ToList();

            var orderedEmployeesFromDb = employeesFromDb.Where(x => x.EmployeeId != employeeId && x.ManagerEmployeeId != employeeId).OrderBy(x => x.ManagerEmployeeId).ToList();
            var combinedLists = managerFirstLevelList.Concat(orderedEmployeesFromDb).ToList();
            foreach (var emp in combinedLists)
            {
                //Add requested employee as a root manager
                if (emp.ManagerEmployeeId == employeeId)
                {
                    result.ManagedEmployees.Add(mapper.Map<Employee>(emp));
                    continue;
                }

                //Call recursion to nest employees
                NestEmploees(result.ManagedEmployees, combinedLists, emp);
            }

            return result;
        }

        public async Task<bool> AddOrUpdateAsync(EmployeeData employeeData, CancellationToken ct)
        {
            var employeeDataToSave = mapper.Map<EmployeeDbModel>(employeeData);
            if (employeeData.EmployeeId < 1)
            {
                return await employeeRepository.CreateAsync(employeeDataToSave, ct);
            }

            return await employeeRepository.UpdateAsync(employeeDataToSave, ct);
        }

        public async Task<bool> DeleteAsync(int employeeId, CancellationToken ct)
        {
            var managedEmployees = await employeeRepository.GetByIdAsync(employeeId, ct);
            var employeeForDeletion = managedEmployees.Where(x => x.EmployeeId == employeeId).FirstOrDefault();
            var managedEmployeesToUpdate = managedEmployees.Where(x => x.ManagerEmployeeId == employeeId).ToList();

            //Update directly managed employees
            foreach (var emp in managedEmployeesToUpdate)
            {
                await employeeRepository.UpdateEmployeeManagerAsync(emp.EmployeeId, employeeForDeletion?.ManagerEmployeeId, ct);
            }

            var isSusseccsullDeletion = await employeeRepository.DeleteAsync(employeeId, ct);

            if (!isSusseccsullDeletion)
            {
                //If deletion failed, revert the changes
                foreach (var emp in managedEmployeesToUpdate)
                {
                    await employeeRepository.UpdateEmployeeManagerAsync(emp.EmployeeId, employeeId, ct);
                }
            }

            return isSusseccsullDeletion;
        }

        private void NestEmploees(List<Employee> result, IEnumerable<EmployeeDbModel> employeeDbModels, EmployeeDbModel employeedb)
        {
            result.Where(x => x.EmployeeId == employeedb.ManagerEmployeeId).FirstOrDefault()?.ManagedEmployees.Add(mapper.Map<Employee>(employeedb));
            if (employeeDbModels.Where(x => x.ManagerEmployeeId == employeedb.EmployeeId) != null)
            {
                foreach (var emp in employeeDbModels.Where(x => x.ManagerEmployeeId == employeedb.EmployeeId))
                {
                    //Validation to avoid circular reference
                    if (result.Where(x => x.EmployeeId == employeedb.ManagerEmployeeId).FirstOrDefault() != null)
                    {
                        NestEmploees(result.Where(x => x.EmployeeId == employeedb.ManagerEmployeeId).FirstOrDefault()?.ManagedEmployees, employeeDbModels, emp);
                    }
                }
            }
        }

    }
}
