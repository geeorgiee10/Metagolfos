using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class InterfaceManager : MonoBehaviour
{
	public OptionScreen optionScreen;
	public SessionSetup sessionSetup;
	public SessionScreenUI sessionScreen;
	public ResultsScreenUI resultsScreen;
	public PerformanceUI performance;
	public PauseMenuUI pauseMenu;
	public PostGameUI postgameUI;

	public Canvas worldCanvas;

	public UIScreen mainScreen;
	public UIScreen sessionSetupScreen;
	public UIScreen scoreboard;
	public UIScreen hud;

	public Animator raceCountdownAnimator;

	public static InterfaceManager Instance { get; private set; }
	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
		sessionSetup.Init();
	}

	private void Update()
	{
		if (GameManager.Instance?.Object?.IsValid == true && GameManager.State.Current == GameState.EGameState.Game)
		{
			if (Input.GetKey(KeyCode.Tab))
			{
				// show scoreboard
				if (UIScreen.activeScreen != scoreboard)
					UIScreen.Focus(scoreboard);
			}

			if (Input.GetKeyUp(KeyCode.Tab))
			{
				// hide scoreboard
				UIScreen.activeScreen.Back();
			}

			if (Input.GetKeyDown(KeyCode.P))
			{
				if (UIScreen.activeScreen == scoreboard || UIScreen.activeScreen == hud)
				{
					if (UIScreen.activeScreen == scoreboard)
					{
						UIScreen.activeScreen.Back();
					}
					UIScreen.Focus(pauseMenu.Screen);
				}
			}
		}
	}

	public void StartCountdown()
    {
		raceCountdownAnimator.SetTrigger("StartCountdown");
    }
}
