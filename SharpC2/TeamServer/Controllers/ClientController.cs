using Serilog;

using SharpC2.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamServer.Controllers
{
    public class ClientController
    {
        private ServerController Server { get; set; }
        public List<string> ConnectedClients { get; private set; } = new List<string>();

        private event EventHandler<ServerEvent> OnClientEvent;

        public ClientController(ServerController server)
        {
            Server = server;
            OnClientEvent += OnClientEventHandler;
        }

        public ClientAuthenticationResult ClientLogin(ClientAuthenticationRequest request)
        {
            var result = new ClientAuthenticationResult();

            if (string.IsNullOrEmpty(request.Nick) || string.IsNullOrEmpty(request.Password))
            {
                result.Result = ClientAuthenticationResult.AuthResult.InvalidRequest;
                Log.Logger.Warning("CLIENT {AuthResult} {Nick}", result.Result.ToString(), request.Nick);
            }
            else if (!AuthenticationController.ValidatePassword(request.Password))
            {
                result.Result = ClientAuthenticationResult.AuthResult.BadPassword;
                Log.Logger.Warning("CLIENT {AuthResult} {Nick}", result.Result.ToString(), request.Nick);
            }
            else if (ConnectedClients.Contains(request.Nick, StringComparer.OrdinalIgnoreCase))
            {
                result.Result = ClientAuthenticationResult.AuthResult.NickInUse;
                Log.Logger.Warning("CLIENT {AuthResult} {Nick}", result.Result.ToString(), request.Nick);
            }
            else
            {
                result.Result = ClientAuthenticationResult.AuthResult.LoginSuccess;
                result.Token = AuthenticationController.GenerateAuthenticationToken(request.Nick);

                Log.Logger.Information("CLIENT {AuthResult} {Nick}", result.Result.ToString(), request.Nick);

                AddNewClient(request.Nick);
            }

            return result;
        }

        private void AddNewClient(string nick)
        {
            ConnectedClients.Add(nick);
            OnClientEvent?.Invoke(this, new ServerEvent(ServerEventType.UserLogon, nick));
        }

        public bool ClientLogoff(string nick)
        {
            var result = ConnectedClients.Remove(nick);
            if (result)
            {
                OnClientEvent?.Invoke(this, new ServerEvent(ServerEventType.UserLogoff, nick));
                Log.Logger.Information("CLIENT {Nick} LoggedOff", nick);
            }
            return result;
        }

        private void OnClientEventHandler(object sender, ServerEvent e)
        {
            Server.ServerEventHandler(this, e);
        }
    }
}