using Microsoft.ServiceFabric.Services.Remoting;

namespace Communication
{
  public interface IRegistrationServiceInterface : IService
  {
    Task<User> registerUser(User user);
  }
}
