using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace FlyBattels
{
    public class ButtonDisplayAndroid : MonoBehaviour, IPointerClickHandler
    {

        public event Action OnClickButton;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClickButton?.Invoke();
        }
    }
}
