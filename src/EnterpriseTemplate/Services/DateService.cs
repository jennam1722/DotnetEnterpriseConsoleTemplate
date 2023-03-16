using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseTemplate.Services
{
    public class DateService : IDateService
    {
        public DateTime Today => DateTime.Today;

        public bool IsFirstDayOfYear => Today.ToString("MMdd").Equals("0101");
    }
}
