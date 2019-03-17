#region namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
#endregion
public class ViewAround : MonoBehaviour {

    public Transform pivot;
    public float     horizontalLookSpeed = 1.0f;
    // Use this for initialization
    void Start () {
        XRSettings.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
        float rotSPeed = 0;
        rotSPeed = Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * horizontalLookSpeed;
        transform.RotateAround(pivot.transform.position, Vector3.up, rotSPeed * Time.deltaTime);
    }
}
