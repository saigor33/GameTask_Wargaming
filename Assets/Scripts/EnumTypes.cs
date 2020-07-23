using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlyBattels
{
    public enum TypeShot
    {
        UsualShoot
    }

    public class ParseDate
    {
        public static TypeShot GetTypeShotByString (string strTypeShot)
        {
            switch(strTypeShot)
            {
                case GlobalDataSettings.MESSAGE_TYPE_USUAL_SHOT: return TypeShot.UsualShoot;
                default:
                    {
                        Debug.LogError($"Project(strTypeShot = {strTypeShot}): сценарий не предусмотрен. Возвращён обычный выстрел");
                        return TypeShot.UsualShoot;
                    }
            }
        }
    }
}
