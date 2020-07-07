using System;
using System.Collections.Generic;
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
    public class UserController : ControllerBase
    {
        //GET api/User/GetAlexaUsersForDevice
        [HttpGet("GetAlexaUsersForDevice")]
        [AuthorizeRoles(AuthorizationRoleType.AlexaMaster)]
        public IActionResult GetAlexaUsersForDevice(string deviceId)
        {
            IContainer container = IocService.BeginRequest();
            ServiceResult<List<AlexaUserDto>> result = container.GetInstance<IUserService>().GetAlexaUsersForDevice(deviceId);
            IocService.EndRequest(container);

            if (result.ResultType == ResultType.Error)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        //GET api/User/GetAlexaUserForUsername
        [HttpGet("GetAlexaUserForUsername")]
        [AuthorizeRoles(AuthorizationRoleType.AlexaMaster)]
        public IActionResult GetAlexaUserForUsername(string username)
        {
            IContainer container = IocService.BeginRequest();
            ServiceResult<AlexaUserDto> result = container.GetInstance<IUserService>().GetAlexaUserForUsername(username);
            IocService.EndRequest(container);

            if (result.ResultType == ResultType.Error)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        //GET api/User/GetUserSession
        [HttpGet("GetUserSession")]
        public IActionResult GetUserSession(string username)
        {
            IContainer container = IocService.BeginRequest();
            ServiceResult<UserSessionDto> result = container.GetInstance<IUserService>().GetUserSession(username);
            IocService.EndRequest(container);

            if (result.ResultType == ResultType.Error)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        //POST api/User/UpdateUser
        [HttpPost("UpdateUser")]
        public IActionResult UpdateUser([FromBody] UserRequest userRequest)
        {
            IContainer container = IocService.BeginRequest();
            ServiceResult<Guid> result = container.GetInstance<IUserService>().UpdateUser(userRequest);
            IocService.EndRequest(container);

            if (result.ResultType == ResultType.Error)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }
    }
}