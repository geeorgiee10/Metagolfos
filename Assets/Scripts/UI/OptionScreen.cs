using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.PostProcessing;

public class OptionScreen : MonoBehaviour
{

	public TMP_Dropdown graphicsDropdown;
	public Toggle postprocessingToggle, cameraShakeToggle;
	public Slider masterVol;
	public Slider sfxVol;
	public Slider uiVol;

	public GameObject audioPanel, graphicsPanel, controlsPanel;

    private void Awake()
    {
		InitGraphicsDropdown();
    }

	private void OnEnable()
	{
		masterVol.SetValueWithoutNotify(AudioManager.GetFloatNormalized(AudioManager.mainVolumeParam));
		sfxVol.SetValueWithoutNotify(AudioManager.GetFloatNormalized(AudioManager.sfxVolumeParam));
		uiVol.SetValueWithoutNotify(AudioManager.GetFloatNormalized(AudioManager.uiVolumeParam));	
	}

	public void InitGraphicsDropdown()
	{
		string[] names = QualitySettings.names;
		List<string> options = new List<string>();

		for (int i = 0; i < names.Length; i++)
		{
			options.Add(names[i]);
		}
		graphicsDropdown.AddOptions(options);
		QualitySettings.SetQualityLevel(graphicsDropdown.options.Count - 1);
		graphicsDropdown.value = graphicsDropdown.options.Count - 1;
	}

	public void SetGraphicsQuality()
	{
		QualitySettings.SetQualityLevel(graphicsDropdown.value);
	}

	public void TogglePostProcessing()
	{
		if (Camera.main.TryGetComponent(out PostProcessLayer ppl))
		{
			ppl.enabled = postprocessingToggle.isOn;
		}
	}

	public void ToggleCameraShake()
    {
		if (CameraController.Instance)
		{
			if (CameraController.Instance.Shake != null)
			{
				CameraController.Instance.Shake.enabled = cameraShakeToggle.isOn;
			}
		}
	}


	public void SetVolumeMaster(float value)
	{
		AudioManager.SetVolumeMaster(value);
	}

	public void SetVolumeSFX(float value)
	{
		AudioManager.SetVolumeSFX(value);
	}

	public void SetVolumeUI(float value)
	{
		AudioManager.SetVolumeUI(value);
	}

	public void SetActivePanel(GameObject panel)
    {
        //close all panels
        audioPanel.SetActive(false);
        graphicsPanel.SetActive(false);
		controlsPanel.SetActive(false);

        //open selected
        panel.SetActive(true);
    }
}
