using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FlytDex.Shared;
using FlytDex.Shared.Dtos;
using FlytDex.Shared.Requests;

namespace FlytDex.Domain.Services.Interfaces
{
    public interface IHomeworkService
    {
        ServiceResult<List<HomeworkDto>> GetHomeworkForLesson(Guid lessonId, params Expression<Func<HomeworkDto, object>>[] includes);

		ServiceResult<List<HomeworkDto>> GetPastHomework(Guid schoolId, Guid employeeId, Guid subjectId, Guid studentGroupId, params Expression<Func<HomeworkDto, object>>[] includes);

		ServiceResult<HomeworkDto> CreateHomework(HomeworkRequest homeworkRequest);

		ServiceResult<HomeworkDto> UpdateHomework(HomeworkRequest homeworkRequest);

        ServiceResult<HomeworkDto> RemoveHomework(Guid homeworkId);

        ServiceResult<HomeworkTemplateDto> CreateHomeworkTemplate(HomeworkTemplateRequest homeworkTemplateRequest);

        ServiceResult<Guid?> UpdateHomeworkTemplate(HomeworkTemplateRequest homeworkTemplateRequest);

        string ValidateHomeworkRequest(HomeworkRequest homeworkRequest);

        string ValidateHomeworkTemplate(bool creating, Guid schoolId, ICollection<ResourceDto> resources);

        string ValidateHomeworkTemplateRequest(HomeworkTemplateRequest homeworkTemplateRequest);
    }
}
