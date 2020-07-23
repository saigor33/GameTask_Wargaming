using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyBattels;
using PlayerIO.GameLibrary;

namespace MushroomsUnity3DExample
{
    public partial class GameCode
    {
        public float _timeAfterStart = 0;

        private List<Bullet> _listBulletCreated = new List<Bullet>();

        private void OnTick()
        {
            CalculationsData();
            SendMessages();

            _timeAfterStart += GlobalDataSettings.TIME_TICK;
        }

        private void CalculationsData()
        {
            CaluclationMovingPlayers();
            CalculationShootingPlayers();
            CalculationMovingBullets();
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

        private void CalculationShootingPlayers()
        {
            foreach (var player in Players)
            {
                StatusAndСoordinates shootingDirection = player.shootingDirection;
                if (shootingDirection.isEnabled)
                {
                    Skill usedSkill = player.UsualSkill;
                    if (_timeAfterStart - usedSkill._timeLastUse >= usedSkill._cooldown)
                    {
                        Bullet bullet = usedSkill._bulletConfig;

                        bullet.SetPosition(player.posX, player.posY);
                        bullet.DirectionX = shootingDirection.X;
                        bullet.DirectionY = shootingDirection.Y;
                        bullet.TimeStartShoot = _timeAfterStart;

                        AddSendMessageBroadcast(GlobalDataSettings.MESSAGE_TYPE_SHOOTING, GlobalDataSettings.MESSAGE_TYPE_USUAL_SHOT, 
                            player.ConnectUserId,
                            bullet.PosX,
                            bullet.PosY, 
                            bullet.DirectionX, 
                            bullet.DirectionY,
                            bullet.TimeLive,
                            bullet.SpeedInSecond,
                            bullet.IdPrefabInPool);

                        _listBulletCreated.Add(bullet);

                        usedSkill._timeLastUse = _timeAfterStart;
                        player.UsualSkill = usedSkill;
                    }
                }
            }
        }

        private void CalculationMovingBullets()
        {
            for (int i = _listBulletCreated.Count-1; i>=0; i--)
            {
                Bullet bullet = _listBulletCreated[i];

                if (_timeAfterStart - bullet.TimeStartShoot >= bullet.TimeLive)
                {
                    _listBulletCreated.RemoveAt(i);
                    Console.WriteLine($"LastPositinBullet = ({bullet.PosX}; {bullet.PosY}) ");
                    continue;
                }

                //TO DO: проверка на столкновение с противников и с декором

                bullet.PosX += bullet.DirectionX * bullet.SpeedInTick;
                bullet.PosY += bullet.DirectionY * bullet.SpeedInTick;
                _listBulletCreated[i] = bullet;
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
