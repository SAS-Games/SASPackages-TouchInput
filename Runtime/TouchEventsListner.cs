using System;
using UnityEngine;

namespace SAS.TouchManagment
{
    public sealed class TouchEventsListner : MonoBehaviour
    {
        private IPointerDown _pointerDownListner;
        private IPointerUp _pointerUpListner;
        private IPointerDragStart _pointerDragStartListner;
        private IPointerDrag _pointerDragListner;
        private IPointerDragEnd _pointerDragEndListner;
        private IPointerFlick _pointerFlickedListner;

        private TouchManager touchManager;

        private void Awake()
        {
            touchManager = TouchManager.Instance;
            IEventListner[] listners = GetComponents<IEventListner>();
            foreach (IEventListner listner in listners)
                RegisterListener(listner as IEventListner);
        }

        private void RegisterListener(IEventListner listener)
        {
            if (listener is IPointerDown)
                _pointerDownListner = (listener as IPointerDown);
            if (listener is IPointerUp)
                _pointerUpListner = (listener as IPointerUp);
            if (listener is IPointerDragStart)
                _pointerDragStartListner = (listener as IPointerDragStart);
            if (listener is IPointerDrag)
                _pointerDragListner = (listener as IPointerDrag);
            if (listener is IPointerDragEnd)
                _pointerDragEndListner = (listener as IPointerDragEnd);
            if (listener is IPointerFlick)
                _pointerFlickedListner = (listener as IPointerFlick);
        }

        private void OnEnable()
        {
            touchManager?.AddEvents(OnPointerDown, OnPointerUp, OnPointerDragStart, OnPointerDrag, OnPointerDragEnd, OnPointerFlick);
        }

        private void OnDisable()
        {
            touchManager?.RemoveEvents(OnPointerDown, OnPointerUp, OnPointerDragStart, OnPointerDrag, OnPointerDragEnd, OnPointerFlick);
        }

        void OnPointerDown(TouchEventData eventData)
        {
            _pointerDownListner?.OnPointerDown(eventData);
        }

        void OnPointerUp(TouchEventData eventData)
        {
            _pointerUpListner?.OnPointerUp(eventData);
        }

        void OnPointerDragStart(TouchEventData eventData)
        {
            _pointerDragStartListner?.OnPointerDragStart(eventData);
        }

        void OnPointerDrag(TouchEventData eventData)
        {
            _pointerDragListner?.OnPointerDrag(eventData);
        }

        void OnPointerDragEnd(TouchEventData eventData)
        {
            _pointerDragEndListner?.OnPointerDragEnd(eventData);
        }

        void OnPointerFlick(TouchEventData eventData, Vector2 dir)
        {
            _pointerFlickedListner?.OnPointerFlick(eventData, dir);
        }
    }
}
