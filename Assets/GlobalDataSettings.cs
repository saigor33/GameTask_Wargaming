using System.Collections;
using System.Collections.Generic;


namespace FlyBattels
{

    public class GlobalDataSettings
    {
        public const string PLAYER_IO_GAME_ID = "airbattel-ozp0hw4waectvgyeevkvg";

        public const string MESSAGE_TYPE_MOVE_TO_POINT = "Move";
        public const string MESSAGE_TYPE_FINISH_MOVE_TO_POINT = "FinishMove";
        public const string MESSAGE_TYPE_PLAYER_JOINED = "PlayerJoined";
        public const string MESSAGE_TYPE_PLAYER_LEFT = "PlayerLeft";
        public const string MESSAGE_TYPE_PLAYER_PARAMETERS = "PlayerParameters";

        public const string MESSAGE_TYPE_SHOOTING = "Shooting";
        public const string MESSAGE_TYPE_FINISH_SHOOTING = "FinishShooting";
        public const string MESSAGE_TYPE_USUAL_SHOT = "UsualShot";

        public const int POOL_PREFAB_ID_BULLET_USUAL = 2;

        public const float TIME_TICK = 0.025f;
    }
}