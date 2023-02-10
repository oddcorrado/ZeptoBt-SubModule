using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace ZeptoBt
{
    public class NodeLeafVelocity : NodeLeaf
    {
        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;

                if (base.Params.Length > 0)
                {
                    if (!float.TryParse(base.Params[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vx))
                        vxVar = base.Params[0];
                    applyVx = true;
                }

                if (base.Params.Length > 1)
                {
                    if (!float.TryParse(base.Params[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vy))
                        vyVar = base.Params[1];
                    applyVy = true;
                }
            }
        }

        private bool applyVx;
        private bool applyVy;
        private float vx;
        private string vxVar;
        private float vy;
        private string vyVar;

        public override void Abort(int index)
        {
            Tree.ApplyVx = false;
            Tree.ApplyVy = false;
        }
        public override void Tick()
        {
            // Debug.Log($"BT TICK - {this}");
            Tree.Vx = vxVar != null && Root.Evaluator.Variables.ContainsKey(vxVar) ? (float)Root.Evaluator.Variables[vxVar] : vx;
            Tree.ApplyVx = applyVx;
            Tree.Vy = vyVar != null && Root.Evaluator.Variables.ContainsKey(vyVar) ? (float)Root.Evaluator.Variables[vyVar] : vy;
            Tree.ApplyVy = applyVy;
            Status = NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF VEL {Index} {applyVx} {vx}";
        }
    }

    public class NodeLeafRoam : NodeLeaf
    {
        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;

                if (base.Params.Length > 0) vel.Set(base.Params[0]);
                if (base.Params.Length > 1) radius.Set(base.Params[1]);
            }
        }

        private NodeParam<float> vel = new NodeParam<float>(2);
        private NodeParam<float> radius = new NodeParam<float>(3);
        private float randomTargetUpdateDate;
        Vector2 target;
        Vector2 spawnPos;

        public override void Init()
        {
            base.Init();
            spawnPos = Tree.transform.position;
        }
        public override void Tick()
        {
            // Debug.Log($"BT TICK - {this}");

            if (Tree.CurrentTime > randomTargetUpdateDate)
            {
                var angle = Random.Range(0, Mathf.PI * 2f);
                target = spawnPos + radius.Get(Root.Evaluator) * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                randomTargetUpdateDate = Tree.CurrentTime + 2;
            }

            float d = (target - new Vector2(Tree.transform.position.x, Tree.transform.position.y)).magnitude;
            Vector2 velocity =
                Mathf.Min(vel.Get(Root.Evaluator), d)
                * (target - new Vector2(Tree.transform.position.x, Tree.transform.position.y)).normalized;
            Tree.Vx = velocity.x;
            Tree.Vy = velocity.y;
            Tree.ApplyVx = true;
            Tree.ApplyVy = true;

            Status = NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF ROAM {Index} {target}";
        }
    }

    public class NodeLeafProwl : NodeLeaf
    {
        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;

                if (base.Params.Length > 0) target.Set(base.Params[0]);
                if (base.Params.Length > 1) vel.Set(base.Params[1]);
                if (base.Params.Length > 2) radius.Set(base.Params[2]);
                if (base.Params.Length > 3) angleStep.Set(base.Params[3]);
            }
        }

        private NodeParam<string> target = new NodeParam<string>("Player");
        private NodeParam<float> vel = new NodeParam<float>(2);
        private NodeParam<float> radius = new NodeParam<float>(3);
        private NodeParam<float> angleStep = new NodeParam<float>(0.1f);
        private float angle;

        public override void Tick()
        {
            // Debug.Log($"BT TICK - {this}");

            var go = Tree.GetTriggerObject(target.Get(Root.Evaluator));
            if (go == null)
            {
                Status = NodeReturn.Failure;
                return;
            }

            Vector2 targetPos = go.transform.position;

            angle += angleStep.Get(Root.Evaluator);
            targetPos = targetPos + radius.Get(Root.Evaluator) * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            if ((targetPos - new Vector2(Tree.transform.position.x, Tree.transform.position.y)).magnitude > 0.01f)
            {
                Vector2 velocity =
                    vel.Get(Root.Evaluator)
                    * (targetPos - new Vector2(Tree.transform.position.x, Tree.transform.position.y)).normalized;
                Tree.Vx = velocity.x;
                Tree.Vy = velocity.y;
            }
            else
            {
                Tree.Vx = 0;
                Tree.Vy = 0;
            }

            Tree.ApplyVx = true;
            Tree.ApplyVy = true;

            Status = NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF ROAM {Index} {target}";
        }
    }

    public class NodeLeafMoveTo : NodeLeaf
    {
        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;

                if (base.Params.Length > 0)
                {
                    vel.Set(base.Params[0]);
                }

                switch (base.Params.Length)
                {
                    case 1:
                        mode = Mode.Random;
                        break;
                    case 2:
                        mode = Mode.Trigger;
                        triggerTarget = base.Params[1];
                        break;
                    case 3:
                        mode = Mode.Pos;
                        x.Set(base.Params[1]);
                        y.Set(base.Params[2]);
                        break;

                }
            }
        }

        private NodeParam<float> vel = new NodeParam<float>();
        private NodeParam<float> x = new NodeParam<float>();
        private NodeParam<float> y = new NodeParam<float>();
        private string triggerTarget;
        enum Mode { Pos, Trigger, Random }
        private Mode mode;
        private float randomTargetUpdateDate;
        Vector2 target;

        public override void Tick()
        {
            // Debug.Log($"BT TICK - {this}");

            switch (mode)
            {
                case Mode.Pos:
                    target = new Vector2(x.Get(Root.Evaluator), y.Get(Root.Evaluator));
                    break;

                case Mode.Trigger:
                    var go = Tree.GetTriggerObject(triggerTarget);
                    if (go == null)
                    {
                        Status = NodeReturn.Failure;
                        return;
                    }

                    target = go.transform.position;
                    break;

                case Mode.Random:
                    // if (Tree.CurrentTime > randomTargetUpdateDate)
                    {
                        target =
                            new Vector2(Tree.transform.position.x, Tree.transform.position.y)
                            + new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f)).normalized * 5;
                        randomTargetUpdateDate = Tree.CurrentTime + 2;
                    }
                    break;
            }

            float d = (target - new Vector2(Tree.transform.position.x, Tree.transform.position.y)).magnitude;
            Vector2 velocity =
                Mathf.Min(vel.Get(Root.Evaluator), d / Tree.TickPeriod)
                * (target - new Vector2(Tree.transform.position.x, Tree.transform.position.y)).normalized;
            Tree.Vx = velocity.x;
            Tree.Vy = velocity.y;
            Tree.ApplyVx = true;
            Tree.ApplyVy = true;

            Status = NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF MOVE TO {Index} {mode} {triggerTarget} {target}";
        }
    }


    public class NodeLeafDodge : NodeLeaf
    {
        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;

                if (base.Params.Length > 0)
                {
                    if (!float.TryParse(base.Params[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vx))
                        vxVar = base.Params[0];
                }

                if (base.Params.Length > 1)
                {
                    if (!float.TryParse(base.Params[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vy))
                        vyVar = base.Params[1];
                }

                if (base.Params.Length > 2)
                {
                    triggerTarget = base.Params[2];
                }

                if (base.Params.Length > 3)
                {
                    bool.TryParse(base.Params[3], out isJump);
                }

                if (base.Params.Length > 4)
                {
                    bool.TryParse(base.Params[4], out isJump);
                }
            }
        }

        private float vx;
        private string vxVar;
        private float vy;
        private string vyVar;
        private string triggerTarget;
        private bool isJump;

        public override void Tick()
        {
            // Debug.Log($"BT TICK - {this}");


            var go = Tree.GetTriggerObject(triggerTarget);
            if (go == null)
            {
                Status = NodeReturn.Success;
                return;
            }

            Vector2 dir = Vector2.up;
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) dir = rb.velocity.Rotate(90f).normalized;
            Vector2 velocity = new Vector2(
                        dir.x * (vxVar != null ? (float)Root.Evaluator.Variables[vxVar] : vx),
                        dir.y * (vyVar != null ? (float)Root.Evaluator.Variables[vyVar] : vy));
            Tree.Vx = velocity.x;
            Tree.ApplyVx = true;
            if (!isJump || (Tree.TriggerCounts.ContainsKey("Ground") && Tree.TriggerCounts["Ground"] > 0))
            {
                Tree.ApplyVy = true;
                Tree.Vy = Mathf.Abs(velocity.y);
            }
            else
                Tree.ApplyVy = false;

            Status = NodeReturn.Runnning;
        }

        public override string ToString()
        {
            return $"NODE LEAF DODGE {Index} {triggerTarget}";
        }
    }

    public class NodeLeafScale : NodeLeaf
    {
        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;

                if (base.Params.Length > 0)
                {
                    if (!float.TryParse(base.Params[0], NumberStyles.Float, CultureInfo.InvariantCulture, out sx))
                        sxVar = base.Params[0];
                    applySx = true;
                }

                if (base.Params.Length > 1)
                {
                    if (!float.TryParse(base.Params[1], NumberStyles.Float, CultureInfo.InvariantCulture, out sy))
                        syVar = base.Params[1];
                    applySy = true;
                }
            }
        }

        private bool applySx;
        private bool applySy;
        private float sx;
        private string sxVar;
        private float sy;
        private string syVar;

        public override void Tick()
        {
            // Debug.Log($"BT TICK - {this}");
            if (applySx) Tree.Sx = sxVar != null ? (float)Root.Evaluator.Variables[sxVar] : sx;
            if (applySy) Tree.Sy = syVar != null ? (float)Root.Evaluator.Variables[syVar] : sy;

            Status = NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF SCALE {Index} {applySx} {sx}";
        }
    }

    public class NodeLeafTrigger : NodeLeaf
    {
        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;

                if (base.Params.Length > 0)
                {
                    type = base.Params[0];
                }

                if (base.Params.Length > 1)
                {
                    bool.TryParse(base.Params[1], out isOn);
                }
            }
        }

        private string type;
        private bool isOn;

        public override void Abort()
        {
        }
        public override void Tick()
        {
            // Debug.Log($"BT TICK - {this}");
            if ((Tree.TriggerCounts.ContainsKey(type) && Tree.TriggerCounts[type] > 0) == isOn)
                Status = NodeReturn.Success;
            else
                Status = NodeReturn.Failure;
        }

        public override string ToString()
        {
            return ""; // $"NODE LEAF TRIGGER {Index} {isOn} {type} {Tree.TriggerCounts[(int)type] > 0 == isOn}";
        }
    }
}
