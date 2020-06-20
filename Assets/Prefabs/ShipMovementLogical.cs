using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityNightPool;

public class ShipMovementLogical : MonoBehaviour
{
    [SerializeField] private int _indexPlaneInPoll;

    [Header("Параметры коробля")]
    [SerializeField] private float _speedMoving;
    [SerializeField] private float _speedRotate;

    private void Update()
    {
        if (!Input.anyKey)
            return;
        MovementLogical();
        CheckLaunchPlan();
    }

    private void MovementLogical()
    {

        float shipMovingDirect = Input.GetAxis("ShipMoving");
        if (shipMovingDirect !=0)
        {
            Vector3 movementDirection = new Vector3(0, shipMovingDirect, 0);
            transform.Translate(movementDirection * _speedMoving * Time.deltaTime);
        }

        float shipRotateDirect = Input.GetAxis("ShipRotation");
        if (shipRotateDirect != 0)
        {
            Vector3 rotationDirect = new Vector3(0, 0, shipRotateDirect);
            transform.Rotate(rotationDirect * _speedRotate * Time.deltaTime);
        }
    }

    private void CheckLaunchPlan()
    {
        if (Input.GetButtonDown("LaunchPlane"))
        {
            PoolObject plane = PoolManager.Get(_indexPlaneInPoll);
            plane.transform.position = transform.position;
            plane.transform.rotation = transform.rotation;
            PlaneMovementLogical  planeMovementLogical = plane.GetComponent<PlaneMovementLogical>();
            AirTrafficController.AddPlaneInList(planeMovementLogical);
            planeMovementLogical.Init(transform);
        }
    }
}


