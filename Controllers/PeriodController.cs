using System;
using System.Collections.Generic;
using FlytDex.Domain.Services;
using FlytDex.Domain.Services.Interfaces;
using FlytDex.Shared;
using FlytDex.Shared.Attributes;
using FlytDex.Shared.Dtos;
using FlytDex.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StructureMap;

namespace FlytDex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PeriodController : ControllerBase
    {
        // GET api/Period/GetPeriods
        [HttpGet("GetPeriods")]
        [AuthorizeRoles(AuthorizationRoleType.Admin, AuthorizationRoleType.Teacher, AuthorizationRoleType.Technician)]
        public IActionResult GetPeriods(Guid employeeId, Guid schoolId, DateTime startDateTime, DateTime endDateTime)
        {
            IContainer container = IocService.BeginRequest();
            ServiceResult<List<PeriodDto>> result = container.GetInstance<IPeriodService>().GetPeriods(employeeId, schoolId, startDateTime, endDateTime);
            IocService.EndRequest(container);

            if (result.ResultType == ResultType.Error)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }
    }
}
