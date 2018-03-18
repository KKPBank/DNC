using CSM.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Entity
{
    public class DoNotCallTimePeriodEntity
    {
        public int Id { get; set; }
        public string FromTimeStr { get; set; }
        public string ToTimeStr { get; set; }
        public string ExecuteTimeStr { get; set; }

        public DateTime ToDateTime => CalculateDateTime(ToTimeStr);

        public DateTime FromDateTime
        {
            get
            {
                DateTime fromDateTime = CalculateDateTime(FromTimeStr);
                // if fromDate time is before toDate time, it means the fromDate time start yesterday.
                return fromDateTime <= ToDateTime ? fromDateTime : fromDateTime.AddDays(-1);
            }
        }

        DateTime CalculateDateTime(string time)
        {
            var times = time.Split(':'); // format "HH:mm"
            int hour = int.Parse(times[0]);
            int min = int.Parse(times[1]);
            return DateTime.Today.AddHours(hour).AddMinutes(min);
        }
    }
}
