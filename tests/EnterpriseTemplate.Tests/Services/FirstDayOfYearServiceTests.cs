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
    public class FirstDayOfYearServiceTests
    {
        [DataRow("2021-01-01", true)]
        [DataRow("2021-01-02", false)]
        [TestMethod]
        public void FirstDayOfYearService_IsFirstDayOfYear_ShouldWork(string testDate,bool expectedResult) 
        {
            var mocker = new AutoMocker();
            var mockDateService = mocker.GetMock<IDateService>();
            mockDateService.Setup(a => a.Today).Returns(DateTime.Parse(testDate));
            var firstDayService = mocker.CreateInstance<FirstDayOfYearService>();
            var result = firstDayService.IsFirstDayOfYear();
            Assert.AreEqual(expectedResult, result);
        }
    }
}
