using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
  public interface IBotLogicServiceInterface : IService
  {
    Task<BotState> InitializeState(BattleshipGrid probabilityGrid, BattleshipGrid playerGrid);

    Task<BotState> getNextMove(BotState currentState);

    Task<BattleshipGrid> placeShips();

  }
}
