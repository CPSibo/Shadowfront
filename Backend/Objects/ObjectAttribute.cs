using System;
using static Shadowfront.Backend.ClampedValue;

namespace Shadowfront.Backend.Board.BoardPieces
{
    public partial class ObjectAttribute : GameObject
    {
        public readonly record struct ObjectAttribute_MinChangedEvent(Guid OwnerId, string Name, float PreviousValue, float NewValue) : IEventType;
        public readonly record struct ObjectAttribute_MaxChangedEvent(Guid OwnerId, string Name, float PreviousValue, float NewValue) : IEventType;
        public readonly record struct ObjectAttribute_CurrentChangedEvent(Guid OwnerId, string Name, float PreviousValue, float NewValue) : IEventType;
        public readonly record struct ObjectAttribute_CurrentAtMinEvent(Guid OwnerId, string Name, float PreviousValue, float NewValue) : IEventType;

        public Guid OwnerId
        {
            get => OwnerId;

            set
            {
                _ownerId = value;
                Value.OwnerId = value;
            }
        }

        public string? Description { get; set; }

        public ClampedValue Value { get; init; }

        public ObjectAttribute()
        {
            Value = new()
            {
                OwnerId = OwnerId
            };

            Value.MinChanged += OnMinChanged;
            Value.MaxChanged += OnMaxChanged;
            Value.CurrentChanged += OnCurrentChanged;
            Value.CurrentAtMin += OnCurrentAtMin;
        }

        ~ObjectAttribute()
        {
            Value.MinChanged -= OnMinChanged;
            Value.MaxChanged -= OnMaxChanged;
            Value.CurrentChanged -= OnCurrentChanged;
            Value.CurrentAtMin -= OnCurrentAtMin;
        }

        private void OnMinChanged(object? sender, MinChangedEventArgs e)
        {
            EventBus.Emit(new ObjectAttribute_MinChangedEvent(OwnerId, Name, e.PreviousValue, e.NewValue));
        }

        private void OnMaxChanged(object? sender, MaxChangedEventArgs e)
        {
            EventBus.Emit(new ObjectAttribute_MaxChangedEvent(OwnerId, Name, e.PreviousValue, e.NewValue));
        }

        private void OnCurrentChanged(object? sender, CurrentChangedEventArgs e)
        {
            EventBus.Emit(new ObjectAttribute_CurrentChangedEvent(OwnerId, Name, e.PreviousValue, e.NewValue));
        }

        private void OnCurrentAtMin(object? sender, CurrentAtMinEventArgs e)
        {
            EventBus.Emit(new ObjectAttribute_CurrentAtMinEvent(OwnerId, Name, e.PreviousValue, e.NewValue));
        }
    }
}
