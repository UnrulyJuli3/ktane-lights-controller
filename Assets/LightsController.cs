using System.Linq;
using Assets.Scripts.Pacing;
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

	private bool _isPacingDisabled = false;

	private bool _isOn = false;

	public void Start()
	{
		_pacingWarning.SetActive(false);

		_switch.OnInteract += () =>
		{
			OnSwitchPressed();
			return false;
		};

		GameplayState.OnLightsOnEvent += () => _hasStarted = true;
	}

	private void Update()
	{
		bool on = SceneManager.Instance.GameplayState?.Room?.CeilingLight?.currentState == LightState.On;
		if (on == _isOn)
			return;

		_isOn = on;
		_switchAnimator.SetBool("IsOn", on);
	}
	
	private void OnSwitchPressed()
	{
        if (!_hasStarted)
            return;

		var gameplay = SceneManager.Instance.GameplayState;

		var paceMaker = gameplay.paceMaker;

		var room = gameplay.Room;
        var light = room.CeilingLight;

		if (_isPacingDisabled && room is FacilityRoom)
		{
			// cancel pacing event
			// warning: find better solution?
			// if modded gameplay rooms use coroutines then those can get stopped too
			room.StopAllCoroutines();
		}

		switch (light.currentState)
		{
			case LightState.On:
				_offPunch.AddInteractionPunch();
				_audio.PlayGameSoundAtTransform(SoundEffect.Switch, _switch.transform);
                _audio.PlayGameSoundAtTransform(SoundEffect.LightBuzzShort, light.transform);
                light.TurnOff(false);
				var pacingAction = paceMaker.actions.FirstOrDefault(action => action.EventType == PaceEvent.Idle_DoingWell);
                if (pacingAction != null)
					paceMaker.actions.Remove(pacingAction);
					_pacingWarning.SetActive(true);
					_isPacingDisabled = true;
                break;
            case LightState.Off:
			case LightState.Buzz:
				_onPunch.AddInteractionPunch();
                _audio.PlayGameSoundAtTransform(SoundEffect.Switch, _switch.transform);
                _audio.PlayGameSoundAtTransform(SoundEffect.LightBuzzShort, light.transform);
                light.TurnOn(true);
                break;
        }
    }
}
