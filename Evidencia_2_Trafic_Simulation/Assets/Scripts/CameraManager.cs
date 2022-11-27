using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject[] camArr;
    int n = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      if(Input.GetKeyDown(KeyCode.Space)){
        if(n == camArr.Length - 1){
            camArr[n].SetActive(false);
            camArr[0].SetActive(true);
            n = 0;
        }else{
            camArr[n].SetActive(false);
            camArr[n+1].SetActive(true);
            n = n + 1;
        }
      }  
    }
}
