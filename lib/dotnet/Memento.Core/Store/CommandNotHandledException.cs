using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core;

public class CommandNotHandledException : Exception {
    public Command Command { get; }

    public CommandNotHandledException(Command command)
        : base($"The command is not handled. {command.GetType().Name}") {
        Command = command;
    }
}