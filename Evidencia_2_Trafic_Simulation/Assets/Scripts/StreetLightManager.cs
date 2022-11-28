using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetLightManager : MonoBehaviour
{
    public GameObject[] SL1;
    public GameObject[] SL2;
    public GameObject[] SL3;
    public GameObject[] SL4;

    public int Sl_status;
    public int S2_status;
    public int S3_status;
    public int S4_status;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < SL1.Length; i++){
            SL1[i].SetActive(false);
            SL2[i].SetActive(false);
            SL3[i].SetActive(false);
            SL4[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Street Light 1
        if(Sl_status == 1){
            SL1[0].SetActive(false);
            SL1[1].SetActive(false);
            SL1[2].SetActive(true);
        }
        
        else if(Sl_status == 2){
            SL1[0].SetActive(false);
            SL1[1].SetActive(true);
            SL1[2].SetActive(false);
        }
    
        else if(Sl_status == 3){
            SL1[0].SetActive(true);
            SL1[1].SetActive(false);
            SL1[2].SetActive(false);
        }
    
        // Street Light 2
        if(S2_status == 1){
            SL2[0].SetActive(false);
            SL2[1].SetActive(false);
            SL2[2].SetActive(true);
        }
        
        else if(S2_status == 2){
            SL2[0].SetActive(false);
            SL2[1].SetActive(true);
            SL2[2].SetActive(false);
        }
    
        else if(S2_status == 3){
            SL2[0].SetActive(true);
            SL2[1].SetActive(false);
            SL2[2].SetActive(false);
        }

        // Street Light 3
        if(S3_status == 1){
            SL3[0].SetActive(false);
            SL3[1].SetActive(false);
            SL3[2].SetActive(true);
        }
        
        else if(S3_status == 2){
            SL3[0].SetActive(false);
            SL3[1].SetActive(true);
            SL3[2].SetActive(false);
        }
    
        else if(S3_status == 3){
            SL3[0].SetActive(true);
            SL3[1].SetActive(false);
            SL3[2].SetActive(false);
        }

        // Street Light 4
        if(S4_status == 1){
            SL4[0].SetActive(false);
            SL4[1].SetActive(false);
            SL4[2].SetActive(true);
        }
        
        else if(S4_status == 2){
            SL4[0].SetActive(false);
            SL4[1].SetActive(true);
            SL4[2].SetActive(false);
        }
    
        else if(S4_status == 3){
            SL4[0].SetActive(true);
            SL4[1].SetActive(false);
            SL4[2].SetActive(false);
        }
    }
}
