using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeptoBtTestViewerSwitch : MonoBehaviour
{
    [SerializeField] ZeptoBtTree tree;
    [SerializeField] GameObject selected;

    ZeptoBtViewer rootViewer;
    void Start()
    {
        rootViewer = FindObjectOfType<ZeptoBtViewer>();
    }
    void Update()
    {
        if (rootViewer == null) return;

        if (rootViewer.Root != tree.Root)
            selected.SetActive(false);

        if (!rootViewer.IsActive) return;
        
        if(Input.GetMouseButtonDown(0))
        {
            var pos = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            pos.z = transform.position.z;
            if ((transform.position - pos).magnitude < 1)
            {
                rootViewer.Root = tree.Root;
                selected.SetActive(true);
            }
        }

    }
}
