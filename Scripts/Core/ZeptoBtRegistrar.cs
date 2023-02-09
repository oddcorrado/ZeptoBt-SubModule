using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ZeptoBt
{
    public class ZeptoBtRegistrar
    {
        static public Dictionary<string, string> NameToNode { get; set; } = new Dictionary<string, string>();

        static ZeptoBtRegistrar()
        {
            Init();
        }
        static void Init()
        {
            /* string allNodesStr = Fili.ReadAllText("nodes.txt");
            Debug.Log($"Core registrar init ??? {allNodesStr}");

            allNodesStr = allNodesStr.Replace("\r", "");
            var allNodes = allNodesStr.Split("\n");
            allNodes.ToList().ForEach(v =>
            {
                string[] values = v.Split(",");
                // NameToNode.Add(values[0], values[1]);
            });*/

            var allInits = Resources.LoadAll("", typeof(ZeptoBtNameToNode));

            Debug.Log($"allinits {allInits.Length}");
            foreach (var init in allInits) (init as ZeptoBtNameToNode).Init();
        }
    }
}
