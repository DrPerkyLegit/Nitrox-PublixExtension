using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.ConsoleCommands.Abstract;

namespace Nitrox_PublixExtension.Core.Commands
{
    //credits to nitrox for this command, its just pasted from source with 1 modification
    internal class HelpCommandOverwrite : Command
    {
        public override IEnumerable<string> Aliases { get; } = new[] { "?" };

        public HelpCommandOverwrite() : base("help", Perms.PLAYER, "Displays this")
        {
            AddParameter(new TypeString("command", false, "Command to see help information for"));
        }

        protected override void Execute(CallArgs args)
        {
            List<string> cmdsText;
            if (args.IsConsole)
            {
                cmdsText = GetHelpText(Perms.CONSOLE, false, args.IsValid(0) ? args.Get<string>(0) : null);
                foreach (string cmdText in cmdsText)
                {
                    Log.Info(cmdText);
                }
            }
            else
            {
                cmdsText = GetHelpText(args.Sender.Value.Permissions, true, args.IsValid(0) ? args.Get<string>(0) : null);

                foreach (string cmdText in cmdsText)
                {
                    SendMessageToPlayer(args.Sender, cmdText);
                }
            }
        }

        private List<string> GetHelpText(Perms permThreshold, bool cropText, string singleCommand)
        {
            static bool CanExecuteAndProcess(Command cmd, Perms perms)
            {
                return cmd.CanExecute(perms) && !(perms == Perms.CONSOLE && cmd.Flags.HasFlag(PermsFlag.NO_CONSOLE));
            }

            //Runtime query to avoid circular dependencies
            IEnumerable<Command> allCommands = NitroxServiceLocator.LocateService<IEnumerable<Command>>().Concat(Publix.getPluginManager().GetAllCommands());

            if (singleCommand != null && !allCommands.Any(cmd => cmd.Name.Equals(singleCommand)))
            {
                return new List<string> { "Command does not exist" };
            }
            List<string> cmdsText = new();
            cmdsText.Add(singleCommand != null ? $"=== Showing help for {singleCommand} ===" : "=== Showing command list ===");
            cmdsText.AddRange(allCommands.Where(cmd => CanExecuteAndProcess(cmd, permThreshold) && (singleCommand == null || cmd.Name.Equals(singleCommand)))
                                             .OrderByDescending(cmd => cmd.Name)
                                             .Select(cmd => cmd.ToHelpText(singleCommand != null, cropText)));
            return cmdsText;
        }
    }
}
