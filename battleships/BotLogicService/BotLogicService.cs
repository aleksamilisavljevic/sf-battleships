using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Communication;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace BotLogicService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class BotLogicService : StatelessService, IBotLogicServiceInterface
    {


    private static readonly object lck = new object();
    private static int callCount = 0;

    private void called() {
      lock(lck) { callCount++; }
    }

    public BotLogicService(StatelessServiceContext context)
            : base(context)
        { }

    public async Task<BotState> getNextMove(BotState state)
    {
      called();
      if (state.hunting == -1)
      {
        long i = state.moveOrder[state.curStep] / 10;
        long j = state.moveOrder[state.curStep] % 10;
        while ((i + j) % 2 == 0 || neigh_guess(state.guessGrid, i, j) || state.guessGrid.grid[i][j] >= 2)
        {
          state.curStep++;
          i = state.moveOrder[state.curStep] / 10;
          j = state.moveOrder[state.curStep] % 10;
        }
        state.guessGrid.grid[i][j] += 2;
        if(state.guessGrid.grid[i][j]==3)
        {
          state.hunting = i * 10 + j;
        }
        state.lastRow = i;
        state.lastCol = j;
        state.curStep++;
      }
      else
      {
        long i = state.hunting / 10;
        long j = state.hunting % 10;
        long ni = i;
        long nj = j;
        bool ask = false;
        long[] di = { -1, 1, 0, 0 };
        long[] dj = { 0, 0, -1, 1 };
        for (int k = 0; k < 4 && !ask; k++)
        {
          ni = i;
          nj = j;
          while (ni >= 0 && ni <= 9 && nj >= 0 && nj <= 9)
          {
            if (state.guessGrid.grid[ni][nj] < 2)
            {
              state.guessGrid.grid[ni][nj] += 2;
              ask = true;
              break;
            }
            else
            {
              if (state.guessGrid.grid[ni][nj] == 2) break;
            }
            ni += di[k];
            nj += dj[k];
          }
        }
        long mii = i;
        long mai = i;
        long mij = j;
        long maj = j;
        while (mii >= 0 && state.guessGrid.grid[mii][j] == 3) mii--;
        while (mai <= 9 && state.guessGrid.grid[mai][j] == 3) mai++;
        while (mij >= 0 && state.guessGrid.grid[i][mij] == 3) mij--;
        while (maj <= 9 && state.guessGrid.grid[i][maj] == 3) maj++;
        if (mai - mii > 2)
        {
          if ((mii == -1 || state.guessGrid.grid[mii][j] == 2) && (mai == 10 || state.guessGrid.grid[mai][j] == 2)) state.hunting = -1;
        }
        if (maj - mij > 2)
        {
          if ((mij == -1 || state.guessGrid.grid[i][mij] == 2) && (maj == 10 || state.guessGrid.grid[i][maj] == 2)) state.hunting = -1;
        }
        state.lastRow = ni;
        state.lastCol = nj;
      }
      return state;
    }



    private bool neigh_guess(BattleshipGrid b, long i, long j)
    {
      long[] di = { -1, 1, 0, 0 };
      long[] dj = { 0, 0, -1, 1 };
      for (int k = 0; k < 4; k++)
      {
        long ni = i + di[k];
        long nj = j + dj[k];
        if (ni < 0 || nj < 0 || ni > 9 || nj > 9) continue;
        if (b.grid[ni][nj] == 3)
        {
          return true;
        }
      }
      return false;
    }

    public async Task<BotState> InitializeState(BattleshipGrid prob,BattleshipGrid playerGrid)
    {
      called();
      BotState bs = new BotState();
      bs.curStep = 0;
      bs.hunting = -1;
      bs.lastRow = -1;
      bs.lastCol = -1;
      bs.guessGrid = new BattleshipGrid();
      bs.moveOrder = new long[100];
      long sum = 0;
      Random rand = new Random();
      long[][] tmpGrid = new long[10][];
      bs.guessGrid.grid = new long[10][];
      for (int i = 0; i < 10; i++)
      {
        tmpGrid[i] = new long[10];
        bs.guessGrid.grid[i] = new long[10];
        for (int j = 0; j < 10; j++)
        {
          tmpGrid[i][j] = prob.grid[i][j];
          bs.guessGrid.grid[i][j] = playerGrid.grid[i][j];
          sum += prob.grid[i][j];
        }
      }
      for (int mv = 0; mv < 100; mv++)
      {
        long cur = rand.NextInt64(sum);
        bool fn = false;
        for (int i = 0; i < 10 && !fn; i++)
        {
          for (int j = 0; j < 10 && !fn; j++)
          {
            if (cur < tmpGrid[i][j])
            {
              bs.moveOrder[mv] = i * 10 + j;
              sum -= tmpGrid[i][j];
              tmpGrid[i][j] = 0;
              fn = true;
            }
            else
            {
              cur -= tmpGrid[i][j];
            }
          }
        }
      }
      return bs;
    }

    private bool verifyPlacement(BattleshipGrid b)
    {
      long[][] grid = new long[10][];
      for (int i = 0; i < 10; i++)
      {
        grid[i] = new long[10];
        for (int j = 0; j < 10; j++)
        {
          grid[i][j] = b.grid[i][j];
        }
      }
      int two = 2;
      int three = 2;
      int four = 1;
      int five = 1;
      bool ok = true;
      int c = 2;
      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 10; j++)
        {
          if (grid[i][j] == 1)
          {
            int mi = i;
            int mj = j;
            while (mi + 1 < 10 && grid[mi + 1][j] == 1)
            {
              mi++;
            }
            while (mj + 1 < 10 && grid[i][mj + 1] == 1)
            {
              mj++;
            }
            if (mi > i && mj > j)
            {
              ok = false;
            }
            else
            {
              if (mi == i && mj == j)
              {
                ok = false;
              }
              else
              {
                int ci = mi - i + 1;
                int cj = mj - j + 1;
                int d = ci;
                if (cj > d)
                {
                  d = cj;
                }
                if (d == 2)
                {
                  two--;
                }
                else
                {
                  if (d == 3)
                  {
                    three--;
                  }
                  else
                  {
                    if (d == 4)
                    {
                      four--;
                    }
                    else
                    {
                      if (d == 5)
                      {
                        five--;
                      }
                      else
                      {
                        ok = false;
                      }
                    }
                  }
                }
                mi = i;
                mj = j;
                grid[i][j] = c;
                while (mi + 1 < 10 && grid[mi + 1][j] == 1)
                {
                  mi++;
                  grid[mi][j] = c;
                }
                while (mj + 1 < 10 && grid[i][mj + 1] == 1)
                {
                  mj++;
                  grid[i][mj] = c;
                }
                c++;
              }
            }
          }
        }
      }
      if (two != 0)
      {
        two = 0;
        ok = false;
      }
      if (three != 0)
      {
        three = 0;
        ok = false;
      }
      if (four != 0)
      {
        four = 0;
        ok = false;
      }
      if (five != 0)
      {
        five = 0;
        ok = false;
      }
      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 9; j++)
        {
          if (grid[i][j] > 0 && grid[i][j + 1] > 0 && grid[i][j] != grid[i][j + 1])
          {
            ok = false;
          }
          if (grid[j][i] > 0 && grid[j + 1][i] > 0 && grid[j][i] != grid[j + 1][i])
          {
            ok = false;
          }
        }
      }
      return ok;
    }

    public async Task<BattleshipGrid> placeShips()
    {
      called();
      BattleshipGrid b = new BattleshipGrid();
      b.grid = new long[10][];
      bool[][] okpo = new bool[10][];
      for (int i = 0; i < 10; i++)
      {
        okpo[i] = new bool[10];
        b.grid[i] = new long[10];
        for (int j = 0; j < 10; j++)
        {
          okpo[i][j] = true;
          b.grid[i][j] = 0;
        }
      }
      Random rand = new Random();
      long[] len = { 2, 2, 3, 3, 4, 5 };
      for (int it = 0; it < len.Length; it++)
      {
        int dir = rand.Next(2);
        int sum = 0;
        for (int i = 0; i < 10; i++)
        {
          for (int j = 0; j + len[it] - 1 < 10; j++)
          {
            int ok = 1;
            for (int c = 0; c < len[it]; c++)
            {
              if (dir == 0 && okpo[i][j + c] == false) ok = 0;
              if (dir == 1 && okpo[j + c][i] == false) ok = 0;
            }
            sum += ok;
          }
        }
        if (sum == 0) return await placeShips();
        int fin = rand.Next(sum);
        bool fn = false;
        for (int i = 0; i < 10 && !fn; i++)
        {
          for (int j = 0; j + len[it] - 1 < 10 && !fn; j++)
          {
            int ok = 1;
            for (int c = 0; c < len[it]; c++)
            {
              if (dir == 0 && okpo[i][j + c] == false) ok = 0;
              if (dir == 1 && okpo[j + c][i] == false) ok = 0;
            }
            if (fin < ok)
            {
              for (int c = -1; c <= len[it]; c++)
              {
                if (j + c < 0 || j + c >= 10) continue;
                if (dir == 0)
                {
                  okpo[i][j + c] = false;
                }
                else
                {
                  okpo[j + c][i] = false;
                }
              }
              for (int k = -1; k <= 1; k++)
              {
                for (int c = 0; c < len[it]; c++)
                {
                  if (i + k < 0 || i + k >= 10) continue;
                  if (dir == 0)
                  {
                    okpo[i + k][j + c] = false;
                    b.grid[i][j + c] = 1;
                  }
                  else
                  {
                    okpo[j + c][i + k] = false;
                    b.grid[j + c][i] = 1;
                  }
                }
              }
              fn = true;
            }
            else
            {
              fin -= ok;
            }
          }
        }
      }
      if (!verifyPlacement(b)) return await placeShips();
      return b;
    }

    /// <summary>
    /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
    /// </summary>
    /// <returns>A collection of listeners.</returns>
    protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
    }

    /// <summary>
    /// This is the main entry point for your service instance.
    /// </summary>
    /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
      await Autoscalling();
      while (true)
      {
        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        this.Partition.ReportLoad(new List<LoadMetric> { new LoadMetric("CallCount", callCount) });
        ServiceEventSource.Current.ServiceMessage(this.Context, $"BotLogicService Load updated to {callCount}");
        lock (lck)
        {
          callCount = 0;
        }
      }
    }
    private async Task Autoscalling()
    {
      FabricClient fabricClient = new FabricClient();

      StatelessServiceUpdateDescription serviceUpdateDescription = new StatelessServiceUpdateDescription();
      var callCountMetric = new StatelessServiceLoadMetricDescription
      {
        Name = "CallCount",
        DefaultLoad = 0,
        Weight = ServiceLoadMetricWeight.High
      };
      serviceUpdateDescription.Metrics = new IMetricsList();
      serviceUpdateDescription.Metrics.Add(callCountMetric);
      AveragePartitionLoadScalingTrigger trigger = new AveragePartitionLoadScalingTrigger();
      PartitionInstanceCountScaleMechanism mechanism = new PartitionInstanceCountScaleMechanism();
      mechanism.MaxInstanceCount = 3;
      mechanism.MinInstanceCount = 1;
      mechanism.ScaleIncrement = 1;
      trigger.MetricName = "CallCount";
      trigger.ScaleInterval = TimeSpan.FromSeconds(60);
      trigger.LowerLoadThreshold = 1.0;
      trigger.UpperLoadThreshold = 10.0;
      ScalingPolicyDescription policy = new ScalingPolicyDescription(mechanism, trigger);
      serviceUpdateDescription.ScalingPolicies ??= new List<ScalingPolicyDescription>();
      serviceUpdateDescription.ScalingPolicies.Add(policy);

      await fabricClient.ServiceManager.UpdateServiceAsync(Context.ServiceName, serviceUpdateDescription);
    }

  }
}
