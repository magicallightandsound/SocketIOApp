using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script1 : MonoBehaviour {

    private Vector3 curPos;
    private Vector3 lastPos;
    private float t = 0.0f;

    [SerializeField]
    public string resourceName = null;

    // Use this for initialization
    void Start () {
        curPos = transform.GetComponent<Rigidbody>().position;
        lastPos = curPos;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void Awake()
    {
        if (resourceName != null)
        {
            if (Main.main)
            {
                Main.main.SyncObject(resourceName, gameObject, true);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        t = t + Time.deltaTime;
        if (t > 1.0f)
        {
            if (resourceName != null)
            {
                curPos = transform.GetComponent<Rigidbody>().position;
                if (curPos != lastPos)
                {
                    lastPos = curPos;
                    Main.main.SyncObject(resourceName, gameObject, false, true);
                }

            }

            t = 0.0f;
        }


    }

    private void OnDestroy()
    {
        if (resourceName != null)
        {
            Main.main.SyncObject(resourceName, gameObject, false, false, true);
        }
    }
}
