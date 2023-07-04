using System.Collections;
using System.Collections.Generic;

public interface IAction
{
    public string GetName();
    public void Execute();
}
