using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class RegionUI : MonoBehaviour
{
	readonly string[] optionsKeys = { "USA East", "Europe", "Asia", "Japan", "South America", "South Korea" };
	readonly string[] optionsValues = { "us", "eu", "asia", "jp", "sa", "kr" };

	private void Awake()
	{
		if (TryGetComponent(out TMP_Dropdown dropdown))
		{
			dropdown.AddOptions(new List<string>(optionsKeys));
			dropdown.onValueChanged.AddListener((index) =>
			{
				Fusion.Photon.Realtime.PhotonAppSettings.Global.AppSettings.FixedRegion = optionsValues[index];
				Debug.Log($"Setting region to {optionsKeys[index]} ({optionsValues[index]})");
				PlayerPrefs.SetString("regionPref", optionsValues[index]);
			});

			string curRegion = Fusion.Photon.Realtime.PhotonAppSettings.Global.AppSettings.FixedRegion;
			Debug.Log($"Initial region is {curRegion}");

			int curIndex = optionsValues.ToList().IndexOf(curRegion);
			dropdown.value = curIndex != -1 ? curIndex : 0;
		}
	}
}