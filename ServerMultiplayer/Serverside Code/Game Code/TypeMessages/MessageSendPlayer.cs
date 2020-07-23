namespace MushroomsUnity3DExample
{
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
