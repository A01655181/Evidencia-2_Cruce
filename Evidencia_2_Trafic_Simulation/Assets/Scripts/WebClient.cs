using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public struct CarInfo
{
    public int id;
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
    public GameObject[] carPrefabs;
    public GameObject ambPrefab;
    GameObject[] carsInstances;
    GameObject[] tfInstances;

    Step step;
    int[] ids;
    bool firstStep = true;
    bool received = false;
    float chrono = 0.0f;

    Vector3[] moveVects;
    public float scale = 2.0f;
    public float fps;

    int index_by_id(int id)
    {
        for (int i = 0; i < ids.Length; i++)
        {
            if (ids[i] == id)
                return i;
        }
        return -1;
    }

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
                
                if (!firstStep)
                {
                moveVects = new Vector3[carsInstances.Length];
                for (int i = 0; i < carsInstances.Length; i++)
                {
                    // Match id with car if it exists
                    int index = -1;
                    for (int j = 0; j < step.cars.Length; j++)
                    {
                        if (step.cars[j].id == ids[i])
                            index = j;
                    }
                    // If match exists then measure displacement
                    if (index > -1)
                    {
                        Vector3 goal = new Vector3(step.cars[index].pos[0], 0.5f, step.cars[index].pos[1]);
                        moveVects[i] = goal - carsInstances[i].transform.position;
                    }
                    else
                    {
                        moveVects[i] = Vector3.zero;
                    }
                }
                }
                else
                {
                    firstStep = false;
                }

                received = true;
                Debug.Log("<-------RESPONSE------->");
            }
        }
    }

    // Start is called before the first frame update
    async void Start()
    {
        carsInstances = new GameObject[0];
        #if UNITY_EDITOR
        StartCoroutine(SendData());
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        if (received)
        {
            // Assign positions and get movement vectors
            if (firstStep || chrono > 1 / fps)
            {
                received = false;
                chrono = 0.0f;
                foreach (GameObject car in carsInstances)
                {
                    Destroy(car, 0.0f);
                }
                carsInstances = new GameObject[step.cars.Length];
                ids = new int[step.cars.Length];
                for (int i = 0; i < carsInstances.Length; i++)
                {
                    Vector3 pos = new Vector3(step.cars[i].pos[0], 0.5f, step.cars[i].pos[1]);
                    Quaternion rot = new Quaternion(0,0,0,1);
                    if (step.cars[i].name == "Car")
                    {
                        carsInstances[i] = GameObject.Instantiate(carPrefabs[0], pos, rot);
                    }
                    else
                    {
                        carsInstances[i] = GameObject.Instantiate(ambPrefab, pos, rot);
                    }
                    ids[i] = step.cars[i].id;
                }
                #if UNITY_EDITOR
                StartCoroutine(SendData());
                #endif
            }
            // Move
            else
            {
                chrono += Time.deltaTime;
                for (int i = 0; i < carsInstances.Length; i++)
                {
                    carsInstances[i].transform.position += moveVects[i] * Time.deltaTime * fps;
                }
            }
        }
    }
}
