using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIOClient;
using System;

namespace FlyBattels
{
    public partial class ManagerMultiplayer : MonoBehaviour
    {

        private Connection _connection;
        private List<Message> _msgList = new List<Message>(); //  Messsage queue implementation

        private string _playerId;

        private void Awake()
        {
            if (_playerController == null)
                Debug.LogError($"Project({this}, _playerController): Не добавлен корабль игрока");

        }

        private void Start()
        {
            _playerId = System.DateTime.Now.ToString();
            Dictionary<string, string> dictionaryUser = new Dictionary<string, string>
             {
                { "userId", _playerId },
             };

            PlayerIO.Authenticate(
                GlobalDataSettings.PLAYER_IO_GAME_ID,            //Your game id
                "public",                                       //Your connection id
                dictionaryUser,                                 //Authentication arguments
                null,                                           //PlayerInsight segments
                OnConnectPlayerIO,
                OnErrorConnectPlayerIO
                );
        }



        void FixedUpdate()
        {
            // process message queue
            foreach (Message message in _msgList)
            {
                switch (message.Type)
                {
                    case GlobalDataSettings.MESSAGE_TYPE_PLAYER_PARAMETERS:
                        {
                            SetPlayerParameters(message);
                            break;
                        }
                    case GlobalDataSettings.MESSAGE_TYPE_MOVE_TO_POINT:
                        {
                            MoveToPointFromMessage(message);
                            break;
                        }
                    case GlobalDataSettings.MESSAGE_TYPE_PLAYER_JOINED:
                        {
                            OnJoinPlayer(message);
                            break;
                        }
                    case GlobalDataSettings.MESSAGE_TYPE_PLAYER_LEFT:
                        {
                            OnPlayerLeft(message);
                            break;
                        }
                    case GlobalDataSettings.MESSAGE_TYPE_SHOOTING:
                        {
                            PlayerShoot(message);
                            break;
                        }
                    default:
                        {
                            Debug.Log($"Project ({this}): message.Type)= {message.Type} Сценарий не предусмотрен");
                            break;
                        }
                }
            }
            _msgList.Clear();
        }

        private void OnApplicationQuit()
        {
            _connection.Disconnect();
        }



        private void OnConnectPlayerIO(Client client)
        {
            Debug.Log("Successfully connected to Player.IO");

            //target.transform.Find("NameTag").GetComponent<TextMesh>().text = _userId;
            //target.transform.name = _userId;

            Debug.Log("Create ServerEndpoint");
            // Comment out the line below to use the live servers instead of your development server
            client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

            Debug.Log("CreateJoinRoom");
            //Create or join the room 
            client.Multiplayer.CreateJoinRoom(
                    "UnityDemoRoom",                    //Room id. If set to null a random roomid is used
                    "UnityMushrooms",                   //The room type started on the server
                    true,                               //Should the room be visible in the lobby?
                    null,
                    null,
                    OnCorrectJoinRoom,
                    OnErrorJoinRoom
                    );
        }

        private void OnErrorConnectPlayerIO(PlayerIOError error)
        {
            Debug.Log("Error connecting: " + error.ToString());
        }

        private void OnCorrectJoinRoom(Connection conn)
        {
            Debug.Log("Joined Room.");
            // We successfully joined a room so set up the message handler
            _connection = conn;
            _connection.OnMessage += HandleMessage;
        }

        private void OnErrorJoinRoom(PlayerIOError error)
        {
            Debug.Log("Error Joining Room: " + error.ToString());
        }

        private void HandleMessage(object sender, Message m)
        {
            _msgList.Add(m);
        }

    }
}