using PlayerIO.GameLibrary;

namespace MushroomsUnity3DExample
{
    public class Player : BasePlayer
    {
        public float posX = 0;
        public float posY = 0;
        public StatusAndСoordinates movingDirection;
        public int toadspicked = 0;
        public float speedPlayer = 0.1f;
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