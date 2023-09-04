using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float verticalInput, horizontalInput, brakingForce, steeringAngle;
    private bool isBreaking, autonomousMode = false;
    public NeuralNetwork network;
    private Rigidbody rb;
    private PathFinder pathFinder;
    private Vector3 destination, vectorToTarget;
    private Vector3 startPosition, startRotation;
    private List<Node> pathList;
    private int index = 0;
    private float minDistance = float.MaxValue, lastDistance = float.MaxValue, distanceToDestination = float.MaxValue;

    [SerializeField] private float motorForce, brakeForce, maxSteeringAngle;
    [SerializeField] private WheelCollider FLC, BLC, FRC, BRC;
    [SerializeField] private Transform FLT, BLT, FRT, BRT;

    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;

        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void Update() {

        if (Input.GetKeyUp(KeyCode.T))
        {
            autonomousMode = true;
        }

        if (Input.GetKeyUp(KeyCode.Q) && !autonomousMode)
        {
            destination = GameObject.FindGameObjectWithTag("destination").transform.position;
            pathFinder = new PathFinder(transform.position, destination, transform.forward);
            pathList = pathFinder.GetPath();
        }

        if (autonomousMode)
        {
            //startAutonomousCar();
            for (int i = 0; i < pathList.Count; i++)
            {
                Debug.DrawLine(pathList[i].leftFrontCornerPosition, pathList[i].rightFrontCornerPosition);
                Debug.DrawLine(pathList[i].rightFrontCornerPosition, pathList[i].rightBackCornerPosition);
                Debug.DrawLine(pathList[i].rightBackCornerPosition, pathList[i].leftBackCornerPosition);
                Debug.DrawLine(pathList[i].leftBackCornerPosition, pathList[i].leftFrontCornerPosition);

                Debug.DrawLine(pathList[i].leftFrontCornerPosition + Vector3.up, pathList[i].leftFrontCornerPosition + Vector3.down);
                Debug.DrawLine(pathList[i].rightFrontCornerPosition + Vector3.up, pathList[i].rightFrontCornerPosition + Vector3.down);
                Debug.DrawLine(pathList[i].rightBackCornerPosition + Vector3.up, pathList[i].rightBackCornerPosition + Vector3.down);
                Debug.DrawLine(pathList[i].leftBackCornerPosition + Vector3.up, pathList[i].leftBackCornerPosition + Vector3.down);
                if (i != pathList.Count - 1)
                    Debug.DrawLine(new Vector3(0, 3f, 0) + pathList[i].position, new Vector3(0, 3f, 0) + pathList[i + 1].position);
            }
            FollowWayPoints();
            DestinationHandler();
        }
        else
            InputHandler();

        CarMotionsHandler();
        SteeringHandler();
        UpdateWheels();
        
    }

    // --------------------------------------------------------------Car controller---------------------------------------------------------
    private void InputHandler()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void CarMotionsHandler()
    {
        FLC.motorTorque = verticalInput * motorForce;
        FRC.motorTorque = verticalInput * motorForce;

        brakingForce = isBreaking ? brakeForce : 0f;

        BrakingHandler();
    }

    private void BrakingHandler()
    {
        FLC.brakeTorque = brakingForce;
        FRC.brakeTorque = brakingForce;
        BLC.brakeTorque = brakingForce;
        BRC.brakeTorque = brakingForce;
    }

    private void SteeringHandler()
    {
        steeringAngle = maxSteeringAngle * horizontalInput;
        FLC.steerAngle = steeringAngle;
        FRC.steerAngle = steeringAngle;
    }

    private void UpdateWheels()
    {
        UpdateWheel(FLC, FLT);
        UpdateWheel(FRC, FRT);
        UpdateWheel(BLC, BLT);
        UpdateWheel(BRC, BRT);
    }

    private void UpdateWheel(WheelCollider collider, Transform wheelTransform)
    {

        collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
        wheelTransform.rotation = rotation;
        wheelTransform.position = position;
    }

    // --------------------------------------------------------------Autonomous controller---------------------------------------------------------
    private void FollowWayPoints()
    {
        if (index >= pathList.Count)
        {
            return;
        }

        Vector3 nextWayPoint = pathList[index].position;
        float distanceToWayPoint = (nextWayPoint - transform.position).magnitude;

        while (distanceToWayPoint < 3f && index < pathList.Count - 1)
        {
            nextWayPoint = pathList[++index].position;
            distanceToWayPoint = (nextWayPoint - transform.position).magnitude;
        }

        if (distanceToWayPoint > 10f || nextWayPoint == destination)
        {
            // Handling complte control over destination's handler if getting too close
            if (distanceToWayPoint > 2f)
            {
                vectorToTarget = (nextWayPoint - transform.position).normalized;
                vectorToTarget.y = transform.forward.y;
                
                float angle = Vector3.SignedAngle(transform.forward, vectorToTarget, Vector3.up);
                horizontalInput = Mathf.Clamp(angle / 45f, -1f, 1f);
                
                avoidObstacles();

                if (rb.velocity.magnitude > 10f)
                {
                    verticalInput = 0;
                }
                else if (rb.velocity.magnitude > 15f)
                {
                    isBreaking = true;
                }
                else
                {
                    verticalInput = (float)(1.1f - Mathf.Abs(horizontalInput) / 1.0);
                }
            }
        }
        else
            index++;
    }

    private void DestinationHandler()
    {

        distanceToDestination = (new Vector2(destination.x - transform.position.x, destination.z - transform.position.z)).magnitude;
        if (distanceToDestination < 1f)
        {
            verticalInput = 0f;
            horizontalInput = 0f;
            isBreaking = true;
            rb.velocity = new Vector3(0, 0, 0);
            autonomousMode = false;
            index = 0;
        }
        else if (distanceToDestination < 10f)
        {
            Vector3 desiredSpeed = distanceToDestination * rb.velocity.normalized;
            if (rb.velocity.magnitude > desiredSpeed.magnitude)
                rb.velocity = distanceToDestination * rb.velocity.normalized;
        }
    }


    public void saveTrainingData(float[] sensorInputs)
    {
        string inputs_path = "Assets/Script/Training_inputs.txt", output_path = "Assets/Script/Training_outputs.txt";
        File.Create(inputs_path).Close();

        StreamWriter writer = new StreamWriter(inputs_path, true);

        foreach (float input in sensorInputs)
        {
            writer.Write(input + " ");
        }
        writer.WriteLine();

        File.Create(output_path).Close();

        writer = new StreamWriter(output_path, true);

        writer.WriteLine(verticalInput.ToString() + " " + horizontalInput.ToString() + " " + isBreaking);
    }

    private float[] getSensorInputs()
    {
        float[] sensorInputs = new float[11];
        Vector3 frontCenterForward = transform.forward,
            frontLeft = transform.forward - transform.right,
            frontRight = transform.forward + transform.right,
            frontCenterLeft = transform.forward - 0.25f * transform.right,
            frontCenterRight = transform.forward + 0.25f * transform.right,
            backCenterBackward = -transform.forward,
            backLeft = -transform.forward - transform.right,
            backRight = -transform.forward + transform.right;

        Vector3[] sensors = new Vector3[] {frontCenterForward,
            backCenterBackward,
            frontLeft,
            frontRight,
            frontCenterLeft,
            frontCenterRight,
            backLeft,
            backRight
        };

        int i = 0;
        foreach (Vector3 sensor in sensors)
        {
            RaycastHit hit;
            float distanceToBlocker = 30, maxDistance = 30;
            Vector3 sensorPosition, sensorLine;

            if (i <= 1)
            {
                sensorPosition = transform.position + 2.5f * sensor;
            }
            else
            {
                sensorPosition = transform.position + 1.75f * sensor;
            }

            if (i == 4)
                sensorPosition.x -= 0.5f * transform.right.x;
            else if (i == 5)
                sensorPosition.x += 0.5f * transform.right.x;

            // Direct the sensors to look diagonally
            sensorLine = transform.position + maxDistance * sensor;

            if (Physics.Raycast(sensorPosition + new Vector3(0, 1, 0), sensor, out hit, maxDistance))
            {
                if (hit.transform.tag == "terrain" || hit.transform.tag == "blocker")
                {
                    distanceToBlocker = hit.distance;
                    Debug.DrawLine(sensorPosition, sensorLine, Color.red);
                }
            }
            else
            {
                Debug.DrawLine(sensorPosition, sensorLine, Color.green);
            }

            sensorInputs[i++] = distanceToBlocker;
        }

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        sensorInputs[i++] = rigidbody.velocity.magnitude;
        Vector3 directionalVector = destination - transform.position;
        sensorInputs[i++] = directionalVector.x;
        sensorInputs[i++] = directionalVector.z;

        return sensorInputs;
    }

    private void avoidObstacles()
    {
        float[] sensorInputs = getSensorInputs();
        if ((sensorInputs[0] < 7f) ||
            (sensorInputs[2] < 7f) ||
            (sensorInputs[3] < 7f) ||
            (sensorInputs[4] < 7f) ||
            (sensorInputs[5] < 7f))
        {
            horizontalInput = (sensorInputs[2] + sensorInputs[4]) > (sensorInputs[3] + sensorInputs[5]) ? -1 : 1;
        }
    }

    private void checkOnProgress()
    {
        if (distanceToDestination <= 2.0f)
            autonomousMode = false;
        if (distanceToDestination >= (lastDistance))
            Reset();
        else
            network.saveBestWeightsAndBiases();
    }

    private void Reset()
    {
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
        network.loadFromFile();
        network.mutate();
        startAutonomousCar();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (minDistance < distanceToDestination)
        {
            minDistance = distanceToDestination;
        }
        //Reset();
    }

    private void startAutonomousCar()
    {
        network = new NeuralNetwork();
        float[] sensorInputs = getSensorInputs();
        float[] decisions = network.forwardPass(sensorInputs);
        verticalInput = decisions[0] > 0 ? 1 : -1;
        horizontalInput = decisions[1] > 0 ? 1 : -1;
        isBreaking = decisions[2] > 0 ? true : false;
    }
}
