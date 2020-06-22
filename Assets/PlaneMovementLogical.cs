using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityNightPool;

public class PlaneMovementLogical : MonoBehaviour
{
    public enum StatePlane { BeginFly, Fly, FinishFly }

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
    private float _timeStartFinishedFly;
    private float _currentSpeed;
    private float _currentNormalSpeed;
    private Coroutine _coroutineResetSpeed;

    private StatePlane CurrentStatePlane { get; set; }
    public UnityEvent EventAfterFinishFly { get; } = new UnityEvent();

    /// <summary>
    /// Позиция самолёта, через CountFrameСalculatingNextPosition кадров.
    /// </summary>
    public Vector3 NextPositionAfterNumbersFrames { get; private set; }

    /// <summary>
    /// Номер кадра, для которого расчитывается NextPositionAfterNumbersFrames
    /// </summary>
    public uint CountFrameСalculatingNextPosition { get; set; } = 1;

    /// <summary>
    /// Получить радиус дистанции на которой самолёт должен держаться от других самолётов
    /// </summary>
    public float GetRadiusDistansPlanes { get { return _radiusDistansPlanes; } }

    public float GetCurrentSpeed { get { return _currentSpeed; } }

    public float GetMaxSpeed { get { return _maxSpeed; } }
    public float GetMinSpeed { get { return _minSpeed; } }

    private void Awake()
    {
        _pool = GetComponent<PoolObject>();
        if (_pool == null)
            Debug.LogError($"Project({this}, _pool): Не добавлен объект PoolObject");
        if (_countFlyCircles ==0)
            Debug.LogError($"Project({this}, _countFlyCircles): Самолёт не может не совершать кругов");
    }

    void Update()
    {
        switch (CurrentStatePlane)
        {
            case StatePlane.BeginFly:
                {
                    FlyPlaneLine(_currentSpeed, Vector3.up, Space.Self, false);
                    if (Vector3.Distance(_transformShip.position, transform.position) >= _radiusDistansShip)
                    {
                        _currentSpeed = GetSpeedForCircle(_countFlyCircles, _radiusDistansShip, _timeFly);
                        _currentNormalSpeed = _currentSpeed;
                        CurrentStatePlane = StatePlane.Fly;
                    }
                    break;
                }
            case StatePlane.Fly:
                {
                    //Debug.Log($"crnt={transform.position} NextPosCircle= {GetNextPos(_transformShip.position, _currentSpeed, Vector3.forward)}");
                    FlyPlaneCircle(_transformShip.position, _currentSpeed, Vector3.forward);
                    break;
                }
            case StatePlane.FinishFly:
                {
                    FlyReturnOnShip();
                    break;
                }
        }
    }

    private void FlyReturnOnShip()
    {
        Vector3 movementDirection = _transformShip.position - transform.position;
        float angleBeetweenPlaneAndShip = GetAngleBeetweenTwoPoints(_transformShip.position, transform.position);
        float angleRotatePlaneOnShip = GetVerticalAngleShipRotate();
        //Debug.Log($"angle= {angleBeetweenPlaneAndShip} angleBeetweenPlaneAndShip={angleRotatePlaneOnShip}");

        if (angleBeetweenPlaneAndShip == angleRotatePlaneOnShip)
        {

            //проверяем находится ли корабль между текущем положением и положением в следующем кадре.
            //Если да, то считаем, что самолёт вернулся на корабль
            Vector3 nexPos = GetNextPosFlyPlaneLine(_currentSpeed, movementDirection);
            if (CheckPointBetweenTwoPoints(transform.position, nexPos, _transformShip.position))
                FinishFly();

            FlyPlaneLine(_currentSpeed, movementDirection, Space.World);
        }
        else
        {
            //изменяем скорость в зависимости от оставшегося времени т.к. корабль может двигаться и увеличивать растояние
            // TO DO: не учитывается путь до корабля (не понятно что делать, если время закончиться т.к. корабль может уплыть)
            float lastTime = (_timeLive - _timeFly) - (Time.deltaTime - _timeStartFinishedFly);
            float speedLinear = GetSpeedForCircle(_countFlyCircles, _radiusDistansShip, lastTime);

            float nextAngleBeetweenPlaneAndShip = angleBeetweenPlaneAndShip + speedLinear;

            //подгоняем угол поворота самолёта под угол приземление в случае, если он проскочит угол приземления в следующем кадре
            if (angleBeetweenPlaneAndShip < angleRotatePlaneOnShip && nextAngleBeetweenPlaneAndShip > angleRotatePlaneOnShip)
            {
                speedLinear = CorrectSpeedLine(angleRotatePlaneOnShip - angleBeetweenPlaneAndShip);
                nextAngleBeetweenPlaneAndShip = angleBeetweenPlaneAndShip + speedLinear;
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
                Vector3 nexPos = GetNextPosFlyPlaneLine(_currentSpeed, movementDirection);
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
                FlyPlaneCircle(_transformShip.position, speedLinear, directionMinDistanseToFinish);
        }
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
        float speedLinear = CorrectSpeedLine(speedAngle * _radiusDistansShip);

        return speedLinear;
    }

    public void СourseAdjustments(List<Vector3> listPoints)
    {
        //найдём минимальные и максимальные точки X и Y
        //с помощью них определим центр прямоугольника, который является центром описанной окружности
        // данного прямоугольника. С помощью этой окружности зададим облёта препятсвия


        string str = "Points: ";
        Vector3 pointLeft, pointRight, pointTop, pointBottom;

        float minX = listPoints[0].x;
        float maxX = listPoints[0].x;
        float maxY = listPoints[0].y;
        float minY = listPoints[0].y;

        foreach (var point in listPoints)
        {
            if (minX > point.x)
            {
                minX = point.x;
                pointLeft = point;
            }

            if (maxX < point.x)
            {
                maxX = point.x;
                pointRight = point;
            }

            if (maxY < point.y)
            {
                maxY = point.x;
                pointTop = point;
            }

            if (minY > point.y)
            {
                minY = point.x;
                pointBottom = point;
            }
            str += $"${point}$ ";
        }

        //точка вершины прямоугольника, которая также нахдится на окружности
        Vector3 pointRectTopLeft = new Vector3(minX, maxY, 0);
        Vector3 centerCircle = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0);
        float radiusCircle = Vector3.Distance(pointRectTopLeft, centerCircle);

        float speedAngle = _countFlyCircles * 2 * Mathf.PI * radiusCircle / _timeFly;
        if (speedAngle > _maxAngleSpeed)
            speedAngle = _maxAngleSpeed;

        //float speedLinear = speedAngle * distansePlaneAndShip / 2f;
        float speedLinear = CorrectSpeedLine(speedAngle * radiusCircle);
        FlyPlaneCircle(centerCircle, speedLinear, Vector3.forward);
    }

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
    /// Перемещение объекта по кругу
    /// </summary>
    /// <param name="сentralRotationPoint"></param>
    /// <param name="speedLinear"></param>
    /// <param name="directionMinDistanseToFinish"></param>
    private void FlyPlaneCircle(Vector3 сentralRotationPoint, float speedLinear, Vector3 directionMinDistanseToFinish)
    {
        Vector3 nextPos = GetNextPosFlyPlaneCircle(сentralRotationPoint, speedLinear, directionMinDistanseToFinish);
        RotatePlaneToNextPoint(nextPos);
        NextPositionAfterNumbersFrames = GetNextPosFlyPlaneCircle(сentralRotationPoint, speedLinear, directionMinDistanseToFinish, CountFrameСalculatingNextPosition);

        transform.RotateAround(сentralRotationPoint, directionMinDistanseToFinish, speedLinear);
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

    private void FlyPlaneLine(float speed, Vector3 direction, Space coordinateSpace = Space.Self, bool needCorrectRotate = true)
    {
        if(needCorrectRotate)
        {
            Vector3 nextPos = GetNextPosFlyPlaneLine(speed, direction);
            RotatePlaneToNextPoint(nextPos);
        }
        NextPositionAfterNumbersFrames = GetNextPosFlyPlaneLine(speed, direction, CountFrameСalculatingNextPosition);
        transform.Translate(direction.normalized * speed * Time.deltaTime, coordinateSpace);
    }


    private Vector3 GetNextPosFlyPlaneLine(float speed, Vector3 direction, uint countFrame=1)
    {
        Vector3 nextPos = transform.position;
        for (uint i=0; i< countFrame; i++)
        {
            nextPos = nextPos + direction.normalized * speed * Time.deltaTime;
        }
        return nextPos;
    }

    private void RotatePlaneToNextPoint(Vector3 nextPoint)
    {
        float angleBeetweenNextPoint = GetAngleBeetweenTwoPoints(transform.position, nextPoint);
        transform.eulerAngles = new Vector3(0, 0, angleBeetweenNextPoint);
    }


    public void Init(Transform transformShip)
    {
        _transformShip = transformShip;
        CurrentStatePlane = StatePlane.BeginFly;

        _currentSpeed = (_maxSpeed + _minSpeed) / 2f;
        _currentNormalSpeed = _currentSpeed;
        StartCoroutine(FinishFlyAfterTime(_timeFly));
    }

    private IEnumerator FinishFlyAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Start FinishFlyAfterTime()");
        CurrentStatePlane = StatePlane.FinishFly;

        _timeStartFinishedFly = Time.time;
    }

    private void FinishFly()
    {
        AirTrafficController.RemovePlaneInList(this);
        EventAfterFinishFly?.Invoke();
        EventAfterFinishFly.RemoveAllListeners();
        _pool.Return();
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

        if (angle == 180)
            return true;
        else
            return false;
    }

    public void UpdatingSpeedPlaneForWhile(float speed, float time, bool needAddSpeed)
    {
        if (_coroutineResetSpeed != null)
            StopCoroutine(_coroutineResetSpeed);
        _coroutineResetSpeed = null;
        _currentSpeed =CorrectSpeedLine(needAddSpeed ? _currentSpeed + speed : speed);

        _coroutineResetSpeed = StartCoroutine(ResetSpeedToNormal(time));
    }

    private IEnumerator ResetSpeedToNormal(float delay)
    {
        //Debug.Log($"StartCoroutine = {delay}");
        yield return new WaitForSeconds(delay);
        _currentSpeed = _currentNormalSpeed;
    }

    /// <summary>
    /// Округление значений типа Vector3 до символов после запятой
    /// </summary>
    /// <param name="position"></param>
    /// <param name="decimals"></param>
    /// <returns></returns>
    private Vector3 MathfRound(Vector3 position, int decimals = 0)
    {
        float x = (float)Math.Round(position.x, decimals);
        float y = (float)Math.Round(position.y, decimals);
        float z = (float)Math.Round(position.z, decimals);

        return new Vector3(x, y, z);
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
}
