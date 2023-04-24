using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseTemplate.Services
{
    public class FirstDayOfYearService : IFirstDayOfYearService
    {
        private readonly IDateService dateService;

        public FirstDayOfYearService(IDateService dateService)
        {
            this.dateService = dateService;
        }
        public bool IsFirstDayOfYear()
        {
            return dateService.Today.ToString("MMdd").Equals("0101");
        }
    }
}
