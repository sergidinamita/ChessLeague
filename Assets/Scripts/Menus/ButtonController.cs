using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    bool activeSettings = false;

    [SerializeField]
    private GameObject settings;
    // Start is called before the first frame update


    public void ActivateSettings()
    {
        if (!activeSettings)
        {
            settings.SetActive(true);
            activeSettings = true;
        }
        else
        {
            settings.SetActive(false); 
            activeSettings = false;
        }
    }
}
