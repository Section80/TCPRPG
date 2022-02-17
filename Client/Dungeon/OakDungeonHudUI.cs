using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OakDungeonHudUI : MonoBehaviour
{
    public static OakDungeonHudUI instance;

    public Text objective1Text;
    public int objective1;

    public BackToTownUI backToTownUI;

    public void UpdateObjective(int objectiveID, int val)
    {
        objective1 = val;
        objective1Text.text = "ø¿≈© ≈∑ " + objective1 + " / 1";

        if (objective1 == 1)
        {
            if(HudUI.instance.backToTownUI.gameObject.activeInHierarchy) {
                HudUI.instance.backToTownUI.gameObject.SetActive(false);
            }

            backToTownUI.gameObject.SetActive(true);
        }
    }

    private void Awake()
    {
        instance = GetComponent<OakDungeonHudUI>();
    }
}
