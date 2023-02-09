using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ZeptoBtDragger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] ZeptoBtViewer zeptoBtViewer;
    [SerializeField] Image image;
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        zeptoBtViewer.DragNode();
        image.color = Color.grey;
    }
    public void OnPointerUp(PointerEventData pointerEventData)
    {
        image.color = Color.white;
    }

    private bool interactable;
    public bool Interactable
    {
        get => interactable;
        set
        {
            interactable = value;
            image.color = interactable ? Color.white : Color.gray;
        }
    }
}
