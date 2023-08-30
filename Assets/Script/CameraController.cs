using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CameraController : MonoBehaviour
{
    private Vector3 dragOrigin;
    private bool carCameraSwitched = false;

    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform target;
    [SerializeField] private float translateSpeed;
    [SerializeField] private float rotationSpeed;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.X))
            carCameraSwitched ^= true;
    }

    private void FixedUpdate() {

        if (carCameraSwitched) {
            TranslationHandler();
            RotationHandler();
        }
        else {

            float targetZoom = Camera.main.orthographicSize,
            scroll = Input.GetAxis("Mouse ScrollWheel"),
            velocity = 0;
            targetZoom -= Mathf.Clamp(3 * scroll, 2, 8);
            Camera.main.orthographicSize = Mathf.SmoothDamp(Camera.main.orthographicSize, targetZoom, ref velocity, 0.25f);

            if (Input.GetMouseButtonDown(0))
            {
                dragOrigin = Input.mousePosition;
                return;
            }
    
            if (!Input.GetMouseButton(0))
                return;
    
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(-pos.x*0.75f, 0, -pos.y*0.75f);
    
            transform.Translate(move, Space.World);
        }
    }

    private void TranslationHandler() {
        var targetPosition = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, translateSpeed*Time.deltaTime);
    }

    private void RotationHandler() {
        var direction = target.position - transform.position;
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed*Time.deltaTime);
    }
}
