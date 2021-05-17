using System.Threading.Tasks;
using System.Collections.Generic;

using BibleBot.Lib;

namespace BibleBot.Backend.Models
{
    public class CommandGroup
    {
        private readonly string Name;
        private readonly bool IsOwnerOnly;
        private readonly ICommand DefaultCommand;
        private readonly List<ICommand> Commands;
    }
}