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
        [SerializeField] private ButtonDisplayAndroid _btnStartFly;
        [SerializeField] private ManagerMultiplayer _managerMultiplayer;

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
            if (_btnStartFly == null)
                Debug.LogError($"Project({this}, _btnStartFly): не добавлена кнопка, отвечающая за вылет самолётов");
            if(_managerMultiplayer==null)
                Debug.LogError($"Project({this}, _managerMultiplayer): не добавлена объект, осуществляющий взаимодейстие с сервером");

            _joystickMovement.OnChangePositionJoystick += MovementLogical;
            _joystickMovement.OnDropJoystick += FinishMoventLogical;
            _btnStartFly.OnClickButton += StartFlyPlane;
        }

        //private void Update()
        //{
        //    if (!Input.anyKey)
        //        return;
        //    //MovementLogical();
        //    //CheckLaunchPlan();
        //}

        private void OnDestroy()
        {
            _joystickMovement.OnChangePositionJoystick -= MovementLogical;
            _joystickMovement.OnDropJoystick -= FinishMoventLogical;
            _btnStartFly.OnClickButton -= StartFlyPlane;
        }

        private void MovementLogical(Vector3 movementDirection)
        {
            //_managerMultiplayer.SendMessageOnServer_MoveToPoint(movementDirection);
            _managerMultiplayer.InformMoveToDirection(movementDirection);
            //transform.Translate(movementDirection.normalized * _speedMoving * Time.deltaTime);
        }

        private void FinishMoventLogical()
        {
            _managerMultiplayer.InformFinishToMove();
        }

        public void MoveToPosition(Vector3 targetPosition)
        {
            //Vector3 targetPos = new Vector3 (transform.position.x + position.x, transform.position.y + position.y,0) ;
             StartCoroutine(MoveOnTime(GlobalDataSettings.TIME_TICK, targetPosition));
            //transform.Translate(position);
        }

        private IEnumerator MoveOnTime(float time, Vector2 targetPosition)
        {
            Vector2 startPosition = transform.position;
            float startTime = Time.realtimeSinceStartup;
            float fraction = 0f;
            while (fraction < 1f)
            {
                fraction = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / time);
                transform.position = Vector2.Lerp(startPosition, targetPosition, fraction);
                yield return null;
            }
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

        private void StartFlyPlane()
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