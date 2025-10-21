using NitroxServer;
using NitroxServer.Communication;
using NitroxServer.GameLogic;
using System.Reflection;

namespace Nitrox_PublixExtension.Core.ReflectionWrappers
{
    public class PlayerManager
    {
        //MethodInfo getPlayerMethod = null;

        public NitroxServer.GameLogic.PlayerManager internalPlayerManager;
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
