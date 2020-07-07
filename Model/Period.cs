using System;

namespace FlytDex.Domain.Model.FlytDex
{
    public class Period : SchoolEntity
    {
        public string Name { get; set; }

        public string Day { get; set; }

        /// <summary>
        /// This field allows us to link back to a WondeLesson (and therefore a WondePeriod)
        /// </summary>
        public int PeriodInstanceId { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }
    }
}
