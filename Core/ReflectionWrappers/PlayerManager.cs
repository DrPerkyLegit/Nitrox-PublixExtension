using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Communication;
using System.Reflection;
using Nitrox.Server.Subnautica;

namespace Nitrox_PublixExtension.Core.ReflectionWrappers
{
    public class PlayerManager
    {
        //MethodInfo getPlayerMethod = null;

        public Nitrox.Server.Subnautica.Models.GameLogic.PlayerManager internalPlayerManager;
        public PlayerManager(NitroxServer NitroxRawServer) 
        {
            internalPlayerManager = ((Nitrox.Server.Subnautica.Models.GameLogic.PlayerManager)NitroxRawServer.GetType().GetField("playerManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(NitroxRawServer));
            
        }

        public Player GetPlayerByConnection(INitroxConnection connection)
        {
            return internalPlayerManager.GetPlayer(connection);
        }
    }
}
