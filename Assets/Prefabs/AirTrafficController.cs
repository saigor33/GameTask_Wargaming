using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class AirTrafficController : MonoBehaviour
{
    private static List<PlaneMovementLogical> _listPlanes = new List<PlaneMovementLogical>();
    private static Dictionary<PlaneMovementLogical, int> _dictionaryLastFrameUpdateSpeedPlane = new Dictionary<PlaneMovementLogical, int>();

    [SerializeField] private uint _countFrameСalculatingNextPosition;

    public static void AddPlaneInList(PlaneMovementLogical plane)
    {
        CheckCorrectListPlanes();
        _listPlanes.Add(plane);
        if (!_dictionaryLastFrameUpdateSpeedPlane.ContainsKey(plane))
            _dictionaryLastFrameUpdateSpeedPlane.Add(plane, -1); //-1 т.к. обновления ещё не выполнялись
        else
            Debug.LogError($"Project({nameof(AirTrafficController)}, _dictionaryLastFrameUpdateSpeedPlane): объект уже существует в списке. Такого быть не должно... Где-то не почистил ссылку");
    }

    public static void RemovePlaneInList (PlaneMovementLogical plane)
    {
        CheckCorrectListPlanes();
        _listPlanes.Remove(plane);
        if (_dictionaryLastFrameUpdateSpeedPlane.ContainsKey(plane))
            _dictionaryLastFrameUpdateSpeedPlane.Remove(plane); 
        else
            Debug.LogError($"Project({nameof(AirTrafficController)}, _dictionaryLastFrameUpdateSpeedPlane): объект уже был удалён из списка. Такого быть не должно...");

    }

    private static void  CheckCorrectListPlanes()
    {
        for(int i = _listPlanes.Count-1; i>=0; i--)
        {
            if (_listPlanes[i] == null)
                _listPlanes.RemoveAt(i);
        }
    }


    void Update()
    {
        CheckDistancePlanes();
    }

    private void CheckDistancePlanes()
    {
        if (_listPlanes.Count == 0)
            return;


        // копируем список т.к. во время 
        List<PlaneMovementLogical> listPlanesCopy = new List<PlaneMovementLogical>();

        foreach (var plane in _listPlanes)
        {
            //немного костыльный способ...
            //если не совпадает номер кадра, в котором считается следующая позиция, то устанавливаем 
            //нужный номер и в текущем кадре не считаем дистанцию для данного самолёта (не добавляем в список, в котором считаем)
            if (plane.CountFrameСalculatingNextPosition != _countFrameСalculatingNextPosition)
                plane.CountFrameСalculatingNextPosition = _countFrameСalculatingNextPosition;
            else
                listPlanesCopy.Add(plane);
        }

        //Заполняем иерархии в виде ёлочки (выглядит как настройка слоёв взаимодействия физи в unity)
        int countItem = listPlanesCopy.Count;
        float[,] distanseBeetweenAllPlanes = new float[countItem, countItem];
        for (int i=0; i< countItem; i++ )
        {
            for (int j = countItem-1-i; j >=0 ; j--)
            {
                float distanseTwoPlane = (float)Math.Round(Vector3.Distance(listPlanesCopy[i].NextPositionAfterNumbersFrames, listPlanesCopy[countItem-1-j].NextPositionAfterNumbersFrames), 1);
                distanseBeetweenAllPlanes[i, j] = distanseTwoPlane;
            }
        }

        string str = "";
        //проходим половину массива
        for (int i = 0; i < countItem; i++)
        {
            float distancePlanesFirstPlane = listPlanesCopy[i].GetRadiusDistansPlanes;

            str += "<";
            for (int j = 0; j< countItem-i; j++)
            {
                if (j == countItem - 1 - i)
                    continue; //на побочной диагонали расположены пересечения с самими собой
                //if (listPlanesCopy[i].GetHashCode() == listPlanesCopy[countItem-1 - i].GetHashCode())
                //    Debug.Log($"Has = [{i},{j}]");

                float distancePlanesSecondPlane = listPlanesCopy[i].GetRadiusDistansPlanes;

                //TO DO: не учитывается факт того, что самолёт мог на большой скорости проскачить область другого самолёта
                float maxNeedDistance = (distancePlanesSecondPlane > distancePlanesFirstPlane) ? distancePlanesSecondPlane : distancePlanesFirstPlane;
                if (distancePlanesSecondPlane == distancePlanesFirstPlane)
                {
                    if (maxNeedDistance > distanseBeetweenAllPlanes[i, j])
                        ControlDistancePlanes(listPlanesCopy[i], listPlanesCopy[countItem - 1 - j], maxNeedDistance);
                }
                str += $"{distanseBeetweenAllPlanes[i, j]}_";
            }
            str += "> ";
        }
       // Debug.Log(str);
    }

    private void ControlDistancePlanes(PlaneMovementLogical firstPlane, PlaneMovementLogical secondPlane, float maxNeedDistance)
    {
        //Проверяем когда последний раз, для данного самолёта обновлялся кадр. Если меньше чем число предсказания, 
        // то пропускаем итераци.
        int numCurrentFrame = Time.frameCount;
        if ((numCurrentFrame - _dictionaryLastFrameUpdateSpeedPlane[firstPlane] < _countFrameСalculatingNextPosition)
            && (numCurrentFrame - _dictionaryLastFrameUpdateSpeedPlane[secondPlane] < _countFrameСalculatingNextPosition))
            return;

        if (firstPlane.Equals(secondPlane))
            Debug.Log("Equals()=true");

            // Debug.Log("ControlDistancePlanes()");

            Vector3 nextPosAfterNumberFrame_firstPlane = firstPlane.NextPositionAfterNumbersFrames;
        Vector3 nextPosAfterNumberFrame_secondPlane = secondPlane.NextPositionAfterNumbersFrames;

        Vector3 vectorMovingFirstPlane = nextPosAfterNumberFrame_firstPlane - firstPlane.transform.position;
        Vector3 vectorMovingSecondPlane = nextPosAfterNumberFrame_secondPlane - secondPlane.transform.position;

        //скалярное произведени
        float multiplicationScaler = vectorMovingFirstPlane.x * vectorMovingSecondPlane.x +
            vectorMovingFirstPlane.y * vectorMovingSecondPlane.y + vectorMovingFirstPlane.z * vectorMovingSecondPlane.z;

        float lenghtVectorFirstPlane = Mathf.Sqrt(Mathf.Pow(vectorMovingFirstPlane.x, 2) + Mathf.Pow(vectorMovingFirstPlane.y, 2) + Mathf.Pow(vectorMovingFirstPlane.z, 2));
        float lenghtVectorSecondPlane = Mathf.Sqrt(Mathf.Pow(vectorMovingSecondPlane.x, 2) + Mathf.Pow(vectorMovingSecondPlane.y, 2) + Mathf.Pow(vectorMovingSecondPlane.z, 2));

        float cosBeetwenTwoVectors = multiplicationScaler / (lenghtVectorFirstPlane * lenghtVectorSecondPlane);

        float distancePlane = Vector3.Distance(nextPosAfterNumberFrame_firstPlane, nextPosAfterNumberFrame_secondPlane);
        float differenceDistansPlanes = maxNeedDistance - distancePlane;


        //суммарная скорость на которую нужно изменить скорость самолётов, чтобы компенсировать дистанцию
        float overallSpeedOfChange = differenceDistansPlanes / _countFrameСalculatingNextPosition;

        //float speedReserveFirtsPlane = firstPlane.GetMaxSpeed - firstPlane.GetCurrentSpeed; //запас увеличения скорости
        //float speedReserveSecondPlane = secondPlane.GetCurrentSpeed - secondPlane.GetMinSpeed; //запас уменьшения скорости


        if (cosBeetwenTwoVectors>0)
        {
            //firstPlane впереди
            //secondPlane позади
            //float travelDistanceTransfering = Vector3.Distance(firstPlane.transform.position, nextPosAfterNumberFrame_firstPlane);
            //float currentSpeedPerFrame = travelDistanceTransfering / _countFrameСalculatingNextPosition;

            ////суммарная скорость на которую нужно изменить скорость самолётов, чтобы компенсировать дистанцию
            //float overallSpeedOfChange = differenceDistansPlanes / _countFrameСalculatingNextPosition;

            //float speedReserveFirtsPlane = firstPlane.GetMaxSpeed - firstPlane.GetCurrentSpeed; //запас увеличения скорости
            //float speedReserveSecondPlane = secondPlane.GetCurrentSpeed - secondPlane.GetMinSpeed; //запас уменьшения скорости

            //float speedCoefficientReserveFirstPlane;
            //if (speedReserveFirtsPlane != 0)
            //    speedCoefficientReserveFirstPlane = (speedReserveFirtsPlane + speedReserveSecondPlane) / speedReserveFirtsPlane;
            //else
            //    speedCoefficientReserveFirstPlane = 0;

            //float speedChangeFirstPlane = overallSpeedOfChange * speedCoefficientReserveFirstPlane;
            //float speedChangeSecondPlane = -overallSpeedOfChange * (1-speedCoefficientReserveFirstPlane); // "-" т.к. замедляем

            //firstPlane.UpdatingSpeedPlaneForWhile(speedChangeFirstPlane, timeBetveansCountFrame, true);
            //secondPlane.UpdatingSpeedPlaneForWhile(speedChangeSecondPlane, timeBetveansCountFrame, true);
            Debug.Log($">0 firstPlane={firstPlane.transform.position} secondPlane={secondPlane.transform.position}");
            ChangeSpeedPlanes(firstPlane, secondPlane, overallSpeedOfChange);
        }
        else
        {
            Debug.Log($"<0 secondPlane={secondPlane.transform.position} firstPlane={firstPlane.transform.position}");
            //secondPlane впереди
            //firstPlane  позади
            ChangeSpeedPlanes(secondPlane, firstPlane , overallSpeedOfChange);

            //коэффициент запаса скорости у первого самолёта. На основе его будет будет добавлена скорость или вычтена
            //Значение от 0 до 1

            //float speedCoefficientReserveSecondPlane;
            //if (speedReserveFirtsPlane != 0)
            //    speedCoefficientReserveSecondPlane = (speedReserveFirtsPlane + speedReserveSecondPlane) / speedReserveFirtsPlane;
            //else
            //    speedCoefficientReserveSecondPlane = 0;

            //float speedChangeSecondPlane = overallSpeedOfChange * speedCoefficientReserveSecondPlane;
            //float speedChangeFirstPlane = -overallSpeedOfChange * (1 - speedCoefficientReserveSecondPlane);

            //secondPlane.UpdatingSpeedPlaneForWhile(speedChangeSecondPlane, timeBetveansCountFrame, true);
            //firstPlane.UpdatingSpeedPlaneForWhile(speedChangeFirstPlane, timeBetveansCountFrame, true);
        }
        _dictionaryLastFrameUpdateSpeedPlane[firstPlane] = Time.frameCount;
        _dictionaryLastFrameUpdateSpeedPlane[secondPlane] = Time.frameCount;

    }

    private void ChangeSpeedPlanes(PlaneMovementLogical increaseSpeedPlane, PlaneMovementLogical reduceSpeedPlane, float overallSpeedOfChange)
    {
        float speedReserveIncreaseSpeedPlane = increaseSpeedPlane.GetMaxSpeed - increaseSpeedPlane.GetCurrentSpeed; //запас увеличения скорости
        float speedReserveReduceSpeedPlane = reduceSpeedPlane.GetCurrentSpeed - reduceSpeedPlane.GetMinSpeed; //запас уменьшения скорости


        //коэффициент запаса скорости у первого самолёта. На основе его будет будет добавлена скорость или вычтена
        //Значение от 0 до 1
        float speedCoefficientReserveFirstPlane;
        if (speedReserveIncreaseSpeedPlane != 0)
            speedCoefficientReserveFirstPlane = speedReserveIncreaseSpeedPlane/(speedReserveIncreaseSpeedPlane + speedReserveReduceSpeedPlane);
        else
            speedCoefficientReserveFirstPlane = 0;

        float speedChangeIncreaseSpeedPlane = overallSpeedOfChange * speedCoefficientReserveFirstPlane;
        float speedChangeReduceSpeedPlane = -overallSpeedOfChange * (1 - speedCoefficientReserveFirstPlane); // "-" т.к. замедляем

        float timeBetveansCountFrame = Time.time/Time.frameCount  * _countFrameСalculatingNextPosition;
        increaseSpeedPlane.UpdatingSpeedPlaneForWhile(speedChangeIncreaseSpeedPlane, timeBetveansCountFrame, true);
        reduceSpeedPlane.UpdatingSpeedPlaneForWhile(speedChangeReduceSpeedPlane, timeBetveansCountFrame, true);
    }
}
