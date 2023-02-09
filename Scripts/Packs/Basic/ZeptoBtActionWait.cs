using System.Globalization;
using UnityEngine;
using ZeptoBt;

public class ZeptoBtActionWait : ZeptoBtAction
{
    public override string[] Params
    {
        get => base.Params;
        set 
        { 
            base.Params = value;
            if(base.Params.Length > 0)
                float.TryParse(base.Params[0], NumberStyles.Float, CultureInfo.InvariantCulture, out delay);
        }
    }

    private float delay = 1;
    enum Status { Idle, Running, Done }
    Status status;
    private float stopDate;

    public override void Abort()
    {
        status = Status.Idle;
    }

    public override NodeReturn Tick()
    {
        switch (status)
        {
            case Status.Idle:
                stopDate = Time.time + delay;
                status = Status.Running;
                return NodeReturn.Runnning;
            case Status.Running:
                if (Time.time > stopDate)
                {
                    status = Status.Done;
                    return NodeReturn.Success;
                }
                else
                    return NodeReturn.Runnning;
            case Status.Done:
                return NodeReturn.Success;
        }
        return NodeReturn.Success;
    }
}
