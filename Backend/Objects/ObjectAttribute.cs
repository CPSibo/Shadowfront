using static Shadowfront.Backend.AttributeValue;

namespace Shadowfront.Backend.Board.BoardPieces
{
    public partial class ObjectAttribute
    {
        public readonly record struct ValueMinChangedEventArgs(AttributeValue ClampedValue, float PreviousValue, float NewValue);
        public readonly record struct ValueMaxChangedEventArgs(AttributeValue ClampedValue, float PreviousValue, float NewValue);
        public readonly record struct ValueCurrentChangedEventArgs(AttributeValue ClampedValue, float PreviousValue, float NewValue);
        public readonly record struct ValueCurrentAtMinEventArgs(AttributeValue ClampedValue, float PreviousValue, float NewValue);

        public delegate void ValueMinChangedEventHandler(ObjectAttribute sender, ValueMinChangedEventArgs e);
        public delegate void ValueMaxChangedEventHandler(ObjectAttribute sender, ValueMaxChangedEventArgs e);
        public delegate void ValueCurrentChangedEventHandler(ObjectAttribute sender, ValueCurrentChangedEventArgs e);
        public delegate void ValueCurrentAtMinEventHandler(ObjectAttribute sender, ValueCurrentAtMinEventArgs e);

        public event ValueMinChangedEventHandler? ValueMinChanged;
        public event ValueMaxChangedEventHandler? ValueMaxChanged;
        public event ValueCurrentChangedEventHandler? ValueCurrentChanged;
        public event ValueCurrentAtMinEventHandler? ValueCurrentAtMin;

        public string Key { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public AttributeValue Value { get; init; } = new();

        public ObjectAttribute()
        {
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

        private void OnMinChanged(AttributeValue sender, MinChangedEventArgs e)
        {
            ValueMinChanged?.Invoke(this, new(sender, e.PreviousValue, e.NewValue));
        }

        private void OnMaxChanged(AttributeValue sender, MaxChangedEventArgs e)
        {
            ValueMaxChanged?.Invoke(this, new(sender, e.PreviousValue, e.NewValue));
        }

        private void OnCurrentChanged(AttributeValue sender, CurrentChangedEventArgs e)
        {
            ValueCurrentChanged?.Invoke(this, new(sender, e.PreviousValue, e.NewValue));
        }

        private void OnCurrentAtMin(AttributeValue sender, CurrentAtMinEventArgs e)
        {
            ValueCurrentAtMin?.Invoke(this, new(sender, e.PreviousValue, e.NewValue));
        }
    }
}
