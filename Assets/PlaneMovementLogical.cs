using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityNightPool;

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
    [SerializeField] private float _timeLive;
    [SerializeField] private float _anglePlaneLanding;

    [SerializeField] private float _disappearanceDistancePlane;

    [Header("Логирование")]
    [SerializeField] private bool _log_FinishFly_isFlyLine;
    [SerializeField] private float _log_anglePlaneOnShip;
    [SerializeField] private float _log_angleVerticalShip;
    [SerializeField] private bool _log_OnLine;


    private StatePlane IsStatePlane { get; set; }
    private Transform _transformShip;
    private float _angle;

    private Vector3 _positionStartMovingFinish;
    private float _timeStartFinishedFly;

    private PoolObject _pool;

     
    private void Start()
    {
        _pool = GetComponent<PoolObject>();
        if (_pool == null)
            Debug.LogError($"Project({this}, _pool) Не добавлен объект PoolObject");
    }

    void Update()
    {
        //Vector3 pos1 = new Vector3(-1, -1, -1);
        //Vector3 pos2 = new Vector3(1, 1, 1);
        //Vector3 pos3 = new Vector3(2, 2, 2);
        //Vector3 pos4 = new Vector3(0, 0, 0);


        //Debug.Log($"result = {CheckPointBetweenTwoPoints(pos1, pos2, pos3)}"); 
        //return;
        switch (IsStatePlane)
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

                    float speedLinear = CheckCorrectSpeedLine(speedAngle * _radiusDistansShip);
                    FlyPlaneCircle(_transformShip.position, speedLinear, Vector3.forward);
                    break;
                }
            case StatePlane.FinishFly:
                {
                    FlyReturnOnShip();
                    break;

                    float angleShipRotat = Mathf.Round(_transformShip.rotation.eulerAngles.z);

                    float width = _transformShip.position.x - transform.position.x;
                    float height = _transformShip.position.y - transform.position.y;

                    float angleBeetweenPlaneAndShip =-90+ Mathf.Round((Mathf.PI + Mathf.Atan2(height, width)) * Mathf.Rad2Deg);
                    if (angleBeetweenPlaneAndShip < 0)
                        angleBeetweenPlaneAndShip = 360 +angleBeetweenPlaneAndShip;

                    _log_anglePlaneOnShip = angleBeetweenPlaneAndShip;

                    float verticalAngleShipRotate = (180 + angleShipRotat);
                    if (verticalAngleShipRotate > 360)
                        verticalAngleShipRotate -= 360;

                    _log_angleVerticalShip = verticalAngleShipRotate;
                    //float minAngle = verticalAngleShipRotate - _anglePlaneLanding;

                    if (angleBeetweenPlaneAndShip == verticalAngleShipRotate)
                    {
                        _log_FinishFly_isFlyLine = true;
                        //TO DO: нужно сделать чтобы плавно закруглялся

                        Vector3 movementDirection = _transformShip.position - transform.position;

                        Vector3 nexPos = transform.position + movementDirection.normalized * _minSpeed * Time.deltaTime;
                        _log_OnLine = CheckPointBetweenTwoPoints(transform.position, nexPos, _transformShip.position);

                        if (CheckPointBetweenTwoPoints(transform.position, nexPos, _transformShip.position))
                        {
                            transform.position = nexPos;
                            FinishFly();
                        }
                        else
                        {
                            //transform.Translate(movementDirection.normalized * _minSpeed * Time.deltaTime, Space.World);         
                            //if (!CheckPointBetweenTwoPoints(transform.position, _transformShip.position, nexPos))
                            FlyPlaneLine(_minSpeed, movementDirection);
                        }

                        //TO DO: нужно проверить не пролетит ли мимо на большой скорости
                    }
                    else
                    {
                        _log_FinishFly_isFlyLine = false;
                        float distansePlaneAndShip = Vector3.Distance(_transformShip.position, transform.position);
                        float speedAngle = _countFlyCircles * Mathf.PI * distansePlaneAndShip / (_timeLive- _timeFly);
                        if (speedAngle > _maxAngleSpeed)
                            speedAngle = _maxAngleSpeed;

                        float speedLinear = speedAngle * distansePlaneAndShip / 2f;

                        float nexAngle = angleBeetweenPlaneAndShip + speedLinear;

                        if (angleBeetweenPlaneAndShip < verticalAngleShipRotate && nexAngle > verticalAngleShipRotate)
                            speedLinear = verticalAngleShipRotate - angleBeetweenPlaneAndShip;


                        float minAngle = verticalAngleShipRotate - _anglePlaneLanding;
                        float maxAngle = verticalAngleShipRotate + _anglePlaneLanding;

                        if (angleBeetweenPlaneAndShip> minAngle && angleBeetweenPlaneAndShip < maxAngle)
                        {
                            Vector3 movementDirection = _transformShip.position - transform.position;

                            Vector3 nexPos = transform.position + movementDirection.normalized * _minSpeed * Time.deltaTime;
                            if (CheckPointBetweenTwoPoints(transform.position, nexPos, _transformShip.position))
                            {
                                FinishFly();
                            }
                            else
                            {
                                //transform.Translate(movementDirection.normalized * _minSpeed * Time.deltaTime, Space.World);     
                                //if (!CheckPointBetweenTwoPoints(transform.position, _transformShip.position, nexPos))
                                FlyPlaneLine(_minSpeed, movementDirection);
                            }
                        }
                        FlyPlaneCircle(_transformShip.position, speedLinear, Vector3.forward);

                    }
                    {                     //angleBeetweenPlaneAndShip
                                          //ниже оси х положительные
                                          //выше отрицательные


                        //if (angleBeetweenPlaneAndShip - angleShipRotat)
                        //Debug.Log($"angleShipRotat = {angleShipRotat} angleBeetweenPlaneAndShip {angleBeetweenPlaneAndShip}");

                        //float diameterPlaneAndShip = Vector3.Distance(_transformShip.position, transform.position);

                        //float width = _transformShip.position.x - transform.position.x;
                        //float height = _transformShip.position.y - transform.position.y;

                        //float centerX = width / 2f;
                        //float centerY = height / 2f;

                        //float timeLeft = (_timeLive-_timeFly)-Mathf.Abs(_timeStartFinishedFly - Time.time);
                        //if (timeLeft <= 0)
                        //    FinishFly();

                        //float speedAngle = 2 * Mathf.PI * _radiusDistansShip / timeLeft;
                        //if (speedAngle > _maxAngleSpeed)
                        //    speedAngle = _maxAngleSpeed;

                        //float speedLinear = speedAngle * diameterPlaneAndShip/2f;
                        //if (speedLinear > _maxSpeed)
                        //    speedLinear = _maxSpeed;

                        //if (speedLinear < _minSpeed)
                        //    speedLinear = _minSpeed;

                        //float posX = centerX + Mathf.Cos(_angle) * speedLinear;
                        //float posY = centerY + Mathf.Sin(_angle) * speedLinear;
                        //_angle = _angle + Time.deltaTime * _maxAngleSpeed;


                        //Vector3 nextPos = new Vector3(posX, posY, 0);

                        ////TO DO: избавиться от перемещения-телепортации
                        //transform.position = nextPos;

                        //float angleBeetweenPlaneAndShip = Mathf.Atan2(height, width) * 180/Mathf.PI ;
                        //Debug.Log($"angleBeetweenPlaneAndShip = {angleBeetweenPlaneAndShip}");
                        //if (angleBeetweenPlaneAndShip >= 85 && angleBeetweenPlaneAndShip <= 95)
                        //    FinishFly();
                        ////if (transform.position.y <= _transformShip.position.y)
                        ////{
                        ////    if (transform.position.x-1 < _transformShip.position.x ||
                        ////        transform.position.x + 1 > _transformShip.position.x)
                        ////        _pool.Return();
                        ////}
                    }


                    break;
                }
        }
    }

    private void FlyReturnOnShip()
    {
        float angleShipRotat = Mathf.Round(_transformShip.rotation.eulerAngles.z);

        float width = _transformShip.position.x - transform.position.x;
        float height = _transformShip.position.y - transform.position.y;

        float angleBeetweenPlaneAndShip = -90 + Mathf.Round((Mathf.PI + Mathf.Atan2(height, width)) * Mathf.Rad2Deg);
        if (angleBeetweenPlaneAndShip < 0)
            angleBeetweenPlaneAndShip = 360 + angleBeetweenPlaneAndShip;

        _log_anglePlaneOnShip = angleBeetweenPlaneAndShip;

        float verticalAngleShipRotate = (180 + angleShipRotat);
        if (verticalAngleShipRotate > 360)
            verticalAngleShipRotate -= 360;

        _log_angleVerticalShip = verticalAngleShipRotate;

        Vector3 movementDirection = _transformShip.position - transform.position;
        Vector3 nexPos;
        if (angleBeetweenPlaneAndShip == verticalAngleShipRotate)
        {
            nexPos = transform.position + movementDirection.normalized * _minSpeed * Time.deltaTime;
            Debug.Log("VerticalLine");
            if (CheckPointBetweenTwoPoints(transform.position, nexPos, _transformShip.position))
                FinishFly();
            FlyPlaneLine(_minSpeed,movementDirection);

           // Debug.Log($"crntPos={transform.position} nexPos={nexPos}");
        }
        else
        {
            //float distansePlaneAndShip = Vector3.Distance(_transformShip.position, transform.position);
            //float speedAngle = _countFlyCircles * Mathf.PI * distansePlaneAndShip / (_timeLive - _timeFly);
            float lastTime = (_timeLive - _timeFly) - (Time.deltaTime - _timeStartFinishedFly);
            float speedAngle = _countFlyCircles *2* Mathf.PI * _radiusDistansShip / lastTime;
            if (speedAngle > _maxAngleSpeed)
                speedAngle = _maxAngleSpeed;

            //float speedLinear = speedAngle * distansePlaneAndShip / 2f;
            float speedLinear = CheckCorrectSpeedLine(speedAngle * _radiusDistansShip);

            float nexAngle = angleBeetweenPlaneAndShip + speedLinear;

            if (angleBeetweenPlaneAndShip < verticalAngleShipRotate && nexAngle > verticalAngleShipRotate)
                speedLinear = CheckCorrectSpeedLine(verticalAngleShipRotate - angleBeetweenPlaneAndShip);

            float minAngle = verticalAngleShipRotate - _anglePlaneLanding;
            float maxAngle = verticalAngleShipRotate + _anglePlaneLanding;

            nexPos = transform.position + movementDirection.normalized * _minSpeed*Time.deltaTime;

            nexAngle = angleBeetweenPlaneAndShip + speedLinear;
            if ((angleBeetweenPlaneAndShip< minAngle && nexAngle>maxAngle) || (angleBeetweenPlaneAndShip > minAngle && angleBeetweenPlaneAndShip < maxAngle))
            //if (angleBeetweenPlaneAndShip > minAngle && angleBeetweenPlaneAndShip < maxAngle)
            {
                Debug.Log("Sector");
            if (CheckPointBetweenTwoPoints(transform.position, nexPos, _transformShip.position))
                FinishFly();
                FlyPlaneLine(_minSpeed, movementDirection);
            }

            Vector3 directionMinDistanseToFinish = Vector3.zero;
            if (maxAngle - angleBeetweenPlaneAndShip < 0)
                directionMinDistanseToFinish = Vector3.back;

            if (minAngle - angleBeetweenPlaneAndShip > 0)
                directionMinDistanseToFinish = Vector3.forward;

            if (directionMinDistanseToFinish != Vector3.zero)
                FlyPlaneCircle(_transformShip.position, speedLinear, directionMinDistanseToFinish);
        }
        // Debug.Log($"distane= {Vector3.Distance(transform.position, nexPos)}");

    }

    public void СourseAdjustments(List<Vector3> listPoints)
    {
        //найдём минимальные и максимальные точки X и Y
        //с помощью них определим центр прямоугольника, который является центром описанной окружности
        // данного прямоугольника. С помощью этой окружности зададим облёта препятсвия


        string str = "Points: ";
        Vector3 pointLeft , pointRight, pointTop, pointBottom;

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

            if(maxX < point.x)
            {
                maxX = point.x;
                pointRight = point;
            }

            if(maxY < point.y)
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
        Vector3 centerCircle = new Vector3((minX+maxX)/2f , (minY + maxY) / 2f, 0);
        float radiusCircle = Vector3.Distance(pointRectTopLeft, centerCircle);

        float speedAngle = _countFlyCircles * 2 * Mathf.PI * radiusCircle / _timeFly;
        if (speedAngle > _maxAngleSpeed)
            speedAngle = _maxAngleSpeed;

        //float speedLinear = speedAngle * distansePlaneAndShip / 2f;
        float speedLinear = CheckCorrectSpeedLine( speedAngle * radiusCircle);
        FlyPlaneCircle(centerCircle, speedLinear, Vector3.forward);
    }

    private float CheckCorrectSpeedLine(float speedLinear)
    {
       // string str = $"speedLined Before = {speedLinear}";

        if (speedLinear > _maxSpeed)
            speedLinear = _maxSpeed;

        if (speedLinear < _minSpeed)
            speedLinear = _minSpeed;

        return speedLinear;
        //str += $"after = {speedLinear}";
        //Debug.Log(str);
    }

    private void FlyPlaneCircle(Vector3 сentralRotationPoint, float speedLinear,Vector3 directionMinDistanseToFinish )
    {
        //if (speedLinear > _maxSpeed)
        //    speedLinear = _maxSpeed;

        //if (speedLinear < _minSpeed)
        //    speedLinear = _minSpeed;

        //float posX = _transformShip.position.x + Mathf.Cos(_angle) * speedLinear;
        //float posY = _transformShip.position.y + Mathf.Sin(_angle) * speedLinear;
        //_angle = _angle + Time.deltaTime * _maxAngleSpeed;

        //Vector3 nextPos = new Vector3(posX, posY, 0);

        //TO DO: избавиться от перемещения-телепортации
        //transform.position = nextPos;
        //transform.Translate(nextPos);
        transform.RotateAround(сentralRotationPoint, directionMinDistanseToFinish, speedLinear);


        if (_angle >= 360)
            _angle = 0;
    }

    private void FlyPlaneLine(float speed, Vector3 direction)
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
    }

    private void CheckFinishPosition(Vector3 currentPosition, Vector3 targetPosition)
    {

    }

    public void Init(Transform transformShip)
    {
        _transformShip = transformShip;
        _angle = 90;  //устанавливаем угол 90, чтобы вращение начиналось перед кораблём
        IsStatePlane = StatePlane.BeginFly;
        StartCoroutine(FinishFlyAfterTime(_timeFly));
    }

    private IEnumerator FinishFlyAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Start FinishFlyAfterTime()");
        IsStatePlane = StatePlane.FinishFly;
        _timeStartFinishedFly = Time.time;
        _positionStartMovingFinish = transform.position;
    }

    private void FinishFly()
    {
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
        Vector3 vectorPosFirstAndCurrent = new Vector3(firstPont.x -checkPoint.x, firstPont.y - checkPoint.y, firstPont.z - checkPoint.z);
        Vector3 vectorPosCurrentAndSecond = new Vector3(secondPoint.x - checkPoint.x, secondPoint.y - checkPoint.y, secondPoint.z - checkPoint.z);
        float angle = Vector3.Angle(vectorPosFirstAndCurrent, vectorPosCurrentAndSecond);
        Debug.Log(angle);
        if ( angle == 180)
            return true;
        else
            return false;



        //уравнение прямой (x-x1)/(x2-x1)=(y-y1)/(y2-y1)=(z-z1)/(z2-z1)
        //float denominatorX = secondPoint.x - firstPont.x;
        //float valueX = (denominatorX != 0) ? ((checkPoint.x - firstPont.x) / denominatorX) : (checkPoint.x - firstPont.x);

        //float denominatorY = secondPoint.y - firstPont.y;
        //float valueY = (denominatorY != 0) ? ((checkPoint.y - firstPont.y) / denominatorY) : (checkPoint.y - firstPont.y);

        ////float denominatorZ = secondPoint.z - firstPont.z;
        ////float valueZ = (denominatorZ != 0) ? ((checkPoint.z - firstPont.z) / denominatorZ) : (checkPoint.z - firstPont.z);

        //bool isOnLine =  (valueX == valueY) ? true : false;
        //if (isOnLine)
        //{
        //    // определяем какая из двух точек firstPont или secondPoint находиться ниже и левее относительно друг друга

        //    bool pointIsBeetweenTwoPoint_AxisX;
        //    if(secondPoint.x > firstPont.x)
        //        pointIsBeetweenTwoPoint_AxisX = (firstPont.x <= checkPoint.x && secondPoint.x >= checkPoint.x) ? true : false;
        //    else
        //        pointIsBeetweenTwoPoint_AxisX = (secondPoint.x <= checkPoint.x && firstPont.x >= checkPoint.x ) ? true : false;


        //    bool pointIsBeetweenTwoPoint_AxisY;
        //    if (secondPoint.y > firstPont.y)
        //        pointIsBeetweenTwoPoint_AxisY = (firstPont.y <= checkPoint.y && secondPoint.y >= checkPoint.y) ? true : false;
        //    else
        //        pointIsBeetweenTwoPoint_AxisY = ( secondPoint.y <= checkPoint.y && firstPont.y >= checkPoint.y ) ? true : false;


        //    bool pointIsBeetweenTwoPoint = (pointIsBeetweenTwoPoint_AxisX && pointIsBeetweenTwoPoint_AxisY) ? true : false;

        //    return pointIsBeetweenTwoPoint;
        //}
        //else
        //    return false;
       

        ////вычисляем угол cosA через формулу скалярного произведения
        //double numerator = (vectorPosFirstAndCurrent.x * vectorPosCurrentAndSecond.x 
        //    + vectorPosFirstAndCurrent.y * vectorPosCurrentAndSecond.y 
        //    + vectorPosFirstAndCurrent.z * vectorPosCurrentAndSecond.z);

        //double numFirstDenominator = Math.Round(Math.Sqrt(vectorPosFirstAndCurrent.x * vectorPosFirstAndCurrent.x
        //    + vectorPosFirstAndCurrent.y * vectorPosFirstAndCurrent.y
        //    + vectorPosFirstAndCurrent.z * vectorPosFirstAndCurrent.z), 4);

        //double numSecondDenominator = Math.Round(Math.Sqrt(vectorPosCurrentAndSecond.x * vectorPosCurrentAndSecond.x
        //    + vectorPosCurrentAndSecond.y * vectorPosCurrentAndSecond.y
        //    + vectorPosCurrentAndSecond.z * vectorPosCurrentAndSecond.z), 4);

        //double denominator = numFirstDenominator * numSecondDenominator;
        ////если знаменатель равен 0, то один из векторов является нулевым вектором. 
        ////Это значит что проверяемая точка равна либо первой, либо второй точке.
        //if (denominator == 0)
        //    return true;
        //double cosAngleBeetwenVectors = numerator / denominator;
        //Debug.Log($"numerator/ denominator= {cosAngleBeetwenVectors}");
        ////double angleBeetwenVectors = Math.Acos(numerator/ denominator); //*Mathf.Rad2Deg
        ////Debug.Log($"angleBeetwenVectors {angleBeetwenVectors}");
        //if (cosAngleBeetwenVectors == 1 || cosAngleBeetwenVectors == (-1))
        //    return true;
        //else
        //    return false;
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

        return new Vector3(x,y,z);
    }
}
