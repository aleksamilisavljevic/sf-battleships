using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace RegistrationService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class RegistrationService : StatelessService, IRegistrationServiceInterface
    {
        public RegistrationService(StatelessServiceContext context)
            : base(context)
        { }

    public async Task<User> registerUser(User user)
    {
      string username = user.username;

      var partitionId = (username.GetHashCode() % 3 + 3) % 3;

      var statefulProxy = ServiceProxy.Create<IUserDataInterface>(
      new Uri("fabric:/battleships/UserData"), new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

      HashAlgorithm sha = SHA512.Create();

      var pepper = File.ReadAllText(Path.Join(Environment.GetEnvironmentVariable("C:\\Users\\aleksam\\Documents\\battleships\\battleships\\battleships\\secrets"), "RegistrationPepper"));

      user.password = GetHash(sha, user.password + pepper);

      var retUser = await statefulProxy.addUser(user);

      return retUser;
    }

    private static string GetHash(HashAlgorithm hashAlgorithm, string input)
    {

      byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

      var sBuilder = new StringBuilder();

      for (int i = 0; i < data.Length; i++)
      {
        sBuilder.Append(data[i].ToString("x2"));
      }

      return sBuilder.ToString();
    }

    /// <summary>
    /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
    /// </summary>
    /// <returns>A collection of listeners.</returns>
    protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }
    }
}
