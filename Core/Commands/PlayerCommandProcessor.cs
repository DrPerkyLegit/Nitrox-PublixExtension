

using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Logger;
using Nitrox.Server.Subnautica;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;

namespace Nitrox_PublixExtension.Core.Commands
{
    public class PlayerCommandProcessor
    {
        private readonly Dictionary<string, Command> commands = new Dictionary<string, Command>();

        private readonly char[] splitChar = new char[1] { ' ' };

        public PlayerCommandProcessor(IEnumerable<Command> cmds)
        {
            foreach (Command cmd in cmds)
            {
                if (commands.ContainsKey(cmd.Name))
                {
                    Log.Info("Command " + cmd.Name + " is registered multiple times.");
                    continue;
                }

                commands[cmd.Name] = cmd;
                foreach (string alias in cmd.Aliases)
                {
                    if (commands.ContainsKey(alias))
                    {
                        Log.Info("Command " + alias + " is registered multiple times.");
                        continue;
                    }

                    commands[alias] = cmd;
                }
            }
        }

        public bool ProcessCommand(string msg, Optional<Player> sender, Perms permissions)
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                Span<string> span = msg.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                if (!commands.TryGetValue(span[0], out Command value))
                {
                    //Command.SendMessage(sender, "Command not found: " + span[0]);
                    return false;
                }
                else if (!sender.HasValue && value.Flags.HasFlag(PermsFlag.NO_CONSOLE))
                {
                    Log.Error("This command cannot be used by CONSOLE");
                }
                else if (value.CanExecute(permissions))
                {
                    value.TryExecute(sender, span.Slice(1, span.Length - 1));
                    return true;
                }
                else
                {
                    Command.SendMessage(sender, "You do not have the required permissions for this command !");
                }
            }

            return false;
        }
    }
}
