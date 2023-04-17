using EnterpriseTemplate.Services;
using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseTemplate.Tests.Services
{
    [TestClass]
    public class DateServiceTests
    {
        [TestMethod]
        public void DateService_IsnewYear_shouldWork()
        {
            //string date, bool expectedReturn
            var mocker = new AutoMocker();
            var dateService = mocker.CreateInstance<DateService>();
            var result = dateService.Today;
            Assert.AreEqual(DateTime.Today.Date, result.Date);
        }
    }
}
