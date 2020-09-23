using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DebugHUD : Singleton<DebugHUD>
{
    class Node
    {
        public Node(string name, object data)
        {
            Name = name;
            Data = data;
        }
        public string Name { get; set; }
        public object Data { get; set; }
    }

    private List<Node> nodes = new List<Node>();
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

    public void AddNode(string name, object value)
    {
        Node temp = new Node(name,value);
        nodes.Add(temp);
    }

    private void UpdateUI()
    {
        string data = "";
        foreach (Node node in nodes)
        {
            data += node.Name + ": " + node.Data.ToString() + "\n";
        }
        mainTextBox.text = data;
    }

    // Update is called once per frame
    void Update()
    {
        //if(mainCanvas.worldCamera != Camera.current)
        //{
        //    mainCanvas.worldCamera = Camera.current;
        //}

        UpdateUI();
    }
}
