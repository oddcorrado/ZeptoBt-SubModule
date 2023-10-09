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
    public enum TriggerEvent { Enter, Exit, Stay }
    [SerializeField] Trigger[] triggers;
    [SerializeField] bool continuousStayCheck;

    public delegate void Trigger2DEnterDelegate(string triggerType, TriggerEvent triggEvent, Collider2D other);
    public event Trigger2DEnterDelegate Trigger2DEnterEvent;

    public delegate void Trigger3DEnterDelegate(string triggerType, TriggerEvent triggEvent, Collider other);
    public event Trigger3DEnterDelegate Trigger3DEnterEvent;

    void OnTriggerEnter2D(Collider2D other)
    {
        foreach (var trigger in triggers)
        {
            if ((trigger.name == null || trigger.name == "" || trigger.name == other.name)
                && (trigger.tag == null || trigger.tag == "" || trigger.tag == other.tag))
            {
                Trigger2DEnterEvent?.Invoke(trigger.type, TriggerEvent.Enter, other);
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!continuousStayCheck) return;
        foreach (var trigger in triggers)
        {
            if ((trigger.name == null || trigger.name == "" || trigger.name == other.name)
                && (trigger.tag == null || trigger.tag == "" || trigger.tag == other.tag))
            {
                Trigger2DEnterEvent?.Invoke(trigger.type, TriggerEvent.Stay, other);
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
                Trigger2DEnterEvent?.Invoke(trigger.type, TriggerEvent.Exit, other);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        foreach (var trigger in triggers)
        {
            if (trigger.name == other.name || trigger.tag == other.tag)
            {
                Trigger3DEnterEvent?.Invoke(trigger.type, TriggerEvent.Enter, other);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!continuousStayCheck) return;
        foreach (var trigger in triggers)
        {
            if ((trigger.name == null || trigger.name == "" || trigger.name == other.name)
                && (trigger.tag == null || trigger.tag == "" || trigger.tag == other.tag))
            {
                Trigger3DEnterEvent?.Invoke(trigger.type, TriggerEvent.Stay, other);
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        foreach (var trigger in triggers)
        {
            if (trigger.name == other.name || trigger.tag == other.tag)
            {
                Trigger3DEnterEvent?.Invoke(trigger.type, TriggerEvent.Exit, other);
            }
        }
    }
}

