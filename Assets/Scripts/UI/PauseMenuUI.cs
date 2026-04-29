using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
	[field: SerializeField] public UIScreen Screen { get; private set; }

	private void OnEnable()
	{
		Cursor.lockState = CursorLockMode.None;
	}

	public void Resume()
	{
		UIScreen.activeScreen.BackOrClose();
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void Leave()
	{
		StartCoroutine(LeaveRoutine());
	}

	IEnumerator LeaveRoutine()
	{
		Task task = GameManager.Instance.Runner.Shutdown();
		while (!task.IsCompleted)
		{
			yield return null;
		}
		
		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
		UIScreen.BackToInitial();
	}
}
