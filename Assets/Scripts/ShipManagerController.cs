using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityNightPool;

namespace FlyBattels
{



    public class ShipManagerController : MonoBehaviour
    {
        [SerializeField] private int _indexPlaneInPoll;
        [SerializeField] private UsualJoystickContoller _joystickMovement;
        [SerializeField] private ButtonDisplayAndroid _btnStartFly;
        [SerializeField] private ManagerMultiplayer _managerMultiplayer;

        [Header("Параметры коробля")]
        [SerializeField] private float _speedMoving;
        [SerializeField] private float _speedRotate;
        [SerializeField] private float _cooldownLaunchPlane;
        [SerializeField] private int _maxPlaneInAir;
        [SerializeField] private float _distansUsualShoot;

        [Header("Стрельба")]
        [SerializeField] private LineRenderer _lineDirectionShoot;
        [SerializeField] private UsualJoystickContoller _joystickUsualShoot;

        private float _lastTimeLaunchPlane;
        private float _currentPlaneInAir;

        private bool _isShooting;

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
            if(_lineDirectionShoot == null)
                Debug.LogError($"Project({this}, _lineDirectionShoot): не добавлена объект, отрисовывающий направление тсрельбы");
            if (_joystickUsualShoot == null)
                Debug.LogError($"Project({this}, _joystickUsualShoot): Не добавлен джостик, отвечающий за наведение стрельбы коробля");

            ResetTargeting();

            _joystickMovement.OnChangePositionJoystick_ModeUsed += MovementLogical;
            _joystickMovement.OnDropJoystick += FinishMoventLogical;

            _joystickUsualShoot.OnChangePositionJoystick_ModeTargeting += TargetingUsualShoot;
            _joystickUsualShoot.OnChangePositionJoystick_ModeUsed += UsualShot;
            _joystickUsualShoot.OnDropJoystick += FinishTargeting;

            _btnStartFly.OnClickButton += StartFlyPlane;
        }


        private void OnDestroy()
        {
            _joystickMovement.OnChangePositionJoystick_ModeUsed -= MovementLogical;
            _joystickMovement.OnDropJoystick -= FinishMoventLogical;

            _joystickUsualShoot.OnChangePositionJoystick_ModeTargeting -= TargetingUsualShoot;
            _joystickUsualShoot.OnChangePositionJoystick_ModeUsed -= UsualShot;
           _joystickUsualShoot.OnDropJoystick -= FinishTargeting;

            _btnStartFly.OnClickButton -= StartFlyPlane;
        }

        #region Moving
        private void MovementLogical(Vector3 movementDirection)
        {
            _managerMultiplayer.InformMoveToDirection(movementDirection);
        }

        private void FinishMoventLogical()
        {
            _managerMultiplayer.InformFinishToMove();
        }

        public void MoveToPosition(Vector3 targetPosition)
        {
            StartCoroutine(MoveOnTime(GlobalDataSettings.TIME_TICK, targetPosition));
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
        #endregion

        #region Shooting
        /// <summary>
        /// Наведение обычного выстрела
        /// </summary>
        /// <param name="targetingDirection"></param>
        private void TargetingUsualShoot(Vector3 targetingDirection)
        {
            if(_isShooting)
                FinishUsualShoot();

            Vector3 firstPos = Vector3.zero;
            Vector3 secondPos = targetingDirection.normalized * _distansUsualShoot;
            Debug.Log($"Distance shot = {Vector3.Distance(secondPos, firstPos)}");
            _lineDirectionShoot.SetPosition(0, secondPos);
            _lineDirectionShoot.SetPosition(1, firstPos);
        }

        /// <summary>
        /// Событие при завершении наведения
        /// </summary>
        private void FinishTargeting()
        {
            FinishUsualShoot();
            ResetTargeting();
        }

        private void FinishUsualShoot()
        {
            _isShooting = false;
            _managerMultiplayer.InformFinishShoot(TypeShot.UsualShoot);
        }

        private void ResetTargeting()
        {
            Vector3 vectorZero = Vector3.zero;
            _lineDirectionShoot.SetPositions(new Vector3[] { vectorZero, vectorZero });
        }

        private void UsualShot(Vector3 targetingDirection)
        {
            TargetingUsualShoot(targetingDirection);
            _isShooting = true;
            _managerMultiplayer.InformShotToDirection(TypeShot.UsualShoot, targetingDirection);
        }

        public void SetDistansShooting(TypeShot typeShot, float distans)
        {
            switch (typeShot)
            {
                case TypeShot.UsualShoot:
                    {
                        _distansUsualShoot = distans;
                        break;
                    }
            }
        }


        #endregion





        #region OldCode


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

        #endregion
    }


}