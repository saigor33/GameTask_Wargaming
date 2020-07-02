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
        [SerializeField] private GameObject _joystickMovement;
        [SerializeField] private GameObject _joystickButton;
        [SerializeField] private GameObject _joystickContainer;

        public event Action<Vector3> OnChangePositionJoystick;

        private Vector2 _defaultPositionJoystickMovement;
        private Vector3 _directionMovingJoystick;
        private float _radiusMovingJoystic;
        private bool _isMoving;

        private void Awake()
        {
            if (_joystickMovement == null)
                Debug.LogError($"Project({_joystickMovement}): не добавлен объект _joystickMovement джостик урпавления передвиженим");
            _defaultPositionJoystickMovement = _joystickMovement.transform.position;

            if (_joystickButton == null)
                Debug.LogError($"Project({_joystickButton}): не добавлен объект _joystickButton, отвечающий за направление движения");
            if (_joystickContainer == null)
                Debug.LogError($"Project({_joystickContainer}): не добавлен объект _joystickBackground подложка джостика");

            _radiusMovingJoystic = _joystickContainer.GetComponent<RectTransform>().rect.width / 2f;
        }


        private void Update()
        {
            if(_isMoving)
            {
                _directionMovingJoystick = new Vector2(_joystickButton.transform.position.x - _joystickContainer.transform.position.x,
                    _joystickButton.transform.position.y - _joystickContainer.transform.position.y);
                OnChangePositionJoystick.Invoke(_directionMovingJoystick);
            }
        }



        public void OnBeginDrag(PointerEventData eventData)
        {

            _joystickMovement.transform.position = eventData.pointerCurrentRaycast.screenPosition;
            _isMoving = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            float distanseBeetwenJoysticButtonAndContainer = Vector3.Distance(eventData.position, _joystickContainer.transform.position);
            _directionMovingJoystick = new Vector2( eventData.position.x - _joystickContainer.transform.position.x, eventData.position.y - _joystickContainer.transform.position.y);
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
            _joystickMovement.transform.position = _defaultPositionJoystickMovement;
            _joystickButton.transform.localPosition = Vector3.zero;
            _isMoving = false;
        }
    }
}