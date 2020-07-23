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
            iints = 0;
        }

        public string TypeMessage { get { return _typeMessage; } }
        public object[] ParametrsMessage { get { return _parametrsMessage; } }

        public int iints;
    }
}
