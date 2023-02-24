using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ZeptoBt;

public class ZeptoBtDoc : MonoBehaviour
{
    [SerializeField] TMP_Text prototype;
    [SerializeField] TMP_Text description;
    [SerializeField] TMP_Text paramPrefab;

    List<TMP_Text> parameters = new List<TMP_Text>();
    public Doc Doc
    {
        set
        {
            prototype.text = value.prototype;
            description.text = value.description;
            parameters.RemoveAll(p => { Destroy(p.gameObject); return true; });
            if(value.parameters != null)
            {
                foreach (var p in value.parameters)
                {
                    var t = Instantiate(paramPrefab);
                    t.text = $"<#00eeff><b>{p.name}:</b> <#0099ff>{p.description}";
                    t.transform.SetParent(transform);
                    parameters.Add(t);
                }
            }
        }
    }
}
