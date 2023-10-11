using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection : MonoBehaviour
{
    // Start is called before the first frame update
    public static Connection Instance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public bool Host { get; set; }
    public string GameId { get; set; }
}
