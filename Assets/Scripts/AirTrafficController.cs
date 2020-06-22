using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class AirTrafficController : MonoBehaviour
{
    private static List<PlaneMovementLogical> _listPlanes = new List<PlaneMovementLogical>();
    private static Dictionary<PlaneMovementLogical, int> _dictionaryLastFrameUpdateSpeedPlane = new Dictionary<PlaneMovementLogical, int>();

    [SerializeField] private uint _countFrameСalculatingNextPosition;

    /// <summary>
    /// Добавить самолёт в список контроля
    /// </summary>
    /// <param name="plane"></param>
    public static void AddPlaneInList(PlaneMovementLogical plane)
    {
        CheckCorrectListPlanes();
        _listPlanes.Add(plane);
        if (!_dictionaryLastFrameUpdateSpeedPlane.ContainsKey(plane))
            _dictionaryLastFrameUpdateSpeedPlane.Add(plane, -1); //-1 т.к. обновления ещё не выполнялись
        else
            Debug.LogError($"Project({nameof(AirTrafficController)}, _dictionaryLastFrameUpdateSpeedPlane): объект уже существует в списке. Такого быть не должно... Где-то не почистил ссылку");
    }

    /// <summary>
    /// Удалить самолёт из списока контроля
    /// </summary>
    /// <param name="plane"></param>
    public static void RemovePlaneInList(PlaneMovementLogical plane)
    {
        CheckCorrectListPlanes();
        _listPlanes.Remove(plane);
        if (_dictionaryLastFrameUpdateSpeedPlane.ContainsKey(plane))
            _dictionaryLastFrameUpdateSpeedPlane.Remove(plane);
        else
            Debug.LogError($"Project({nameof(AirTrafficController)}, _dictionaryLastFrameUpdateSpeedPlane): объект уже был удалён из списка. Такого быть не должно...");

    }

    /// <summary>
    /// Проверка на пустышки в списке
    /// </summary>
    private static void CheckCorrectListPlanes()
    {
        for (int i = _listPlanes.Count - 1; i >= 0; i--)
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

        List<PlaneMovementLogical> listPlanesCopy = new List<PlaneMovementLogical>();

        foreach (var plane in _listPlanes)
        {
            //немного костыльный способ...
            //если не совпадает номер кадра, в котором считается следующая позиция самолёта, то устанавливаем 
            //нужный номер и пропускаем текущую итерацию, т.к. нельзя будет получить корректное значение следующей позиции
            if (plane.CountFrameСalculatingNextPosition != _countFrameСalculatingNextPosition)
                plane.CountFrameСalculatingNextPosition = _countFrameСalculatingNextPosition;
            else
                listPlanesCopy.Add(plane);
        }

        //Заполняем массив иерархии в виде ёлочки (выглядит как настройка слоёв взаимодействия физики в unity)
        //* * * * Всё что ниже диагонали будет дублироваться, по этому не трубуется для расчёта
        //* * * 0
        //* * 0 0 
        //* 0 0 0
        int countItem = listPlanesCopy.Count;
        float[,] distanseBeetweenAllPlanes = new float[countItem, countItem];
        for (int i = 0; i < countItem; i++)
        {
            for (int j = countItem - 1 - i; j >= 0; j--)
            {
                float distanseTwoPlane = (float)Math.Round(Vector3.Distance(listPlanesCopy[i].NextPositionAfterNumbersFrames, listPlanesCopy[countItem - 1 - j].NextPositionAfterNumbersFrames), 1);
                distanseBeetweenAllPlanes[i, j] = distanseTwoPlane;
            }
        }

        //Перебираем список и где дистанция нарушена отправляем на корректировку
        for (int i = 0; i < countItem; i++)
        {
            float distancePlanesFirstPlane = listPlanesCopy[i].GetRadiusDistansPlanes;

            for (int j = 0; j < countItem - i; j++)
            {
                if (j == countItem - 1 - i)
                    continue; //на побочной диагонали расположены пересечения с самими собой

                float distancePlanesSecondPlane = listPlanesCopy[i].GetRadiusDistansPlanes;

                //TO DO: не учитывается факт того, что самолёт мог на большой скорости проскачить область другого самолёта
                float maxNeedDistance = (distancePlanesSecondPlane > distancePlanesFirstPlane) ? distancePlanesSecondPlane : distancePlanesFirstPlane;
                if (distancePlanesSecondPlane == distancePlanesFirstPlane)
                {
                    if (maxNeedDistance > distanseBeetweenAllPlanes[i, j])
                        ControlDistancePlanes(listPlanesCopy[i], listPlanesCopy[countItem - 1 - j], maxNeedDistance);
                }
            }
        }
    }

    private void ControlDistancePlanes(PlaneMovementLogical firstPlane, PlaneMovementLogical secondPlane, float maxNeedDistance)
    {
        //Проверяем когда последний раз, для данного самолёта обновлялся кадр. Если меньше чем число предсказания, 
        // то пропускаем итераци.
        int numCurrentFrame = Time.frameCount;
        if ((numCurrentFrame - _dictionaryLastFrameUpdateSpeedPlane[firstPlane] < _countFrameСalculatingNextPosition)
            && (numCurrentFrame - _dictionaryLastFrameUpdateSpeedPlane[secondPlane] < _countFrameСalculatingNextPosition))
            return;

        Vector3 nextPosAfterNumberFrame_firstPlane = firstPlane.NextPositionAfterNumbersFrames;
        Vector3 nextPosAfterNumberFrame_secondPlane = secondPlane.NextPositionAfterNumbersFrames;

        Vector3 vectorMovingFirstPlane = nextPosAfterNumberFrame_firstPlane - firstPlane.transform.position;
        Vector3 vectorMovingSecondPlane = nextPosAfterNumberFrame_secondPlane - secondPlane.transform.position;
        float cosBeetwenTwoVectors = GetCosinusScalerMultiplication(vectorMovingFirstPlane, vectorMovingSecondPlane);

        float distancePlane = Vector3.Distance(nextPosAfterNumberFrame_firstPlane, nextPosAfterNumberFrame_secondPlane);
        float differenceDistansPlanes = maxNeedDistance - distancePlane;

        //суммарная скорость на которую нужно изменить скорость самолётов, чтобы компенсировать дистанцию
        float overallSpeedOfChange = differenceDistansPlanes / _countFrameСalculatingNextPosition;
        if (cosBeetwenTwoVectors > 0)
        {
            //firstPlane впереди
            //secondPlane позади
            ChangeSpeedPlanes(firstPlane, secondPlane, overallSpeedOfChange);
        }
        else
        {
            //secondPlane впереди
            //firstPlane  позади
            ChangeSpeedPlanes(secondPlane, firstPlane, overallSpeedOfChange);
        }
        _dictionaryLastFrameUpdateSpeedPlane[firstPlane] = Time.frameCount;
        _dictionaryLastFrameUpdateSpeedPlane[secondPlane] = Time.frameCount;
    }

    /// <summary>
    /// Получить значение косинуса между двумя векторами
    /// </summary>
    /// <param name="vectorMovingFirstPlane"></param>
    /// <param name="vectorMovingSecondPlane"></param>
    /// <returns></returns>
    private float GetCosinusScalerMultiplication(Vector3 vectorMovingFirstPlane, Vector3 vectorMovingSecondPlane)
    {
        //скалярное произведени
        float multiplicationScaler = vectorMovingFirstPlane.x * vectorMovingSecondPlane.x +
            vectorMovingFirstPlane.y * vectorMovingSecondPlane.y + vectorMovingFirstPlane.z * vectorMovingSecondPlane.z;

        float lenghtVectorFirstPlane = Mathf.Sqrt(Mathf.Pow(vectorMovingFirstPlane.x, 2) + Mathf.Pow(vectorMovingFirstPlane.y, 2) + Mathf.Pow(vectorMovingFirstPlane.z, 2));
        float lenghtVectorSecondPlane = Mathf.Sqrt(Mathf.Pow(vectorMovingSecondPlane.x, 2) + Mathf.Pow(vectorMovingSecondPlane.y, 2) + Mathf.Pow(vectorMovingSecondPlane.z, 2));

        float cosBeetwenTwoVectors = multiplicationScaler / (lenghtVectorFirstPlane * lenghtVectorSecondPlane);

        return cosBeetwenTwoVectors;
    }

    /// <summary>
    /// Изменяем значение скоростей самолётов, чтобы они смогли выравнить растояние между друг другом
    /// </summary>
    /// <param name="increaseSpeedPlane"></param>
    /// <param name="reduceSpeedPlane"></param>
    /// <param name="overallSpeedOfChange"></param>
    private void ChangeSpeedPlanes(PlaneMovementLogical increaseSpeedPlane, PlaneMovementLogical reduceSpeedPlane, float overallSpeedOfChange)
    {
        //TO DO: немного костыльно т.к. учитывается в данный момент только два самолёта, а их может быть несколько. 
        //т.е. один будет перебивать другого

        float speedReserveIncreaseSpeedPlane = increaseSpeedPlane.GetMaxSpeed - increaseSpeedPlane.CurrentSpeed; //запас увеличения скорости
        float speedReserveReduceSpeedPlane = reduceSpeedPlane.CurrentSpeed - reduceSpeedPlane.GetMinSpeed; //запас уменьшения скорости

        //коэффициент запаса скорости у первого самолёта. На основе его будет будет добавлена скорость или вычтена
        //Значение от 0 до 1
        float speedCoefficientReserveFirstPlane;
        if (speedReserveIncreaseSpeedPlane != 0)
            speedCoefficientReserveFirstPlane = speedReserveIncreaseSpeedPlane / (speedReserveIncreaseSpeedPlane + speedReserveReduceSpeedPlane);
        else
            speedCoefficientReserveFirstPlane = 0;

        float speedChangeIncreaseSpeedPlane = overallSpeedOfChange * speedCoefficientReserveFirstPlane;
        float speedChangeReduceSpeedPlane = -overallSpeedOfChange * (1 - speedCoefficientReserveFirstPlane); // "-" т.к. замедляем

        float timeBetveansCountFrame = Time.time / Time.frameCount * _countFrameСalculatingNextPosition;
        increaseSpeedPlane.UpdatingSpeedPlaneForWhile(speedChangeIncreaseSpeedPlane, timeBetveansCountFrame, true);
        reduceSpeedPlane.UpdatingSpeedPlaneForWhile(speedChangeReduceSpeedPlane, timeBetveansCountFrame, true);
    }
}
