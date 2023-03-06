using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseTemplate.Tests.Extensions
{
    public static class Common
    {
        public static Mock<ILogger<T>> VerifyLogMessage<T>(this Mock<ILogger<T>> logger, LogLevel logLevel, string expectedMessage, int callCount)
        {
            Func<object, Type, bool> state = (v, t) =>
            {
                return v is not null && v.ToString()!.Equals(expectedMessage, StringComparison.OrdinalIgnoreCase);
            };

            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == logLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => state(v, t)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Exactly(callCount));

            return logger;
        }
    }
}
