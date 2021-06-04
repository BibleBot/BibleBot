using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using BibleBot.Lib;
using BibleBot.Backend.Services;

namespace BibleBot.Backend.Models
{
    public interface ICommand
    {
        string Name { get; set; }
        string ArgumentsError { get; set; }
        int ExpectedArguments { get; set; }
        List<Permissions> PermissionsRequired { get; set; }
        IResponse ProcessCommand(Request req, List<string> args);
    }
}