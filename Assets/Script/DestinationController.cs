
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationController : MonoBehaviour
{
    private RaycastHit hit;
    private bool circlePlaced = false;
    [SerializeField] Camera cam;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Q))
            circlePlaced ^= true;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.tag == "Untagged")
            {
                if (circlePlaced)
                    return;
                transform.position = hit.point;
            }
        }
    }
}
