using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityNightPool;
using TMPro;

public class PlaneMovementLogical : MonoBehaviour
{
    public enum StatePlane { BeginFly, Fly, FinishFly }

    [Header("Отображение текущих параметров")]
    [SerializeField] private bool _showParametrOnDisplay;
    [SerializeField] private TextMeshPro _textNumberPlane;
    [SerializeField] private TextMeshPro _textCurrentPosPlane;
    [SerializeField] private TextMeshPro _textNextPosPlane;
    [SerializeField] private TextMeshPro _textSpeedPlane;

    [Space]
    [Header("Параметры самолёта")]
    [SerializeField] private float _radiusDistansShip;
    [SerializeField] private float _radiusDistansPlanes;
    [SerializeField] private float _minSpeed;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _maxAngleSpeed;

    [SerializeField] private float _timeFly;
    [SerializeField] private float _countFlyCircles;

    [SerializeField] private float _timeLive;
    [SerializeField] private float _angleSectorLanding;


    private PoolObject _pool;
    private Transform _transformShip;
    private Coroutine _coroutineResetSpeed;
    private int _currentUnicalNumberPlane;
    private float _timeStartFinishedFly;
    private float _currentSpeed;
    private float _currentNormalSpeed;
    private Vector3 _nextPositionAfterNumbersFrames;

    private StatePlane CurrentStatePlane { get; set; }
    public UnityEvent EventAfterFinishFly { get; } = new UnityEvent();

    /// <summary>
    /// Позиция самолёта, через CountFrameСalculatingNextPosition кадров.
    /// </summary>
    public Vector3 NextPositionAfterNumbersFrames
    {
        get { return _nextPositionAfterNumbersFrames; }
        private set
        {
            _nextPositionAfterNumbersFrames = value;
            if (_showParametrOnDisplay)
                ShowCurrentParametrsOnDisplay();
        }
    }

    private void ShowCurrentParametrsOnDisplay()
    {
        if (CurrentSpeed == _currentNormalSpeed)
        {
            _textSpeedPlane.text = $"Speed: <color=white>{CurrentSpeed}</color>";
        }
        else
        {
            if (CurrentSpeed > _currentNormalSpeed)
                _textSpeedPlane.text = $"Speed: <color=yellow>{CurrentSpeed}</color>";
            else
                _textSpeedPlane.text = $"Speed: <color=red>{CurrentSpeed}</color>";
        }

        _textSpeedPlane.text += $"  <color=white>[{_minSpeed} , {_maxSpeed}]</color>";

        _textCurrentPosPlane.text = $"Pos:{transform.position}";
        _textNextPosPlane.text = $"NextPos({CountFrameСalculatingNextPosition}):{_nextPositionAfterNumbersFrames}";
    }


    public int GetNumberPlane { get { return _currentUnicalNumberPlane; } }
    /// <summary>
    /// Номер следующего кадра, для которого расчитывается NextPositionAfterNumbersFrames
    /// </summary>
    public uint CountFrameСalculatingNextPosition { get; set; } = 1;

    /// <summary>
    /// Получить радиус дистанции, на которой самолёт должен держаться от других самолётов
    /// </summary>
    public float GetRadiusDistansPlanes { get { return _radiusDistansPlanes; } }

    /// <summary>
    /// Получить текущую скорость объекта
    /// </summary>
    public float CurrentSpeed 
    { 
        get { return _currentSpeed; } 
        private set { _currentSpeed = CorrectSpeedLine(value); }
    }

    /// <summary>
    /// Получить максимальную скорость объекта
    /// </summary>
    public float GetMaxSpeed { get { return _maxSpeed; } }

    /// <summary>
    /// Получить минимимальную скорость объекта
    /// </summary>
    public float GetMinSpeed { get { return _minSpeed; } }

    private void Awake()
    {
        _pool = GetComponent<PoolObject>();
        if (_pool == null)
            Debug.LogError($"Project({this}, _pool): Не добавлен объект PoolObject");
        if(_textNumberPlane == null)
            Debug.LogError($"Project({this}, _textNumberPlane): Не добавлен объект TextMeshPro, для отображения номера самолёта");
        if (_countFlyCircles ==0)
            Debug.LogError($"Project({this}, _countFlyCircles): Самолёт не может не совершать кругов");

        _textSpeedPlane.text = "";
        _textCurrentPosPlane.text = "";
        _textNextPosPlane.text = "";
    }

    void Update()
    {
        MovingPlane();
    }

    public void Init(Transform transformShip)
    {
        _transformShip = transformShip;
        CurrentStatePlane = StatePlane.BeginFly;

        _currentUnicalNumberPlane = GenerateUnicalNumberPlane.GenerateUnicalNumberForPlane;
        _textNumberPlane.text = _currentUnicalNumberPlane.ToString();
        gameObject.name = _currentUnicalNumberPlane.ToString();

        _currentNormalSpeed = (_maxSpeed + _minSpeed) / 2f;
        CurrentSpeed = _currentNormalSpeed;
        StartCoroutine(FinishFlyAfterTime(_timeFly));
    }


    private void MovingPlane()
    {
        switch (CurrentStatePlane)
        {
            case StatePlane.BeginFly:
                {
                    BeginFly();
                    break;
                }
            case StatePlane.Fly:
                {
                    FlyPlaneCircle(_transformShip.position, CurrentSpeed, Vector3.forward);
                    break;
                }
            case StatePlane.FinishFly:
                {
                    //FlyReturnOnShip();
                    FinishFly();
                    break;
                }
        }
    }

    /// <summary>
    /// Стадия начала полёта. Самолёт летит до назначенной дистанции от коробля
    /// </summary>
    private void BeginFly()
    {
        FlyPlaneLine(CurrentSpeed, Vector3.up, Space.Self, false);
        if (Vector3.Distance(_transformShip.position, transform.position) >= _radiusDistansShip)
        {
            _currentNormalSpeed = GetSpeedForCircle(_countFlyCircles, _radiusDistansShip, _timeFly);
            CurrentSpeed = _currentNormalSpeed;
            CurrentStatePlane = StatePlane.Fly;
        }
    }

    /// <summary>
    /// Стадия завершения полёта. Самолёт ищет короткий путь на окружности к задней части коробля. 
    /// При поподании нахождение в секторе посадке начинает двигаться к центру.
    /// </summary>
    private void FlyReturnOnShip()
    {
        //TO DO: не изменяется скорость

        Vector3 movementDirection = _transformShip.position - transform.position;
        float angleBeetweenPlaneAndShip = GetAngleBeetweenTwoPoints(_transformShip.position, transform.position);
        float angleRotatePlaneOnShip = GetVerticalAngleShipRotate();

        if (angleBeetweenPlaneAndShip == angleRotatePlaneOnShip)
        {
            //проверяем находится ли корабль между текущем положением и положением в следующем кадре.
            //Если да, то считаем, что самолёт вернулся на корабль
            //Vector3 nexPos = GetNextPosFlyPlaneLine(_currentSpeed, movementDirection);
            Vector3 nexPos = GetNextPosFlyPlaneLine(CurrentSpeed, movementDirection);
            if (CheckPointBetweenTwoPoints(transform.position, nexPos, _transformShip.position))
                FinishFly();

            //FlyPlaneLine(_currentSpeed, movementDirection, Space.World);
            FlyPlaneLine(CurrentSpeed, movementDirection, Space.World);
        }
        else
        {
            //изменяем скорость в зависимости от оставшегося времени т.к. корабль может двигаться и увеличивать растояние
            // TO DO: не учитывается путь до корабля (не понятно что делать, если время закончиться т.к. корабль может уплыть)
            float lastTime = (_timeLive - _timeFly) - (Time.deltaTime - _timeStartFinishedFly);
            //float speedLinear = GetSpeedForCircle(_countFlyCircles, _radiusDistansShip, lastTime);
            CurrentSpeed = GetSpeedForCircle(_countFlyCircles, _radiusDistansShip, lastTime);


            //float nextAngleBeetweenPlaneAndShip = angleBeetweenPlaneAndShip + speedLinear;
            float nextAngleBeetweenPlaneAndShip = angleBeetweenPlaneAndShip + CurrentSpeed;

            //подгоняем угол поворота самолёта под угол приземление в случае, если он проскочит угол приземления в следующем кадре
            if (angleBeetweenPlaneAndShip < angleRotatePlaneOnShip && nextAngleBeetweenPlaneAndShip > angleRotatePlaneOnShip)
            {
                //speedLinear = CorrectSpeedLine(angleRotatePlaneOnShip - angleBeetweenPlaneAndShip);
                //nextAngleBeetweenPlaneAndShip = angleBeetweenPlaneAndShip + speedLinear;
                CurrentSpeed = CorrectSpeedLine(angleRotatePlaneOnShip - angleBeetweenPlaneAndShip);
                nextAngleBeetweenPlaneAndShip = angleBeetweenPlaneAndShip + CurrentSpeed;
            }

            float minAngleSectorLanding = angleRotatePlaneOnShip - _angleSectorLanding / 2f;
            float maxAngleSectorLanding = angleRotatePlaneOnShip + _angleSectorLanding / 2f;

            //Проверяем проскочит ли самолёт сектор приземления в следующем кадре или он находится в секторе приземление. 
            //Если да, то двигаем по прямой к кораблю 
            if ((angleBeetweenPlaneAndShip < minAngleSectorLanding && nextAngleBeetweenPlaneAndShip > maxAngleSectorLanding)
                || (angleBeetweenPlaneAndShip >= minAngleSectorLanding && angleBeetweenPlaneAndShip <= maxAngleSectorLanding))
            {
                //проверяем находится ли корабль между текущем положением и положением в следующем кадре.
                //Если да, то считаем, что самолёт вернулся на корабль
                //Vector3 nexPos = GetNextPosFlyPlaneLine(_currentSpeed, movementDirection);
                Vector3 nexPos = GetNextPosFlyPlaneLine(CurrentSpeed, movementDirection);
                if (CheckPointBetweenTwoPoints(transform.position, nexPos, _transformShip.position))
                    FinishFly();

                FlyPlaneLine(_currentSpeed, movementDirection, Space.World);
            }

            //Определяем направление с минимальным растоянием до сектора преземления
            Vector3 directionMinDistanseToFinish = Vector3.zero;
            if (maxAngleSectorLanding - angleBeetweenPlaneAndShip < 0)
                directionMinDistanseToFinish = Vector3.back;

            if (minAngleSectorLanding - angleBeetweenPlaneAndShip > 0)
                directionMinDistanseToFinish = Vector3.forward;

            if (directionMinDistanseToFinish != Vector3.zero)
                FlyPlaneCircle(_transformShip.position, CurrentSpeed, directionMinDistanseToFinish);
                //FlyPlaneCircle(_transformShip.position, speedLinear, directionMinDistanseToFinish);
        }
    }


    #region FlyCircle
    /// <summary>
    /// Перемещение объекта по кругу
    /// </summary>
    /// <param name="сentralRotationPoint"></param>
    /// <param name="speedLinear"></param>
    /// <param name="directionMinDistanseToFinish"></param>
    private void FlyPlaneCircle(Vector3 сentralRotationPoint, float speedLinear, Vector3 directionMinDistanseToFinish)
    {
        Vector3 nextPos = GetNextPosFlyPlaneCircle(сentralRotationPoint, speedLinear, directionMinDistanseToFinish);
        RotatePlaneToNextPoint(nextPos);

        transform.RotateAround(сentralRotationPoint, directionMinDistanseToFinish, speedLinear);
        NextPositionAfterNumbersFrames = GetNextPosFlyPlaneCircle(сentralRotationPoint, speedLinear, directionMinDistanseToFinish, CountFrameСalculatingNextPosition);
    }

    /// <summary>
    /// Получить следующую точку перемещения на окружности, при вращении по оси Z
    /// </summary>
    /// <param name="сentralRotationPoint"></param>
    /// <param name="speedLinear"></param>
    /// <param name="directionMinDistanseToFinish"></param>
    /// <returns></returns>
    private Vector3 GetNextPosFlyPlaneCircle(Vector3 сentralRotationPoint, float speedLinear, Vector3 directionMinDistanseToFinish, uint countFrame = 1)
    {
        // устанавливаем смещение координат в 0 т.к. позицию на окружности нужно считать без смещения 
        float currentAngle = GetAngleBeetweenTwoPoints(сentralRotationPoint, transform.position, 0);
        float nexAngle = currentAngle;
        for (uint i = 0; i < countFrame; i++)
        {
            nexAngle = nexAngle + speedLinear * directionMinDistanseToFinish.z;
        }
        float radius = Vector3.Distance(transform.position, сentralRotationPoint);
        //Debug.Log($"currentAngle= {currentAngle}, nexAngle= {nexAngle} r= {radius} cent={сentralRotationPoint}");
        float posX = сentralRotationPoint.x + Mathf.Cos(nexAngle * Mathf.PI / 180) * radius;
        float posY = сentralRotationPoint.y + Mathf.Sin(nexAngle * Mathf.PI / 180) * radius;

        return new Vector3(posX, posY, 0);
    }

    /// <summary>
    /// Получить скорость полёта по окружности
    /// </summary>
    /// <param name="countCircle"></param>
    /// <param name="radiusCircle"></param>
    /// <param name="timeOnAllCircle"></param>
    /// <returns></returns>
    private float GetSpeedForCircle(float countCircle, float radiusCircle, float timeOnAllCircle)
    {
        float speedAngle = countCircle * 2 * Mathf.PI * radiusCircle / timeOnAllCircle;
        speedAngle = CorrectSpeedAngle(speedAngle);
        //float speedLinear = CorrectSpeedLine(speedAngle * _radiusDistansShip);
        float speedLinear = CorrectSpeedLine(speedAngle * radiusCircle);

        return speedLinear;
    }

    #endregion

    #region FlyLine
    /// <summary>
    /// Переместить объект по прямой
    /// </summary>
    /// <param name="speed"></param>
    /// <param name="direction"></param>
    /// <param name="coordinateSpace"></param>
    /// <param name="needCorrectRotate"></param>
    private void FlyPlaneLine(float speed, Vector3 direction, Space coordinateSpace = Space.Self, bool needCorrectRotate = true)
    {
        if (needCorrectRotate)
        {
            Vector3 nextPos = GetNextPosFlyPlaneLine(speed, direction);
            RotatePlaneToNextPoint(nextPos);
        }
        transform.Translate(direction.normalized * speed * Time.deltaTime, coordinateSpace);
        NextPositionAfterNumbersFrames = GetNextPosFlyPlaneLine(speed, direction, CountFrameСalculatingNextPosition);
    }

    /// <summary>
    /// Получить следующую позицию перемещения объекта
    /// </summary>
    /// <param name="speed"></param>
    /// <param name="direction"></param>
    /// <param name="countFrame"></param>
    /// <returns></returns>
    private Vector3 GetNextPosFlyPlaneLine(float speed, Vector3 direction, uint countFrame = 1)
    {
        Vector3 nextPos = transform.position;
        for (uint i = 0; i < countFrame; i++)
        {
            nextPos = nextPos + direction.normalized * speed * Time.deltaTime;
        }
        return nextPos;
    }
    #endregion

    /// <summary>
    /// Скорректировать скорость самолёта на основе параметро максимального и минимального значения линейной скорости
    /// </summary>
    /// <param name="speedLinear"></param>
    /// <returns></returns>
    public float CorrectSpeedLine(float speedLinear)
    {
        if (speedLinear > _maxSpeed)
            speedLinear = _maxSpeed;

        if (speedLinear < _minSpeed)
            speedLinear = _minSpeed;

        return speedLinear;
    }

    /// <summary>
    /// Корректировка значения угловой скорости по допустимому пределу
    /// </summary>
    /// <param name="speedAngle"></param>
    /// <returns></returns>
    private float CorrectSpeedAngle(float speedAngle)
    {
        if (speedAngle > _maxAngleSpeed)
            speedAngle = _maxAngleSpeed;

        return speedAngle;
    }

    /// <summary>
    /// Изменить скорость объекта на ввремя
    /// </summary>
    /// <param name="speed"></param>
    /// <param name="time"></param>
    /// <param name="needAddSpeed"></param>
    public void UpdatingSpeedPlaneForWhile(float speed, float time, bool needAddSpeed)
    {
        if (_coroutineResetSpeed != null)
            StopCoroutine(_coroutineResetSpeed);

        _coroutineResetSpeed = null;
        CurrentSpeed = needAddSpeed ? CurrentSpeed + speed : speed;

        _coroutineResetSpeed = StartCoroutine(ResetSpeedToNormal(time));
    }

    /// <summary>
    /// Сбросить скорость до значения, которое должно быть в текущем состояния 
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator ResetSpeedToNormal(float delay)
    {
        yield return new WaitForSeconds(delay);
        _currentSpeed = _currentNormalSpeed;
    }


    /// <summary>
    /// Получить угол между двумя точками в диапозоне от 0 до 360.
    /// Имеет значение в каком порядке указывать точки.
    /// Расчёт длин отрезков на осях осуществляется как pointFirst.x - pointSecond.x
    /// pointFirts - это точка относительно которой считается угол второй точки
    /// Угол смещения начала отсчёта координат (shiftTheStartingAngle) -90 т.к. отсчёт оси у корабля начинается в этой точке 
    /// </summary>
    /// <param name="pointFirst"></param>
    /// <param name="pointSecond"></param>
    /// <param name="shiftTheStartingAngle"></param>
    /// <returns></returns>
    private float GetAngleBeetweenTwoPoints(Vector3 pointFirst, Vector3 pointSecond, float shiftTheStartingAngle = -90)
    {
        float width = pointFirst.x - pointSecond.x;
        float height = pointFirst.y - pointSecond.y;

        float angleBeetweenPoints = shiftTheStartingAngle + Mathf.Round((Mathf.PI + Mathf.Atan2(height, width)) * Mathf.Rad2Deg);
        if (angleBeetweenPoints < 0)
            angleBeetweenPoints = 360 + angleBeetweenPoints;

        return angleBeetweenPoints;

    }

    /// <summary>
    /// Получить вертикальный угол текущего поворота корабля в диапозоне от 0 до 360
    /// (Получение угла под которым самолёт должен вернуться на корабль)
    /// </summary>
    /// <returns></returns>
    private float GetVerticalAngleShipRotate()
    {
        float angleShipRotat = Mathf.Round(_transformShip.rotation.eulerAngles.z);
        float verticalAngleShipRotate = (180 + angleShipRotat);
        if (verticalAngleShipRotate > 360)
            verticalAngleShipRotate -= 360;

        return verticalAngleShipRotate;
    }


    /// <summary>
    /// Заверешние полёта. Объект вернулся в исходной положение. Происходит отписка от событий и удаления.
    /// </summary>
    private void FinishFly()
    {
        AirTrafficController.RemovePlaneInList(this);
        EventAfterFinishFly?.Invoke();
        EventAfterFinishFly.RemoveAllListeners();
        _pool.Return();
    }

    /// <summary>
    /// Завершение полёта через назначенное время (Переход в режим FinishFly)
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator FinishFlyAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        CurrentStatePlane = StatePlane.FinishFly;
        _timeStartFinishedFly = Time.time;
    }

    /// <summary>
    /// Повернуть объект в сторону точки
    /// </summary>
    /// <param name="nextPoint"></param>
    private void RotatePlaneToNextPoint(Vector3 nextPoint)
    {
        float angleBeetweenNextPoint = GetAngleBeetweenTwoPoints(transform.position, nextPoint);
        transform.eulerAngles = new Vector3(0, 0, angleBeetweenNextPoint);
    }


    /// <summary>
    /// Проверка нахождения точки между двумя другими точками
    /// </summary>
    /// <param name="firstPont"></param>
    /// <param name="secondPoint"></param>
    /// <param name="checkPoint"></param>
    /// <returns></returns>
    private bool CheckPointBetweenTwoPoints(Vector3 firstPont, Vector3 secondPoint, Vector3 checkPoint)
    {
        Vector3 vectorPosFirstAndCurrent = new Vector3(firstPont.x - checkPoint.x, firstPont.y - checkPoint.y, firstPont.z - checkPoint.z);
        Vector3 vectorPosCurrentAndSecond = new Vector3(secondPoint.x - checkPoint.x, secondPoint.y - checkPoint.y, secondPoint.z - checkPoint.z);
        float angle = Vector3.Angle(vectorPosFirstAndCurrent, vectorPosCurrentAndSecond);

        return (angle == 180) ? true : false;
    }
}
