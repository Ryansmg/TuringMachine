using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static Vector3 _velocity = Vector3.zero;
    public GameObject scripts;
    public float smoothness;
    public static bool moveCamera = true;

    private Main _main;

    // Update is called once per frame
    private void Start()
    {
        _main = scripts.GetComponent<Main>();
    }

    private void LateUpdate()
    {
        if (!moveCamera) return;
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(_main.currentIndex, 0, -10), ref _velocity, smoothness);
    }
}
