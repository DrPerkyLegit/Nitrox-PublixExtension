using NitroxModel.Logger;
using NitroxServer;
using NitroxServer.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nitrox_PublixExtension.Core.ReflectionWrappers
{
    public class PlayerManager
    {
        //MethodInfo getPlayerMethod = null;

        NitroxServer.GameLogic.PlayerManager internalPlayerManager;
        public PlayerManager(NitroxServer.Communication.NitroxServer NitroxRawServer) 
        {
            internalPlayerManager = ((NitroxServer.GameLogic.PlayerManager)NitroxRawServer.GetType().GetField("playerManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(NitroxRawServer));
            
        }

        public Player GetPlayerByConnection(INitroxConnection connection)
        {
            return internalPlayerManager.GetPlayer(connection);
        }
    }
}
