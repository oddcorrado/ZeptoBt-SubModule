using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeptoBtTrigger : MonoBehaviour
{
    [System.Serializable]
    public class Trigger
    {
        public string name;
        public string tag;
        public string type;
    }
    public enum TriggerEvent { Enter, Exit }
    [SerializeField] Trigger[] triggers;

    public delegate void TriggerEnterDelegate(string triggerType, TriggerEvent triggEvent, Collider2D other);
    public event TriggerEnterDelegate TriggerEnterEvent;

    void OnTriggerEnter2D(Collider2D other)
    {
        foreach (var trigger in triggers)
        {
            if ((trigger.name == null || trigger.name == "" || trigger.name == other.name)
                && (trigger.tag == null || trigger.tag == "" || trigger.tag == other.tag))
            {
                TriggerEnterEvent?.Invoke(trigger.type, TriggerEvent.Enter, other);
                // return;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        foreach (var trigger in triggers)
        {
            if ((trigger.name == null || trigger.name == "" || trigger.name == other.name)
                && (trigger.tag == null || trigger.tag == "" || trigger.tag == other.tag))
            {
                TriggerEnterEvent?.Invoke(trigger.type, TriggerEvent.Exit, other);
                // return;
            }
        }
    }
}

