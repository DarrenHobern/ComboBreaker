using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RecordComboScript : Attackable {

    public static int comboCount = 0;
    public ArrayList comboList;

    private void Awake()
    {
        if (!Application.isEditor)
            Destroy(gameObject);
    }

    private void Start()
    {
        combo = ScriptableObject.CreateInstance<Combo>();
        combo.c = new Buttons[0];
        comboList = new ArrayList();
    }

    public override void RestoreCombo()
    {
        Debug.Log("This doesn't do anything...");
        return;
    }

    public override HitType Hit(Buttons buttons)
    {
        comboList.Add(buttons);
        print(buttons.ToString());
        return HitType.Hit;
    }
    public override HitType Hit(Buttons buttons, Vector3 source, float knockForce)
    {
        return Hit(buttons);
    }

    public override int GetComboIndex()
    {
        return combo.c.Length;
    }

    private void Update()
    {
#if UNITY_EDITOR
        // Apply the array list
        if (Input.GetKeyDown(KeyCode.Return))
        {
            combo.c = (Buttons[])comboList.ToArray(typeof(Buttons));
            print(combo.ToString());
            AssetDatabase.CreateAsset(combo, "Assets/Resources/Combos/" + combo.ToString() + ".asset");
            combo = ScriptableObject.CreateInstance<Combo>();
            combo.c = new Buttons[0];
            comboList = new ArrayList();
            comboCount++;
        }

        // Remove the last element
        if (Input.GetKeyDown(KeyCode.Backspace) && comboList.Count > 0)
        {
            print("Removed: " + comboList[comboList.Count - 1].ToString());
            comboList.RemoveAt(comboList.Count - 1);
        }
#endif
    }

}
