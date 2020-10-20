using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamServer.Controllers
{
    public class ClientController
    {
        private ServerController Server { get; set; }
        public List<string> ConnectedClients { get; private set; } = new List<string>();

        private event EventHandler<ServerEvent> ClientEvent;

        public ClientController(ServerController server)
        {
            Server = server;
            ClientEvent += ClientEventHandler;
        }

        public ClientAuthResponse ClientLogin(ClientAuthRequest request)
        {
            var result = new ClientAuthResponse();

            if (string.IsNullOrEmpty(request.Nick) || string.IsNullOrEmpty(request.Password))
            {
                result.Result = ClientAuthResult.InvalidRequest;
                ClientEvent?.Invoke(this, new ServerEvent(ServerEventType.FailedAuth, result.Result.ToString(), request.Nick));
            }
            else if (!AuthenticationController.ValidatePassword(request.Password))
            {
                result.Result = ClientAuthResult.BadPassword;
                ClientEvent?.Invoke(this, new ServerEvent(ServerEventType.FailedAuth, result.Result.ToString(), request.Nick));
            }
            else if (ConnectedClients.Contains(request.Nick, StringComparer.OrdinalIgnoreCase))
            {
                result.Result = ClientAuthResult.NickInUse;
                ClientEvent?.Invoke(this, new ServerEvent(ServerEventType.FailedAuth, result.Result.ToString(), request.Nick));
            }
            else
            {
                result.Result = ClientAuthResult.LoginSuccess;
                result.Token = AuthenticationController.GenerateAuthenticationToken(request.Nick);

                ClientEvent?.Invoke(this, new ServerEvent(ServerEventType.UserLogon, result.Result.ToString(), request.Nick));

                AddNewClient(request.Nick);
            }

            return result;
        }

        private void AddNewClient(string nick)
        {
            ConnectedClients.Add(nick);
        }

        public bool ClientLogoff(string nick)
        {
            var result = ConnectedClients.Remove(nick);

            if (result)
            {
                ClientEvent?.Invoke(this, new ServerEvent(ServerEventType.UserLogoff, result.ToString(), nick));
            }

            return result;
        }

        private void ClientEventHandler(object sender, ServerEvent e)
        {
            Server.ServerEventHandler(this, e);
        }
    }
}