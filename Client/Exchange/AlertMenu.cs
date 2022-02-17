using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlertMenu : MonoBehaviour
{
    public Text contentText;

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
