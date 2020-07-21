using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace FlyBattels
{



    public class JoystickContollerMovement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Компоненты джостика")]
        [SerializeField] private GameObject _joystickContainer;
        [SerializeField] private GameObject _joystickButton;
        [SerializeField] private GameObject _joystickBackground;

        public event Action<Vector3> OnChangePositionJoystick;
        public event Action OnDropJoystick;

        private Vector2 _defaultPositionJoystickMovement;
        private Vector3 _directionMovingJoystick;
        private float _radiusMovingJoystic;
        private bool _isMoving;

        private void Awake()
        {
            if (_joystickContainer == null)
                Debug.LogError($"Project({_joystickContainer}): не добавлен объект _joystickMovement джостик урпавления передвиженим");
            _defaultPositionJoystickMovement = _joystickContainer.transform.localPosition;
            Debug.Log($"_defaultPositionJoystickMovement = {_defaultPositionJoystickMovement} local={ _joystickContainer.transform.localPosition}");
            if (_joystickButton == null)
                Debug.LogError($"Project({_joystickButton}): не добавлен объект _joystickButton, отвечающий за направление движения");
            if (_joystickBackground == null)
                Debug.LogError($"Project({_joystickBackground}): не добавлен объект _joystickBackground подложка джостика");

            _radiusMovingJoystic = _joystickBackground.GetComponent<RectTransform>().rect.width / 2f;
        }


        private void Update()
        {
            if(_isMoving)
            {
                _directionMovingJoystick = new Vector2(_joystickButton.transform.position.x - _joystickBackground.transform.position.x,
                    _joystickButton.transform.position.y - _joystickBackground.transform.position.y);
                OnChangePositionJoystick?.Invoke(_directionMovingJoystick);
            }
        }



        public void OnBeginDrag(PointerEventData eventData)
        {

            _joystickContainer.transform.position = eventData.pointerCurrentRaycast.screenPosition;
            _isMoving = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            float distanseBeetwenJoysticButtonAndContainer = Vector3.Distance(eventData.position, _joystickBackground.transform.position);
            _directionMovingJoystick = new Vector2( eventData.position.x - _joystickBackground.transform.position.x, eventData.position.y - _joystickBackground.transform.position.y);
            if (distanseBeetwenJoysticButtonAndContainer > _radiusMovingJoystic)
            {
                float angle = Mathf.Atan2(_directionMovingJoystick.y, _directionMovingJoystick.x) / Mathf.PI * 180;
                float posX = _radiusMovingJoystic * Mathf.Cos(angle * Mathf.PI / 180);
                float posY = _radiusMovingJoystic * Mathf.Sin(angle * Mathf.PI / 180);

                Vector2 newPosButtonJoystic = new Vector2(posX, posY);

                _joystickButton.transform.localPosition = newPosButtonJoystic;
            }
            else
            {
                _joystickButton.transform.position = eventData.pointerCurrentRaycast.screenPosition;
            }
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            _isMoving = false;
            _joystickContainer.transform.localPosition = _defaultPositionJoystickMovement;
            _joystickButton.transform.localPosition = Vector3.zero;
            OnDropJoystick?.Invoke();
        }
    }
}