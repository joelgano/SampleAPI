using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FlytDex.Domain.Model.FlytDex;
using FlytDex.Domain.Services.Interfaces;
using FlytDex.Shared;
using FlytDex.Shared.Dtos;

namespace FlytDex.Domain.Services
{
    public class PeriodService : IPeriodService
    {
        private IFlytDexDbContext flytDexContext;
        private IMapper mapper;
        private IErrorService errorService;

        public PeriodService(IFlytDexDbContext flytDexContext, IMapper mapper, IErrorService errorService)
        {
            this.flytDexContext = flytDexContext;
            this.mapper = mapper;
            this.errorService = errorService;
        }

        public ServiceResult<List<PeriodDto>> GetPeriods(Guid employeeId, Guid schoolId, DateTime startDateTime, DateTime endDateTime)
        {
            List<PeriodDto> periodDtos = flytDexContext.Periods
                .Where(p =>
                    p.SchoolId == schoolId &&
                    p.StartDateTime.Value.Date >= startDateTime.Date &&
                    p.EndDateTime.Value.Date <= endDateTime.Date)
                .OrderBy(p => p.StartDateTime)
                .ProjectTo<PeriodDto>(mapper.ConfigurationProvider)
                .ToList();



            List<Event> cachedEvents = flytDexContext.Events
                .Where(e =>
                    e.SchoolId == schoolId &&
                    e.EmployeeId == employeeId &&
                    e.StartDateTime.Date <= startDateTime.Date &&
                    e.EndDateTime.Date >= endDateTime.Date).ToList();

            foreach (PeriodDto period in periodDtos)
            {
                period.DisplayName = period.PeriodNameShort;
                if (cachedEvents.Any(e => e.StartDateTime == period.StartDateTime && e.EndDateTime == period.EndDateTime))
                {
                    Event evnt = cachedEvents.Where(e => e.StartDateTime == period.StartDateTime && e.EndDateTime == period.EndDateTime).FirstOrDefault();
                    period.DisplayName += " - " + evnt.LinkEventStudentGroups.First().StudentGroup.GroupName;
                }
            }


            if (periodDtos.Count == 0)
            {
                return errorService.Warn<List<PeriodDto>>("No Periods found.");
            }

            return new ServiceResult<List<PeriodDto>>(periodDtos);
        }

        public ServiceResult<PeriodDto> CreatePeriod(bool save, Guid schoolId, string name, DateTime startDateTime, DateTime endDateTime)
        {
            Period period = new Period()
            {
                SchoolId = schoolId,
                StartDateTime = startDateTime,
                Day = startDateTime.DayOfWeek.ToString(),
                EndDateTime = endDateTime,
                Name = name
            };

            flytDexContext.Periods.Add(period);

            if (save)
            {
                if (flytDexContext.SaveChanges() == -1)
                {
                    return errorService.Error<PeriodDto>("Error adding period");
                }
            }

            PeriodDto periodDto = mapper.Map<PeriodDto>(period);

            return new ServiceResult<PeriodDto>(periodDto);
        }
    }
}
