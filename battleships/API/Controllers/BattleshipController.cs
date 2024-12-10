using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BotActor.Interfaces;
using Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Newtonsoft.Json;

namespace API.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class BattleshipController : ControllerBase
  {

    [EnableCors("CORSPolicy")]
    [HttpGet]
    [Route("login")]
    public async Task<IActionResult> Login([FromQuery] string username, [FromQuery] string password)
    {
      var statelessProxy = ServiceProxy.Create<IAuthenticationServiceInterface>(
      new Uri("fabric:/battleships/AuthenticationService"));

      var response = await statelessProxy.authenticateUser(username, password);


      if (response != null)
      {
        return Ok( new { response });
      }
      else
      {
        return StatusCode(404);
      }
    }

    [EnableCors("CORSPolicy")]
    [HttpPost]
    [Route("register")]
    public async Task<String> Register([FromBody] User user)
    {

      var statelessProxy = ServiceProxy.Create<IRegistrationServiceInterface>(
      new Uri("fabric:/battleships/RegistrationService"));

      var createdUser = await statelessProxy.registerUser(user);

      if (createdUser != null)
      {
        return "User successfully created!";
      }
      else
      {
        return "User already exists!";
      }
    }

    [Authorize]
    [EnableCors("CORSPolicy")]
    [HttpPost]
    [Route("start")]
    public async Task Start([FromQuery] string username,[FromBody] BattleshipGrid battleshipGrid)
    {
      var actorId = new ActorId(username);
      var proxy = ActorProxy.Create<IBotActor>(actorId,
      new Uri("fabric:/battleships/BotActorService"));

      await proxy.startGame(battleshipGrid, new CancellationToken());
    }

    [Authorize]
    [EnableCors("CORSPolicy")]
    [HttpPost]
    [Route("move")]
    public async Task<long> Move([FromQuery] string username, [FromQuery] long playermove)
    {
      var actorId = new ActorId(username);
      var proxy = ActorProxy.Create<IBotActor>(actorId,
      new Uri("fabric:/battleships/BotActorService"));
      return await proxy.makeMove(playermove, new CancellationToken());
    }

  }
}
