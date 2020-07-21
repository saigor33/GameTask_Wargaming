using PlayerIOClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlyBattels
{
    public partial class ManagerMultiplayer
    {
        [SerializeField] private ShipManagerController _playerController;
        [SerializeField] private GameObject _prefabPlayerShip;
        private Dictionary<string, ShipOtherPlayerManagerController> _listPlayers = new Dictionary<string, ShipOtherPlayerManagerController>();

        private bool _serverIsReceiveDirectionMoving;
        private Vector3 _lastDirectionMoving;

        private void MoveToPointFromMessage(Message message)
        {
            string idPlayerMove = message.GetString(0);
            float nextPosX = message.GetFloat(1);
            float nextPosY = message.GetFloat(2);
            Vector3 moveToPositon = new Vector3(nextPosX, nextPosY, 0);

            if (idPlayerMove == _playerId)
                _playerController.MoveToPosition(moveToPositon);
            else
                _listPlayers[idPlayerMove].MoveToPoint(moveToPositon); //  .transform.Translate(moveToPositon);

            _serverIsReceiveDirectionMoving = true;
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

        public void InformMoveToDirection(Vector3 direction)
        {
            if (_serverIsReceiveDirectionMoving)
            {
                if (_lastDirectionMoving != direction)
                    SendMessageOnServer_MoveToPoint(direction);
            }
            else
            {
                SendMessageOnServer_MoveToPoint(direction);
            }
        }

        public void InformFinishToMove()
        {
            _serverIsReceiveDirectionMoving = false;
            _connection.Send(GlobalDataSettings.MESSAGE_TYPE_FINISH_MOVE_TO_POINT);
        }

        private void SendMessageOnServer_MoveToPoint(Vector3 direction)
        {
            Vector3 normalizedDirection = direction.normalized;
            float directX = normalizedDirection.x;
            float directY = normalizedDirection.y;
            _connection.Send(GlobalDataSettings.MESSAGE_TYPE_MOVE_TO_POINT, directX, directY);

            //Vector3 normalizedDirection = direction.normalized;
            //float directX = normalizedDirection.x;
            //float directY = normalizedDirection.y;
            //_connection.Send(GlobalDataSettings.MESSAGE_TYPE_MOVE_TO_POINT, directX, directY);
        }

    }
}
