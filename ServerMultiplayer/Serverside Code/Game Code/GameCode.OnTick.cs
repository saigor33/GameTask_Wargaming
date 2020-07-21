using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MushroomsUnity3DExample
{
    public partial class GameCode
    {
        //private event 

        private void OnTick()
        {
            SendMessages();
            CalculationsData();
        }

        private void CalculationsData()
        {
            CaluclationMovingPlayers();
        }

        private void CaluclationMovingPlayers()
        {
            foreach (var player in Players)
            {
                StatusAndСoordinates movingDirection = player.movingDirection;
                if (movingDirection.isEnabled)
                {
                    float moveX = movingDirection.X * player.speedPlayer;
                    float moveY = movingDirection.Y * player.speedPlayer;

                    //TO DO: проверка на возможность двжиения в эти координаты
                    player.posX += moveX;
                    player.posY += moveY;

                    //TO DO: сделать одно большое сообщение и отправлять через Broadcast
                    //AddSendMessageBroadcast(GlobalDataSettings.MESSAGE_TYPE_MOVE_TO_POINT, player.ConnectUserId, moveX, moveY);
                    AddSendMessageBroadcast(GlobalDataSettings.MESSAGE_TYPE_MOVE_TO_POINT, player.ConnectUserId, player.posX, player.posY);
                }
            }
        }

        private void SendMessages()
        {
            //TO DO: добавить подписку на событие, если новый тик, а отправка сообщений не закончилась
            //TO DO: сделать сортировку соощейний и отправлять их большими блоками

            //отправка сообщений всем игрокам
            for (int i = 0; i < _listMessageBroadcastSendNextTicket.Count; i++)
            {
                MessageSend message = _listMessageBroadcastSendNextTicket[i];
                Broadcast(message.TypeMessage, message.ParametrsMessage);
            }
            _listMessageBroadcastSendNextTicket.Clear();

            //отправка сообщений конкретным игрокам
            for (int i = 0; i < _listMessagePlaerSendNextTicket.Count; i++)
            {
                MessageSendPlayer message = _listMessagePlaerSendNextTicket[i];
                Player recipient = message.Player;
                if (recipient != null)
                    recipient.Send(message.TypeMessage, message.ParametrsMessage);
            }
            _listMessagePlaerSendNextTicket.Clear();
        }

    }
}
