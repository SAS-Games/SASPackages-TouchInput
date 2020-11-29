using UnityEngine;
using System.Collections;
using System;

namespace SAS.TouchManagment
{
    public interface IEventListner
    {

    }

    public interface IPointerDown : IEventListner
    {
        void OnPointerDown(TouchEventData eventData);
    }

    public interface IPointerUp : IEventListner
    {
        void OnPointerUp(TouchEventData eventData);
    }

    public interface IPointerDragStart : IEventListner
    {
        void OnPointerDragStart(TouchEventData eventData);
    }

    public interface IPointerDrag : IEventListner
    {
        void OnPointerDrag(TouchEventData eventData);
    }

    public interface IPointerDragEnd : IEventListner
    {
        void OnPointerDragEnd(TouchEventData eventData);
    }

    public interface IPointerFlick : IEventListner
    {
        void OnPointerFlick(TouchEventData eventData, Vector2 dir);
    }

    public interface ITwoPointerDrag : IEventListner
    {
        void OnPointerTwoFinger(Vector2 dir);
    }
}


