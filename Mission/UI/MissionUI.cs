using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using GRPG.GameLogic;

public class MissionUI : MonoBehaviour
{
    //public Team HumanTeam { get; set; }
    //public Team AITeam { get; set; }

    private List<string> _log = new List<string>() { "Loading..." };

    public static UIState playerTurnUIState;

    private UIState _uiState;
    private Mission _mission;
    private SceneObjects _sceneObjects;
    private Actor _activeActor;
    private Team _activeTeam;

    private GameObject ActiveActorCircle;

    public string ActionCursorText = "";
    /// <summary>
    /// Will override ActionCursorText
    /// </summary>
    public string HoverCursorText = "";

    public void Log(string line)
    {
        if (_log.Count == 10)
        {
            _log.RemoveAt(0);
        }
        _log.Add(line);
    }

    public void LogActionResult(ActionResult result)
    {
        Log(result.Actor.Name + " " + result.Action.Name + " on " +
                (result.Target.Location >= 0 ? result.Target.Location : result.Target.Actor.Name) + "\n" +
                (result.IsSuccess ? "Success! " : "Fail.. ") + result.DiceRoll + "/" + result.Difficulty
                );
    }

    public void LogActorDamaged(Actor actor, Damage damage)
    {
        Log(actor.Name + " took " + damage.DamageAmount + " damage!\n" + "HP:" + actor.Resources[Resource.HitPoints]);
    }

    public void Init(Mission mission, SceneObjects sceneObjects) //baaad
    {
        _mission = mission;
        _sceneObjects = sceneObjects;
        if(_mission.CurrentTeam == GRPG.GameLogic.Team.Human)
        {
            //_mission.GetTeamMembers(GRPG.GameLogic.Team.Human).GetEnumerator().MoveNext();
            //SetActiveActor(_mission.GetTeamMembers(GRPG.GameLogic.Team.Human).GetEnumerator().Current);
            SetPlayerTurnUIState();
        }
    }

    /*public UIState GetUIState()
    {
        return _uiState;
    }*/

    public void SetActiveActor(Actor actor)
    {
        if (_activeActor != null) {
            _sceneObjects.GetActorGO(_activeActor).GetComponent<SceneActor>().IsActive = false;
        } else
        {
            ActiveActorCircle = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ActiveActorCircle"));
        }
        _activeActor = actor;
        _sceneObjects.GetActorGO(_activeActor).GetComponent<SceneActor>().IsActive = true;
        ActiveActorCircle.GetComponent<ActiveActorCircleScript>().Target = _sceneObjects.GetActorGO(_activeActor);
    }

    public Actor GetActiveActor()
    {
        return _activeActor;
    }

    public void SetActiveTeam(Team team)
    {
        _activeTeam = team;
    }

    public Team GetActiveTeam()
    {
        return _activeTeam;
    }

    public void HideDialoguePanel()
    {
        _sceneObjects.DialoguePanel.SetActive(false);
    }

    public void ShowDialoguePanel(string text, CharacterPresentation characterPresentation)
    {
        _sceneObjects.DialoguePanel.transform.Find("SpeakerName").GetComponent<TMP_Text>().text = characterPresentation.Name;
        _sceneObjects.DialoguePanel.transform.Find("DialogueText").GetComponent<TMP_Text>().text = text;
        _sceneObjects.DialoguePanel.transform.Find("SpeakerPortrait").GetComponent<Image>()
            .sprite = Resources.Load<Sprite>("Images/Portraits/" + 
            (characterPresentation.Portrait == null ? "question" : characterPresentation.Portrait));

        _sceneObjects.DialoguePanel.SetActive(true);
    }

    /// <summary>
    /// Must be called before making button active.
    /// </summary>
    /// <param name="show"></param>
    public void SetShowCharacterPanel(bool show)
    {
        if (show) // Load visuals
        {
            // HP
            _sceneObjects.CharacterPanel.transform.Find("HealthCounter").GetComponent<TMP_Text>().text = _activeActor.Resources[Resource.HitPoints].ToString();
            // Name
            _sceneObjects.CharacterPanel.transform.Find("CharacterName").GetComponent<TMP_Text>().text = _activeActor.Name.ToUpper();
            // Portrait and background
            Material backgroundMaterial = null;
            if (_sceneObjects.CharacterPresentations[_activeActor.Name].BackgroundMaterial != null)
            {
                backgroundMaterial = Resources
                    .Load<Material>("Images/" + _sceneObjects.CharacterPresentations[_activeActor.Name].BackgroundMaterial);
            }
            _sceneObjects.CharacterPanel.transform.Find("CharacterBackground").GetComponent<Image>()
                .material = backgroundMaterial;
            Sprite portraitSprite;
            if (_sceneObjects.CharacterPresentations[_activeActor.Name].Portrait != null)
            {
                portraitSprite = Resources
                    .Load<Sprite>("Images/Portraits/" + _sceneObjects.CharacterPresentations[_activeActor.Name].Portrait);
            } else
            { // Placeholder portrait
                portraitSprite = Resources.Load<Sprite>("Images/Portraits/question");
            }
            _sceneObjects.CharacterPanel.transform.Find("CharacterPortrait").GetComponent<Image>()
                .sprite = portraitSprite;

            // Action buttons
            List <Action> characterActions = _activeActor.GetAllActions();
            List<Action> availableActions = _activeActor.GetAvailableActions();
            List<string> assignedButtons = new List<string>();
            foreach (Action action in characterActions)
            {

                if (!_sceneObjects.ActionPresentations.ContainsKey(action.Name))
                {
                    Debug.LogError("action not found in presentation dictionary: " + action.Name);
                    continue;
                }
                ActionPresentationData presentation = _sceneObjects.ActionPresentations[action.Name];
                GameObject actionButton = _sceneObjects.CharacterPanel.transform.Find(presentation.iconSlot).gameObject;
                ActionButtonScript buttonScript = actionButton.GetComponent<ActionButtonScript>();

                buttonScript.SetButtonActive(false);
                buttonScript.SetButtonAction(action, presentation);
                buttonScript.SetButtonExists(true, this);
                if (availableActions.Contains(action))
                {
                    int targetCount = _activeActor.GetAvailableTargets(action).Count;
                    if (targetCount > 0)
                    {
                        buttonScript.SetButtonActionAvailable(true, targetCount + " valid target" + (targetCount == 1 ? "" : "s"));
                    } else
                    {
                        buttonScript.SetButtonActionAvailable(false, "No valid targets");
                    }
                } else
                {
                    buttonScript.SetButtonActionAvailable(false, "Action not ready");
                }

                assignedButtons.Add(presentation.iconSlot);
            }
            // Default buttons
            GameObject skipButton = _sceneObjects.CharacterPanel.transform.Find("SkipIcon").gameObject;
            ActionButtonScript skipButtonScript = skipButton.GetComponent<ActionButtonScript>();
            skipButtonScript.SetButtonActive(false);
            skipButtonScript.SetButtonExists(true, this);
            skipButtonScript.SetButtonActionAvailable(true, "");

            // Blank unexisting action buttons
            if (!assignedButtons.Contains(ActionPresentationData.IconSlotMove))
                SetCharacterButtonStateBlank(ActionPresentationData.IconSlotMove);
            if (!assignedButtons.Contains(ActionPresentationData.IconSlotPrimary))
                SetCharacterButtonStateBlank(ActionPresentationData.IconSlotPrimary);
            if (!assignedButtons.Contains(ActionPresentationData.IconSlotSecondary))
                SetCharacterButtonStateBlank(ActionPresentationData.IconSlotSecondary);
            if (!assignedButtons.Contains(ActionPresentationData.IconSlotSpecial))
                SetCharacterButtonStateBlank(ActionPresentationData.IconSlotSpecial);

            // Character selectors
            List<Actor> teamMembers = _activeActor.Mission.GetTeamMembers(_activeTeam).Where(a => a.Name != _activeActor.Name).ToList();
            int memberIndex = 0;
            foreach (Actor actor in teamMembers)
            {
                memberIndex++;
                if (memberIndex > 2) continue; // we only support 3 member teams for now, TODO
                Debug.Log(memberIndex + "~~HP " + actor.Name + actor.Resources[Resource.HitPoints] + "/" + ActorManager.Instance.ActorsStartingHP[actor.Name]);
                CharacterSelectorButton selector = _sceneObjects.CharacterPanel.transform.Find("CharacterSelector" + memberIndex)
                    .GetComponent<CharacterSelectorButton>();
                selector.Set(actor, _sceneObjects.CharacterPresentations[actor.Name], _sceneObjects.CharacterPanel, this);
                /*_sceneObjects.CharacterPanel.transform.
                    Find("CharacterSelector" + memberIndex).GetComponent<Image>().sprite =
                        Resources.Load<Sprite>("Images/Portraits/" + _sceneObjects.CharacterPresentations[actor.Name].Portrait);
                _sceneObjects.CharacterPanel.transform.
                    Find("CharacterSelectorHP" + memberIndex).localScale
                    = new Vector3((float)actor.Resources[Resource.HitPoints] / (float)ActorManager.Instance.ActorsStartingHP[actor.Name], 1, 1);*/
            }

        }

        _sceneObjects.CharacterPanel.SetActive(show);
    }

    public void SetCharacterButtonStateBlank(string name)
    {
        _sceneObjects.CharacterPanel.transform.Find(name).GetComponent<ActionButtonScript>().SetButtonActive(false);
        _sceneObjects.CharacterPanel.transform.Find(name).GetComponent<ActionButtonScript>().SetButtonExists(false, this);
    }

    public void UpdateCharacterAction(Actor actor, Action action)
    {
        // TODO temporary solution
        if (_activeActor == actor && _sceneObjects.CharacterPanel.activeSelf == true)
        {
            SetShowCharacterPanel(true);
        }
        _sceneObjects.GetActorGO(actor).GetComponent<SceneActor>().AddInfoText("+ " + action.Name);
    }

    public void ShowDialogueWindow(CharacterPresentation characterPresentation, string text)
    {
        //Log(characterPresentation.Name.ToUpper() + ": " + text);
        ShowDialoguePanel(text, characterPresentation);
    }

    public void SetNextActionOrContinue()
    {
        Action nextOne = null;
        foreach(Action action in _activeActor.GetAvailableActions())
        {
            if (_activeActor.GetAvailableTargets(action).Count > 0)
                nextOne = action;
        }
        if (nextOne == null)
        {
            Log("No more options for " + _activeActor.Name);
            SetNextActorOrEndTurn();

            return;
        }

        SetPlayerSelectedActionUIState(nextOne);
    }

    // TODO skipTurn state za actore, endturn odvojena opcija
    public void SetNextActorOrEndTurn()
    {
        Actor nextOne = null;
        foreach(Actor actor in _mission.GetTeamMembers(_activeTeam))
        {
            if (actor == _activeActor || 
                actor.GetAvailableActions().Count == 0 || 
                HumanPlayer.Instance.SkipTurnActors.Contains(actor)
                ) continue;

            nextOne = actor;
        }
        if (nextOne == null)
        {
            Log("Ending turn for " + _activeTeam.ToString());
            _mission.EndTurn();

            ComputerPlayer.Instance.RunMoves();

            return;
        }

        SetActiveActor(nextOne);
        SetPlayerSelectedActionUIState(nextOne.GetAvailableActions()[0]);
    }

    public void SetPlayerTurnUIState()
    {
        if (_uiState != null) _uiState.Exit();
        _uiState = new PlayerTurnUIState(this, _sceneObjects);
        _uiState.Enter();
    }

    public void SetPlayerSelectedActionUIState(Action action)
    {
        if (_uiState != null) _uiState.Exit();
        _uiState = new PlayerSelectedActionUIState(this, _sceneObjects, _activeActor, action);
        _uiState.Enter();
    }

    public void SetAnimationSequenceUIState()
    {
        if (_uiState != null) _uiState.Exit();
        _uiState = new AnimationSequenceUIState(this, _sceneObjects);
        _uiState.Enter();
    }

    public void SetSceneSequenceUIState(string sequenceName)
    {
        if (_uiState != null) _uiState.Exit();
        _uiState = new SceneSequenceUIState(this, _sceneObjects, MissionData.Instance.SceneSequences.Sequences[sequenceName], _uiState);
        _uiState.Enter();
    }

    public void SetSceneSequenceUIStateWithReturnedControl(string sequenceName)
    {
        if (_uiState != null) _uiState.Exit();
        _uiState = new SceneSequenceUIState(this, _sceneObjects, MissionData.Instance.SceneSequences.Sequences[sequenceName], null);
        _uiState.Enter();
    }

    public void UnsetSceneSequenceUIState(UIState previousState)
    {
        if (_uiState != null) _uiState.Exit();
        _uiState = previousState;
        _uiState.Enter();
    }

    public void SetAITurnUIState()
    {
        if (_uiState != null) _uiState.Exit();
        _uiState = new AITurnUIState(this, _sceneObjects);
        _uiState.Enter();
    }

    public void Continue()
    {
        _uiState.ContinueButtonClicked();
    }

    public void Skip()
    {
        _uiState.SkipButtonClicked();
    }

    private void Update()
    {
        if (_uiState == null) return;

        _uiState.Update();

        if (Input.GetMouseButtonUp(0))
        {
            _uiState.LeftButtonClicked();
        } else if (Input.GetMouseButtonUp(1))
        {
            _uiState.RightButtonClicked();
        } else if (Input.GetKeyUp(KeyCode.Space))
        {
            //_uiState.SkipButtonClicked();
            Continue();
        }/* else if (Input.GetKeyUp(KeyCode.Escape))
        {
            Skip();
        }*/
    }

    private void OnGUI()
    {
        //DrawLog();

        GUI.Label(new Rect(0, 0, 200, 20), _uiState.GetType().Name);

        string cursorText = ActionCursorText;
        if (HoverCursorText != "")
            cursorText = HoverCursorText;

        if (cursorText != "")
        {
            int labelWidth = 250;
            float xPos = Input.mousePosition.x + 20;
            if ((Screen.width - xPos) > Screen.width - labelWidth)
                xPos -= Screen.width - labelWidth;
            GUI.Label(new Rect(xPos, Screen.height - (Input.mousePosition.y-20), labelWidth, 20), cursorText);
        }

        /*List<SceneLocation> locations = _sceneObjects.GetAllLocations();
        foreach(SceneLocation loc in locations)
        {
            Vector3 locScreenPos = Camera.main.WorldToScreenPoint(loc.transform.position);
            GUI.Label(new Rect(locScreenPos.x, Screen.height - locScreenPos.y, 100, 50), _sceneObjects.GetAmountOfActorsOnLocation(loc.Index).ToString());
        }*/
        //DrawTeams();
    }

    //
    // Drawing functions
    //

    private void DrawLog()
    {
        string log = string.Join("\n", _log);
        GUI.Label(new Rect(10, 10, 400, 600), log);
    }

    /*private Rect humanTeamRect = new Rect(450, 10, 600, 300);
    private Rect aiTeamRect = new Rect(450, 350, 600, 300);
    private static int humanTeamWindowID = 0;
    private static int aiTeamWindowID = 1;

    private void DrawTeams()
    {
        if (_activeMissionState == null) return;
        if (HumanTeam == null || AITeam == null) return;

        humanTeamRect = GUI.Window(humanTeamWindowID, humanTeamRect, TeamWindowFunction, HumanTeam.name);
        aiTeamRect = GUI.Window(aiTeamWindowID, aiTeamRect, TeamWindowFunction, AITeam.name);
    }

    void TeamWindowFunction(int windowID)
    {
        Team team;
        bool isHumanPlayer = false;
        if (windowID == humanTeamWindowID)
        {
            team = HumanTeam;
            isHumanPlayer = true;
        } else
        {
            team = AITeam;
        }

        int actorIndex = 0;
        int actorOffsetY = 50;
        int actionButtonOffsetX = 175;
        foreach (Actor actor in team.members)
        {
            // Header
            string actorHeader = actor.name + "  MP:" + actor.MovementPoints + "/" + actor.baseStats.MP;
            GUI.Label(new Rect(10, 20 + actorIndex*actorOffsetY, 200, 50), actorHeader);

            // Action buttons
            if (isHumanPlayer && _activeMissionState.GetName() == "human_turn")
            {
                List<string> actions = new List<string>() { "Simple Action (5)", "Advanced Action (10)", "Skip turn" };
                int actionIndex = 0;
                foreach (string action in actions)
                {
                    GUI.Button(new Rect(10 + actionIndex * actionButtonOffsetX, 45 + actorIndex * actorOffsetY, 175, 20), action);
                    actionIndex++;
                }
            }

            actorIndex++;
        }
    }

    //
    // Event handlers
    //
    
    public void HandleMissionStateExit(IMissionState exitedState)
    {
        Debug.Log("BING!");
    }

    public void HandleMissionStateEnter(IMissionState state)
    {
        _activeMissionState = state;
    }*/

}
