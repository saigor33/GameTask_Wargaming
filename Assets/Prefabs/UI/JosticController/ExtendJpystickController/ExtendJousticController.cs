using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FlyBattels
{
    //public class ExtendJousticController :UsualJoystickContoller
    //{
    //    [SerializeField] GameObject _joystickTransistionBorder;
    //    [SerializeField] float _radiusTransistionBorder;

    //    protected override void CheckErorrData()
    //    {
    //        base.CheckErorrData();

    //        if(_joystickTransistionBorder == null)
    //            Debug.LogError($"Project({_joystickTransistionBorder}): не добавлен объект _joystickTransistionBorder обозначающий границу перехода в другой режим");
    //    }

    //    public override void OnDrag(PointerEventData eventData)
    //    {
    //        //base.OnDrag(eventData);
    //        _directionMovingJoystick = new Vector2(eventData.position.x - _joystickBackground.transform.position.x, eventData.position.y - _joystickBackground.transform.position.y);
    //        float distanseBeetwenJoysticButtonAndContainer = Vector3.Distance(eventData.position, _joystickBackground.transform.position);
    //        //if (distanseBeetwenJoysticButtonAndContainer > _radiusMovingJoystic)
    //        //{
    //        //}
    //        //else
    //        //{
    //        //    _joystickButton.transform.position = eventData.pointerCurrentRaycast.screenPosition;
    //        //}

    //        float angle = Mathf.Atan2(_directionMovingJoystick.y, _directionMovingJoystick.x) / Mathf.PI * 180;
    //        float posX = _radiusMovingJoystic * Mathf.Cos(angle * Mathf.PI / 180);
    //        float posY = _radiusMovingJoystic * Mathf.Sin(angle * Mathf.PI / 180);

    //        Vector2 newPosButtonJoystic = new Vector2(posX, posY);

    //        _joystickButton.transform.localPosition = newPosButtonJoystic;

    //    }

    //}
}