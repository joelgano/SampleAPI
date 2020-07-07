using System;
using System.Collections.Generic;
using FlytDex.Shared;
using FlytDex.Shared.Dtos;
using FlytDex.Shared.Enums;
using FlytDex.Shared.Requests;

namespace FlytDex.Domain.Services.Interfaces
{
    public interface IUserService
    {
        ServiceResult<Guid> AddUser(string forename, string lastname, string password, UserType userType, IEnumerable<AuthorizationRoleType> roles, string email, Guid schoolId);

        List<UserDto> GetAllUsers();

        string GenerateUniqueUsername(string forename, string surname, UserType userType, string email, Guid schoolId);

        ServiceResult<List<AlexaUserDto>> GetAlexaUsersForDevice(string deviceId);

        ServiceResult<AlexaUserDto> GetAlexaUserForUsername(string username);

        ServiceResult<UserSessionDto> GetUserSession(string username);

        ServiceResult<Guid> UpdateUser(UserRequest userRequest);
    }
}
