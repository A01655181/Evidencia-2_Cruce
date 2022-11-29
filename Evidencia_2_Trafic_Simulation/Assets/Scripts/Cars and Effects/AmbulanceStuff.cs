using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbulanceStuff : MonoBehaviour
{
    public Light Siren;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Siren.intensity = Mathf.PingPong(Time.time, 4);   
    }
}
