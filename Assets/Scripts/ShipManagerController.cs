using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityNightPool;

namespace FlyBattels
{



    public class ShipManagerController : MonoBehaviour
    {
        [SerializeField] private int _indexPlaneInPoll;
        [SerializeField] private JoystickContollerMovement _joystickMovement;

        [Header("Параметры коробля")]
        [SerializeField] private float _speedMoving;
        [SerializeField] private float _speedRotate;
        [SerializeField] private float _cooldownLaunchPlane;
        [SerializeField] private int _maxPlaneInAir;

        private float _lastTimeLaunchPlane;
        private float _currentPlaneInAir;

        private void Awake()
        {
            _lastTimeLaunchPlane = -_cooldownLaunchPlane;
            if (!PoolManager.CheckHaveIndexPrefab(_indexPlaneInPoll))
                Debug.LogError($"Project({this}, _indexPlaneInPoll): Такого индекса не существует в пуле объектов");
            if (_joystickMovement == null)
                Debug.LogError($"Project({this}, _joystickMovement): Не добавлен джостик, отвечающий за передвижение коробля");

            _joystickMovement.OnChangePositionJoystick += MovementLogical;
        }

        private void Update()
        {
            if (!Input.anyKey)
                return;
            //MovementLogical();
            CheckLaunchPlan();
        }

        private void OnDestroy()
        {
            _joystickMovement.OnChangePositionJoystick -= MovementLogical;
        }

        private void MovementLogical(Vector3 movementDirection)
        {
            transform.Translate(movementDirection.normalized * _speedMoving * Time.deltaTime);
        }

        /// <summary>
        /// Логика управления передвижение корабля
        /// </summary>
        private void MovementLogical()
        {
            float shipMovingDirect = Input.GetAxis("ShipMoving");
            if (shipMovingDirect != 0)
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

        /// <summary>
        /// Проверить вызов события запуска самолёта
        /// </summary>
        private void CheckLaunchPlan()
        {
            if (Input.GetButtonDown("LaunchPlane"))
            {
                if (Time.time - _lastTimeLaunchPlane > _cooldownLaunchPlane && _currentPlaneInAir < _maxPlaneInAir)
                {
                    _lastTimeLaunchPlane = Time.time;

                    PoolObject plane = PoolManager.Get(_indexPlaneInPoll);
                    plane.transform.position = transform.position;
                    plane.transform.rotation = transform.rotation;
                    PlaneMovementLogical planeMovementLogical = plane.GetComponent<PlaneMovementLogical>();
                    AirTrafficController.AddPlaneInList(planeMovementLogical);
                    planeMovementLogical.EventAfterFinishFly.AddListener(PlaneFinishFly);
                    _currentPlaneInAir++;
                    planeMovementLogical.Init(transform);
                }
            }
        }

        /// <summary>
        /// Событие, которое необходимо подписать на момент "уничтожения" самолёта.
        /// Освобождает ячейку свободного самолёта
        /// </summary>
        private void PlaneFinishFly()
        {
            _currentPlaneInAir--;
        }
    }


}