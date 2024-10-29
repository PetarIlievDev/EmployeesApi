namespace EmployeesApi.Controllers
{
    using System.Collections.Generic;
    using Employees.Service.Interface;
    using Employees.Service.Models;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]")]
    public class EmployeeController(IEmployeeService employeeService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Employee>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAsync(CancellationToken ct)
        {
            var employees = await employeeService.GetAllAsync(ct);
            return Ok(employees);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IEnumerable<Employee>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
        {
            var employees = await employeeService.GetByIdAsync(id, ct);
            return Ok(employees);
        }

        [HttpPut]
        [ProducesResponseType(typeof(bool), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutByIdAsync([FromBody] EmployeeData employeeData, CancellationToken ct)
        {
            var result = await employeeService.AddOrUpdateAsync(employeeData, ct);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IEnumerable<Employee>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteByIdAsync(int id, CancellationToken ct)
        {
            var employees = await employeeService.DeleteAsync(id, ct);
            return Ok(employees);
        }
    }    
}
