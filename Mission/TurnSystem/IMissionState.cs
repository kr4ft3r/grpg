#nullable enable

using System.Collections;
using System.Collections.Generic;

public interface IMissionState
{
    public string GetName();
    /// <summary>
    /// Runs when state is switched to
    /// </summary>
    public void Enter();
    /// <summary>
    /// Runs when state is switched from
    /// </summary>
    public void Exit();
    /// <summary>
    /// Send input command to state.
    /// </summary>
    /// <returns>IMissionState if it has changed, or null if it remains the same</returns>
    public IMissionState? HandleCommand(IAction action);
}
