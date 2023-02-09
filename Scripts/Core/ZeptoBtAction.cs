using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeptoBt;

public class ZeptoBtAction : MonoBehaviour
{
    public virtual string[] Params { get; set; }
    public virtual NodeReturn Tick()
    {
        return NodeReturn.Success;
    }

    public virtual void Abort()
    { }
}
