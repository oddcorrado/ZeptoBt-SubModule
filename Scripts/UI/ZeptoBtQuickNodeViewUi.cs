using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZeptoBt;

public class ZeptoBtQuickNodeViewUi : MonoBehaviour
{
    [SerializeField] TMP_Text shortNameDisplay;
    [SerializeField] TMP_Text parametersDisplay;
    [SerializeField] TMP_Text commentsDisplay;
    [SerializeField] Image image;
    [SerializeField] GameObject container;
    [SerializeField] Color successColor = new Color(0, 0.6f, 0, 1);
    [SerializeField] Color failColor = new Color(0.7f, 0, 0, 1);
    [SerializeField] Color runColor = new Color(0, 0.5f, 0.5f, 1);
    [SerializeField] Color unprocessedColor = new Color(0.5f, 0.5f, 0.5f, 1);

    ZeptoBtViewer zeptoBtViewer = null;

    public bool IsActive { get; set; }
    public NodeReturn Status
    {
        set
        {
            image.color = value switch
            {
                NodeReturn.Success => successColor,
                NodeReturn.Failure => failColor,
                NodeReturn.Runnning => runColor,
                _ => unprocessedColor
            };
        }
    }

    public void Tick(string shortName, string parameters, string comments, NodeReturn statusIn)
    {
        shortNameDisplay.text = shortName;
        parametersDisplay.text = parameters;
        commentsDisplay.text = comments;
        Status = statusIn;
    }

    IEnumerator Start()
    {
        zeptoBtViewer = FindObjectOfType<ZeptoBtViewer>();

        while (zeptoBtViewer == null)
        {
            yield return new WaitForSeconds(0.5f);
            zeptoBtViewer = FindObjectOfType<ZeptoBtViewer>();
        }
        zeptoBtViewer.quickViewModeEvent += ToogleView;
    }

    public void ToogleView(bool isOn)
    {
        IsActive = isOn;
        container.SetActive(isOn);
    }
}
