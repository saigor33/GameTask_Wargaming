using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MushroomsUnity3DExample.ConfigsPlayer
{
    public enum TypeBullet
    {
        UsualBullet
    }

    public class FactoryInitBullets
    {
        public static Bullet GetConfigBullet(TypeBullet typeBullet, Player player)
        {
            switch (typeBullet)
            {
                case TypeBullet.UsualBullet:
                    {
                        return InitBullet_Usual(player);
                    }
                default:
                    {
                        Console.WriteLine($"WARRING({nameof(FactoryInitBullets)}): для typeBullet = {typeBullet} не предусмотрен сценарий. Назначен обычный выстред");
                        return InitBullet_Usual(player);
                    }
            }
        }

        private static Bullet InitBullet_Usual(Player player)
        {
            float posX = 0f;
            float posY = 0f;
            float damage = 10f;
            float speedInSecond = 8f;
            float directionX = 0f;
            float directionY = 0f;
            float timeLive = 1f;
            int idPrefabInPool = FlyBattels.GlobalDataSettings.POOL_PREFAB_ID_BULLET_USUAL;

            return new Bullet(posX, posY, damage, speedInSecond,
                directionX, directionY, timeLive, idPrefabInPool, player);
        }
    }
}
