using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core;

public class Store<TState> : AbstractStore<TState, Command.StateHasChanged>
        where TState : class {

    public Store(StateInitializer<TState> initializer) : base(initializer, Reducer) {
    }

    static TState Reducer(TState state, Command.StateHasChanged command) {
        return (TState)command.State;
    }

    public void Mutate(Func<TState, TState> reducer) {
        var state = State;
        ComputedAndApplyState(reducer(state), new Command.StateHasChanged(state));
    }
}
