using System;
using System.Collections.Generic;
using PlayerIO.GameLibrary;
using FlyBattels;


namespace MushroomsUnity3DExample
{
    public class Toad
    {
        public int id = 0;
        public float posx = 0;
        public float posz = 0;
    }

    [RoomType("UnityMushrooms")]
    public partial class GameCode : Game<Player>
    {
        private List<MessageSend> _listMessageBroadcastSendNextTicket = new List<MessageSend>();
        private List<MessageSendPlayer> _listMessagePlaerSendNextTicket = new List<MessageSendPlayer>();



        // This method is called when an instance of your the game is created
        public override void GameStarted()
        {
            //// anything you write to the Console will show up in the 
            //// output window of the development server
            //Console.WriteLine("Game is started: " + RoomId);


            //// respawn new toads each 5 seconds
            ////AddTimer(respawntoads, 5000);
            //// reset game every 2 minutes
            ////AddTimer(resetgame, 120000);
            int timveTickIn_ms = (int)(GlobalDataSettings.TIME_TICK * 1000); //перевод тиков в ms
            AddTimer(OnTick, timveTickIn_ms);

        }

        private void AddSendMessageBroadcast(string typeMessage, params object[] parametrs)
        {
            MessageSend message = new MessageSend(typeMessage, parametrs);
            _listMessageBroadcastSendNextTicket.Add(message);
        }

        private void AddSendMessagePlayer (Player player, string typeMessage, params object[] parametrs)
        {
            MessageSendPlayer message = new MessageSendPlayer(player, typeMessage, parametrs);
            _listMessagePlaerSendNextTicket.Add(message);
        }



        // This method is called when the last player leaves the room, and it's closed down.
        public override void GameClosed()
        {
            Console.WriteLine("RoomId: " + RoomId);
        }

        // This method is called whenever a player joins the game
        public override void UserJoined(Player currentPlayer)
        {
            Console.WriteLine($"user {currentPlayer.Id} join");
            SendParametrsPlayer(currentPlayer);

            foreach (Player player in Players)
            {
                if (player.ConnectUserId != currentPlayer.ConnectUserId)
                {
                    //player.Send("PlayerJoined", currentPlayer.ConnectUserId, 0, 0);
                    //currentPlayer.Send("PlayerJoined", player.ConnectUserId, player.posX, player.posY);
                    AddSendMessagePlayer(player, GlobalDataSettings.MESSAGE_TYPE_PLAYER_JOINED, currentPlayer.ConnectUserId, 0, 0);
                    AddSendMessagePlayer(currentPlayer, GlobalDataSettings.MESSAGE_TYPE_PLAYER_JOINED, player.ConnectUserId, player.posX, player.posY);
                }
            }

        }

        // This method is called when a player leaves the game
        public override void UserLeft(Player player)
        {
            //Broadcast("PlayerLeft", player.ConnectUserId);
            AddSendMessageBroadcast(GlobalDataSettings.MESSAGE_TYPE_PLAYER_LEFT, player.ConnectUserId);
        }

        // This method is called when a player sends a message into the server code
        public override void GotMessage(Player player, Message message)
        {
            switch (message.Type)
            {
                case GlobalDataSettings.MESSAGE_TYPE_MOVE_TO_POINT:
                    {
                        float directPosX = message.GetFloat(0);
                        float directPosY = message.GetFloat(1);

                        StatusAndСoordinates movingDirection = player.movingDirection;
                        movingDirection.InitParametrs(true, directPosX, directPosY);
                        player.movingDirection = movingDirection;
                        break;
                    }
                case GlobalDataSettings.MESSAGE_TYPE_FINISH_MOVE_TO_POINT:
                    {
                        player.movingDirection.isEnabled = false;
                        break;
                    }
                case GlobalDataSettings.MESSAGE_TYPE_SHOOTING:
                    {
                        Shooting(player, message);
                        break;
                    }
                case GlobalDataSettings.MESSAGE_TYPE_FINISH_SHOOTING:
                    {
                        FinishShooting(player, message);
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"WARRING: message.Type = {message.Type} не предусмотрен сценарий");
                        break;
                    }
            }

        }

        private void Shooting(Player player, Message message)
        {
            string typeShoot = message.GetString(0);

            float directPosX = message.GetFloat(1);
            float directPosY = message.GetFloat(2);
            StatusAndСoordinates shootingDirection = player.shootingDirection;
            shootingDirection.InitParametrs(true, directPosX, directPosY);

            switch (typeShoot)
            {
                case GlobalDataSettings.MESSAGE_TYPE_USUAL_SHOT:
                    {
                        player.shootingDirection = shootingDirection;
                        break; 
                    }
            }
        }

        private void FinishShooting(Player player, Message message)
        {
            string typeShoot = message.GetString(0);
            switch (typeShoot)
            {
                case GlobalDataSettings.MESSAGE_TYPE_USUAL_SHOT:
                    {
                        StatusAndСoordinates shootingDirection = player.shootingDirection;
                        shootingDirection.isEnabled = false;
                        player.shootingDirection = shootingDirection;
                        break;
                    }
            }
        }

        private void SendParametrsPlayer(Player player)
        {
            Skill usedSkill = player.UsualSkill;
            Bullet bulletConfig = usedSkill._bulletConfig;
            float distansShotUsualSkill = bulletConfig.SpeedInSecond * bulletConfig.TimeLive;
            AddSendMessagePlayer(
                player,
                GlobalDataSettings.MESSAGE_TYPE_PLAYER_PARAMETERS,
                GlobalDataSettings.MESSAGE_TYPE_USUAL_SHOT,
                distansShotUsualSkill);
                }

        // =============================================================================================================================
        // =============================================================================================================================
        // =============================================================================================================================
        // =============================================================================================================================
        // =============================================================================================================================
        // =============================================================================================================================

        // This method is called when an instance of your the game is created
        //public override void GameStarted()
        //{
        //    // anything you write to the Console will show up in the 
        //    // output window of the development server
        //    Console.WriteLine("Game is started: " + RoomId);

        //    // spawn 10 toads at server start
        //    System.Random random = new System.Random();
        //    for (int x = 0; x < 10; x++)
        //    {

        //        int px = random.Next(-9, 9);
        //        int pz = random.Next(-9, 9);
        //        Toad temp = new Toad();
        //        temp.id = last_toad_id;
        //        temp.posx = px;
        //        temp.posz = pz;
        //        Toads.Add(temp);
        //        last_toad_id++;

        //    }

        //    // respawn new toads each 5 seconds
        //    //AddTimer(respawntoads, 5000);
        //    // reset game every 2 minutes
        //    //AddTimer(resetgame, 120000);
        //}

        //// This method is called when a player sends a message into the server code
        //public override void GotMessage(Player player, Message message)
        //{
        //     switch (message.Type)
        //     {
        //         // called when a player clicks on the ground
        //         case "Move":
        //             player.posx = message.GetFloat(0);
        //             player.posz = message.GetFloat(1);
        //             Broadcast("Move", player.ConnectUserId, player.posx, player.posz);
        //             break;
        //         case "MoveHarvest":
        //             // called when a player clicks on a harvesting node
        //             // sends back a harvesting command to the player, a move command to everyone else
        //             player.posx = message.GetFloat(0);
        //             player.posz = message.GetFloat(1);
        //             foreach (Player pl in Players)
        //             {
        //                 if (pl.ConnectUserId != player.ConnectUserId)
        //                 {
        //                     pl.Send("Move", player.ConnectUserId, player.posx, player.posz);
        //                 }
        //             }
        //             player.Send("Harvest", player.ConnectUserId, player.posx, player.posz);
        //             break;
        //         case "Pickup":
        //             // called when the player is actually close to the harvesting node
        //             int pickupid = int.Parse(message.GetString(0).Replace("Toad", ""));

        //             // Find a toad by its id
        //             Toad result = Toads.Find(delegate (Toad td) { return td.id == pickupid; });

        //             if (result != null)
        //             {
        //                 // sends everyone information that a toad as been picked up
        //                 // increases player toad count
        //                 Broadcast("Picked", result.id);
        //                 Toads.Remove(result);
        //                 player.toadspicked++;
        //                 player.Send("ToadCount", player.toadspicked);
        //             }
        //             else
        //             {
        //                 // id of the toad doesn't exist, either the player
        //                 // is trying to cheat, or someone else already picked 
        //                 // that toadstool
        //                 Console.WriteLine("Not found: {0}", pickupid);
        //             }
        //             break;
        //         case "Chat":
        //             foreach (Player pl in Players)
        //             {
        //                 if (pl.ConnectUserId != player.ConnectUserId)
        //                 {
        //                     pl.Send("Chat", player.ConnectUserId, message.GetString(0));
        //                 }
        //             }
        //             break;
        //     }
        //}
        //private void resetgame()
        //{
        //    // scoring system
        //    Player winner = new Player();
        //    int maxscore = -1;
        //    foreach (Player pl in Players)
        //    {
        //        if (pl.toadspicked > maxscore)
        //        {
        //            winner = pl;
        //            maxscore = pl.toadspicked;
        //        }
        //    }

        //    // broadcast who won the round
        //    if (winner.toadspicked > 0)
        //    {
        //        Broadcast("Chat", "Server", winner.ConnectUserId + " picked " + winner.toadspicked + " Toadstools and won this round.");
        //    }
        //    else
        //    {
        //        Broadcast("Chat", "Server", "No one won this round.");
        //    }

        //    // reset everyone's score
        //    foreach (Player pl in Players)
        //    {
        //        pl.toadspicked = 0;
        //    }
        //    Broadcast("ToadCount", 0);
        //}

        //private void respawntoads()
        //{
        //    if (Toads.Count == 10)
        //        return;

        //    System.Random random = new System.Random();
        //    // create new toads if there are less than 10
        //    for (int x = 0; x < 10 - Toads.Count; x++)
        //    {
        //        int px = random.Next(-9, 9);
        //        int pz = random.Next(-9, 9);
        //        Toad temp = new Toad();
        //        temp.id = last_toad_id;
        //        temp.posx = px;
        //        temp.posz = pz;
        //        Toads.Add(temp);
        //        last_toad_id++;

        //        // broadcast new toad information to all players
        //        Broadcast("Toad", temp.id, temp.posx, temp.posz);
        //    }
        //}
    }
}