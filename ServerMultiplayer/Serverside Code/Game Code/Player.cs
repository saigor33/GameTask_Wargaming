using PlayerIO.GameLibrary;

namespace MushroomsUnity3DExample
{
    public class Player : BasePlayer
    {
        public float posX = 0;
        public float posY = 0;
        public StatusAndСoordinates movingDirection;
        public StatusAndСoordinates shootingDirection;
        //public Bullet usualBullet;
        public int toadspicked = 0;
        public float speedPlayer = 0.1f;

        public Skill UsualSkill;

        public Player():base()
        {
            Bullet usualBullet = ConfigsPlayer.FactoryInitBullets
                .GetConfigBullet(ConfigsPlayer.TypeBullet.UsualBullet, this);
            UsualSkill = new Skill(0.2f, usualBullet);
        }
    }

    public struct StatusAndСoordinates
    {
        public bool isEnabled;
        public float X;
        public float Y;

        public StatusAndСoordinates (bool isEnabled= false, float coordinateX=0, float coordinateY=0)
        {
            this.isEnabled = isEnabled;
            X = coordinateX;
            Y = coordinateY;
        }

        public void InitParametrs (bool isEnabled = false, float coordinateX = 0, float coordinateY = 0)
        {
            this.isEnabled = isEnabled;
            X = coordinateX;
            Y = coordinateY;
        }
    }
}