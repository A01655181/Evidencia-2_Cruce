using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public struct CarInfo
{
    public float[] pos;
    public string name;
}
[Serializable]
public struct TrafficLights
{
    public float[] pos;
    public string status;
}
[Serializable]
struct Step
{
    public CarInfo[] cars;
    public TrafficLights[] tf;
}

public class WebClient : MonoBehaviour
{
    public GameObject[] cars;
    public GameObject[] tfs;

    Step step;
    bool firstStep = true;
    bool received = false;
    float chrono = 0.0f;
    Vector3[] moveVects;
    public float scale = 2.0f;

    // IEnumerator - yield return
    IEnumerator SendData()
    {
        WWWForm form = new WWWForm();
        form.AddField("bundle", "the data");
        //string url = "http://localhost:8585";
        string url = "http://localhost:8000/";

        //string url = "https://tec-server.mybluemix.net/api/points";
        //string url = "https://tec-server-demo.mybluemix.net/api/points";
        //using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes("");
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();          // Talk to Python
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                step = JsonUtility.FromJson<Step>(www.downloadHandler.text);
                Debug.Log("<-------RESPONSE------->");
                if (!firstStep)
                {
                    moveVects = new Vector3[cars.Length];
                    for (int i = 0; i < cars.Length; i++)
                    {
                        Vector3 goal = new Vector3(step.cars[i].pos[0] * scale, 0.5f, step.cars[i].pos[1] * scale);
                        Vector3 displacement = goal - cars[i].transform.position;
                        if (displacement.magnitude > 3.0f * scale)
                        {
                            displacement.Normalize();
                            cars[i].transform.position = goal + displacement * 3;
                            displacement = goal - cars[i].transform.position;
                        }
                        moveVects[i] = displacement;
                    }
                }
                received = true;
            }
        }
    }

    // Start is called before the first frame update
    async void Start()
    {
        #if UNITY_EDITOR
        StartCoroutine(SendData());
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        if (firstStep)
        {
            if (received)
            {
                firstStep = false;
                for (int i = 0; i < tfs.Length; i++)
                {
                    Vector3 pos = new Vector3(step.tf[i].pos[0] * scale, 2.5f, step.tf[i].pos[1] * scale);
                    tfs[i].transform.position = pos;
                    if (step.tf[i].status == "red")
                    {
                        tfs[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
                    }
                    else if (step.tf[i].status == "yellow")
                    {
                        tfs[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.yellow);
                    }
                    else if (step.tf[i].status == "green")
                    {
                        tfs[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
                    }
                }
                for (int i = 0; i < cars.Length; i++)
                {
                    Vector3 pos = new Vector3(step.cars[i].pos[0] * scale, 0.5f, step.cars[i].pos[1] * scale);
                    cars[i].transform.position = pos;
                }
                #if UNITY_EDITOR
                StartCoroutine(SendData());
                #endif
            }
            return;
        }
        if (received)
        {
            // Debug.Log("Chrono: " + chrono);
            chrono += Time.deltaTime;
            double diff = 0.0f;
            for (int i = 0; i < cars.Length; i++)
            {
                cars[i].transform.position += moveVects[i] * Time.deltaTime;
            }
            if (chrono > 1.0f)
            {
                received = false;
                for (int i = 0; i < tfs.Length; i++)
                {
                    if (step.tf[i].status == "red")
                    {
                        tfs[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
                    }
                    else if (step.tf[i].status == "yellow")
                    {
                        tfs[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.yellow);
                    }
                    else if (step.tf[i].status == "green")
                    {
                        tfs[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
                    }
                }
                for (int i = 0; i < cars.Length; i++)
                {
                    Vector3 pos = new Vector3(step.cars[i].pos[0] * scale, 0.5f, step.cars[i].pos[1] * scale);
                    cars[i].transform.position = pos;
                }
                #if UNITY_EDITOR
                StartCoroutine(SendData());
                #endif
                chrono = 0.0f;
            }
        }
    }
}
