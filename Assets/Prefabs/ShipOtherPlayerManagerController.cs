using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FlyBattels
{
    public class ShipOtherPlayerManagerController : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _textIdPlayer;
        private string _idPlayer;


        public string IdPlayer
        {
            get { return _idPlayer; }
            set
            {
                _idPlayer = value;
                _textIdPlayer.text = _idPlayer;
            }
        }

        private void Awake()
        {
            if (_textIdPlayer == null)
                Debug.LogError($"Project({this}, _textIdPlayer): Не добавлен объект для отображения ника");
        }


        public void MoveToPoint(Vector3 targetPositon)
        {
            // transform.Translate(translation);
            StartCoroutine(MoveOnTime(GlobalDataSettings.TIME_TICK, targetPositon));
        }

        // ==== Дублированный код из ShipManagerController ============
        private IEnumerator MoveOnTime(float time, Vector2 targetPosition)
        {
            Vector2 startPosition = transform.position;
            float startTime = Time.realtimeSinceStartup;
            float fraction = 0f;
            while (fraction < 1f)
            {
                fraction = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / time);
                transform.position = Vector2.Lerp(startPosition, targetPosition, fraction);
                yield return null;
            }
        }


        public void OnPlayerLeft()
        {
            Destroy(gameObject);
        }
    }
}