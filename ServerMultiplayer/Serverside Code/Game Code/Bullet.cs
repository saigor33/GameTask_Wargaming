using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MushroomsUnity3DExample
{
    public struct Bullet
    {
        private Player _playerShot;
        private int _idPrefab;

        private float _damage;
        private float _speedInSecond;
        private float _speedInTick;

        private float _timeStartShoot;
        private float _timeLive;

        private float _directionX;
        private float _directionY;
        private float _posX;
        private float _posY;



        public Bullet (float posX, float posY, float damage, float speedInSecnd, float directionX, float directionY, 
            float timeLive, int idPrefab, Player player = null)
        {
            _posX = posX;
            _posY = posY;
            if (damage < 0)
            {
                Console.WriteLine($"WARRING: _damage = {damage} равен отрицательному значению. Значение установлено в 0");
                _damage = 0;
            }
            else
            {
                _damage = damage;
            }

            _speedInSecond = speedInSecnd;
            _speedInTick = _speedInSecond * FlyBattels.GlobalDataSettings.TIME_TICK;
            _directionX = directionX;
            _directionY= directionY;
            _timeStartShoot = 0;
            _timeLive = timeLive;

            _idPrefab = idPrefab;
            _playerShot = player;
        }

        public float Damage
        {
            get { return _damage; }
            set
            {
                if (value >= 0)
                    _damage = value;
                else
                    Console.WriteLine($"WARRING: _damage = {value} равен отрицательному значению. Значение не присвоено");
            }
        }

        public float SpeedInSecond
        {
            get { return _speedInSecond; }
            set
            {
                if (value > 0)
                {
                    _speedInSecond = value;
                    _speedInTick = GetSpeedInTick(value);
                }
                else
                    Console.WriteLine($"WARRING: _speedInSecond = {value} равен отрицательному значению (или 0). Значение не присвоено");
            }
        }

        public float SpeedInTick { get { return _speedInTick; } }

        public float DirectionX
        {
            get { return _directionX; }
            set { _directionX = value; }
        }

        public float DirectionY
        {
            get { return _directionY; }
            set { _directionY = value; }
        }

        public float PosX
        {
            get { return _posX; }
            set { _posX = value; }
        }

        public float PosY
        {
            get { return _posY; }
            set { _posY = value; }
        }

        public float TimeLive
        {
            get { return _timeLive; }
            set { _timeLive = value; }
        }

        public float TimeStartShoot
        {
            get { return _timeStartShoot; }
            set { _timeStartShoot = value; }
        }

        public int IdPrefabInPool
        {
            get { return _idPrefab; }
            set { _idPrefab = value; }
        }

        public Player PlayerShot
        {
            get { return _playerShot; }
            set { _playerShot = value; }
        }

        private float GetSpeedInTick (float speedInSecond)
        {
           return speedInSecond * FlyBattels.GlobalDataSettings.TIME_TICK;
        }

        public void SetPosition (float posX, float posY)
        {
            _posX = posX;
            _posY = posY;
        }
    }
}
