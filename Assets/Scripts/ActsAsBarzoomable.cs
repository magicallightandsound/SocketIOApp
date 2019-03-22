using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActsAsBarzoomable : MonoBehaviour
{


    private Vector3 curPos;
    private Vector3 lastPos;
    private float t = 0.0f;

    [SerializeField]
    public string resourceName = null;

    [SerializeField]
    public bool reflect = false;

    private void Awake()
    {
        if (resourceName != null)
        {
            if (Main.main != null && reflect)
            {
               Main.main.SyncObject(resourceName, gameObject, true);
            }
            
        }
    }

    // Use this for initialization
    void Start()
    {
        curPos = transform.GetComponent<Rigidbody>().position;
        lastPos = curPos;

        if (Main.main != null && reflect)
        {
            Main.main.SyncObject(resourceName, gameObject, false, true);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        t = t + Time.deltaTime;
        if (t > 0.016f)
        {
            if (resourceName != null)
            {
                curPos = transform.GetComponent<Rigidbody>().position;
                if (curPos != lastPos)
                {
                    lastPos = curPos;
                    if (Main.main != null)
                    {
                        Main.main.SyncObject(resourceName, gameObject, false, false, true);
                    }
                    
                }
                
            }

            t = 0.0f;
        }


    }


    private void OnDestroy()
    {
        if (resourceName != null && reflect)
        {
            Main.main.SyncObject(resourceName, gameObject, false, false, false, true);
        }
    }
}