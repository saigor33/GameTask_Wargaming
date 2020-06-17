using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMovementLogical : MonoBehaviour
{
    [SerializeField] private float _speedMoving;
    [SerializeField] private float _speedRotate;


    private void FixedUpdate()
    {
        if (!Input.anyKey)
            return;

       // MovementLogical();

    }

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
        if (Input.GetKeyDown("LaunchPlane"))
        {

        }
    }
}
