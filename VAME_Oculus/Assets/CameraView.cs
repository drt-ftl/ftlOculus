using UnityEngine;
using System.Collections;

public class CameraView : MonoBehaviour
{
    float scaleRot = 0.002f;
    Vector3 lastMp;


	void Start () {
	
	}
	
	void Update ()
    {
        if (camScript.mainMenu.active)
        {
            lastMp = Input.mousePosition;
            return;
        }
        var mp = Input.mousePosition;
        var xFactor = (mp.x - lastMp.x) * scaleRot;
        var yFactor = -(mp.y - lastMp.y) * scaleRot;

        if (Input.GetMouseButton(0))
        {
            transform.RotateAround(transform.up, xFactor);
            transform.RotateAround(transform.right, yFactor);
        }
        var zoom = Input.mouseScrollDelta.y * 0.3f;
        var pos = GameObject.Find("Camera Rig").transform.localPosition;
        pos.z += zoom;
        GameObject.Find("Camera Rig").transform.localPosition = pos;
        lastMp = mp;
    }
}
