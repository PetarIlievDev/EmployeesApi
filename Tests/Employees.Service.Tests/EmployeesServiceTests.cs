namespace Employees.Service.Tests
{
    using AutoMapper;
    using Employees.DataAccess.Interface;
    using Employees.DataAccess.Models;
    using Employees.Service.MapperProfiles;
    using Employees.Service.Models;
    using NSubstitute;

    public class EmployeesServiceTests
    {
        private readonly IEmployeeRepository employeeRepositoryMock;
        private readonly IMapper mapper;
        private readonly EmployeeService employeeService;

        public EmployeesServiceTests()
        {
            employeeRepositoryMock = Substitute.For<IEmployeeRepository>();
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<EmployeeMapProfile>());
            mapper = mapperConfig.CreateMapper();
            employeeService = new EmployeeService(employeeRepositoryMock, mapper);
        }

        [Test]
        public async Task GetAllAsyncReturnCorrectData()
        {
            var employeesFromDb = SetEmployeesFromDb();
            employeeRepositoryMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(employeesFromDb);
            var expected = ExpNestedResult(employeesFromDb);
            var result = await employeeService.GetAllAsync(CancellationToken.None) as List<Employee>;

            Assert.IsNotNull(result);
            for (var i = 0; i < result.Count(); i++)
            {
                Assert.That(result[i].EmployeeId, Is.EqualTo(expected[i].EmployeeId));
                Assert.That(result[i].FullName, Is.EqualTo(expected[i].FullName));
                Assert.That(result[i].Title, Is.EqualTo(expected[i].Title));
            }
        }

        [Test]
        public async Task GetByIdAsyncReturnCorrectData()
        {
            var employeesFromDb = SetEmployeesFromDbForSigneEmployee();
            employeeRepositoryMock.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(employeesFromDb);
            var expected = ExpNestedResult(employeesFromDb);
            var result = await employeeService.GetByIdAsync(4, CancellationToken.None) as Employee;

            Assert.IsNotNull(result);
            Assert.That(result.FullName, Is.EqualTo(expected[0].FullName));
            Assert.That(result.Title, Is.EqualTo(expected[0].Title));
        }

        [Test]
        public async Task AddOrUpdateAsyncCallsTheCerroctMethod()
        {
            var result = await employeeService.AddOrUpdateAsync(new EmployeeData() { EmployeeId = 0 }, CancellationToken.None);
            await employeeRepositoryMock.Received(1).CreateAsync(Arg.Any<EmployeeDbModel>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task AddOrUpdateAsyncCallsTheCerrectUpdateMethod()
        {
            var result = await employeeService.AddOrUpdateAsync(new EmployeeData() { EmployeeId = 1 }, CancellationToken.None);
            await employeeRepositoryMock.Received(1).UpdateAsync(Arg.Any<EmployeeDbModel>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task DeleteAsyncSuccessfully()
        {
            var employeesFromDb = SetEmployeesFromDbForSigneEmployee();
            employeeRepositoryMock.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(employeesFromDb);
            employeeRepositoryMock.DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);
            var result = await employeeService.DeleteAsync(1, CancellationToken.None);
            await employeeRepositoryMock.Received(2).UpdateEmployeeManagerAsync(Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
            await employeeRepositoryMock.Received(1).DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
            employeeRepositoryMock.ClearReceivedCalls();
        }

        [Test]
        public async Task DeleteAsyncNotSuccessfully()
        {
            var employeesFromDb = SetEmployeesFromDbForSigneEmployee();
            employeeRepositoryMock.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(employeesFromDb);
            employeeRepositoryMock.DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(false);
            var result = await employeeService.DeleteAsync(1, CancellationToken.None);
            await employeeRepositoryMock.Received(4).UpdateEmployeeManagerAsync(Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
            await employeeRepositoryMock.Received(1).DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
            employeeRepositoryMock.ClearReceivedCalls();
        }

        private List<Employee> ExpNestedResult(List<EmployeeDbModel> employeesFromDb)
        {
            var result = new List<Employee>();
            var orderedEmployeesFromDb = employeesFromDb.OrderBy(x => x.ManagerEmployeeId).ToList();

            foreach (var emp in orderedEmployeesFromDb)
            {
                if (emp.ManagerEmployeeId == 0)
                {
                    result.Add(mapper.Map<Employee>(emp));
                    continue;
                }
                NestEmploees(result, employeesFromDb, emp);
            }
            return result;
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

        private static List<EmployeeDbModel> SetEmployeesFromDb()
        {
            var employees = new List<EmployeeDbModel>
            {
                new() {
                    EmployeeId = 1,
                    FullName = "Sam Roth",
                    Title = "Principle Software Engineer",
                    ManagerEmployeeId = 0
                },
                new() {
                    EmployeeId = 2,
                    FullName = "John Doe",
                    Title = "Software Engineer",
                    ManagerEmployeeId = 1
                },
                new() {
                    EmployeeId = 4,
                    FullName = "Rudolf Ram",
                    Title = "Senior Software Engineer",
                    ManagerEmployeeId = 0
                },
                new() {
                    EmployeeId = 5,
                    FullName = "Peter Manfred",
                    Title = "Senior Software Engineer",
                    ManagerEmployeeId = 1
                }
            };

            return employees;
        }

        private static List<EmployeeDbModel> SetEmployeesFromDbForSigneEmployee()
        {
            var employees = new List<EmployeeDbModel>
            {
                new() {
                    EmployeeId = 2,
                    FullName = "John Doe",
                    Title = "Software Engineer",
                    ManagerEmployeeId = 1
                },
                new() {
                    EmployeeId = 4,
                    FullName = "Rudolf Ram",
                    Title = "Senior Software Engineer",
                    ManagerEmployeeId = 0
                },
                new() {
                    EmployeeId = 5,
                    FullName = "Peter Manfred",
                    Title = "Senior Software Engineer",
                    ManagerEmployeeId = 1
                }
            };

            return employees;
        }
    }
}