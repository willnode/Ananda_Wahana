using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUIHook : MonoBehaviour
{
    public Camera target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            var g = GetComponent<RectTransform>();
            var min = g.localToWorldMatrix.MultiplyPoint(g.rect.min);
            var max = g.localToWorldMatrix.MultiplyPoint(g.rect.max);
            //var area = GetComponentInParent<Canvas>().pixelRect;
            target.pixelRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
