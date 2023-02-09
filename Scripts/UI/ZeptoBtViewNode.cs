using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ZeptoBt;
using UnityEngine.UI;

public class ZeptoBtViewNode : MonoBehaviour
{
    [SerializeField] TMP_Text shortNameDisplay;
    [SerializeField] TMP_Text parametersDisplay;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Image image;

    public Node Node { get; set; }
    public string ShortName
    {
        set
        {
            shortNameDisplay.text = value;
        }
    }

    public string Parameters
    {
        set
        {
            parametersDisplay.text = value;
        }
    }

    public bool Selected
    {
        set
        {
            if (spriteRenderer != null) spriteRenderer.color = value ? Color.grey : Color.white;
            if (image != null) image.color = value ? Color.grey : Color.white;
        }
    }

    public NodeReturn Status
    {
        set
        {
            image.color = value switch
            {
                NodeReturn.Success => Color.green,
                NodeReturn.Failure => Color.red,
                NodeReturn.Runnning => Color.cyan,
                _ => Color.white
            };
        }
    }
}
