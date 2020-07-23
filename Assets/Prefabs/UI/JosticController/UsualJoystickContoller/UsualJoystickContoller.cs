using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace FlyBattels
{



    public class UsualJoystickContoller : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Компоненты джостика")]
        [SerializeField] protected GameObject _joystickContainer;
        [SerializeField] protected GameObject _joystickButton;
        [SerializeField] protected GameObject _joystickBackground;
        [SerializeField] protected GameObject _joystickTransistionBorder;

        public event Action<Vector3> OnChangePositionJoystick_ModeUsed;
        public event Action<Vector3> OnChangePositionJoystick_ModeTargeting;
        public event Action OnDropJoystick;

        protected Vector2 _defaultPositionJoystickMovement;
        protected Vector3 _directionMovingJoystick;
        protected float _radiusMaxMovingJoystic;
        protected float _borderTransitionRadius;
        protected bool _isMoving;

        private void Awake()
        {
            CheckErorrData();
            _defaultPositionJoystickMovement = _joystickContainer.transform.localPosition;
            //Debug.Log($"_defaultPositionJoystickMovement = {_defaultPositionJoystickMovement} local={ _joystickContainer.transform.localPosition}");
            _radiusMaxMovingJoystic = _joystickBackground.GetComponent<RectTransform>().rect.width / 2f;
            _borderTransitionRadius = _joystickTransistionBorder.GetComponent<RectTransform>().rect.width / 2f;
        }



        private void Update()
        {
            //if (_isMoving)
            //{
            //    _directionMovingJoystick = new Vector2(_joystickButton.transform.position.x - _joystickBackground.transform.position.x,
            //        _joystickButton.transform.position.y - _joystickBackground.transform.position.y);
            //    OnChangePositionJoystick?.Invoke(_directionMovingJoystick);
            //}
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {

            _joystickContainer.transform.position = eventData.pointerCurrentRaycast.screenPosition;
            _isMoving = true;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            _directionMovingJoystick = new Vector2(eventData.position.x - _joystickBackground.transform.position.x, eventData.position.y - _joystickBackground.transform.position.y);
            float distanseBeetwenJoysticButtonAndContainer = Vector3.Distance(eventData.position, _joystickBackground.transform.position);

            float radiusCircleMode = (distanseBeetwenJoysticButtonAndContainer <= _borderTransitionRadius) ?
                _borderTransitionRadius : _radiusMaxMovingJoystic;

            float angle = Mathf.Atan2(_directionMovingJoystick.y, _directionMovingJoystick.x) / Mathf.PI * 180;
            float posX = radiusCircleMode * Mathf.Cos(angle * Mathf.PI / 180);
            float posY = radiusCircleMode * Mathf.Sin(angle * Mathf.PI / 180);

            Vector2 newPosButtonJoystic = new Vector2(posX, posY);

            _joystickButton.transform.localPosition = newPosButtonJoystic;

            if (distanseBeetwenJoysticButtonAndContainer <= _borderTransitionRadius)
                OnChangePositionJoystick_ModeTargeting?.Invoke(_directionMovingJoystick);
            else
                OnChangePositionJoystick_ModeUsed?.Invoke(_directionMovingJoystick);
        }


        public virtual void OnEndDrag(PointerEventData eventData)
        {
            _isMoving = false;
            _joystickContainer.transform.localPosition = _defaultPositionJoystickMovement;
            _joystickButton.transform.localPosition = Vector3.zero;
            OnDropJoystick?.Invoke();
        }

        protected virtual void CheckErorrData()
        {
            if (_joystickContainer == null)
                Debug.LogError($"Project({_joystickContainer}): не добавлен объект _joystickMovement джостик урпавления передвиженим");
            if (_joystickButton == null)
                Debug.LogError($"Project({_joystickButton}): не добавлен объект _joystickButton, отвечающий за направление движения");
            if (_joystickBackground == null)
                Debug.LogError($"Project({_joystickBackground}): не добавлен объект _joystickBackground подложка джостика");

            if (_joystickTransistionBorder == null)
                Debug.LogError($"Project({_joystickTransistionBorder}): не добавлен объект _joystickTransistionBorder обозначающий границу перехода в другой режим");
        }

    }
}