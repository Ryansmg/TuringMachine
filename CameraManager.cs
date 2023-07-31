using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static Vector3 velocity = Vector3.zero;
    public GameObject scripts;
    public float smoothness;
    public static bool moveCamera = true;
    // Update is called once per frame
    void LateUpdate()
    {
        if (!moveCamera) return;
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(scripts.GetComponent<Main>().currentIndex, 0, -10), ref velocity, smoothness);
    }
}
