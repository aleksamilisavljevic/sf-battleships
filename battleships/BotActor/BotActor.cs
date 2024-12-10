using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using BotActor.Interfaces;
using Communication;
using System.Fabric.Management.ServiceModel;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace BotActor
{
  /// <remarks>
  /// This class represents an actor.
  /// Every ActorID maps to an instance of this class.
  /// The StatePersistence attribute determines persistence and replication of actor state:
  ///  - Persisted: State is written to disk and replicated.
  ///  - Volatile: State is kept in memory only and replicated.
  ///  - None: State is kept in memory only and not replicated.
  /// </remarks>
  [StatePersistence(StatePersistence.Persisted)]
  internal class BotActor : Actor, IBotActor
  {
    private String probabilityGrid = "probabilityGrid";
    private String myGrid = "myGrid";
    private String playerGrid = "playerGrid";
    private String myGuess = "myGuess";
    private String playerGuess = "playerGuess";
    private String botState = "botState";
    public BotActor(ActorService actorService, ActorId actorId) 
        : base(actorService, actorId)
    {
    }

    public async Task<long> makeMove(long playerMove, CancellationToken cancellationToken)
    {
      long pg = await this.StateManager.GetStateAsync<long>(playerGuess, cancellationToken);
      long mg = await this.StateManager.GetStateAsync<long>(myGuess, cancellationToken);
      BattleshipGrid pyg = await this.StateManager.GetStateAsync<BattleshipGrid>(playerGrid, cancellationToken);
      BattleshipGrid myg = await this.StateManager.GetStateAsync<BattleshipGrid>(myGrid, cancellationToken);
      BotState curState = await this.StateManager.GetStateAsync<BotState>(botState, cancellationToken);
      long i = 0, j = 0, ret = 0;
      var statelessProxy = ServiceProxy.Create<IBotLogicServiceInterface>(
      new Uri("fabric:/battleships/BotLogicService"));
      curState = await statelessProxy.getNextMove(curState);
      i = playerMove / 10;
      j = playerMove % 10;
      pg += myg.grid[i][j];
      mg += curState.guessGrid.grid[curState.lastRow][curState.lastCol] - 2;
      ret = myg.grid[i][j] * 100 + curState.lastRow * 10 + curState.lastCol;
      if (pg == 19) return -100;
      if (mg == 19) return -200 - curState.lastRow * 10 - curState.lastCol;
      await this.StateManager.AddOrUpdateStateAsync(botState, curState, (k, v) => curState, cancellationToken);
      await this.StateManager.AddOrUpdateStateAsync(playerGrid, pyg, (k,v) => pyg, cancellationToken);
      await this.StateManager.AddOrUpdateStateAsync(playerGuess, pg, (k, v) => pg, cancellationToken);
      await this.StateManager.AddOrUpdateStateAsync(myGuess, mg, (k, v) => mg, cancellationToken);
      return ret;
    }

    public async Task startGame(BattleshipGrid playerBattleshipGrid, CancellationToken cancellationToken)
    {
      var statelessProxy = ServiceProxy.Create<IBotLogicServiceInterface>(
      new Uri("fabric:/battleships/BotLogicService"));

      BattleshipGrid myBattleshipGrid = await statelessProxy.placeShips();
      await this.StateManager.AddOrUpdateStateAsync(playerGuess, 0L, (k, v) => 0L, cancellationToken);
      await this.StateManager.AddOrUpdateStateAsync(myGuess, 0L, (k, v) => 0L, cancellationToken);
      await this.StateManager.AddOrUpdateStateAsync(myGrid, myBattleshipGrid, (k, v) => myBattleshipGrid, cancellationToken);
      await this.StateManager.AddOrUpdateStateAsync(playerGrid, playerBattleshipGrid, (k, v) => playerBattleshipGrid, cancellationToken);
      BattleshipGrid prob = await this.StateManager.GetStateAsync<BattleshipGrid>(probabilityGrid, cancellationToken);

      BotState initialState = await statelessProxy.InitializeState(prob, playerBattleshipGrid);

      await this.StateManager.AddOrUpdateStateAsync(botState, initialState, (k, v) => initialState, cancellationToken);
      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 10; j++)
        {
          prob.grid[i][j] = 3 * playerBattleshipGrid.grid[i][j] + Math.Max(1, prob.grid[i][j] / 2);
        }
      }
      await this.StateManager.AddOrUpdateStateAsync(probabilityGrid, prob, (k, v) => prob, cancellationToken);
    }

    

    /// <summary>
    /// This method is called whenever an actor is activated.
    /// An actor is activated the first time any of its methods are invoked.
    /// </summary>
    protected override Task OnActivateAsync()
    {
      ActorEventSource.Current.ActorMessage(this, "Actor activated.");

      // The StateManager is this actor's private state store.
      // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
      // Any serializable object can be saved in the StateManager.
      // For more information, see https://aka.ms/servicefabricactorsstateserialization
      long[][] grid = new long[10][];
      for (int i = 0; i< 10;i++)
      {
        grid[i] = new long[10];
        for(int j = 0; j< 10; j++)
        {
          grid[i][j] = 1;
        }
      }
      BattleshipGrid b = new BattleshipGrid();
      b.grid = grid;
      return this.StateManager.TryAddStateAsync(probabilityGrid, b);
    }
  }
}
