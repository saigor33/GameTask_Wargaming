using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MushroomsUnity3DExample
{
    /// <summary>
    /// Общий тип сообщения (подходит для Broadcast)
    /// </summary>
    public struct MessageSend
    {
        private string _typeMessage;
        object[] _parametrsMessage;

        public MessageSend(string typeMessage, params object[] parametrsMessage)
        {
            _typeMessage = typeMessage;
            _parametrsMessage = parametrsMessage;
        }

        public string TypeMessage { get { return _typeMessage; } }
        public object[] ParametrsMessage { get { return _parametrsMessage; } }
    }

    /// <summary>
    /// Тип для конкретного Player (содержит ссылку на Player)
    /// </summary>
    public struct MessageSendPlayer
    {
        private Player _player;
        private string _typeMessage;
        private object[] _parametrsMessage;

        public MessageSendPlayer(Player player, string typeMessage, params object[] parametrsMessage)
        {
            _player = player;
            _typeMessage = typeMessage;
            _parametrsMessage = parametrsMessage;
        }

        public Player Player { get { return _player; } }
        public string TypeMessage { get { return _typeMessage; } }
        public object[] ParametrsMessage { get { return _parametrsMessage; } }
    }
}
