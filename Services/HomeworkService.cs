using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FlytDex.Domain.Model.FlytDex;
using FlytDex.Domain.Model.FlytDex.Links;
using FlytDex.Domain.Services.Interfaces;
using FlytDex.Shared;
using FlytDex.Shared.Dtos;
using FlytDex.Shared.Enums;
using FlytDex.Shared.Requests;

namespace FlytDex.Domain.Services
{
    public class HomeworkService : IHomeworkService
    {
        private IFlytDexDbContext flytDexContext;
        private IMapper mapper;
        private IErrorService errorService;

        public HomeworkService(IFlytDexDbContext flytDexContext, IMapper mapper, IErrorService errorService)
        {
            this.flytDexContext = flytDexContext;
            this.mapper = mapper;
            this.errorService = errorService;
        }

        public ServiceResult<List<HomeworkDto>> GetPastHomework(Guid schoolId, Guid employeeId, Guid subjectId, Guid studentGroupId, params Expression<Func<HomeworkDto, object>>[] includes)
        {
            if (schoolId == Guid.Empty)
            {
                return errorService.Error<List<HomeworkDto>>("An error occurred: SchoolId is invalid");
            }

            DateTime now = DateTime.Now;
            if (!flytDexContext.Homeworks.Any(h => h.SchoolId == schoolId && h.Lesson.Event.EmployeeId == employeeId))
            {
                return errorService.Warn<List<HomeworkDto>>("No Homework found.");
            }

            List<HomeworkDto> homeworkDtos = flytDexContext.Homeworks
                .Where(h =>
                    h.SchoolId == schoolId &&
                    h.Lesson.Event.EmployeeId == employeeId &&
                    h.Lesson.Event.EndDateTime.Date <= now &&
                    h.Lesson.Event.Subject.Id == subjectId &&
                    h.Lesson.Event.LinkEventStudentGroups.Any(sg => sg.StudentGroupId == studentGroupId))
                .ProjectTo(mapper.ConfigurationProvider, includes)
                .ToList();

            List<PeriodDto> cachedPeriods = flytDexContext.Periods
                .Where(h => h.SchoolId == schoolId && h.PeriodInstanceId != 0)
                .ProjectTo<PeriodDto>(mapper.ConfigurationProvider)
                .ToList();

            foreach (HomeworkDto homeworkDto in homeworkDtos)
            {
                homeworkDto.PeriodId = cachedPeriods
                    .Where(p =>
                        p.StartDateTime == homeworkDto.EventStartDateTime &&
                        p.EndDateTime == homeworkDto.EventEndDateTime)
                    .Select(p => p.PeriodId).SingleOrDefault();
            }

            return new ServiceResult<List<HomeworkDto>>(homeworkDtos);
        }

        public ServiceResult<List<HomeworkDto>> GetHomeworkForLesson(Guid lessonId, params Expression<Func<HomeworkDto, object>>[] includes)
        {
            if (lessonId == Guid.Empty)
            {
                return errorService.Error<List<HomeworkDto>>("An error occurred: Lesson Id is invalid");
            }

            List<HomeworkDto> homeworkDtos = flytDexContext.Homeworks
                .Where(lh => lh.LessonId == lessonId)
                .ProjectTo(mapper.ConfigurationProvider, includes)
                .ToList();

            if (homeworkDtos.Count == 0)
            {
                return errorService.Warn<List<HomeworkDto>>("No Homework found.");
            }

            return new ServiceResult<List<HomeworkDto>>(homeworkDtos);
        }

        public ServiceResult<HomeworkDto> CreateHomework(HomeworkRequest homeworkRequest)
        {
            string validationMessage = ValidateHomeworkRequest(homeworkRequest);

            if (!string.IsNullOrEmpty(validationMessage))
            {
                return errorService.Error<HomeworkDto>(validationMessage);
            }

            if (flytDexContext.Homeworks.Any(h => h.Id == homeworkRequest.Id && h.SchoolId == homeworkRequest.SchoolId && h.LessonId == homeworkRequest.LessonId))
            {
                return errorService.Error<HomeworkDto>("An error occurred: A homework with this Id already exists");
            }

            //ICollection<LinkHomeworkResource> linkHomeworkResources = mapper.Map<ICollection<LinkHomeworkResourceDto>, ICollection<LinkHomeworkResource>>(homeworkRequest.LinkHomeworkResources);
            //ICollection<LinkStudentHomework> linkStudentHomeworks = mapper.Map<ICollection<LinkStudentHomeworkDto>, ICollection<LinkStudentHomework>>(homeworkRequest.LinkStudentHomeworks);


            Homework homework = mapper.Map<HomeworkRequest, Homework>(homeworkRequest);
            //homework.LinkHomeworkResources = linkHomeworkResources;

            flytDexContext.Homeworks.Add(homework);

            if (flytDexContext.SaveChanges() < 0)
            {
                return errorService.Error<HomeworkDto>("An error occurred: Unable to save changes");
            }

            HomeworkDto homeworkDto = mapper.Map<Homework, HomeworkDto>(homework);

            return new ServiceResult<HomeworkDto>(homeworkDto, ResultType.Success, "Success");
        }

        public ServiceResult<HomeworkDto> UpdateHomework(HomeworkRequest homeworkRequest)
        {
            string validationMessage = ValidateHomeworkRequest(homeworkRequest);

            if (!string.IsNullOrEmpty(validationMessage))
            {
                return errorService.Error<HomeworkDto>(validationMessage);
            }

            Homework homework = flytDexContext.Homeworks.SingleOrDefault(h =>
                h.Id == homeworkRequest.Id &&
                h.SchoolId == homeworkRequest.SchoolId &&
                h.LessonId == homeworkRequest.LessonId);

            if (homework == null)
            {
                return errorService.Error<HomeworkDto>("An error occurred: Invalid homework - not found");
            }

            mapper.Map(homeworkRequest, homework);
			
			flytDexContext.LinkStudentHomeworks.RemoveRange(
				flytDexContext.LinkStudentHomeworks.Where(h => h.HomeworkId == homeworkRequest.Id)
			);

			flytDexContext.LinkHomeworkResources.RemoveRange(
				flytDexContext.LinkHomeworkResources.Where(h => h.HomeworkId == homeworkRequest.Id)
			);

			flytDexContext.Homeworks.Update(homework);

            if (flytDexContext.SaveChanges() <= 0)
            {
                return errorService.Error<HomeworkDto>("Error updating homework, see log for error message");
            }

            HomeworkDto homeworkDto = mapper.Map<Homework, HomeworkDto>(homework);

            return new ServiceResult<HomeworkDto>(homeworkDto, ResultType.Success, "Success");
        }

        public ServiceResult<HomeworkDto> RemoveHomework(Guid homeworkId)
        {
            if (homeworkId == Guid.Empty)
            {
                return errorService.Error<HomeworkDto>("An error occurred: Homework Id is invalid");
            }

            Homework homework = flytDexContext.Homeworks.SingleOrDefault(h => h.Id == homeworkId);

            if (homework == null)
            {
                return errorService.Error<HomeworkDto>("An error occurred: A homework does not exist");
            }

            flytDexContext.Homeworks.Remove(homework);

            if (flytDexContext.SaveChanges() <= 0)
            {
                return errorService.Error<HomeworkDto>("Error removing homework, see log for error message");
            }

            HomeworkDto homeworkDto = mapper.Map<Homework, HomeworkDto>(homework);

            return new ServiceResult<HomeworkDto>(homeworkDto, ResultType.Success, "Success");

        }

        public ServiceResult<HomeworkTemplateDto> CreateHomeworkTemplate(HomeworkTemplateRequest homeworkTemplateRequest)
        {
            string validationMessage = ValidateHomeworkTemplateRequest(homeworkTemplateRequest);

            if (!string.IsNullOrEmpty(validationMessage))
            {
                return errorService.Error<HomeworkTemplateDto>(validationMessage);
            }

            if (flytDexContext.HomeworkTemplates.Any(ht => ht.Id == homeworkTemplateRequest.Id))
            {
                return errorService.Error<HomeworkTemplateDto>("An error occurred: A homework template with this Id already exists");
            }

            HomeworkTemplate homeworkTemplate = mapper.Map<HomeworkTemplateRequest, HomeworkTemplate>(homeworkTemplateRequest);

            flytDexContext.HomeworkTemplates.Add(homeworkTemplate);

            if (flytDexContext.SaveChanges() < 0)
            {
                return errorService.Error<HomeworkTemplateDto>("An error occurred: Unable to save HomeworkTemplate");
            }

            HomeworkTemplateDto homeworkTemplateDto = mapper.Map<HomeworkTemplate, HomeworkTemplateDto>(homeworkTemplate);

            return new ServiceResult<HomeworkTemplateDto>(homeworkTemplateDto, ResultType.Success, "Success");
        }

        public ServiceResult<Guid?> UpdateHomeworkTemplate(HomeworkTemplateRequest homeworkTemplateRequest)
        {
            string validationMessage = ValidateHomeworkTemplateRequest(homeworkTemplateRequest);

            if (!string.IsNullOrEmpty(validationMessage))
            {
                return errorService.Error<Guid?>(validationMessage);
            }

            HomeworkTemplate homeworkTemplate = flytDexContext.HomeworkTemplates.SingleOrDefault(ht => ht.Id == homeworkTemplateRequest.Id);

            if (homeworkTemplate == null)
            {
                return errorService.Error<Guid?>("An error occurred: Invalid HomeworkTemplate - not found");
            }

            homeworkTemplate.Description = homeworkTemplateRequest.Description;
            homeworkTemplate.Title = homeworkTemplateRequest.Title;

            flytDexContext.HomeworkTemplates.Update(homeworkTemplate);

            if (flytDexContext.SaveChanges() <= 0)
            {
                return errorService.Error<Guid?>("Error updating HomeworkTemplate, see log for error message");
            }

            return new ServiceResult<Guid?>(homeworkTemplate.Id, ResultType.Success, "Success");
        }

        public string ValidateHomeworkRequest(HomeworkRequest homeworkRequest)
        {
            if (!flytDexContext.Schools.Any(s => s.Id == homeworkRequest.SchoolId))
            {
                return "An error occurred: Invalid SchoolId";
            }

            if (!flytDexContext.Lessons.Any(l => l.Id == homeworkRequest.LessonId))
            {
                return "An error occurred: Invalid LessonId";
            }

            if (homeworkRequest.HomeworkTemplateId != Guid.Empty && homeworkRequest.HomeworkTemplateId != null)
            {
                if (!flytDexContext.HomeworkTemplates.Any(h => h.Id == homeworkRequest.HomeworkTemplateId))
                {
                    return "An error occurred: Invalid HomeworkTemplateId";
                }
            }

            return null;
        }

        public string ValidateHomeworkTemplate(bool creating, Guid schoolId, ICollection<ResourceDto> resources)
        {
            if (!flytDexContext.Schools.Any(s => s.Id == schoolId))
            {
                return "An error occurred: Invalid SchoolId";
            }

            if (resources != null)
            {
                foreach (ResourceDto resourceDto in resources)
                {
                    if (creating && flytDexContext.Resources.Any(r => r.Id == resourceDto.ResourceId))
                    {
                        return "An error occurred: This resource Id already exists";
                    }
                }
            }

            return null;
        }

        public string ValidateHomeworkTemplateRequest(HomeworkTemplateRequest homeworkTemplateRequest)
        {
            if (!flytDexContext.Schools.Any(s => s.Id == homeworkTemplateRequest.SchoolId))
            {
                return "An error occurred: Invalid SchoolId";
            }

            if (!flytDexContext.Lessons.Any(l => l.Id == homeworkTemplateRequest.LessonId))
            {
                return "An error occurred: Invalid LessonId";
            }

            if (homeworkTemplateRequest.Resources != null)
            {
                foreach (ResourceDto resourceDto in homeworkTemplateRequest.Resources)
                {
                    if (flytDexContext.Resources.Any(r => r.Id == resourceDto.ResourceId))
                    {
                        return "An error occurred: This resource Id already exists";
                    }
                }
            }

            return null;
        }

    }
}
