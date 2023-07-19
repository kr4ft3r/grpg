#nullable enable
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Unused
/// </summary>
public class Sequence
{
    public static Sequence? Current { get; set; }

    public int Index { get; private set; }
    public bool Done { get; private set; }
    private List<SequenceEvent> _events;
    
    public Sequence(List<SequenceEvent> events, int index = 0)
    {
        Index = index;
        Done = false;
        _events = events;
    }

    public string? Next()
    {
        SequenceEvent evt = _events[Index];
        Index++;
        if (Index >= _events.Count)
        {
            Done = true;
            Index = 0;
        }

        return evt.text;
    }
}

public struct SequenceEvent
{
    public SequenceEvent(SequenceEventType type, string text)
    {
        this.type = type;
        this.text = text;
    }
    public SequenceEventType type;
    public string text;
}

public enum SequenceEventType
{
    Text
}