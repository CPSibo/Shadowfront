using Godot;
using System.Collections.Generic;
using static Shadowfront.Backend.Board.BoardPieces.ObjectAttribute;

namespace Shadowfront.Backend.Board.BoardPieces
{
    public partial class ObjectAttributes : Node
    {
        public readonly record struct ObjectAttributes_AttributeValueCurrentChangedEvent(
            Node Owner,
            ObjectAttributes Sender,
            ObjectAttribute Attribute,
            AttributeValue Value,
            float PreviousValue,
            float NewValue
        ) : IEventType;

        public readonly record struct ObjectAttributes_AttributeValueCurrentAtMinEvent(
            Node Owner,
            ObjectAttributes Sender,
            ObjectAttribute Attribute,
            AttributeValue Value,
            float PreviousValue,
            float NewValue
        ) : IEventType;

        public readonly record struct AttributeValueMinChangedEventArgs(ObjectAttribute Attribute, AttributeValue ClampedValue, float PreviousValue, float NewValue);
        public readonly record struct AttributeValueMaxChangedEventArgs(ObjectAttribute Attribute, AttributeValue ClampedValue, float PreviousValue, float NewValue);
        public readonly record struct AttributeValueCurrentChangedEventArgs(ObjectAttribute Attribute, AttributeValue ClampedValue, float PreviousValue, float NewValue);
        public readonly record struct AttributeValueCurrentAtMinEventArgs(ObjectAttribute Attribute, AttributeValue ClampedValue, float PreviousValue, float NewValue);

        public delegate void AttributeValueMinChangedEventHandler(ObjectAttributes sender, AttributeValueMinChangedEventArgs e);
        public delegate void AttributeValueMaxChangedEventHandler(ObjectAttributes sender, AttributeValueMaxChangedEventArgs e);
        public delegate void AttributeValueCurrentChangedEventHandler(ObjectAttributes sender, AttributeValueCurrentChangedEventArgs e);
        public delegate void AttributeValueCurrentAtMinEventHandler(ObjectAttributes sender, AttributeValueCurrentAtMinEventArgs e);

        public event AttributeValueMinChangedEventHandler? AttributeValueMinChanged;
        public event AttributeValueMaxChangedEventHandler? AttributeValueMaxChanged;
        public event AttributeValueCurrentChangedEventHandler? AttributeValueCurrentChanged;
        public event AttributeValueCurrentAtMinEventHandler? AttributeValueCurrentAtMin;

        private Dictionary<string, ObjectAttribute> _values = [];

        public ObjectAttributes()
        {
            foreach(var (_, attribute) in _values)
            {
                SubscribeToObjectAttributeEvents(attribute);
            }
        }

        ~ObjectAttributes()
        {
            foreach (var (_, attribute) in _values)
            {
                UnsubscribeFomObjectAttributeEvents(attribute);
            }
        }

        private void SubscribeToObjectAttributeEvents(ObjectAttribute attribute)
        {
            attribute.ValueMinChanged += OnValueMinChanged;
            attribute.ValueMaxChanged += OnValueMaxChanged;
            attribute.ValueCurrentChanged += OnValueCurrentChanged;
            attribute.ValueCurrentAtMin += OnValueCurrentAtMin;
        }

        private void UnsubscribeFomObjectAttributeEvents(ObjectAttribute attribute)
        {
            attribute.ValueMinChanged -= OnValueMinChanged;
            attribute.ValueMaxChanged -= OnValueMaxChanged;
            attribute.ValueCurrentChanged -= OnValueCurrentChanged;
            attribute.ValueCurrentAtMin -= OnValueCurrentAtMin;
        }

        private void OnValueMinChanged(ObjectAttribute sender, ValueMinChangedEventArgs e)
        {
            AttributeValueMinChanged?.Invoke(this, new(sender, e.ClampedValue, e.PreviousValue, e.NewValue));
        }

        private void OnValueMaxChanged(ObjectAttribute sender, ValueMaxChangedEventArgs e)
        {
            AttributeValueMaxChanged?.Invoke(this, new(sender, e.ClampedValue, e.PreviousValue, e.NewValue));
        }

        private void OnValueCurrentChanged(ObjectAttribute sender, ValueCurrentChangedEventArgs e)
        {
            AttributeValueCurrentChanged?.Invoke(this, new(sender, e.ClampedValue, e.PreviousValue, e.NewValue));

            EventBus.Emit(new ObjectAttributes_AttributeValueCurrentChangedEvent(
                GetParent(),
                this,
                sender,
                e.ClampedValue,
                e.PreviousValue,
                e.NewValue
            ));
        }

        private void OnValueCurrentAtMin(ObjectAttribute sender, ValueCurrentAtMinEventArgs e)
        {
            AttributeValueCurrentAtMin?.Invoke(this, new(sender, e.ClampedValue, e.PreviousValue, e.NewValue));

            EventBus.Emit(new ObjectAttributes_AttributeValueCurrentAtMinEvent(
                GetParent(),
                this,
                sender,
                e.ClampedValue,
                e.PreviousValue,
                e.NewValue
            ));
        }

        public ObjectAttribute? this[string key]
        {
            get
            {
                if (!_values.TryGetValue(key, out var value))
                    return null;

                return value;
            }

            set
            {
                if (_values.TryGetValue(key, out var existingValue))
                    UnsubscribeFomObjectAttributeEvents(existingValue);

                if (value is null)
                {
                    _values.Remove(key);
                    return;
                }

                var newValue = value;

                if (existingValue is not null)
                    _values[key] = newValue;
                else
                    _values.Add(key, newValue);

                SubscribeToObjectAttributeEvents(newValue);
            }
        }

        public bool HasKey(string key)
            => _values.ContainsKey(key);
    }
}
