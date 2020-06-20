using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class AirTrafficController : MonoBehaviour
{
    private static List<PlaneMovementLogical> _listPlanes = new List<PlaneMovementLogical>();

    public static void AddPlaneInList(PlaneMovementLogical plane)
    {
        CheckCorrectListPlanes();
        _listPlanes.Add(plane);
    }

    public static void RemovePlaneInList (PlaneMovementLogical plane)
    {
        CheckCorrectListPlanes();
        _listPlanes.Remove(plane);
    }

    private static void  CheckCorrectListPlanes()
    {
        for(int i = _listPlanes.Count-1; i>=0; i--)
        {
            if (_listPlanes[i] == null)
                _listPlanes.RemoveAt(i);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckDistancePlanes();
    }

    private void CheckDistancePlanes()
    {
        // копируем список т.к. во время 
        List<PlaneMovementLogical> listPlanesCopy = new List<PlaneMovementLogical>(_listPlanes);

        int countItem = listPlanesCopy.Count;
        float[,] distanseBeetweenAllPlanes = new float[countItem, countItem];
        for (int i=0; i< countItem; i++ )
        {
            for (int j = countItem-1; j >=0 ; j--)
            {
                distanseBeetweenAllPlanes[i,j] = (float)Math.Round(Vector3.Distance(listPlanesCopy[i].transform.position, listPlanesCopy[j].transform.position),1);
            }
        }

        string str = "";
        //проходил половину массива
        for (int i = 0; i < countItem; i++)
        {
            str += "<";
            for (int j = 0; j< countItem-i; j++)
            {
                //distanseBeetweenAllPlanes[i, j] = Vector3.Distance(listPlanesCopy[i].transform.position, listPlanesCopy[j].transform.position);
                str += $"{distanseBeetweenAllPlanes[i, j]}_";
            }
            str += "> ";
        }
        Debug.Log(str);
    }
}
