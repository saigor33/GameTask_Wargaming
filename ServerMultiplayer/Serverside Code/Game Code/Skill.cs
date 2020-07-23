using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MushroomsUnity3DExample
{
    public struct Skill
    {
        public float _cooldown;
        public float _timeLastUse;
        public Bullet _bulletConfig;

        public Skill(float cooldown, Bullet bulletConfig)
        {
            _cooldown = cooldown;
            _timeLastUse = -_cooldown;
            _bulletConfig = bulletConfig;
        }
    }
}
