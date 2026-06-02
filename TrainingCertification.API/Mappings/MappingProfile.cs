using AutoMapper;
using TrainingCertification.API.DTOs;
using TrainingCertification.API.Models;

namespace TrainingCertification.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Course, CourseDto>()
            .ForCtorParam("Category", opt => opt.MapFrom(src => src.CourseCategory!.Name))
            .ForCtorParam("Prerequisite", opt => opt.MapFrom(src => src.Prerequisites.Select(p => p.PrerequisiteCourse!.Title).FirstOrDefault()));
    }
}
