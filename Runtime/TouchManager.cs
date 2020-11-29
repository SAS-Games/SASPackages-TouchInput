using UnityEngine;
using System;
using SAS.Utilities;

namespace SAS.TouchManagment
{
    public struct TouchEventData
    {
        public TouchPhase phase;
        public int fingerID { get; private set; }
        public Vector2 position { get; set; }
        public Vector2 pressPosition { get; set; }
        public float dragStartTime { get; set; }
        public Vector2 dragStartPosition { get; set; }
        public Vector2 delta { get; set; }
        public int clickCount { get; set; }
        public float clickTime { get; set; }

        public TouchEventData(int fingerID)
        {
            this.fingerID = fingerID;
            this.position = Vector2Int.zero;
            this.pressPosition = Vector2Int.zero;
            this.delta = Vector2Int.zero;
            this.clickCount = 0;
            this.clickTime = 0;
            this.dragStartTime = 0;
            this.dragStartPosition = Vector2Int.zero;
            this.phase = TouchPhase.None;
        }

        public void SetDefault()
        {
            this.position = Vector2Int.zero;
            this.pressPosition = Vector2Int.zero;
            this.delta = Vector2Int.zero;
            this.clickCount = 0;
            this.dragStartTime = 0;
            this.dragStartPosition = Vector2Int.zero;
            this.phase = TouchPhase.None;
        }

        public bool IsMoving()
        {
            return delta.sqrMagnitude > 0;
        }

        public override string ToString()
        {
            return "TouchPhase:  " + phase + " \n fingerID: " + fingerID +
                " \n Position:  " + position + " \n Delta: " + delta + " \n clickCount: " + clickCount;
        }
    }

    public enum TouchPhase
    {
        None,
        Began,
        DragStarted,
        Dragging,
        DragEnded,
        Flicked,
        Ended,
        Canceled
    }

    public class TouchManager : AutoInstantiateSingleton<TouchManager>
    {
        public readonly int Max_TOUCH_COUNT = 2;

        private Action<TouchEventData> _pointerDownEvent;
        private Action<TouchEventData> _pointerUpEvent;
        private Action<TouchEventData> _pointerDragStartEvent;
        private Action<TouchEventData> _pointerDragEvent;
        private Action<TouchEventData> _pointerDragEndEvent;
        private Action<TouchEventData, Vector2> _pointerFlickedEvent;
        private Action<Vector2> _OnTwoPointerDragEvent;

        public int dragThreshold = 5;
        public int flickThreshold = 10;
        public float flickTime = 0.5f;
        public int tapMaxAllowedDrag = 3;

        public static int DragPointerCount { get; private set; }
        public static int CurrentTouchCount { get; private set; }

        private TouchEventData[] _touches;

        private TouchEventData _firstTouch;
        private TouchEventData _secondTouch;

        public TouchManager()
        {
#if UNITY_EDITOR
            Max_TOUCH_COUNT = 2;
#else
            Max_TOUCH_COUNT = 10;
#endif
        }

        protected override void Awake()
        {
            base.Awake();
            flickThreshold = flickThreshold * Screen.width / 1024;
            dragThreshold = dragThreshold * Screen.width / 1024;
            tapMaxAllowedDrag = dragThreshold * Screen.width / 1024;
        }

        public void RegisterListener(IEventListner listener)
        {
            if (listener is IPointerDown)
                _pointerDownEvent += (listener as IPointerDown).OnPointerDown;
            if (listener is IPointerUp)
                _pointerUpEvent += (listener as IPointerUp).OnPointerUp;
            if (listener is IPointerDragStart)
                _pointerDragStartEvent += (listener as IPointerDragStart).OnPointerDragStart;
            if (listener is IPointerDrag)
                _pointerDragEvent += (listener as IPointerDrag).OnPointerDrag;
            if (listener is IPointerDragEnd)
                _pointerDragEndEvent += (listener as IPointerDragEnd).OnPointerDragEnd;
            if (listener is IPointerFlick)
                _pointerFlickedEvent += (listener as IPointerFlick).OnPointerFlick;
            if (listener is ITwoPointerDrag)
                _OnTwoPointerDragEvent += (listener as ITwoPointerDrag).OnPointerTwoFinger;
        }

        public void UnregisterListener(IEventListner listener)
        {
            if (listener is IPointerDown)
                _pointerDownEvent -= (listener as IPointerDown).OnPointerDown;
            if (listener is IPointerUp)
                _pointerUpEvent -= (listener as IPointerUp).OnPointerUp;
            if (listener is IPointerDragStart)
                _pointerDragStartEvent -= (listener as IPointerDragStart).OnPointerDragStart;
            if (listener is IPointerDrag)
                _pointerDragEvent -= (listener as IPointerDrag).OnPointerDrag;
            if (listener is IPointerDragEnd)
                _pointerDragEndEvent -= (listener as IPointerDragEnd).OnPointerDragEnd;
            if (listener is IPointerFlick)
                _pointerFlickedEvent -= (listener as IPointerFlick).OnPointerFlick;
            if (listener is ITwoPointerDrag)
                _OnTwoPointerDragEvent -= (listener as ITwoPointerDrag).OnPointerTwoFinger;
        }

        protected override void Start()
        {
            base.Start();
            _touches = new TouchEventData[Max_TOUCH_COUNT];
            for (int i = 0; i < Max_TOUCH_COUNT; ++i)
                _touches[i] = new TouchEventData(i);
        }

        void Update()
        {
            if (!ProcessTouch())
                ProcessMouse();
        }

        private bool ProcessTouch()
        {
            DragPointerCount = 0;
            CurrentTouchCount = 0;

            for (int i = 0; i < UnityEngine.Input.touchCount; ++i)
            {
                Touch touch = UnityEngine.Input.GetTouch(i);
                ++CurrentTouchCount;

                switch (touch.phase)
                {
                    case UnityEngine.TouchPhase.Began:
                        ProcessPointerDown(touch.fingerId, touch.position);
                        break;
                    case UnityEngine.TouchPhase.Moved:
                    case UnityEngine.TouchPhase.Stationary:
                        ProcessPointer(touch.fingerId, touch.position);
                        break;
                    case UnityEngine.TouchPhase.Ended:
                    case UnityEngine.TouchPhase.Canceled:
                        ProcessPointerUp(touch.fingerId, touch.position);
                        break;
                }
            }
            return UnityEngine.Input.touchCount > 0;
        }

        private void ProcessMouse()
        {
            for (int fingerID = 0; fingerID < 2; ++fingerID)
            {
                if (UnityEngine.Input.GetMouseButtonDown(fingerID))
                    ProcessPointerDown(fingerID, UnityEngine.Input.mousePosition);
                else if (UnityEngine.Input.GetMouseButton(fingerID))
                    ProcessPointer(fingerID, UnityEngine.Input.mousePosition);
                else if (UnityEngine.Input.GetMouseButtonUp(fingerID))
                    ProcessPointerUp(fingerID, UnityEngine.Input.mousePosition);
            }
        }

        private void ProcessEvents(int fingerID)
        {
            switch (_touches[fingerID].phase)
            {
                case TouchPhase.Began:
                    _pointerDownEvent?.Invoke(_touches[fingerID]);
                    break;
                case TouchPhase.DragStarted:
                    _pointerDragStartEvent?.Invoke(_touches[fingerID]);
                    break;
                case TouchPhase.Dragging:
                    _pointerDragEvent?.Invoke(_touches[fingerID]);
                    break;
                case TouchPhase.DragEnded:
                    _pointerDragEndEvent?.Invoke(_touches[fingerID]);
                    break;
                case TouchPhase.Flicked:
                    _pointerFlickedEvent?.Invoke(_touches[fingerID], (_touches[fingerID].position - _touches[fingerID].dragStartPosition).normalized);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    _pointerUpEvent?.Invoke(_touches[fingerID]);
                    _touches[fingerID].phase = TouchPhase.Ended;
                    break;
            }
        }

        private void UpdateTouchInfo(int fingerID, TouchPhase phase, Vector2 position)
        {
            _touches[fingerID].phase = phase;
            _touches[fingerID].position = position;
            ProcessEvents(fingerID);
        }

        private void ProcessPointerDown(int fingerID, Vector2 position)
        {
            _touches[fingerID].pressPosition = position;
            _touches[fingerID].delta = Vector2Int.zero;
            float time = Time.unscaledTime;
            if (time - _touches[fingerID].clickTime < 0.3)
                ++_touches[fingerID].clickCount;
            else
                _touches[fingerID].clickCount = 1;

            _touches[fingerID].clickTime = Time.unscaledTime;
            UpdateTouchInfo(fingerID, TouchPhase.Began, position);
        }

        private void ProcessPointer(int fingerID, Vector2 position)
        {
            _touches[fingerID].delta = position - _touches[fingerID].position;
            _touches[fingerID].position = position;

            if (_touches[fingerID].phase == TouchPhase.Began && ShouldStartDrag(_touches[fingerID]))
            {
                _touches[fingerID].dragStartTime = Time.unscaledTime;
                _touches[fingerID].dragStartPosition = position;
                UpdateTouchInfo(fingerID, TouchPhase.DragStarted, position);
            }

            if (_touches[fingerID].phase == TouchPhase.DragStarted || _touches[fingerID].phase == TouchPhase.Dragging)
            {
                if (_touches[fingerID].IsMoving())
                {
                    ++DragPointerCount;
                    UpdateTouchInfo(fingerID, TouchPhase.Dragging, position);
                    ProcessTwoPointerDrag(_touches[fingerID]);
                }
            }
        }

        private void ProcessPointerUp(int fingerID, Vector2 position)
        {
            if (_touches[fingerID].phase == TouchPhase.Dragging)
                UpdateTouchInfo(fingerID, TouchPhase.DragEnded, position);
            if (IsFlicked(_touches[fingerID]))
                UpdateTouchInfo(fingerID, TouchPhase.Flicked, position);

            UpdateTouchInfo(fingerID, TouchPhase.Ended, position);
            _touches[fingerID].phase = TouchPhase.None;
        }

        private bool ShouldStartDrag(TouchEventData touchEventData)
        {
            return (touchEventData.position - touchEventData.pressPosition).sqrMagnitude > dragThreshold * dragThreshold;
        }

        private bool IsFlicked(TouchEventData touchEventData)
        {
            return (touchEventData.position - touchEventData.dragStartPosition).sqrMagnitude > flickThreshold * flickThreshold
                    && (Time.unscaledTime - touchEventData.dragStartTime < flickTime);
        }

        private void ProcessTwoPointerDrag(TouchEventData touchEventData)
        {
            switch (DragPointerCount)
            {
                case 1:
                    _firstTouch = touchEventData;
                    break;
                case 2:
                    _secondTouch = touchEventData;
                    break;
            }

            if (DragPointerCount >= 2)
            {
                float curDis = Vector2.SqrMagnitude(_firstTouch.position - _secondTouch.position);
                float prevDis = Vector2.SqrMagnitude((_firstTouch.position - _firstTouch.delta) - (_secondTouch.position - _secondTouch.delta));
                //if (mOnTwoPointerDragEvent != null)
                //    mOnTwoPointerDragEvent(curDis - prevDis);
                Debug.Log(curDis - prevDis);
            }
        }

        public void AddEvents(Action<TouchEventData> pointerDown, Action<TouchEventData> pointerUp, Action<TouchEventData> pointerDragStart,
                                Action<TouchEventData> pointerDrag, Action<TouchEventData> pointerDragEnd, Action<TouchEventData, Vector2> pointerFlick)
        {
            _pointerDownEvent += pointerDown;
            _pointerUpEvent += pointerUp;
            _pointerDragStartEvent += pointerDragStart;
            _pointerDragEvent += pointerDrag;
            _pointerDragEndEvent += pointerDragEnd;
            _pointerFlickedEvent += pointerFlick;
        }

        public void RemoveEvents(Action<TouchEventData> pointerDown, Action<TouchEventData> pointerUp, Action<TouchEventData> pointerDragStart,
                                Action<TouchEventData> pointerDrag, Action<TouchEventData> pointerDragEnd, Action<TouchEventData, Vector2> pointerFlick)
        {
            _pointerDownEvent -= pointerDown;
            _pointerUpEvent -= pointerUp;
            _pointerDragStartEvent -= pointerDragStart;
            _pointerDragEvent -= pointerDrag;
            _pointerDragEndEvent -= pointerDragEnd;
            _pointerFlickedEvent -= pointerFlick;
        }

    }
}


