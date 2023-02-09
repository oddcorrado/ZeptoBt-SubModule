using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeptoBtTrigger : MonoBehaviour
{
    public enum TriggerType { Ground, Wall, Sight, Alarm, Attack, Shoot, Flee, Fall, Bullet, Player, Dodge }
    public enum TriggerEvent { Enter, Exit }
    [SerializeField] TriggerType triggerType;
    [SerializeField] string triggerTag;

    public delegate void TriggerEnterDelegate(TriggerType triggerType, TriggerEvent triggEvent, Collider2D other);
    public event TriggerEnterDelegate TriggerEnterEvent;

    void OnTriggerEnter2D(Collider2D other)
    {
        if(triggerTag == null || triggerTag == "" || triggerTag == other.tag)
            TriggerEnterEvent?.Invoke(triggerType, TriggerEvent.Enter, other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (triggerTag == null || triggerTag == "" || triggerTag == other.tag)
            TriggerEnterEvent?.Invoke(triggerType, TriggerEvent.Exit, other);
    }
}
