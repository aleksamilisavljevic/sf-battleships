using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Communication;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace UserData
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class UserData : StatefulService, IUserDataInterface
  {
        public UserData(StatefulServiceContext context)
            : base(context)
        { }

    public async Task<User> getUser(string username)
    {
      var dictUsers = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, User>>("users");

      User user;

      using (var tx = this.StateManager.CreateTransaction())
      {
        var fetchedUser = await dictUsers.TryGetValueAsync(tx, username);

        user = fetchedUser.HasValue ? fetchedUser.Value : null;

        await tx.CommitAsync();
      }

      return user;
    }

    public async Task<User> addUser(User user)
    {
      var dictUsers = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, User>>("users");

      User retUser;

      using (var tx = this.StateManager.CreateTransaction())
      {
        var fetchedUser = await dictUsers.TryGetValueAsync(tx, user.username);

        retUser = fetchedUser.HasValue ? null : user;

        if (!fetchedUser.HasValue)
        {
          await dictUsers.AddAsync(tx, user.username, user);
        }

        await tx.CommitAsync();
      }

      return retUser;
    }

    /// <summary>
    /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
    /// </summary>
    /// <remarks>
    /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
    /// </remarks>
    /// <returns>A collection of listeners.</returns>
    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }
    }
}
