using System;
using System.Collections.Generic;
using FlytDex.Domain.Model.FlytDex.Links;
using FlytDex.Shared.Enums;

namespace FlytDex.Domain.Model.FlytDex
{
    public class User : Entity
    {
        public User(string username, string password, string alexaNickname, string alexaPassCode)
        {
            Username = username;
            Password = password;
            AlexaNickname = alexaNickname;
            AlexaPassCode = alexaPassCode;
            AlexaCodeCreatedDateTime = DateTime.Now;
        }

        public DateTime LastLoginDateTime { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public DateTime LastAlexaUseDateTime { get; set; }

        public string AlexaNickname { get; set; }

        public string AlexaPassCode { get; set; }

        public DateTime AlexaCodeCreatedDateTime { get; set; }

        public PreferredContactMethod PreferredContactMethod { get; set; }

        public string UserEmail { get; set; }

        public string UserPhoneNumber { get; set; }

        public virtual ICollection<AuthorizationRole> AuthorizationRoles { get; set; }

        public virtual ICollection<UserDevice> UserDevices { get; set; }

        public virtual ICollection<LinkUserSchool> LinkUserSchools { get; set; }
    }
}
