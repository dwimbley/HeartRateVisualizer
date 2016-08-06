using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wimbley.AppleWatch.HeartRate.Site.Core
{
    public class HeartRateData
    {
        public string SourceName { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
        public DateTime RecordedDate { get; set; }
    }
}
