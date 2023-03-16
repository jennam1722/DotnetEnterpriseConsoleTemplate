namespace EnterpriseTemplate.Services
{
    public interface IDateService
    {
        bool IsFirstDayOfYear { get; }
        DateTime Today { get; }
    }
}