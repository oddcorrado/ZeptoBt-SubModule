using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeptoBtTrigger : MonoBehaviour
{
    [System.Serializable]
    public class TriggerFilter
    {
        public string name;
        public string tag;
    }
    public enum TriggerEvent { Enter, Exit }
    [SerializeField] string triggerType;
    [SerializeField] TriggerFilter[] filters;

    public delegate void TriggerEnterDelegate(string triggerType, TriggerEvent triggEvent, Collider2D other);
    public event TriggerEnterDelegate TriggerEnterEvent;

    void OnTriggerEnter2D(Collider2D other)
    {
        if(filters.Length == 0) TriggerEnterEvent?.Invoke(triggerType, TriggerEvent.Enter, other);
        else
        {
            foreach(var filter in filters)
            {
                if ((filter.name == null || filter.name == "" || filter.name == other.name)
                    && (filter.tag == null || filter.tag == "" || filter.tag == other.tag))
                {
                    TriggerEnterEvent?.Invoke(triggerType, TriggerEvent.Enter, other);
                    return;
                }
            }
        }   
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (filters.Length == 0) TriggerEnterEvent?.Invoke(triggerType, TriggerEvent.Enter, other);
        else
        {
            foreach (var filter in filters)
            {
                if ((filter.name == null || filter.name == "" || filter.name == other.name)
                    && (filter.tag == null || filter.tag == "" || filter.tag == other.tag))
                {
                    TriggerEnterEvent?.Invoke(triggerType, TriggerEvent.Exit, other);
                    return;
                }
            }
        }
    }
}
