using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneMovementLogical : MonoBehaviour
{
    public enum StatePlane { BeginFly, Fly, FinishFly}

    [SerializeField] private float _radiusDistansShip;
    [SerializeField] private float _radiusDistansPlanes;
    [SerializeField] private float _minSpeed;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _maxAngleSpeed;
    [SerializeField] private float _timeFly;
    [SerializeField] private int _countFlyCircles;

    private StatePlane IsStatePlane { get; set; }
    private Transform _transformShip;
    private float _angle;

    private void Start()
    {
        
    }

    void Update()
    {
        switch(IsStatePlane)
        {
            case StatePlane.BeginFly:
                {
                    transform.Translate(Vector3.up * _minSpeed * Time.deltaTime);
                    if(Vector3.Distance(_transformShip.position, transform.position)>= _radiusDistansShip)
                    {
                        IsStatePlane = StatePlane.Fly;
                    }
                    break;
                }
            case StatePlane.Fly:
                {
                    float speedAngle = _countFlyCircles * 2 * Mathf.PI * _radiusDistansShip / _timeFly;
                    if (speedAngle > _maxAngleSpeed)
                        speedAngle = _maxAngleSpeed;

                    float speedLinear = speedAngle * _radiusDistansShip;
                    if (speedLinear > _maxSpeed)
                        speedLinear = _maxSpeed;

                    if (speedLinear< _minSpeed)
                        speedLinear = _minSpeed;

                    float posX = _transformShip.position.x + Mathf.Cos(_angle) * speedLinear;
                    float posY = _transformShip.position.y + Mathf.Sin(_angle) * speedLinear;
                    _angle = _angle + Time.deltaTime * _maxAngleSpeed;

                    Vector3 nextPos = new Vector3(posX, posY,0);

                    //TO DO: избавиться от перемещения-телепортации
                    transform.position = nextPos;

                    if (_angle >= 360)
                        _angle = 0;
                    break;
                }
            case StatePlane.FinishFly:
                {
                    break;
                }
        }
    }

    public void Init(Transform transformShip)
    {
        _transformShip = transformShip;
        IsStatePlane = StatePlane.BeginFly;
    }
}
