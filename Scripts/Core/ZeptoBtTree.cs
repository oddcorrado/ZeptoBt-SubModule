using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeptoBt;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;
using Unity.Collections;
using Unity.Mathematics;

#if SPINE
using Spine.Unity;
#endif

// TODO
// tree extension (new Node classes)
public class ZeptoBtTree : MonoBehaviour
{
    [SerializeField] protected string filename;
    [SerializeField] protected ZeptoBtTrigger[] triggers;
    [SerializeField] protected float velocitySmoothing;
    protected bool started = false;

#if SPINE
    [SerializeField] SkeletonAnimation spineAnimation;
    [SerializeField] LifeManager lifeManager;
#endif

    public float TickPeriod { get; set; } = 0.1f;
    public float TriggerCheckPeriod { get; set; } = 0.3f;
    public NodeRoot Root { get; set; } = new NodeRoot();
    // public Node CurrentNode { get; set; }
    public float CurrentTime { get; set; }
    public Rigidbody2D MainBody2D { get; set; }
    public Rigidbody MainBody { get; set; }
    public float Vx { get; set; }
    public float Vy { get; set; }
    public float Vz { get; set; }
    public bool ApplyVx { get; set; }
    public bool ApplyVy { get; set; }
    public bool ApplyVz { get; set; }
    public float ImpulseVx { get; set; }
    public float ImpulseVy { get; set; }
    public float ImpulseVz { get; set; }
    public float Sx { set { var ls = transform.localScale; ls.x = value; transform.localScale = ls; } }
    public float Sy { set { var ls = transform.localScale; ls.y = value; transform.localScale = ls; } }
    public float Sz { set { var ls = transform.localScale; ls.z = value; transform.localScale = ls; } }
    protected List<string> animationNames = new List<string>();
    protected Coroutine[] animationCoroutines = new Coroutine[5];

    protected List<Node> nodes = new List<Node>();  

    protected ZeptoBtQuickNodeViewUi zeptoBtQuickNodeViewUi;
    public Dictionary<string, int> TriggerCounts { get; set; } = new Dictionary<string, int>();
    public class TriggerObject
    {
        public string type;
        public GameObject gameObject;
    }

    public struct NativeTriggerObject
    {
        public FixedString32Bytes type;
        public int id;
        public float3? stopPos;
        public float width;
    }

    public virtual List<TriggerObject> TriggerObjects { get; set; } = new List<TriggerObject>();

    public Dictionary<string, GameObject> Children { get; set; } = new Dictionary<string, GameObject>();

    public string FileData { get; set; }

    List<string> bootVarLines;
    public List<string> BootVarLines { get => bootVarLines; }

    public void ReadData()
    {
        if (Fili.FileExists($"{filename}"))
        {
             Debug.Log($" READING CSV ./Data/{filename}");
            FileData = Fili.ReadAllText($"{filename}");
        } else if (Fili.DevFileExists($"{filename}"))
        {
            FileData = Fili.ReadAllTextDev($"{filename}");
        }
    }
    public void ToggleTrigger(string trigName, bool activate)
    {
        if (trigName == "all")
        {
            foreach (var trigger in triggers) trigger.gameObject.SetActive(activate);
            return;
        }

        ZeptoBtTrigger trig = triggers.FirstOrDefault(t => t.gameObject.name.Contains(trigName));
        
        if (trig == null)
        {
            Debug.LogError($"Cannot find trigger {trigName}, trying to toggle it {activate}");
            return;
        }
        trig.gameObject.SetActive(activate);
    }
    
    public void UpdateIndexes()
    {
        nodes = new List<Node>();
        UpdateIndexes(Root, 0);
    }

    private int UpdateIndexes(Node node, int index)
    {
        int localIndex = index;
        
        node.Index = localIndex++;
        nodes.Add(node);

        if(node is NodeComposite)
        {
            (node as NodeComposite).Children.ForEach(child =>
            {
                localIndex = UpdateIndexes(child, localIndex);
            });
        }

        return localIndex;
    }

    void AddBootVariable(string line)
    {
        Regex rxFloat = new(@"[\*]+[\s]*([\w]+)[\s]*=[\s]*([\d]+\.[\d]+)[\s]*(#.*)*");
        if(rxFloat.IsMatch(line))
        {
            var match = rxFloat.Match(line);
            var key = match.Groups[1].Value;
            var val = match.Groups[2].Value;

            if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out var valFloat))
                Root.Evaluator.Variables.Add(key, valFloat);
            return;
        }
        Regex rxInt = new(@"[\*]+[\s]*([\w]+)[\s]*=[\s]*([\d]+)[\s]*(#.*)*");
        if (rxInt.IsMatch(line))
        {
            var match = rxFloat.Match(line);
            var key = match.Groups[1].Value;
            var val = match.Groups[2].Value;

            if (int.TryParse(val, out var valInt))
                Root.Evaluator.Variables.Add(key, valInt);
            return;
        }
        Regex rxBool = new(@"[\*]+[\s]*([\w]+)[\s]*=[\s]*(true|false)[\s]*(#.*)*");
        if (rxBool.IsMatch(line.ToLower()))
        {
            var match = rxBool.Match(line);
            var key = match.Groups[1].Value;
            var val = match.Groups[2].Value.ToLower();

            if (bool.TryParse(val, out var valBool))
                Root.Evaluator.Variables.Add(key, valBool);
            return;
        }
        Regex rxString = new(@"[\*]+[\s]*([\w]+)[\s]*=[\s]*([^#^\s]+)[\s]*(#.*)*");
        if (rxString.IsMatch(line))
        {
            var match = rxString.Match(line);
            var key = match.Groups[1].Value;
            var val = match.Groups[2].Value;

            Root.Evaluator.Variables.Add(key, val);
            return;
        }
    }

    public void HandleVariables(string variables)
    {
        if (variables == null || variables == "") return;
        List<string> lines = new List<string>(variables.Split('\n'));
     
        lines.ForEach(line =>
        {
            List<string> varLines = new List<string>(variables.Split(' ')); ;
            string nodeClass = ZeptoBtRegistrar.NameToNode[varLines[0]];
            Type nodeType = Type.GetType(nodeClass);
            List<string> newVariables = new() {};

            for (int i = 1; i < varLines.Count; i++)
            {
                newVariables.Add($"{varLines[i]} ");
            }
            
            Node nodeLeaf = nodes.Find(node => node.GetType() == nodeType);
            nodeLeaf.Params = newVariables.Count == 0 ?  new[] {" "} : newVariables.ToArray();
        });
    }
    public void EditTree(string StartVariables)
    {
        if (StartVariables == null || StartVariables == "") return;
        List<string> startVar = new List<string>(StartVariables.Split('\n'));   
        List<string> currentVar = new List<string>(FileData.Split('\n'));
     
        string pattern = @"\w.+";
        int varIdx = 0;

        for (int i =0; i < currentVar.Count; i++)
        {
            if (startVar[varIdx] == "") continue;
            Match match = Regex.Match(currentVar[i], pattern);
            if (match.Success && currentVar[i].Contains(startVar[varIdx].Split(" ")[0]))
            {
                currentVar[i] = currentVar[i].Replace(match.Groups[0].ToString(), startVar[varIdx++]);
            }
        }
        CreateTree(currentVar);
    }

    public void CreateTree(List<string> lines = null)
    {
        //if (tree != null) FileData = tree;
        lines = lines ?? (FileData == null ? new List<string>() { } : new List<string>(FileData.Split('\n')));
        List<Node> parentNodes = new List<Node>() { Root };
        int nodeIndex = 0;
        Root.Tree = this;
        Root.Root = Root;
        Root.Index = nodeIndex++;
        Root.Children = new List<Node>();
        nodes = new List<Node>();
        bootVarLines = new List<string>();

        lines.ForEach(line =>
        {
            line = line.Replace("\r", "");
            int depth = 0;

            line.Trim();
            if (line.Length == 0) return;

            if (line[0] == '*')
            {
                bootVarLines.Add(line);
                return;
            }

            while (depth < line.Length && line[depth] == '-')
            {
                depth++; // check
            }

            if (depth == 0) return;

            if (depth > parentNodes.Count) Debug.LogError("BT invalid - count"); // check
            Node parentNode = parentNodes[depth - 1]; // check
            for (int i = parentNodes.Count - 1; i >= depth; i--) parentNodes.RemoveAt(i); // check
            NodeComposite closestParentComposite = Root;
            for (int i = 0; i < depth; i++)
                if (parentNodes[i] is NodeComposite)
                    closestParentComposite = parentNodes[i] as NodeComposite;

            string[] datas = line.Substring(depth).Split(" ");
            string[] parameters;

            Regex rx = new(@"\[(.*)\]");
            string comment = "";
            if (datas.Length > 1 && rx.IsMatch(datas[1]))
            {
                var match = rx.Match(datas[1]);
                comment = match.Groups[1].Value;
                parameters = new string[datas.Length - 2];
                Array.Copy(datas, 2, parameters, 0, datas.Length - 2);
            }
            else
            {
                parameters = new string[datas.Length - 1];
                Array.Copy(datas, 1, parameters, 0, datas.Length - 1);
            }


            switch (datas[0])
            {
                case "?":
                    NodeSelector selector = new NodeSelector();
                    selector.Comment = comment;

                    selector.compositeParent = closestParentComposite;
                    selector.Index = nodeIndex++;
                    selector.Root = Root;
                    selector.Params = parameters;
                    selector.Tree = this;
                    nodes.Add(selector);

                    parentNodes.Add(selector);
                    parentNodes.Add(selector);
                    (parentNode as NodeComposite).Children.Add(selector);
                    if (parentNode is NodeDecorator)
                        parentNode = null;
                    selector.Init();
                    break;

                case ">":
                    NodeSequence sequence = new NodeSequence();
                    sequence.Comment = comment;

                    sequence.compositeParent = closestParentComposite;
                    sequence.Index = nodeIndex++;
                    sequence.Root = Root;
                    sequence.Params = parameters;
                    sequence.Tree = this;
                    nodes.Add(sequence);

                    parentNodes.Add(sequence);
                    (parentNode as NodeComposite).Children.Add(sequence);
                    if (parentNode is NodeDecorator)
                        parentNode = null;
                        
                    sequence.Init();
                    break;

                default:
                    var className = ZeptoBtRegistrar.NameToNode[datas[0]];
                    Type nodeType = Type.GetType(className);
                    if (nodeType == null)
                    {
                        Debug.LogError($"Node type: {className} not recognized");
                        return;
                    }
                    var node = Activator.CreateInstance(nodeType);
                    if(node is NodeLeaf)
                    {
                        var leaf = node as NodeLeaf;
                        leaf.Comment = comment;

                        leaf.Index = nodeIndex++;
                        leaf.compositeParent = closestParentComposite;
                        leaf.Tree = this;
                        leaf.Root = Root;
                        leaf.Params = parameters;
                        leaf.Init();
                        nodes.Add(leaf);
                    }

                    (parentNode as NodeComposite).Children.Add(node as Node);
                    if (parentNode is NodeDecorator)
                        parentNode = null;

                    if (node is NodeDecorator)
                    {
                        NodeDecorator decorator = node as NodeDecorator;
                        decorator.Comment = comment;
                        decorator.Index = nodeIndex++;
                        decorator.compositeParent = closestParentComposite; // check me
                        decorator.Tree = this;
                        decorator.Root = Root;
                        decorator.Params = parameters;
                        parentNodes.Add(decorator);
                        decorator.Init();
                        nodes.Add(decorator);
                    }

                    break;
            }
        });

        bootVarLines.ForEach(line => AddBootVariable(line));
    }

    protected void StopTicker()
    {
        CancelInvoke();
    }

    protected void StartTicker()
    {
        StopTicker();
        InvokeRepeating("InvokeTicker", 0, TickPeriod);
        InvokeRepeating("CheckTriggers", 0, TriggerCheckPeriod);
    }
    protected void InvokeTicker()
    {
        CrossTree();
    }

    // Unfortunately we have to check triggers through polling
    protected void CheckTriggers()
    {
        foreach (var kvp in nameToTrigger)
        {
            if (Root.Evaluator.Variables.ContainsKey(kvp.Key))
            {
                var stayInterval = Root.Evaluator.Variables[kvp.Key];

                if (stayInterval is float)
                    kvp.Value.StayCheckInterval = (float)stayInterval;
            }
        }
    }
    /*protected IEnumerator Ticker()
    {
        while (true)
        {
            if (!started) yield return new WaitForSeconds(TickPeriod);
            yield return new WaitForSeconds(TickPeriod);
            CrossTree();
        }
    }*/

    protected void CrossTree()
    {
        // Debug.Log($"BT ***** CROSS TREE");
        if (Root?.CurrentNode == null) return;
        int inIndex = Root.CurrentNode.Index;
        if(zeptoBtQuickNodeViewUi != null && zeptoBtQuickNodeViewUi.IsActive)
            nodes.ForEach(n => n.Status = NodeReturn.Unprocessed);

        Root.Tick();

        int outIndex = Root.CurrentNode.Index;
        if(zeptoBtQuickNodeViewUi != null && zeptoBtQuickNodeViewUi.IsActive)
        {
            Node node = Root;
            
            nodes.ForEach(n =>
            {
                if (n.Index > node.Index && n.Status != NodeReturn.Unprocessed) 
                    node = n;
            });

            Debug.Log($"QV ${name} ${node}");
            // FIXME optimize
            zeptoBtQuickNodeViewUi.Tick(
                ZeptoBtRegistrar.NodeToName[node.GetType().ToString()],
                node.Params?.Aggregate("", (a, v) => $"{a} {v}"),
                node.Comment,
                node.Status);
        }
        if (outIndex < inIndex && Root.CurrentNode.GetType() == typeof(NodeLeaf))
        {
            Root.Abort(outIndex);
        }
    }

    protected void CheckForEvents(Node node)
    {
        if (node.GetType() == typeof(NodeLeaf))
        {
            var leaf = node as NodeLeaf;

            leaf.ForceTickEvent += CrossTree;
        }
        else
        {
            (node as NodeComposite).Children.ForEach(child => CheckForEvents(child));
        }
    }

    protected Dictionary<string, ZeptoBtTrigger> nameToTrigger =  new Dictionary<string, ZeptoBtTrigger>();

    protected virtual IEnumerator Start()
    {
        MainBody2D = GetComponent<Rigidbody2D>();
        MainBody = GetComponent<Rigidbody>();
        Children = new Dictionary<string, GameObject>();
        for (int i = 0; i <  transform.childCount; i++)
            Children.Add(transform.GetChild(i).name, transform.GetChild(i).gameObject);

        Root.CurrentNode = Root;

        foreach (var trigger in triggers)
            trigger.Trigger2DEnterEvent += Trigger2DEnter;

        yield return null;

        ReadData();

        CreateTree();

        zeptoBtQuickNodeViewUi = GetComponentInChildren<ZeptoBtQuickNodeViewUi>();

        nameToTrigger = new Dictionary<string, ZeptoBtTrigger>();
        foreach (var trigger in triggers)
        {
            if (!Root.Evaluator.Variables.ContainsKey(trigger.gameObject.name))
                Root.Evaluator.Variables.Add(trigger.gameObject.name, trigger.StayCheckInterval);
            if (nameToTrigger.ContainsKey(trigger.gameObject.name))
                Debug.LogError($"ZeptoBtTree trigger name used twice in tree {trigger.gameObject.name} in Agent {this.gameObject.name}");
            else
                nameToTrigger.Add(trigger.gameObject.name, trigger);
        }

        //StartCoroutine(Ticker());
    }

    protected void UpdateXX()
    {
        //Debug.Log(TriggerCounts.Count);
        CurrentTime = Time.time;
    }

    protected virtual void Update()
    {
        CurrentTime = Time.time;
        if (MainBody2D != null)
        {
            var vel = MainBody2D.velocity;
            if (ApplyVx) vel.x = vel.x * 0.95f + Vx * 0.05f;
            if (ApplyVy) vel.y = vel.y * 0.95f + Vy * 0.05f;

            vel.x += ImpulseVx;
            vel.y += ImpulseVy;

            MainBody2D.velocity = vel;

            ImpulseVx = 0;
            ImpulseVy = 0;
        }

        if (MainBody != null)
        {
            var vel = MainBody.velocity;
            if(vel.magnitude > 1) transform.rotation = Quaternion.LookRotation(new Vector3(vel.x, 0, vel.z)); // rotation

            if (ApplyVx) vel.x = vel.x * velocitySmoothing + Vx * (1 - velocitySmoothing);
            if (ApplyVy) vel.y = vel.y * velocitySmoothing + Vy * (1 - velocitySmoothing);
            if (ApplyVz) vel.z = vel.z * velocitySmoothing + Vz * (1 - velocitySmoothing);

            vel.x += ImpulseVx;
            vel.y += ImpulseVy;
            vel.z += ImpulseVz;

            MainBody.velocity = vel;

            ImpulseVx = 0;
            ImpulseVy = 0;
            ImpulseVz = 0;
        }
    }
    protected void Trigger2DEnter(string triggerType, ZeptoBtTrigger.TriggerEvent triggerEvent, Collider2D other)
    {
        if (triggerEvent != ZeptoBtTrigger.TriggerEvent.Stay)
        {
            if (!TriggerCounts.ContainsKey(triggerType)) TriggerCounts[triggerType] = 0;
            TriggerCounts[triggerType] += triggerEvent == ZeptoBtTrigger.TriggerEvent.Enter ? 1 : -1;
            Debug.Log($"TRIGGER {triggerType} {TriggerCounts[triggerType]}");
            if (triggerEvent == ZeptoBtTrigger.TriggerEvent.Enter)
                TriggerObjects.Add(new TriggerObject() { gameObject = other.gameObject, type = triggerType });
            else
                TriggerObjects.RemoveAll(to => to.gameObject == other.gameObject);
        }
        CrossTree();
    }

  
    public GameObject GetTriggerObject(string type)
    {
        var to = TriggerObjects.Find(to => to.type == type);

        if (to == null) return null;
        return to.gameObject;
    }

    public delegate void TraverseDelegate(Node node);

    public void Traverse(Node node, TraverseDelegate traverseDelegate)
    {
        traverseDelegate(node);

        if(node is NodeComposite) (node as NodeComposite).Children.ForEach(child => Traverse(child, traverseDelegate));
    }

#if SPINE
    protected IEnumerator SpineTrackOneShot(string now, int index)
    {
        if (animationCoroutines[index] != null) StopCoroutine(animationCoroutines[index]);
        spineAnimation.state.ClearTrack(index);
        var track = spineAnimation.state.SetAnimation(index, now, false);
        yield return new WaitForSpineAnimationComplete(track, true);
        spineAnimation.state.ClearTrack(index);
    }

    public void SetAnimation(string name, int trackIndex, bool loop)
    {
        if (spineAnimation == null) return;
        if (animationNames.Find(n => n == name) != null) return;
        while (trackIndex >= animationNames.Count) animationNames.Add("");
        if (!loop)
        {
            StartCoroutine(SpineTrackOneShot(name, trackIndex));
        }
        else
        {
            animationNames[trackIndex] = name;
            spineAnimation.AnimationState.SetAnimation(trackIndex, name, loop);
        }
    }


    public int GetLife()
    {
        if (lifeManager == null) return -1;
        return lifeManager.Life;
    }
#endif
}
