using System.Collections.Generic;
// using CodingSeb.ExpressionEvaluator;
using System;
using UnityEngine;

// variable extractor
// Node Spawn (fx + blasts)
// ZeptoBtTree impulse VX/Vy
// Check decorators
// NodeLeafShootAt
// NodeLeafOrientTo (for shield)
// Shield element
// Node release
// Death explosion
// help for parses
namespace ZeptoBt
{
    public enum NodeReturn { Failure, Runnning, Success, Unprocessed }
    public delegate void ForceTickDelegate();

    public class DocParameter
    {
        public string name;
        public string description;
        public bool isEvaluated;
        public string defaultValue;
    }
    public class Doc
    {
        public string name;
        public DocParameter[] parameters;
        public string description;
        public string prototype;
    }
    public class Node
    {
        public NodeComposite compositeParent;
        public NodeRoot Root { get; set; }

        public virtual string[] Params { get; set; }

        public virtual string Documentation { get; } = "Put your doc here";
        public virtual Doc Doc { get; } = new Doc() { name = "none", description = "write your doc !!!" };

        public string Comment { get; set; } = "";

        public int Index { get; set; }

        public ZeptoBtTree Tree { get; set; }

        public ZeptoBtViewNode ViewNode { get; set; }

        private NodeReturn status;
        public NodeReturn Status
        { 
            get => status;
            set
            {
                status = value;
                if(ViewNode != null) ViewNode.Status = Status;
            } 
        }

        public virtual void Tick()
        {
            Status = NodeReturn.Failure;
        }

        public virtual void Abort(int index)
        {
        }

        public virtual void Init()
        { }

        public override string ToString()
        {
            return "node";
        }
    }

    public class NodeParam<T>
    {
        T value;
        string name;
        bool isVar;

        public NodeParam(T init)
        {
            value = init;
        }

        public NodeParam() { }
        public void Set(string data)
        {
            if (typeof(T).IsEnum)
            {
                try
                {
                    value = (T)Enum.Parse(typeof(T), data);
                    isVar = false;
                }
                catch (InvalidCastException e)
                {
                    Debug.LogWarning($"Invalid cast {e.Message} @ {data}");
                    name = data;
                    isVar = true;
                }
                catch (FormatException e)
                {
                    Debug.LogWarning($"Invalid format {e.Message} @ {data}");
                    name = data;
                    isVar = true;
                }
            }
            else
            {
                try
                {
                    value = (T)Convert.ChangeType(data, typeof(T));
                    isVar = false;
                }
                catch (InvalidCastException e)
                {
                    Debug.LogWarning($"Invalid cast {e.Message} @ {data}");
                    name = data;
                    isVar = true;
                }
                catch (FormatException e)
                {
                    Debug.LogWarning($"Invalid format {e.Message} @ {data}");
                    name = data;
                    isVar = true;
                }
            }
        }
        public T Get(DummyEvaluator evaluator)
        {
            /*if(isVar && evaluator.Variables.ContainsKey(name))
            {
                return (T)evaluator.Variables[name];
            } */
            return default(T);
        }
    }

    public class DummyEvaluator
    {
        public object Evaluate(string d) { return null; }
        public Dictionary<string, object> Variables;
    }
    public class NodeRoot : NodeComposite
    {
        public Node CurrentNode { get; set; }
        public DummyEvaluator Evaluator { get; set; } = new DummyEvaluator();

        public override string Documentation { get; } =
            "<#ff9900><b>[root] : </b><#ffff00>Root Node\n" +
            "<#00ff00>First node in the tree, all operations on the tree go trough this node.";

        // evaluator.Variables = new Dictionary<string, object>() {};

        public override void Tick()
        {
            // only useful if a viewer is attached
            if(ViewNode != null)
            {
                Tree.Traverse(this, node =>
                {
                    node.Status = NodeReturn.Unprocessed;
                });
            }

            if(Children.Count > 0)
                Children[0].Tick();
            Status = NodeReturn.Runnning;
            OnExit(CurrentNode.Status);
        }
    }
    public class NodeComposite : Node
    {
        protected List<Node> children = new List<Node>();
        public int ChildIndex { get; set; }
        public List<Node> Children { get => children; set { children = value; } }

        public delegate void ExitDelegate(NodeReturn exitValue);
        public event ExitDelegate ExitEvent;

        protected virtual void OnExit(NodeReturn exitValue)
        {
            ExitEvent?.Invoke(exitValue);
        }
    }
    public class NodeDecorator : NodeComposite
    {
    }

    public class NodeLeaf : Node
    {
        public event ForceTickDelegate ForceTickEvent;
        public ZeptoBtAction ZeptoBtAction { get; set; }

        public override void Abort(int index)
        {
            /* if (index < Index)
            {
                parent.Abort(index);
            } */
        }

        public override void Tick()
        {
            NodeReturn nr = ZeptoBtAction.Tick();
            Status = nr;
        }

        public virtual void Abort()
        { }
    }

#if SPINE
    public class NodeLeafSpine : NodeLeaf
    {
        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;

                if (base.Params.Length > 0)
                {
                    animation = base.Params[0];
                }

                if (base.Params.Length > 1)
                {
                    bool.TryParse(base.Params[1], out loop);
                }

                if (base.Params.Length > 2)
                {
                    int.TryParse(base.Params[2], out trackIndex);
                }
            }
        }

        private string animation;
        private int trackIndex;
        private bool loop;

        public override NodeReturn Tick()
        {
            // Debug.Log($"BT TICK - {this}");
            Tree.SetAnimation(animation, trackIndex, loop);

            return NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF SPINE {Index} {animation} {loop} {trackIndex}";
        }
    }

    public class NodeLeafHit : NodeLeaf
    {
        private int life = 50;

        public override NodeReturn Tick()
        {
            int newLife = Tree.GetLife();
            // Debug.Log($"BT TICK - {this} {newLife} {life}");


            if (newLife != life)
            {
                life = newLife;
                return NodeReturn.Success;
            }

            return NodeReturn.Failure;
        }

        public override string ToString()
        {
            return $"NODE LEAF HIT {Index} {life}";
        }
    }
#endif

}

