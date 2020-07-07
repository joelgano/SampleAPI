using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FlytDex.Domain.Model.FlytDex;
using FlytDex.Domain.Model.FlytDex.Links;
using FlytDex.Domain.Services.Interfaces;
using FlytDex.Shared;
using FlytDex.Shared.Dtos;
using FlytDex.Shared.Enums;
using FlytDex.Shared.Requests;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace FlytDex.Domain.Services
{
    public class UserService : IUserService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(UserService));

        private IFlytDexDbContext flytDexContext;
        private IMapper mapper;
        private IAuthorizationService authorizationService;
        private IAuthorizationRoleService applicationRoleService;
        private IErrorService errorService;

        public UserService(IFlytDexDbContext flytDexContext, IMapper mapper, IAuthorizationService authorizationService, IAuthorizationRoleService applicationRoleService, IErrorService errorService)
        {
            this.flytDexContext = flytDexContext;
            this.mapper = mapper;
            this.authorizationService = authorizationService;
            this.applicationRoleService = applicationRoleService;
            this.errorService = errorService;
        }

        public ServiceResult<List<AlexaUserDto>> GetAlexaUsersForDevice(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return errorService.Error<List<AlexaUserDto>>("Please provide a valid Alexa Device ID.");
            }

            if (!flytDexContext.UserDevices.Any(ud => ud.DeviceId == deviceId))
            {
                return errorService.Warn<List<AlexaUserDto>>("This device is not registered on the system.");
            }

            List<User> users = flytDexContext.Users.Include(u => u.UserDevices).Where(u => u.UserDevices.Any(ud => ud.DeviceId == deviceId)).ToList();

            if (users.Count <= 0)
            {
                return errorService.Warn<List<AlexaUserDto>>("This device is recognised but is not currently associated to any users.");
            }

            List<AlexaUserDto> alexaUserDtos = mapper.Map<List<User>, List<AlexaUserDto>>(users);

            return new ServiceResult<List<AlexaUserDto>>(alexaUserDtos, ResultType.Success, "Success");
        }

        public ServiceResult<AlexaUserDto> GetAlexaUserForUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return errorService.Error<AlexaUserDto>("Please provide a valid Username");
            }

            User user = flytDexContext.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return errorService.Error<AlexaUserDto>("User with username was not found");
            }

            AlexaUserDto alexaUserDto = mapper.Map<User, AlexaUserDto>(user);

            return new ServiceResult<AlexaUserDto>(alexaUserDto, ResultType.Success, "Success");
        }

        public ServiceResult<Guid> AddUser(string forename, string surname, string password, UserType userType, IEnumerable<AuthorizationRoleType> roles, string email, Guid schoolId)
        {
            if (string.IsNullOrEmpty(forename) || string.IsNullOrEmpty(surname))
            {
                return errorService.Error<Guid>("Forename and Surname must be provided");
            }

            if (string.IsNullOrEmpty(password))
            {
                return errorService.Error<Guid>("Password must be provided");
            }

            string username = GenerateUniqueUsername(forename, surname, userType, email, schoolId);
            string hashedPassword = authorizationService.GeneratePassword(password);
            string alexaPassCode = authorizationService.GenerateAlexaPassCode();

            User user = new User(username, hashedPassword, forename, alexaPassCode);

            flytDexContext.Users.Add(user);

            if (flytDexContext.SaveChanges() <= 0)
            {
                return errorService.Error<Guid>("Error adding user, see log for error message");
            }

            if (applicationRoleService.AddRolesToUser(user, roles).ResultType == ResultType.Error)
            {
                return errorService.Warn<Guid>(string.Format("Failed To add user roles for user id {0}", user.Id));
            }

            return new ServiceResult<Guid>(user.Id, ResultType.Success, "Success");
        }

        public List<UserDto> GetAllUsers()
        {
            List<UserDto> userDtos = flytDexContext.Users
                .ProjectTo<UserDto>(mapper.ConfigurationProvider)
                .ToList();

            return userDtos;
        }

        public string GenerateUniqueUsername(string forename, string surname, UserType userType, string email, Guid schoolId)
        {
            string username = string.Empty;
            string domain = string.Empty;

            int i = 0;
            bool unique = false;

            if ((userType == UserType.Employee) || (userType == UserType.Parent))
            {
                if (email == null)
                {

                    Setting setting = flytDexContext.Settings.SingleOrDefault(s => s.SettingType == SettingType.OEEmailFormat);
                    domain = setting.Value.Substring(setting.Value.IndexOf('@'));
                    email = string.Format("{0}.{1}{2}", forename, surname, domain);
                    //username = getUsername(forename, surname, schoolId, userType);
                }
                else
                {
                    if (flytDexContext.Users.Any(u => u.Username == email))
                    {
                        return email;
                    }
                }
                username = email;
            }
            else if (userType == UserType.Student)
            {
                Setting setting = flytDexContext.Settings.SingleOrDefault(s => s.SettingType == SettingType.SchoolEmailFormat && s.SchoolId == schoolId);
                domain = setting.Value.Substring(setting.Value.IndexOf('@'));
                if (setting.Value.Substring(0, setting.Value.IndexOf('.')) == "forename")
                {
                    username = string.Format("{0}.{1}{2}", forename, surname, domain);
                }
                else
                {
                    username = string.Format("{0}.{1}{2}", surname, forename, domain);
                }
            }
            else
            {
                username = string.Format("{0}.{1}", forename, surname);
            }

            string baseName = string.Format("{0}.{1}", forename, surname);
            while (!unique)
            {
                if (i != 0)
                {
                    username = string.Format("{0}.{1}{2}", baseName, i.ToString(), domain);
                }
                if (!flytDexContext.Users.Any(u => u.Username == username))
                {
                    unique = true;
                }

                i++;
            }

            return username;
        }

        public ServiceResult<UserSessionDto> GetUserSession(string username)
        {
            User user = flytDexContext.Users
                .Include(u => u.LinkUserSchools)
                .SingleOrDefault(u => u.Username == username);

            if (user == null)
            {
                return errorService.Error<UserSessionDto>("Error occurred: User not found");
            }

            IEnumerable<Guid> employeeIds = user.LinkUserSchools.Where(lus => lus.UserType == UserType.Employee).Select(lus => lus.UserTypeId);
            IEnumerable<Guid> studentIds = user.LinkUserSchools.Where(lus => lus.UserType == UserType.Student).Select(lus => lus.UserTypeId);
            List<Employee> cachedEmployees = flytDexContext.Employees
                .Include(e => e.LinkEmployeeRoles)
                    .ThenInclude(ler => ler.Role)
                .Where(e => employeeIds.Contains(e.Id))
                .ToList();
            List<Student> cachedStudents = flytDexContext.Students.Where(s => studentIds.Contains(s.Id)).ToList();

            UserSessionDto userSessionDto = new UserSessionDto();
            userSessionDto.UserId = user.Id;
            userSessionDto.Username = user.Username;
            userSessionDto.LastLoginDateTime = user.LastLoginDateTime;

            userSessionDto.UserSchools = new List<LinkUserSchoolDto>();
            foreach (LinkUserSchool linkUserSchool in user.LinkUserSchools)
            {
                if (linkUserSchool.UserType == UserType.Employee)
                {
                    Employee employee = cachedEmployees.SingleOrDefault(e => e.Id == linkUserSchool.UserTypeId);
                    if (employee == null)
                    {
                        logger.Warn(string.Format("Employee User found with no valid Employee attached, User Id: {0}", user.Id));
                        return errorService.Error<UserSessionDto>("Error occurred: User Invalid");
                    }

                    userSessionDto.UserSchools.Add(new LinkUserSchoolDto()
                    {
                        UserId = user.Id,
                        SchoolId = linkUserSchool.SchoolId,

                        UserType = linkUserSchool.UserType,
                        UserTypeId = employee.Id,

                        Roles = employee.LinkEmployeeRoles.Select(ler => ler.Role.RoleTitle).OrderBy(r => r).ToList()
                    });
                }
                else if (linkUserSchool.UserType == UserType.Student)
                {
                    Student student = cachedStudents.SingleOrDefault(s => s.Id == linkUserSchool.UserTypeId);
                    if (student == null)
                    {
                        logger.Warn(string.Format("Student User found with no valid Student attached, User Id: {0}", user.Id));
                        return errorService.Error<UserSessionDto>("Error occurred: User Invalid");
                    }

                    userSessionDto.UserSchools.Add(new LinkUserSchoolDto()
                    {
                        UserId = user.Id,
                        SchoolId = linkUserSchool.SchoolId,

                        UserType = linkUserSchool.UserType,
                        UserTypeId = student.Id,

                        Roles = new List<string>()
                    });
                }
            }

            return new ServiceResult<UserSessionDto>(userSessionDto);
        }

        public ServiceResult<Guid> UpdateUser(UserRequest userRequest)
        {
            User user = flytDexContext.Users.SingleOrDefault(u => u.Id == userRequest.Id);

            if (user == null)
            {
                return errorService.Error<Guid>("Error occurred: User not found");
            }

            List<LinkUserSchool> linkUserSchools = user.LinkUserSchools.Where(lus => lus.UserId == userRequest.Id).ToList();

            foreach (LinkUserSchool linkUserSchool in linkUserSchools)
            {
                if (linkUserSchool.UserType == UserType.Employee)
                {
                    Employee employee = flytDexContext.Employees.SingleOrDefault(e => e.Id == linkUserSchool.UserTypeId);
                    ContactDetails contactDetails = flytDexContext.ContactDetails.SingleOrDefault(cd => cd.Id == employee.ContactDetailsId);

                    if (contactDetails == null)
                    {
                        return errorService.Error<Guid>("Error occurred: Contact details not found");
                    }

                    contactDetails.PreferredEmail = userRequest.PreferredEmail;
                    contactDetails.PreferredPhone = userRequest.PreferredPhone;
                    flytDexContext.ContactDetails.Update(contactDetails);

                }
            }

            user.Password = authorizationService.GeneratePassword(userRequest.Password);
            user.UserEmail = userRequest.PreferredEmail;
            user.UserPhoneNumber = userRequest.PreferredPhone;
            user.LastLoginDateTime = DateTime.Now;
            flytDexContext.Users.Update(user);

            if (flytDexContext.SaveChanges() < 0)
            {
                return errorService.Error<Guid>("An error occurred: Unable to save changes");
            }

            return new ServiceResult<Guid>(user.Id, ResultType.Success, "Success");
        }

        private ContactDetails getContactDetails(LinkUserSchool linkUserSchool)
        {
            ContactDetails contactDetails = new ContactDetails();

            if (linkUserSchool.UserType == UserType.Employee)
            {
                Employee employee = flytDexContext.Employees.SingleOrDefault(e => e.Id == linkUserSchool.UserTypeId);
                contactDetails = flytDexContext.ContactDetails.SingleOrDefault(cd => cd.Id == employee.ContactDetailsId);
            }

            if (linkUserSchool.UserType == UserType.Student)
            {

            }

            return contactDetails;
        }
    }
}
