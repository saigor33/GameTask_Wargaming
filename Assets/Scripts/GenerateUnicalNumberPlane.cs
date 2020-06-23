using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateUnicalNumberPlane 
{
    private static int _lastUnicalNumber = 0;

    /// <summary>
    /// Получить уникальный номер для самолёта
    /// </summary>
    public static int GenerateUnicalNumberForPlane
    {
        get
        {
            _lastUnicalNumber++;
            return _lastUnicalNumber;
        }
    }

}
