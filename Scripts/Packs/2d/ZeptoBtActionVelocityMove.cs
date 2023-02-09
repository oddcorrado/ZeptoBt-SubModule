using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeptoBt;
using System.Globalization;

public class ZeptoBtActionVelocityMove : ZeptoBtAction
{
    public override string[] Params
    {
        get => base.Params;
        set
        {
            base.Params = value;
            if (base.Params.Length > 0)
                float.TryParse(base.Params[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vx);
            if (base.Params.Length > 1)
                float.TryParse(base.Params[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vy);
        }
    }

    private float vx = 1;
    private float vy = 0;
    private Rigidbody2D body;

    public override void Abort()
    {
    }

    public override NodeReturn Tick()
    {
        body.velocity = new Vector2(vx, vy);
        return NodeReturn.Success;
    }

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }
}
