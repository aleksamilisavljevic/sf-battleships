using Microsoft.ServiceFabric.Services.Remoting;

namespace Communication
{
  public interface IUserDataInterface : IService
  {
    Task<User> getUser(string username);

    Task<User> addUser(User user);
  }
}
