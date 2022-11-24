using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public struct CarInfo
{
    public int[] pos;
    public string name;
}
[Serializable]
public struct TrafficLights
{
    public int[] pos;
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
    List<List<Vector3>> positions;
    public GameObject[] cars;
    public GameObject[] tfs;
    public float timeToUpdate = 5.0f;
    private float timer;
    public float dt;
    int numPositions;
    int currPosition;
    Step step;
    bool firstStep = true;
    bool received = false;
    float chrono = 0.0f;
    Vector3[] prevPositions;
    Vector3[] moveVects;

    // IEnumerator - yield return
    IEnumerator SendData(string data)
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
                        Vector3 goal = new Vector3(step.cars[i].pos[0], 0.5f, step.cars[i].pos[1]);
                        Vector3 displacement = goal - cars[i].transform.position;
                        if (displacement.magnitude > 3.0f)
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
        Vector3 fakePos = new Vector3(3.44f, 0, -15.707f);
        string json = EditorJsonUtility.ToJson(fakePos);
        //StartCoroutine(SendData(call));
        StartCoroutine(SendData(json));
        timer = timeToUpdate;
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
                for (int i = 0; i < cars.Length; i++)
                {
                    Vector3 pos = new Vector3(step.cars[i].pos[0], 0.5f, step.cars[i].pos[1]);
                    cars[i].transform.position = pos;
                }
                #if UNITY_EDITOR
                timer = timeToUpdate; // reset the timer
                Vector3 fakePos = new Vector3(3.44f, 0, -15.707f);
                string json = EditorJsonUtility.ToJson(fakePos);
                StartCoroutine(SendData(json));
                #endif
            }
            return;
        }

        if (received)
        {
            // Debug.Log("Chrono: " + chrono);
            double diff = 0.0f;
            for (int i = 0; i < cars.Length; i++)
            {
                cars[i].transform.position += moveVects[i] * Time.deltaTime;
                diff += (new Vector3(step.cars[i].pos[0], 0.5f, step.cars[i].pos[1]) - cars[i].transform.position).magnitude;
            }
            diff = diff / cars.Length;
            if (diff < 0.015)
            {
                received = false;
                for (int i = 0; i < cars.Length; i++)
                {
                    Vector3 pos = new Vector3(step.cars[i].pos[0], 0.5f, step.cars[i].pos[1]);
                    cars[i].transform.position = pos;
                }
                #if UNITY_EDITOR
                timer = timeToUpdate; // reset the timer
                Vector3 fakePos = new Vector3(3.44f, 0, -15.707f);
                string json = EditorJsonUtility.ToJson(fakePos);
                StartCoroutine(SendData(json));
                #endif
            }
        }
    }
}
