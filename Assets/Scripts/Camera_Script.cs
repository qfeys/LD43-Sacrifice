using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Script : MonoBehaviour {

    public Transform player;
    Camera camera;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update () {
        Vector3 mouse = camera.ScreenToWorldPoint(Input.mousePosition).normalized * 2;
        transform.position = player.position + mouse + new Vector3(0, 0, -10);
	}
}
