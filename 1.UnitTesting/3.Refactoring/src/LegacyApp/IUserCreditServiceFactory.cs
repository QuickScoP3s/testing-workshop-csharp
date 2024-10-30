namespace LegacyApp;

public interface IUserCreditServiceFactory
{
    IUserCreditService Create();
}

public class UserCreditServiceFactory : IUserCreditServiceFactory
{
    public IUserCreditService Create()
    {
        return new UserCreditServiceClient();
    }
}