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
    [SerializeField] TMP_Text commentsDisplay;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Image image;
    [SerializeField] Image focusImage;
    [SerializeField] Image connectorTop;
    [SerializeField] Image connectorBot;
    [SerializeField] Color successColor = new Color(0, 0.6f, 0, 1);
    [SerializeField] Color failColor = new Color(0.7f, 0, 0, 1);
    [SerializeField] Color runColor = new Color(0, 0.5f, 0.5f, 1);
    [SerializeField] Color unprocessedColor = new Color(0.5f, 0.5f, 0.5f, 1);

    Color targetColor;

    public Image ConnectorTop { get => connectorTop; }
    public Image ConnectorBot { get => connectorBot; }
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

    public string Comment
    {
        set
        {
            commentsDisplay.text = value;
        }
    }

    public bool Selected
    {
        set
        {
            if (spriteRenderer != null) spriteRenderer.color = value ? Color.grey : Color.white;
            if (image != null) image.color = value ? Color.grey : Color.white;
            if (focusImage != null) focusImage.gameObject.SetActive(value);
        }
    }

    public NodeReturn Status
    {
        set
        {
            targetColor = value switch
            {
                NodeReturn.Success => successColor,
                NodeReturn.Failure => failColor,
                NodeReturn.Runnning => runColor,
                _ => unprocessedColor
            };
        }
    }

    void Update()
    {
        image.color = targetColor; // (1 - Time.deltaTime) * image.color + Time.deltaTime * targetColor;
    }
}
