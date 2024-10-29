namespace Employees.Service.MapperProfiles
{
    using AutoMapper;
    using Employees.DataAccess.Models;
    using Employees.Service.Models;

    public class EmployeeMapProfile : Profile
    {
        public EmployeeMapProfile()
        {
            CreateMap<EmployeeDbModel, Employee>();
                //.EqualityComparison((src, dest) => src.ManagerEmployeeId == dest.EmployeeId)
                //.ForMember(dest => dest.EmployeeId, opts => opts.MapFrom(src => src.ManagerEmployeeId));
                //.ForMember(dest => dest.EmployeeId, opts => opts.MapFrom(src => src.ManagerEmployeeId));

            //CreateMap<List<EmployeeDbModel>, List<Employee>>()
            //    .EqualityComparison((src, dest) => src.ManagerId == dest.EmployeeId)
            //    .ForMember(dest => dest.EmployeeId, opts => opts.MapFrom(src => src.ManagerId));

            CreateMap<EmployeeData, EmployeeDbModel>();
        }
    }
}
