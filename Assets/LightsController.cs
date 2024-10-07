using System.Linq;
using Assets.Scripts.Pacing;
using Events;
using UnityEngine;
using static Assets.Scripts.Props.CeilingLight;
using static KMSoundOverride;

public class LightsController : MonoBehaviour
{
    [SerializeField]
    private KMAudio _audio;

    [SerializeField]
    private KMSelectable _switch;

    [SerializeField]
    private Animator _switchAnimator;

    [SerializeField]
    private KMSelectable _onPunch;

    [SerializeField]
    private KMSelectable _offPunch;

    [SerializeField]
    private GameObject _pacingWarning;

    private bool _hasStarted;

    private bool _isOn;

    public void Start()
    {
        _pacingWarning.SetActive(false);

        _switch.OnInteract += () =>
        {
            OnSwitchPressed();
            return false;
        };

        GameplayState.OnLightsOnEvent += OnGameplayLightsOn;
        EnvironmentEvents.OnLightsOn += OnLightsOn;
        EnvironmentEvents.OnLightsOff += OnLightsOff;
    }

    private void OnDestroy()
    {
        GameplayState.OnLightsOnEvent -= OnGameplayLightsOn;
        EnvironmentEvents.OnLightsOn -= OnLightsOn;
        EnvironmentEvents.OnLightsOff -= OnLightsOff;
    }

    private void OnGameplayLightsOn()
    {
        Debug.Log("[Light Switch] Round started");
        _hasStarted = true;
    }

    private void OnLightsOn(bool _)
    {
        Debug.Log("[Light Switch] Lights have been turned on");
        _isOn = true;
        UpdateSwitch();
    }

    private void OnLightsOff(bool _)
    {
        Debug.Log("[Light Switch] Lights have been turned off");
        _isOn = false;
        UpdateSwitch();
    }

    private void OnSwitchPressed()
    {
        if (!_hasStarted)
            return;

        var gameplay = SceneManager.Instance.GameplayState;

        var paceMaker = gameplay.paceMaker;

        var room = gameplay.Room;
        var light = room.CeilingLight;

        if (room is FacilityRoom)
        {
            // cancel pacing event
            // warning: find better solution?
            // because if modded gameplay rooms use coroutines then those can get stopped too
            room.StopAllCoroutines();
        }

        if (_isOn)
        {
            Debug.Log("[Light Switch] Turning lights off");
            _offPunch.AddInteractionPunch();
            _audio.PlayGameSoundAtTransform(SoundEffect.Switch, _switch.transform);
            _audio.PlayGameSoundAtTransform(SoundEffect.LightBuzzShort, light.transform);
            light.TurnOff(false);
            var pacingAction = paceMaker.actions.FirstOrDefault(action => action.EventType == PaceEvent.Idle_DoingWell);
            if (pacingAction != null)
                paceMaker.actions.Remove(pacingAction);
            _pacingWarning.SetActive(true);
        }
        else
        {
            Debug.Log("[Light Switch] Turning lights on");
            _onPunch.AddInteractionPunch();
            _audio.PlayGameSoundAtTransform(SoundEffect.Switch, _switch.transform);
            _audio.PlayGameSoundAtTransform(SoundEffect.LightBuzzShort, light.transform);
            light.TurnOn(true);
        }
    }

    private void UpdateSwitch() => _switchAnimator.SetBool("IsOn", _isOn);
}
