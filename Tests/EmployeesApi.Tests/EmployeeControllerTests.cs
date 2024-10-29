namespace EmployeesApi.Tests
{
    using Employees.Service.Interface;
    using Employees.Service.Models;
    using EmployeesApi.Controllers;
    using NSubstitute;

    public class EmployeeControllerTests
    {
        private IEmployeeService employeeServiceMock;
        private EmployeeController employeeController;

        [SetUp]
        public void Setup()
        {
            employeeServiceMock = Substitute.For<IEmployeeService>();
            employeeController = new EmployeeController(employeeServiceMock);
        }

        [Test]
        public async Task GetAsyncCallService()
        {
            await employeeController.GetAsync(CancellationToken.None);
            await employeeServiceMock.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetByIdAsyncCallService()
        {
            await employeeController.GetByIdAsync(1, CancellationToken.None);
            await employeeServiceMock.Received(1).GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task PutByIdAsyncCallService()
        {
            await employeeController.PutByIdAsync(new EmployeeData(), CancellationToken.None);
            await employeeServiceMock.Received(1).AddOrUpdateAsync(Arg.Any<EmployeeData>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task DeleteByIdAsyncCallService()
        {
            await employeeController.DeleteByIdAsync(1, CancellationToken.None);
            await employeeServiceMock.Received(1).DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }
    }
}