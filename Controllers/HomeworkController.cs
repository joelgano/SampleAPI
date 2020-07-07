using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FlytDex.Domain.Services;
using FlytDex.Domain.Services.Interfaces;
using FlytDex.Shared;
using FlytDex.Shared.Attributes;
using FlytDex.Shared.Dtos;
using FlytDex.Shared.Enums;
using FlytDex.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StructureMap;

namespace FlytDex.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class HomeworkController : ControllerBase
	{
		// GET api/Homework/GetHomeworkForLesson
		[HttpGet("GetHomeworkForLesson")]
		[AuthorizeRoles(AuthorizationRoleType.Admin, AuthorizationRoleType.Teacher, AuthorizationRoleType.Technician)]
		public IActionResult GetHomeworkForLesson(Guid lessonId)
		{
			IContainer container = IocService.BeginRequest();
			Expression<Func<HomeworkDto, object>>[] includes = new Expression<Func<HomeworkDto, object>>[]
			{
				include => include.Resources
			};
			ServiceResult<List<HomeworkDto>> result = container.GetInstance<IHomeworkService>().GetHomeworkForLesson(lessonId, includes);
			IocService.EndRequest(container);

			if (result.ResultType == ResultType.Error)
			{
				return BadRequest(result.Message);
			}

			return Ok(result);
		}

		//GET api/Homework/GetPastHomework
		[HttpGet("GetPastHomework")]
		[AuthorizeRoles(AuthorizationRoleType.Admin, AuthorizationRoleType.Teacher, AuthorizationRoleType.Technician)]
		public IActionResult GetPastHomework(Guid schoolId, Guid employeeId, Guid subjectId, Guid studentGroupId)
		{
			IContainer container = IocService.BeginRequest();
			Expression<Func<HomeworkDto, object>>[] includes = new Expression<Func<HomeworkDto, object>>[]
			{
				 include => include.Students
			};
			ServiceResult<List<HomeworkDto>> result = container.GetInstance<IHomeworkService>().GetPastHomework(schoolId, employeeId, subjectId, studentGroupId, includes);
			IocService.EndRequest(container);

			if (result.ResultType == ResultType.Error)
			{
				return BadRequest(result.Message);
			}

			return Ok(result);
		}

		//POST api/Homework/CreateHomework
		[HttpPost("CreateHomework")]
		[AuthorizeRoles(AuthorizationRoleType.Admin, AuthorizationRoleType.Teacher, AuthorizationRoleType.Technician)]
		public IActionResult CreateHomework([FromBody] HomeworkRequest homeworkRequest)
		{
			IContainer container = IocService.BeginRequest();
			ServiceResult<HomeworkDto> result = container.GetInstance<IHomeworkService>().CreateHomework(homeworkRequest);
			IocService.EndRequest(container);

			if (result.ResultType == ResultType.Error)
			{
				return BadRequest(result.Message);
			}

			return Ok(result);
		}

		// POST api/Homework/UpdateHomework
		[HttpPost("UpdateHomework")]
		[AuthorizeRoles(AuthorizationRoleType.Admin, AuthorizationRoleType.Teacher, AuthorizationRoleType.Technician)]
		public IActionResult UpdateHomework([FromBody] HomeworkRequest homeworkRequest)
		{
			IContainer container = IocService.BeginRequest();
			ServiceResult<HomeworkDto> result = container.GetInstance<IHomeworkService>().UpdateHomework(homeworkRequest);
			IocService.EndRequest(container);

			if (result.ResultType == ResultType.Error)
			{
				return BadRequest(result.Message);
			}

			return Ok(result);
		}

		// DELETE api/Homework/RemoveHomework
		[HttpDelete("RemoveHomework")]
		[AuthorizeRoles(AuthorizationRoleType.Admin, AuthorizationRoleType.Teacher, AuthorizationRoleType.Technician)]
		public IActionResult RemoveHomework(Guid homeworkId)
		{
			IContainer container = IocService.BeginRequest();
			ServiceResult<HomeworkDto> result = container.GetInstance<IHomeworkService>().RemoveHomework(homeworkId);
			IocService.EndRequest(container);

			if (result.ResultType == ResultType.Error)
			{
				return BadRequest(result.Message);
			}

			return Ok(result);
		}

		//POST api/Homework/CreateHomeworkTemplate
		[HttpPost("CreateHomeworkTemplate")]
		[AuthorizeRoles(AuthorizationRoleType.Admin, AuthorizationRoleType.Teacher, AuthorizationRoleType.Technician)]
		public IActionResult CreateHomeworkTemplate([FromBody] HomeworkTemplateRequest homeworkTemplateRequest)
		{
			IContainer container = IocService.BeginRequest();
			ServiceResult<HomeworkTemplateDto> result = container.GetInstance<IHomeworkService>().CreateHomeworkTemplate(homeworkTemplateRequest);
			IocService.EndRequest(container);

			if (result.ResultType == ResultType.Error)
			{
				return BadRequest(result.Message);
			}

			return Ok(result);
		}

		// POST api/Homework/UpdateHomeworkTemplate
		[HttpPost("UpdateHomeworkTemplate")]
		[AuthorizeRoles(AuthorizationRoleType.Admin, AuthorizationRoleType.Teacher, AuthorizationRoleType.Technician)]
		public IActionResult UpdateHomeworkTemplate([FromBody] HomeworkTemplateRequest homeworkTemplateRequest)
		{
			IContainer container = IocService.BeginRequest();
			ServiceResult<Guid?> result = container.GetInstance<IHomeworkService>().UpdateHomeworkTemplate(homeworkTemplateRequest);
			IocService.EndRequest(container);

			if (result.ResultType == ResultType.Error)
			{
				return BadRequest(result.Message);
			}

			return Ok(result);
		}
	}
}
