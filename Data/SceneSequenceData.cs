using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GRPG.GameLogic;

public struct SceneSequenceData
{
    public SceneSequenceData(
        Dictionary<string, System.Action<MissionUI, Sequencer, SceneObjects, float>> sequences
        )
    {
        Sequences = sequences;
    }

    public Dictionary<string, System.Action<MissionUI, Sequencer, SceneObjects, float>> Sequences { get; set; }
}
