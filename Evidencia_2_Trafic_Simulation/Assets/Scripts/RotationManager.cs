using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnCollisionStay(Collision info)
    {
        Debug.Log("Collision Stay");
    }
    void OnCollisionEnter(Collision info)
    {
        Debug.Log("Collision Enter");
    }
    void OnTriggerEnter(Collider info)
    {
        Debug.Log("Trigger Enter");
    }
    void OnTriggerStay(Collider info)
    {
        Debug.Log("Trigger Stay");
    }
}
