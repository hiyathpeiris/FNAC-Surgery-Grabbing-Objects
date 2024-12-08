using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction
{
    /// <summary>
    /// This component allows IPointables to trigger Unity Events,
    /// enabling event handling directly from the Unity Inspector.
    /// </summary>
    public class PointableUnityEventWrapper : MonoBehaviour
    {
        [SerializeField, Interface(typeof(IPointable))]
        private UnityEngine.Object _pointable; // The IPointable object to monitor.
        private IPointable Pointable; // Interface for interacting with the pointable object.

        private HashSet<int> _pointers; // Tracks active pointer identifiers.

        [SerializeField]
        private UnityEvent<PointerEvent> _whenRelease; // Event triggered on pointer release.
        [SerializeField]
        private UnityEvent<PointerEvent> _whenHover; // Event triggered on pointer hover.
        [SerializeField]
        private UnityEvent<PointerEvent> _whenUnhover; // Event triggered when pointer stops hovering.
        [SerializeField]
        private UnityEvent<PointerEvent> _whenSelect; // Event triggered on pointer selection.
        [SerializeField]
        private UnityEvent<PointerEvent> _whenUnselect; // Event triggered on pointer deselection.
        [SerializeField]
        private UnityEvent<PointerEvent> _whenMove; // Event triggered when pointer moves.
        [SerializeField]
        private UnityEvent<PointerEvent> _whenCancel; // Event triggered on pointer cancellation.

        // Properties to access Unity Events.
        public UnityEvent<PointerEvent> WhenRelease => _whenRelease;
        public UnityEvent<PointerEvent> WhenHover => _whenHover;
        public UnityEvent<PointerEvent> WhenUnhover => _whenUnhover;
        public UnityEvent<PointerEvent> WhenSelect => _whenSelect;
        public UnityEvent<PointerEvent> WhenUnselect => _whenUnselect;
        public UnityEvent<PointerEvent> WhenMove => _whenMove;
        public UnityEvent<PointerEvent> WhenCancel => _whenCancel;

        protected bool _started = false; // Tracks whether the component has been initialized.

        // Called when the script instance is being loaded.
        protected virtual void Awake()
        {
            Pointable = _pointable as IPointable; // Cast the assigned pointable to IPointable.
        }

        // Called when the object is first initialized.
        protected virtual void Start()
        {
            this.BeginStart(ref _started); // Marks the start of initialization.
            this.AssertField(Pointable, nameof(Pointable)); // Ensures the Pointable is valid.
            _pointers = new HashSet<int>(); // Initializes the pointer tracker.
            this.EndStart(ref _started); // Marks the end of initialization.
        }

        // Subscribes to pointer events when the component is enabled.
        protected virtual void OnEnable()
        {
            if (_started)
            {
                Pointable.WhenPointerEventRaised += HandlePointerEventRaised;
            }
        }

        // Unsubscribes from pointer events when the component is disabled.
        protected virtual void OnDisable()
        {
            if (_started)
            {
                Pointable.WhenPointerEventRaised -= HandlePointerEventRaised;
            }
        }

        // Handles pointer events and invokes corresponding Unity Events.
        private void HandlePointerEventRaised(PointerEvent evt)
        {
            switch (evt.Type)
            {
                case PointerEventType.Hover:
                    _whenHover.Invoke(evt); // Triggers hover event.
                    _pointers.Add(evt.Identifier); // Tracks the pointer.
                    break;
                case PointerEventType.Unhover:
                    _whenUnhover.Invoke(evt); // Triggers unhover event.
                    _pointers.Remove(evt.Identifier); // Removes the pointer from tracking.
                    break;
                case PointerEventType.Select:
                    _whenSelect.Invoke(evt); // Triggers select event.
                    break;
                case PointerEventType.Unselect:
                    if (_pointers.Contains(evt.Identifier))
                    {
                        _whenRelease.Invoke(evt); // Triggers release event if pointer is tracked.
                    }
                    _whenUnselect.Invoke(evt); // Triggers unselect event.
                    break;
                case PointerEventType.Move:
                    _whenMove.Invoke(evt); // Triggers move event.
                    break;
                case PointerEventType.Cancel:
                    _whenCancel.Invoke(evt); // Triggers cancel event.
                    _pointers.Remove(evt.Identifier); // Removes the pointer from tracking.
                    break;
            }
        }

        #region Inject

        // Injects all dependencies into the wrapper.
        public void InjectAllPointableUnityEventWrapper(IPointable pointable)
        {
            InjectPointable(pointable);
        }

        // Injects the pointable dependency.
        public void InjectPointable(IPointable pointable)
        {
            _pointable = pointable as UnityEngine.Object; // Casts to UnityEngine.Object for serialization.
            Pointable = pointable; // Assigns the IPointable interface.
        }

        #endregion
    }
}
