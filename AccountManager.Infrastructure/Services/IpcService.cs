﻿using AccountManager.Core.Models;
using IPC.NamedPipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Infrastructure.Services
{
    public class IpcService : IIpcService
    {
        public event EventHandler<IpcReceivedEventArgs> IpcReceived = delegate { };
        private readonly Node _node;
        public IpcService()
        {
            _node = new("omni-account-manager", "omni-account-manager", "localhost", OnReceived);
            _node.Start();
        }

        private void OnReceived(PipeMessage recvMessage)
        {
            if (recvMessage.GetPayloadType() == PipeMessageType.PMTString)
            {
                var message = recvMessage.GetPayload().ToString();
                if (message is null)
                    return;

                var splitMessage = message.Split(":");
                if (splitMessage.Length > 1)
                {
                    var method = splitMessage[0];
                    var json = string.Join(":", splitMessage[1..]);

                    IpcReceived.Invoke(this, new() { MethodName = method, Json = json });
                }
            }
        }
    }
}
