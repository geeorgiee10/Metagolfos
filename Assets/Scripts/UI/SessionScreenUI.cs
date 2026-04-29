using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System.Threading.Tasks;
using System.Linq;
using Helpers.Linq;

public class SessionScreenUI : MonoBehaviour
{
	public UIScreen screen;
	public Transform playerItemHolder;
	public Transform spectatorItemHolder;
	public GameObject spectatorHeader;
	public GameObject spectatorPanel;
	public TMP_Text sessionNameBanner;
	public TMP_Text sessionNameText;
	public TMP_Dropdown courseLengthSetting;
	public TMP_Dropdown holeTimeSetting;
	public TMP_Dropdown maxShotsSetting;
	public Toggle collisionCheck;
	public Toggle privateCheck;

	public Button startGameButton;

	//RGB Sliders
	public Slider slider_R, slider_G, slider_B;
	public Renderer golfBallRend;

	readonly Dictionary<PlayerRef, PlayerSessionItemUI> playerItems = new Dictionary<PlayerRef, PlayerSessionItemUI>();

	bool isUpdatingSession = false;

	// UI hook
	public void AddSubscriptions()
	{
		PlayerRegistry.OnPlayerJoined += PlayerJoined;
		PlayerRegistry.OnPlayerLeft += PlayerLeft;
	}

	private void OnEnable()
	{
		PlayerRegistry.OnPlayerJoined -= PlayerJoined;
		PlayerRegistry.OnPlayerLeft -= PlayerLeft;
		PlayerRegistry.OnPlayerJoined += PlayerJoined;
		PlayerRegistry.OnPlayerLeft += PlayerLeft;

		
		UpdateSessionConfig();
		StartCoroutine(SetSlidersDefault());
	}

	private void OnDisable()
	{
		playerItems.Clear();

		PlayerRegistry.OnPlayerJoined -= PlayerJoined;
		PlayerRegistry.OnPlayerLeft -= PlayerLeft;
	}

	private void Update()
	{
		if (GameManager.Instance?.Runner?.SessionInfo == true)
		{
			if (privateCheck.isOn != !GameManager.Instance.Runner.SessionInfo.IsVisible)
				UpdateSessionConfig();
		}
	}

	public void UpdateSessionConfig()
	{
		if (!isUpdatingSession && gameObject.activeInHierarchy)
		{
			isUpdatingSession = true;
			StartCoroutine(UpdateSessionConfigRoutine());
		}
	}

	IEnumerator UpdateSessionConfigRoutine()
	{
		if (!(GameManager.Instance?.Runner?.SessionInfo == true))
		{
			privateCheck.interactable = collisionCheck.interactable =
				maxShotsSetting.interactable = holeTimeSetting.interactable =
				courseLengthSetting.interactable = false;
			
			yield return new WaitUntil(() => GameManager.Instance?.Runner?.SessionInfo == true);
		}

		if (GameManager.Instance.Runner.IsServer)
			PlayerRegistry.ForEach(p =>
			{
				if (!playerItems.ContainsKey(p.Ref))
					CreatePlayerItem(p.Ref);
			}, true);

		sessionNameBanner.text = sessionNameText.text = GameManager.Instance.Runner.SessionInfo.Name;

		courseLengthSetting.value = GameManager.Instance.CourseLengthIndex;
		holeTimeSetting.value = GameManager.Instance.MaxTimeIndex;
		maxShotsSetting.value = GameManager.Instance.MaxStrokesIndex;
		privateCheck.isOn = !GameManager.Instance.Runner.SessionInfo.IsVisible;
		collisionCheck.isOn = GameManager.Instance.DoCollisions;

		startGameButton.gameObject.SetActive(GameManager.Instance.Runner.IsServer);

		privateCheck.interactable = collisionCheck.interactable =
			maxShotsSetting.interactable = holeTimeSetting.interactable =
			courseLengthSetting.interactable = GameManager.Instance.Runner.IsServer;

		isUpdatingSession = false;
	}

	IEnumerator SetSlidersDefault()
	{
		yield return new WaitUntil(() => PlayerObject.Local && PlayerObject.Local.Color != default);
		PlayerObject p = PlayerObject.Local;
		SetRGB(p.Color.r, p.Color.g, p.Color.b);
	}



	public void PlayerJoined(NetworkRunner runner, PlayerRef player)
	{
		CreatePlayerItem(player);
	}

	private void CreatePlayerItem(PlayerRef pRef)
	{
		if (!playerItems.ContainsKey(pRef))
		{
			if (GameManager.Instance.Runner.CanSpawn)
			{
				PlayerSessionItemUI item = GameManager.Instance.Runner.Spawn(
					prefab: ResourcesManager.Instance.playerSessionItemUI,
					inputAuthority: pRef);
				playerItems.Add(pRef, item);
			}
		}
		else
		{
			Debug.LogWarning($"{pRef} already in dictionary");
		}
	}

	public void PlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		if (playerItems.TryGetValue(player, out PlayerSessionItemUI item))
		{
			if (item)
			{
				Debug.Log($"Removing {nameof(PlayerSessionItemUI)} for {player}");
				runner.Despawn(item.Object);
			}
			else
			{
				Debug.Log($"{nameof(PlayerSessionItemUI)} for {player} was null.");
			}
			playerItems.Remove(player);
		}
		else
		{
			Debug.LogWarning($"{player} not found");
		}
	}

	public void SetRGB32(byte r, byte g, byte b)
	{
		slider_R.value = r;
		slider_G.value = g;
		slider_B.value = b;
		EditRGB();
	}

	public void SetRGB(float r, float g, float b)
	{
		slider_R.value = r * 255;
		slider_G.value = g * 255;
		slider_B.value = b * 255;
		EditRGB();
	}

	public void ClearRGB()
	{
		golfBallRend.material.color = Color.white;
	}

	public void EditRGB()
	{
		golfBallRend.material.color = new Color32((byte)slider_R.value, (byte)slider_G.value, (byte)slider_B.value, 255);
	}

	public void ApplyColorChange()
	{
		PlayerObject.Local.Rpc_SetColor(new Color32((byte)slider_R.value, (byte)slider_G.value, (byte)slider_B.value, 255));
	}

	public void StartGame()
	{
		if (PlayerRegistry.CountPlayers > 0)
		{
			GameManager.State.Server_SetState(GameState.EGameState.Loading);
		}
	}

	public void EditSession()
	{
		UIScreen.Focus(InterfaceManager.Instance.sessionSetupScreen);
	}

	public void ToggleSpectate()
	{
		PlayerObject.Local.Rpc_ToggleSpectate();
	}

	public void Leave()
	{
		StartCoroutine(LeaveRoutine());
	}

	IEnumerator LeaveRoutine()
	{
		Task task = Matchmaker.Instance.Runner.Shutdown();
		while (!task.IsCompleted)
		{
			yield return null;
		}
		UIScreen.BackToInitial();
	}
}
