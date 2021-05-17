using System.Threading.Tasks;
using System.Collections.Generic;

using BibleBot.Lib;

namespace BibleBot.Backend.Models
{
    public interface ICommandGroup
    {
        string Name { get; set; }
        bool IsOwnerOnly { get; set; }
        List<ICommand> Commands { get; set; }
        ICommand DefaultCommand { get; set; }
    }
}