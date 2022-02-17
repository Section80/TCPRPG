using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlimeDungeonHudUI : MonoBehaviour
{
    public static SlimeDungeonHudUI instance;

    public Text objective1Text;
    public int objective1;

    public BackToTownUI backToTownUI;

    public void UpdateObjective(int objectiveID, int val)
    {
        objective1 = val;
        objective1Text.text = "ΩΩ∂Û¿” " + objective1 + " / 5";

        if(objective1 == 5)
        {
            backToTownUI.gameObject.SetActive(true);
        }
    }

    private void Awake()
    {
        instance = GetComponent<SlimeDungeonHudUI>();
    }
}
