using System;
using System.Collections.Generic;
using System.Fabric;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace AuthenticationService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class AuthenticationService : StatelessService, IAuthenticationServiceInterface
    {
        public AuthenticationService(StatelessServiceContext context)
            : base(context)
        { }

    public async Task<AuthResponse> authenticateUser(string username, string password)
    {
      var partitionId = (username.GetHashCode() % 3 + 3) % 3;

      var statefulProxy = ServiceProxy.Create<IUserDataInterface>(
      new Uri("fabric:/battleships/UserData"), new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

      var user = await statefulProxy.getUser(username);

      if (user != null)
      {
        HashAlgorithm sha = SHA512.Create();

        var pepper = File.ReadAllText(Path.Join(Environment.GetEnvironmentVariable("C:\\Users\\aleksam\\Documents\\battleships\\battleships\\battleships\\secrets"), "AuthenticationPepper"));

        if (VerifyHash(sha,password + pepper, user.password))
        {
          AuthResponse authResponse = new AuthResponse();
          authResponse.user = new User();
          authResponse.user.username = user.username;
          authResponse.user.firstName = user.firstName;
          authResponse.user.lastName = user.lastName;
          authResponse.token = GenerateJwtToken();

          return authResponse;
        }
        return null;
      }

      return null;
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

    private static bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash)
    {
      var hashOfInput = GetHash(hashAlgorithm, input);

      StringComparer comparer = StringComparer.OrdinalIgnoreCase;

      return comparer.Compare(hashOfInput, hash) == 0;
    }

    private string GenerateJwtToken()
    {


      var configPackage = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
      var jwtSection = configPackage.Settings.Sections["AuthenticationJwtSettings"];
      var secretKey = File.ReadAllText(Path.Join(Environment.GetEnvironmentVariable("C:\\Users\\aleksam\\Documents\\battleships\\battleships\\battleships\\secrets"), "AuthenticationSecretKey"));
      var issuer = jwtSection.Parameters["Issuer"].Value;
      var audience = jwtSection.Parameters["Audience"].Value;

      var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);



      var token = new JwtSecurityToken(
          issuer: issuer,
          audience: audience,
          claims: new List<Claim>(),
          expires: DateTime.Now.AddMinutes(30),
          signingCredentials: creds);

      return new JwtSecurityTokenHandler().WriteToken(token);
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
