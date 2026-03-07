using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Events;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "ServicesEvents", menuName = "Channels/Services Events")]
    public class ServicesChannel : ScriptableObject
    {
        private readonly Dictionary<ServiceEventType, UnityAction>         _signals      = new();
        private readonly Dictionary<ServiceEventType, UnityAction<string>> _stringEvents = new();

        public void Subscribe(ServiceEventType type, UnityAction listener)
        {
            _signals.TryGetValue(type, out var e);
            _signals[type] = e + listener;
        }

        public void Subscribe(ServiceEventType type, UnityAction<string> listener)
        {
            _stringEvents.TryGetValue(type, out var e);
            _stringEvents[type] = e + listener;
        }

        public void Unsubscribe(ServiceEventType type, UnityAction listener)
        {
            if (!_signals.TryGetValue(type, out var e)) return;
            e -= listener;
            if (e == null) _signals.Remove(type); else _signals[type] = e;
        }

        public void Unsubscribe(ServiceEventType type, UnityAction<string> listener)
        {
            if (!_stringEvents.TryGetValue(type, out var e)) return;
            e -= listener;
            if (e == null) _stringEvents.Remove(type); else _stringEvents[type] = e;
        }

        public void Raise(ServiceEventType type)
        {
            if (_signals.TryGetValue(type, out var e)) e?.Invoke();
        }

        public void Raise(ServiceEventType type, string value)
        {
            if (_stringEvents.TryGetValue(type, out var e)) e?.Invoke(value);
        }

        // NOTE: OnDisable intentionally removed.
        // Clearing subscriptions here caused the build to lose all listeners
        // because OnDisable fires before MonoBehaviour.Start() re-subscribes.
    }
}
