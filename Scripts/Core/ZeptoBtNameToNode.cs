using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeptoBt;

[CreateAssetMenu(fileName = "ZeptoBtConfig", menuName = "ScriptableObjects/ZeptoBtConfig", order = 1)]
public class ZeptoBtNameToNode : ScriptableObject
{
    [System.Serializable]
    public class NameToNode
    {
        public string name;
        public string node;
    }

    public NameToNode[] nameToNodes;

    public void Init()
    {
        Debug.Log("ZeptoBtNameToNode init");
        foreach(var nameToNode in nameToNodes)
        {
            ZeptoBtRegistrar.NameToNode.Add(nameToNode.name, nameToNode.node);
        }
        Debug.Log("ZeptoBtNameToNode done");
    }
}
