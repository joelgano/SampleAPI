using System;
using System.Collections.Generic;
using FlytDex.Shared;
using FlytDex.Shared.Dtos;

namespace FlytDex.Domain.Services.Interfaces
{
    public interface IPeriodService
    {
        ServiceResult<List<PeriodDto>> GetPeriods(Guid employeeId, Guid schoolId, DateTime startTime, DateTime endTime);

        ServiceResult<PeriodDto> CreatePeriod(bool save, Guid schoolId, string name, DateTime startDateTime, DateTime endDateTime);
    }
}
