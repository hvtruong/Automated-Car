using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    private float horizontalInput, verticalInput, brakingForce, steeringAngle;
    private bool isBreaking;

    [SerializeField] private float motorForce, brakeForce, maxSteeringAngle;
    [SerializeField] private WheelCollider FLC, BLC, FRC, BRC;
    [SerializeField] private Transform FLT, BLT, FRT, BRT;

    private void FixedUpdate() {
        InputHandler();
        CarMotionsHandler();
        SteeringHandler();
        UpdateWheels();
    }

    private void InputHandler() {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void CarMotionsHandler() {
        FLC.motorTorque = verticalInput*motorForce;
        FRC.motorTorque = verticalInput*motorForce;
        brakingForce = isBreaking? brakeForce : 0f;

        BrakingHandler();
    }

    private void BrakingHandler() {
        FLC.brakeTorque = brakingForce;
        FRC.brakeTorque = brakingForce;
        BLC.brakeTorque = brakingForce;
        BRC.brakeTorque = brakingForce;
    }

    private void SteeringHandler() {
        steeringAngle = maxSteeringAngle*horizontalInput;
        FLC.steerAngle = steeringAngle;
        FRC.steerAngle = steeringAngle;
    }

    private void UpdateWheels() {
        UpdateWheel(FLC, FLT);
        UpdateWheel(FRC, FRT);
        UpdateWheel(BLC, BLT);
        UpdateWheel(BRC, BRT);
    }

    private void UpdateWheel(WheelCollider collider, Transform wheelTransform) {

        collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
        wheelTransform.rotation = rotation;
        wheelTransform.position = position;
        
    }
}
