using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

        float speed = 50f;
        if (carCameraSwitched)
        {
            TranslationHandler();
            RotationHandler();
        }
        else
        {

            if (Input.GetMouseButtonDown(0))
            {
                dragOrigin = Input.mousePosition;
                return;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
                Vector3 move = -2f * new Vector3(pos.x, 0, pos.y);
                Vector3 moveDir = move.z * transform.forward + move.x * transform.right;
                moveDir.y = 0f;

                transform.Translate(moveDir, Space.World);
            }

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                transform.localEulerAngles += speed * new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f) * Time.deltaTime;
            }

            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                transform.Translate(speed * transform.forward * Time.deltaTime, Space.World);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                transform.Translate(speed * -transform.forward * Time.deltaTime, Space.World);
            }
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
