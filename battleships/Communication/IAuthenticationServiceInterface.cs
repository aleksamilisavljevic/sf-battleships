using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
  public interface IAuthenticationServiceInterface : IService
  {
    Task<AuthResponse> authenticateUser(string username, string password);
  }
}
