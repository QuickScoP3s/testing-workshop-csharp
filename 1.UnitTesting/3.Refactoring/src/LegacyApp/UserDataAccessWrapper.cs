namespace LegacyApp;

public interface IUserDataAccessWrapper
{
    void AddUser(User user);
}

public class UserDataAccessWrapper : IUserDataAccessWrapper
{
    public void AddUser(User user)
    {
        UserDataAccess.AddUser(user);
    }
}
