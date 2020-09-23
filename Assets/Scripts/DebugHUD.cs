using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;

public class DebugHUD : Singleton<DebugHUD>
{

    private Dictionary<string,string> nodes = new Dictionary<string, string>();
    private Canvas mainCanvas;
    private Text mainTextBox;

    // (Optional) Prevent non-singleton constructor use.
    protected DebugHUD() { }

    // Start is called before the first frame update
    void Start()
    {
        mainCanvas = GetComponentInChildren<Canvas>();
        mainTextBox = GetComponentInChildren<Text>();
    }

    public void PrintVariable(string name, object value)
    {
        if(nodes.ContainsKey(name))
        {
            nodes[name] = value.ToString();
        }
        else
        {
            nodes.Add(name, value.ToString());
        }
    }

    private void UpdateUI()
    {
        string data = "";
        foreach (var node in nodes)
        {
            data += node.Key + ": " + node.Value + "\n";
        }
        mainTextBox.text = data;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //if(mainCanvas.worldCamera != Camera.current)
        //{
        //    mainCanvas.worldCamera = Camera.current;
        //}

        UpdateUI();
    }
}
