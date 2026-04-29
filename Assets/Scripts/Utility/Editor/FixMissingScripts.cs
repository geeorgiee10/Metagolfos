using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FixMissingScripts : EditorWindow
{
    public GameObject prefabRef;

    List<GameObject> missingGO = new List<GameObject>();
    List<string> outcomeText = new List<string>();

    [MenuItem("Tools/Find Mission Scripts Window")]
    public static void Open()
    {
        GetWindow<FixMissingScripts>();
    }

    private void OnGUI()
    {
        var aGO = Selection.activeGameObject;
        if (aGO != null)
        {
            var c = aGO.GetComponents<Component>();
            foreach (var comp in c)
            {
                if (comp == null)
                {
                    EditorGUILayout.LabelField("Component missing in:  " + aGO.name);
                }
            }
        }

        EditorGUI.BeginChangeCheck();
        prefabRef = (GameObject)EditorGUILayout.ObjectField(prefabRef, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
        {
            missingGO.Clear();
            outcomeText.Clear();
        }
        if (prefabRef == null)
            return;

        if (GUILayout.Button("Find Missing"))
        {
            missingGO.Clear();
            outcomeText.Clear();

            foreach (Transform t in prefabRef.transform)
            {
                FindMissing(t);
            }
        }

        if (missingGO.Count == 0)
        {
            GUILayout.Label("All components accounted for.");
        }
        else
        {
            for (int i = 0; i < missingGO.Count; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(missingGO[i].name))
                {
                    Selection.activeGameObject = missingGO[i];
                }
                GUILayout.Label(outcomeText[i]);
                GUILayout.EndHorizontal();
            }
        }
    }

    void FindMissing(Transform t)
    {
        var components = t.GetComponents<Component>();
        int totalMissing = 0;
        foreach (var component in components)
        {
            if (component == null)
            {
                if (missingGO.Contains(t.gameObject))
                    continue;
                else
                    missingGO.Add(t.gameObject);
                totalMissing++;
            }
        }

        if (totalMissing > 0)
        {
            outcomeText.Add($"Missing {totalMissing} components");
        }

        foreach (Transform child in t)
        {
            FindMissing(child);
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
