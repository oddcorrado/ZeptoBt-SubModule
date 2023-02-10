using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeptoBt;
#if SPINE
using Spine.Unity;
#endif

// TODO
// tree extension (new Node classes)
public class ZeptoBtTree : MonoBehaviour
{
    [SerializeField] string filename;
    [SerializeField] ZeptoBtTrigger[] triggers;
#if SPINE
    [SerializeField] SkeletonAnimation spineAnimation;
    [SerializeField] LifeManager lifeManager;
#endif

    public float TickPeriod { get; set; } = 0.1f;
    public NodeRoot Root { get; set; } = new NodeRoot();
    // public Node CurrentNode { get; set; }
    public float CurrentTime { get; set; }
    public Rigidbody2D MainBody { get; set; }
    public float Vx { get; set; }
    public float Vy { get; set; }
    public bool ApplyVx { get; set; }
    public bool ApplyVy { get; set; }
    public float ImpulseVx { get; set; }
    public float ImpulseVy { get; set; }
    public float Sx { set { var ls = transform.localScale; ls.x = value; transform.localScale = ls; } }
    public float Sy { set { var ls = transform.localScale; ls.y = value; transform.localScale = ls; } }
    private List<string> animationNames = new List<string>();
    Coroutine[] animationCoroutines = new Coroutine[5];

    public Dictionary<string, int> TriggerCounts { get; set; } = new Dictionary<string, int>();
    public class TriggerObject
    {
        public string type;
        public GameObject gameObject;
    }
    public List<TriggerObject> TriggerObjects { get; set; } = new List<TriggerObject>();

    public Dictionary<string, GameObject> Children { get; set; } = new Dictionary<string, GameObject>();

    public string FileData { get; set; }

    public void ReadData()
    {
        if (Fili.FileExists($"{filename}"))
        {
            // Debug.Log($" READING CSV ./Data/{filename}");
            FileData = Fili.ReadAllText($"{filename}");
        }
    }

    public void CreateTree()
    {
        var lines = new List<string>(FileData.Split('\n'));
        List<Node> parentNodes = new List<Node>() { Root };
        int nodeIndex = 0;
        Root.Tree = this;
        Root.Root = Root;
        Root.Index = nodeIndex++;
        Root.Children = new List<Node>();

        lines.ForEach(line =>
        {
            line = line.Replace("\r", "");
            int depth = 0;

            Debug.Log($"{line[0]}");
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
            string[] parameters = new string[datas.Length - 1];
            Array.Copy(datas, 1, parameters, 0, datas.Length - 1);
            switch (datas[0])
            {
                case "?":
                    NodeSelector selector = new NodeSelector();

                    selector.compositeParent = closestParentComposite;
                    selector.Index = nodeIndex++;
                    selector.Root = Root;
                    selector.Tree = this;

                    parentNodes.Add(selector);
                    parentNodes.Add(selector);
                    (parentNode as NodeComposite).Children.Add(selector);
                    if (parentNode is NodeDecorator)
                        parentNode = null;
                    selector.Init();
                    break;

                case ">":
                    NodeSequence sequence = new NodeSequence();
                    sequence.compositeParent = closestParentComposite;
                    sequence.Index = nodeIndex++;
                    sequence.Root = Root;
                    sequence.Tree = this;

                    parentNodes.Add(sequence);
                    (parentNode as NodeComposite).Children.Add(sequence);
                    if (parentNode is NodeDecorator)
                        parentNode = null;
                        
                    sequence.Init();
                    break;

                default:
                    var className = ZeptoBtRegistrar.NameToNode[datas[0]];
                    var node = Activator.CreateInstance(Type.GetType(className));
                    if(node is NodeLeaf)
                    {
                        var leaf = node as NodeLeaf;

                        leaf.Index = nodeIndex++;
                        leaf.compositeParent = closestParentComposite;
                        leaf.Tree = this;
                        leaf.Root = Root;
                        leaf.Params = parameters;
                        leaf.Init();
                    }

                    (parentNode as NodeComposite).Children.Add(node as Node);
                    if (parentNode is NodeDecorator)
                        parentNode = null;

                    if (node is NodeDecorator)
                    {
                        NodeDecorator decorator = node as NodeDecorator;
                        decorator.Index = nodeIndex++;
                        decorator.compositeParent = closestParentComposite; // check me
                        decorator.Tree = this;
                        decorator.Root = Root;
                        parentNodes.Add(decorator);
                        decorator.Init();
                    }

                    break;
            }
        });
    }
    IEnumerator Ticker()
    {
        while (true)
        {
            yield return new WaitForSeconds(TickPeriod);
            CrossTree();
        }
    }

    void CrossTree()
    {
        // Debug.Log($"BT ***** CROSS TREE");
        int inIndex = Root.CurrentNode.Index;
        Root.Tick();

        int outIndex = Root.CurrentNode.Index;
        if (outIndex < inIndex && Root.CurrentNode.GetType() == typeof(NodeLeaf))
        {
            Root.Abort(outIndex);
        }
    }

    void CheckForEvents(Node node)
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

    private IEnumerator Start()
    {
        MainBody = GetComponent<Rigidbody2D>();

        for(int i = 0; i <  transform.childCount; i++)
            Children.Add(transform.GetChild(i).name, transform.GetChild(i).gameObject);

        Root.CurrentNode = Root;

        foreach (var trigger in triggers) trigger.TriggerEnterEvent += TriggerEnter;

        yield return null;

        ReadData();

        CreateTree();

        StartCoroutine(Ticker());
    }

    void Update()
    {
        CurrentTime = Time.time;
    }

    void FixedUpdate()
    {
        var vel = MainBody.velocity;
        if (ApplyVx) vel.x = Vx;
        if (ApplyVy) vel.y = Vy;

        vel.x += ImpulseVx;
        vel.y += ImpulseVy;

        MainBody.velocity = vel;

        ImpulseVx = 0;
        ImpulseVy = 0;
    }
    private void TriggerEnter(string triggerType, ZeptoBtTrigger.TriggerEvent triggerEvent, Collider2D other)
    {
        if (!TriggerCounts.ContainsKey(triggerType)) TriggerCounts[triggerType] = 0;
        TriggerCounts[triggerType] += triggerEvent == ZeptoBtTrigger.TriggerEvent.Enter ? 1 : -1;
        Debug.Log($"TRIGGER {triggerType} {TriggerCounts[triggerType]}");
        if (triggerEvent == ZeptoBtTrigger.TriggerEvent.Enter)
            TriggerObjects.Add(new TriggerObject() { gameObject = other.gameObject, type = triggerType });
        else
            TriggerObjects.RemoveAll(to => to.gameObject == other.gameObject);
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
