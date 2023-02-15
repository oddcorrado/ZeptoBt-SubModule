using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ZeptoBtOverlay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image image;

    public bool IsOverInspector { get; set; }
    //Detect if the Cursor starts to pass over the GameObject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        //Output to console the GameObject's name and the following message
        Debug.Log("Cursor Entering " + name + " GameObject");
        image.color = Color.grey;
        IsOverInspector = true;
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        //Output the following message with the GameObject's name
        Debug.Log("Cursor Exiting " + name + " GameObject");
        image.color = Color.black;
        IsOverInspector = false;
    }
}
