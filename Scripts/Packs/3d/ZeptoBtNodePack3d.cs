using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace ZeptoBt.NodePack3d
{
    public class NodeLeafVelocity : NodeLeaf
    {
        public override string Documentation { get; } =
            "<#ff9900><b>[vel vx vy vz] :  </b><#ffff00>velocity\n" +
            "<#00ff00>Forces the velocity of the object.\n" +
            "<#00eeff><b>vx: </b><#0099ff>x velocity, float variable, defaults to 0\n" +
            "<#00eeff><b>vy: </b><#0099ff>y velocity, float variable, defaults to 0\n" +
            "<#00eeff><b>vz: </b><#0099ff>y velocity, float variable, defaults to 0\n";

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

                if (base.Params.Length > 2)
                {
                    if (!float.TryParse(base.Params[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vz))
                        vzVar = base.Params[1];
                    applyVz = true;
                }
            }
        }

        private bool applyVx;
        private bool applyVy;
        private bool applyVz;
        private float vx;
        private string vxVar;
        private float vy;
        private string vyVar;
        private float vz;
        private string vzVar;

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
            Tree.Vz = vzVar != null && Root.Evaluator.Variables.ContainsKey(vzVar) ? (float)Root.Evaluator.Variables[vzVar] : vz;
            Tree.ApplyVz = applyVz;
            Status = NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF VEL {Index} {applyVx} {vx}";
        }
    }

    public class NodeLeafRoam : NodeLeaf
    {
        public override string Documentation { get; } =
            "<#ff9900><b>[roam vel radius] : </b><#ffff00>roam\n" +
            "<#00ff00>Roams around spawn position\n" +
            "<#00eeff><b>vel: </b><#0099ff>roaming velocity, float variable, defaults to 0\n" +
            "<#00eeff><b>radius: </b><#0099ff>roaming radius, float variable, defaults to 0\n";
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

        private NodeParam vel = new NodeParam(2);
        private NodeParam radius = new NodeParam(3);
        private float randomTargetUpdateDate;
        Vector3 target;
        Vector3 spawnPos;

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
                target = spawnPos + radius.Get(Root.Evaluator) * new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                randomTargetUpdateDate = Tree.CurrentTime + 2;
            }

            float d = (target - new Vector3(Tree.transform.position.x, Tree.transform.position.y)).magnitude;
            Vector3 velocity =
                Mathf.Min(vel.Get(Root.Evaluator), d)
                * (target - new Vector3(Tree.transform.position.x, Tree.transform.position.y)).normalized;
            Tree.Vx = velocity.x;
            Tree.Vy = velocity.y;
            Tree.Vz = velocity.z;
            Tree.ApplyVx = true;
            Tree.ApplyVy = true;
            Tree.ApplyVz = true;

            Status = NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF ROAM {Index} {target}";
        }
    }

    public class NodeLeafProwl : NodeLeaf
    {
        public override string Documentation { get; } =
            "<#ff9900><b>[prowl target vel radius avel] : </b><#ffff00>prowl\n" +
            "<#00ff00>prowls around an object\n" +
            "<#00eeff><b>target: </b><#0099ff>any valid trigger target, string, defaults to Player0\n" +
            "<#00eeff><b>vel: </b><#0099ff>prowling velocity, float variable, defaults to 2\n" +
            "<#00eeff><b>avel: </b><#0099ff>angular velocity, float variable, defaults to 0.1\n";

        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;

                if (base.Params.Length > 0) target = base.Params[0];
                if (base.Params.Length > 1) vel.Set(base.Params[1]);
                if (base.Params.Length > 2) radius.Set(base.Params[2]);
                if (base.Params.Length > 3) angleStep.Set(base.Params[3]);
            }
        }

        private string target = "Player";
        private NodeParam vel = new NodeParam(2);
        private NodeParam radius = new NodeParam(3);
        private NodeParam angleStep = new NodeParam(0.1f);
        private float angle;

        public override void Tick()
        {
            // Debug.Log($"BT TICK - {this}");

            var go = Tree.GetTriggerObject(target);
            if (go == null)
            {
                Status = NodeReturn.Failure;
                return;
            }

            Vector3 targetPos = go.transform.position;

            angle += angleStep.Get(Root.Evaluator);
            targetPos = targetPos + radius.Get(Root.Evaluator) * new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

            if ((targetPos - new Vector3(Tree.transform.position.x, Tree.transform.position.y)).magnitude > 0.01f)
            {
                Vector3 velocity =
                    vel.Get(Root.Evaluator)
                    * (targetPos - new Vector3(Tree.transform.position.x, Tree.transform.position.y)).normalized;
                Tree.Vx = velocity.x;
                Tree.Vy = velocity.y;
                Tree.Vz = velocity.z;
            }
            else
            {
                Tree.Vx = 0;
                Tree.Vy = 0;
                Tree.Vz = 0;
            }

            Tree.ApplyVx = true;
            Tree.ApplyVy = true;
            Tree.ApplyVz = true;

            Status = NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF ROAM {Index} {target}";
        }
    }

    public class NodeLeafMoveTo : NodeLeaf
    {
        public override string Documentation { get; } =
            "<#ff9900><b>[moveto vel trigger|x y z] : </b><#ffff00>moveto\n" +
            "<#00ff00>moves towards a target (random or object or position) \n" +
            "<#00eeff><b>target: </b><#0099ff>any valid trigger target, string, defaults to Player0\n" +
            "<#00eeff><b>x: </b><#0099ff>x pos, float variable, defaults to 0\n" +
            "<#00eeff><b>y: </b><#0099ff>y pos, float variable, defaults to 0\n" +
            "<#00eeff><b>z: </b><#0099ff>z pos, float variable, defaults to 0\n";

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
                    case 4:
                        mode = Mode.Pos;
                        x.Set(base.Params[1]);
                        y.Set(base.Params[2]);
                        z.Set(base.Params[3]);
                        break;
                }
            }
        }

        private NodeParam vel = new NodeParam();
        private NodeParam x = new NodeParam();
        private NodeParam y = new NodeParam();
        private NodeParam z = new NodeParam();

        private string triggerTarget;
        enum Mode { Pos, Trigger, Random }
        private Mode mode;
        private float randomTargetUpdateDate;
        Vector3 target;

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
                            new Vector3(Tree.transform.position.x, Tree.transform.position.y, Tree.transform.position.z)
                            + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-2f, 2f)).normalized * 5;
                        randomTargetUpdateDate = Tree.CurrentTime + 2;
                    }
                    break;
            }

            float d = (target - new Vector3(Tree.transform.position.x, Tree.transform.position.y, Tree.transform.position.z)).magnitude;
            Vector3 velocity =
                Mathf.Min(vel.Get(Root.Evaluator), d / Tree.TickPeriod)
                * (target - new Vector3(Tree.transform.position.x, Tree.transform.position.y, Tree.transform.position.z)).normalized;
            Tree.Vx = velocity.x;
            Tree.Vy = velocity.y;
            Tree.Vz = velocity.z;
            Tree.ApplyVx = true;
            Tree.ApplyVy = true;
            Tree.ApplyVz = true;

            Status = NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF MOVE TO {Index} {mode} {triggerTarget} {target}";
        }
    }


    public class NodeLeafDodge : NodeLeaf
    {
        public override string Documentation { get; } =
            "<#ff9900><b>[dodge vx vy vz trigger jump] : </b><#ffff00>dodge\n" +
            "<#00ff00>dodges an incoming threat according to its type\n" +
            "<#00eeff><b>vx: </b><#0099ff>dodge x velocity, float variable, defaults to 0\n" +
            "<#00eeff><b>vy: </b><#0099ff>dodge y velocity, float variable, defaults to 0\n" +
            "<#00eeff><b>vz: </b><#0099ff>dodge z velocity, float variable, defaults to 0\n" +
            "<#00eeff><b>target: </b><#0099ff>any valid trigger target, string, defaults to _\n" +
            "<#00eeff><b>jump: </b><#0099ff>allow jumps when groundedt, true or false, default to false";
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
                    if (!float.TryParse(base.Params[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vz))
                        vzVar = base.Params[2];
                }

                if (base.Params.Length > 3)
                {
                    triggerTarget = base.Params[3];
                }

                if (base.Params.Length > 4)
                {
                    bool.TryParse(base.Params[4], out isJump);
                }

                if (base.Params.Length > 5)
                {
                    bool.TryParse(base.Params[5], out isJump);
                }
            }
        }

        private float vx;
        private string vxVar;
        private float vy;
        private string vyVar;
        private float vz;
        private string vzVar;
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

            Vector3 dir = Vector3.up;
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb != null) dir = Vector3.Cross(rb.velocity, Tree.MainBody.velocity).normalized;
            Vector3 velocity = new Vector3(
                        dir.x * (vxVar != null ? (float)Root.Evaluator.Variables[vxVar] : vx),
                        dir.y * (vyVar != null ? (float)Root.Evaluator.Variables[vyVar] : vy),
                        dir.z * (vzVar != null ? (float)Root.Evaluator.Variables[vzVar] : vz));
            Tree.Vx = velocity.x;
            Tree.ApplyVx = true;
            Tree.Vz = velocity.z;
            Tree.ApplyVz = true;
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
        public override string Documentation { get; } =
            "<#ff9900><b>[scale sx sy sz] : </b><#ffff00>moveto\n" +
            "<#00ff00>scales the loocal scale of the object\n" +
            "<#00eeff><b>sx: </b><#0099ff>x scale, float variable, defaults to 0\n" +
            "<#00eeff><b>sy: </b><#0099ff>y scale, float variable, defaults to 0\n" +
            "<#00eeff><b>sz: </b><#0099ff>z scale, float variable, defaults to 0\n";
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

                if (base.Params.Length > 2)
                {
                    if (!float.TryParse(base.Params[1], NumberStyles.Float, CultureInfo.InvariantCulture, out sz))
                        szVar = base.Params[2];
                    applySz = true;
                }
            }
        }

        private bool applySx;
        private bool applySy;
        private bool applySz;
        private float sx;
        private string sxVar;
        private float sy;
        private string syVar;
        private float sz;
        private string szVar;

        public override void Tick()
        {
            // Debug.Log($"BT TICK - {this}");
            if (applySx) Tree.Sx = sxVar != null ? (float)Root.Evaluator.Variables[sxVar] : sx;
            if (applySy) Tree.Sy = syVar != null ? (float)Root.Evaluator.Variables[syVar] : sy;
            if (applySz) Tree.Sz = szVar != null ? (float)Root.Evaluator.Variables[szVar] : sz;

            Status = NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF SCALE {Index} {applySx} {sx}";
        }
    }

    public class NodeLeafTrigger : NodeLeaf
    {
        public override string Documentation { get; } =
            "<#ff9900><b>[trigger target match] : </b><#ffff00>trigger\n" +
            "<#00ff00>reacts to a trigger value\n" +
            "<#00eeff><b>target: </b><#0099ff>target trigger name, string, defaults to ''\n" +
            "<#00eeff><b>match: </b><#0099ff>if true success is returned on target present, true/false, defaults to false\n";

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
