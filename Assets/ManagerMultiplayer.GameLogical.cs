using PlayerIOClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityNightPool;

namespace FlyBattels
{
    public partial class ManagerMultiplayer
    {
        [SerializeField] private ShipManagerController _playerController;
        [SerializeField] private GameObject _prefabPlayerShip;
        private Dictionary<string, ShipOtherPlayerManagerController> _listPlayers = new Dictionary<string, ShipOtherPlayerManagerController>();

        //private bool _serverIsReceiveDirectionMoving;
        //private Vector3 _lastDirectionMoving;

        //private bool _serverIsReceiveDirectionShooting;
        //private Vector3 _lastDirectionShoot;

        private ServerConnect _serverIsReceiveMoving = new ServerConnect();
        private Dictionary<TypeShot, ServerConnect> _serverIsRecieveShooting = new Dictionary<TypeShot, ServerConnect>()
        {
            { TypeShot.UsualShoot, new ServerConnect() }
        };

        public class ServerConnect
        {
            public bool IsConnect { get; set; }
            public Vector3 LastDirection { get; set; }
        }

        #region UpdateListPlayers
        private void SetPlayerParameters(Message message)
        {
            string strTypeShot = message.GetString(0);
            float distansShotUsualSkill = message.GetFloat(1);

            TypeShot typeShot = ParseDate.GetTypeShotByString(strTypeShot);

            _playerController.SetDistansShooting(typeShot, distansShotUsualSkill);
            // TO DO: распределить параметры
        }

        private void OnJoinPlayer(Message message)
        {
            string idPlayerJoined = message.GetString(0);
            Debug.Log($"onJoined id = {idPlayerJoined}");
            float posX = message.GetFloat(1);
            float posY = message.GetFloat(2);
            Vector3 positionNewPlayer = new Vector3(posX, posY, 0);
            GameObject playerJoined = Instantiate(_prefabPlayerShip, positionNewPlayer, Quaternion.identity);
            ShipOtherPlayerManagerController playerJoinedController = playerJoined.GetComponent<ShipOtherPlayerManagerController>();
            playerJoinedController.IdPlayer = idPlayerJoined;

            if (playerJoinedController != null)
            {
                if (!_listPlayers.ContainsKey(idPlayerJoined))
                    _listPlayers.Add(idPlayerJoined, playerJoinedController);
                else
                    Debug.LogError($"Project({this}, _listPlayers): уже содержит ключ {idPlayerJoined}");
            }
            else
            {
                Debug.LogError($"Project({this}, playerJoinedController): нет объекта ShipOtherPlayerManagerController");
            }
        }

        private void OnPlayerLeft(Message message)
        {
            string idPlayerLeft = message.GetString(0);
            //TO DO: что происходит, если ливает текущий игрок?
            if (_playerId == idPlayerLeft)
                return;

            _listPlayers[idPlayerLeft].OnPlayerLeft();
            _listPlayers.Remove(idPlayerLeft);
        }

        #endregion


        #region Moving
        private void MoveToPointFromMessage(Message message)
        {
            string idPlayerMove = message.GetString(0);
            float nextPosX = message.GetFloat(1);
            float nextPosY = message.GetFloat(2);
            Vector3 moveToPositon = new Vector3(nextPosX, nextPosY, 0);

            if (idPlayerMove == _playerId)
            {
                _playerController.MoveToPosition(moveToPositon);
                _serverIsReceiveMoving.IsConnect = true;
            }
            else
                _listPlayers[idPlayerMove].MoveToPoint(moveToPositon); //  .transform.Translate(moveToPositon);

        }

        public void InformMoveToDirection(Vector3 direction)
        {
            if (_serverIsReceiveMoving.IsConnect && _serverIsReceiveMoving.LastDirection == direction)
                return;

            SendMessageOnServer_MoveToPoint(direction);
        }

        public void InformFinishToMove()
        {
            _serverIsReceiveMoving.IsConnect = false;
            _connection.Send(GlobalDataSettings.MESSAGE_TYPE_FINISH_MOVE_TO_POINT);
        }

        private void SendMessageOnServer_MoveToPoint(Vector3 direction)
        {
            Vector3 normalizedDirection = direction.normalized;
            float directX = normalizedDirection.x;
            float directY = normalizedDirection.y;
            _connection.Send(GlobalDataSettings.MESSAGE_TYPE_MOVE_TO_POINT, directX, directY);
        }


        #endregion


        #region Shooting

        public void InformShotToDirection (TypeShot typeShot, Vector3 direction)
        {
            if (_serverIsRecieveShooting[typeShot].IsConnect && _serverIsRecieveShooting[typeShot].LastDirection == direction)
                return;

            //TO DO: сделать проверку, чтобы слать только при изменении или старте

            Vector3 normalizedDirection = direction.normalized;
            float directX = normalizedDirection.x;
            float directY = normalizedDirection.y;

            _serverIsRecieveShooting[typeShot].LastDirection = direction;

            switch (typeShot)
            {
                case TypeShot.UsualShoot:
                    {
                        _connection.Send(GlobalDataSettings.MESSAGE_TYPE_SHOOTING, 
                            GlobalDataSettings.MESSAGE_TYPE_USUAL_SHOT, directX, directY);
                        break;
                    }
            }
        }

        public void PlayerShoot(Message message)
        {
            string typeShot = message.GetString(0);
            string idUserShoot = message.GetString(1);
            float posX = message.GetFloat(2);
            float posY = message.GetFloat(3);
            float directionShootX = message.GetFloat(4);
            float directionShootY = message.GetFloat(5);
            float timeLive = message.GetFloat(6);
            float speedInSecond = message.GetFloat(7);
            int idPrefabInPool = message.GetInt(8);

            if (idUserShoot == _playerId)
                _serverIsRecieveShooting[ParseDate.GetTypeShotByString(typeShot)].IsConnect = true;



            PoolObject bulletPool = PoolManager.Get(idPrefabInPool);
            bulletPool.transform.position = new Vector3(posX, posY, 0);
            //TO DO: сделать поворот пули
            
            Bullet bulletScript = bulletPool.GetComponent<Bullet>();
            Vector2 directionShoot = new Vector2(directionShootX, directionShootY);
            bulletScript.Init(directionShoot, timeLive, speedInSecond);
        }

        public void InformFinishShoot(TypeShot typeShot)
        {
            string strTypeShot = "";
            switch (typeShot)
            {
                case TypeShot.UsualShoot:
                    {
                        strTypeShot = GlobalDataSettings.MESSAGE_TYPE_USUAL_SHOT;
                        break;
                    }
            }

            _serverIsRecieveShooting[typeShot].IsConnect = false;
            _connection.Send(GlobalDataSettings.MESSAGE_TYPE_FINISH_SHOOTING, strTypeShot);
        }

        #endregion


    }
}
