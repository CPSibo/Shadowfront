﻿using Godot;
using System;
using System.Collections.Generic;

namespace Shadowfront.Backend
{
    public class AttributeValue :
        IEquatable<AttributeValue>,
        IComparable<AttributeValue>,
        IEquatable<float>,
        IComparable<float>,
        IEquatable<int>,
        IComparable<int>,
        IEquatable<double>,
        IComparable<double>
    {
        public enum ModifierTargets
        {
            Current,
            Min,
            Max,
            MinAndMax,
            All
        }

        public enum ModiferTypes
        {
            Add,
            Multiply,
            Set
        }

        public readonly record struct AttributeModifier(string Source, ModifierTargets Target, ModiferTypes Type, float Amount);

        public readonly record struct MinChangedEventArgs(float PreviousValue, float NewValue);
        public readonly record struct MaxChangedEventArgs(float PreviousValue, float NewValue);
        public readonly record struct CurrentChangedEventArgs(float PreviousValue, float NewValue);
        public readonly record struct CurrentAtMinEventArgs(float PreviousValue, float NewValue);

        public delegate void MinChangedEventHandler(AttributeValue sender, MinChangedEventArgs e);
        public delegate void MaxChangedEventHandler(AttributeValue sender, MaxChangedEventArgs e);
        public delegate void CurrentChangedEventHandler(AttributeValue sender, CurrentChangedEventArgs e);
        public delegate void CurrentAtMinEventHandler(AttributeValue sender, CurrentAtMinEventArgs e);

        public event MinChangedEventHandler? MinChanged;
        public event MaxChangedEventHandler? MaxChanged;
        public event CurrentChangedEventHandler? CurrentChanged;
        public event CurrentAtMinEventHandler? CurrentAtMin;

        private float _min = float.MinValue;

        private float _max = float.MaxValue;

        private float _current;

        public float Min
        {
            get => _min;
            set => SetMin(value);
        }

        public float Max
        {
            get => _max;
            set => SetMax(value);
        }

        public float Current
        {
            get => _current;
            set => SetCurrent(value);
        }

        public bool EnableEvents { get; set; }

        public List<AttributeModifier> Modifiers { get; init; } = [];

        private void SetMin(float newValue)
        {
            var previousValue = _min;

            _min = Math.Min(newValue, _max);

            if (EnableEvents && !Mathf.IsEqualApprox(previousValue, _min))
                MinChanged?.Invoke(this, new(previousValue, _min));

            if (_min > _current)
                SetCurrent(_min);
        }

        private void SetMax(float newValue)
        {
            var previousValue = _max;

            _max = Math.Max(newValue, _min);

            if (EnableEvents && !Mathf.IsEqualApprox(previousValue, _max))
                MaxChanged?.Invoke(this, new(previousValue, _max));

            if (_max < _current)
                SetCurrent(_max);
        }

        private void SetCurrent(float newValue)
        {
            var previousValue = _current;

            _current = Math.Clamp(newValue, _min, _max);

            if (EnableEvents && !Mathf.IsEqualApprox(previousValue, _current))
            {
                CurrentChanged?.Invoke(this, new(previousValue, _current));

                if(Mathf.IsEqualApprox(_current, _min))
                    CurrentAtMin?.Invoke(this, new(previousValue, _current));
            }
        }

        public bool Equals(AttributeValue? other)
        {
            if(ReferenceEquals(this, other)) return true;

            if(other is null) return false;

            return Mathf.IsEqualApprox(_current, other._current);
        }

        public bool Equals(float other)
            => Mathf.IsEqualApprox(_current, other);

        public bool Equals(int other)
            => Mathf.IsEqualApprox(_current, other);

        public bool Equals(double other)
            => Mathf.IsEqualApprox(_current, other);

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            if (obj is null) return false;

            return obj switch
            {
                float f => Equals(f),
                int i => Equals(i),
                double d => Equals(d),
                AttributeValue cv => Equals(cv),
                _ => false
            };
        }

        public int CompareTo(AttributeValue? other)
            => _current.CompareTo(other?._current);

        public int CompareTo(int other)
            => _current.CompareTo(other);

        public int CompareTo(float other)
            => _current.CompareTo(other);

        public int CompareTo(double other)
            => _current.CompareTo(other);

        public static bool operator ==(AttributeValue left, AttributeValue right)
            => left is null ? right is null : left.Equals(right);

        public static bool operator !=(AttributeValue left, AttributeValue right)
            => !(left == right);

        public static bool operator <(AttributeValue left, AttributeValue right)
            => left is null ? right is not null : left.CompareTo(right) < 0;

        public static bool operator <=(AttributeValue left, AttributeValue right)
            => left is null || left.CompareTo(right) <= 0;

        public static bool operator >(AttributeValue left, AttributeValue right)
            => left is not null && left.CompareTo(right) > 0;

        public static bool operator >=(AttributeValue left, AttributeValue right)
            => left is null ? right is null : left.CompareTo(right) >= 0;

        public override int GetHashCode()
            => (_current, _min, _max, EnableEvents).GetHashCode();

        public AttributeValue Clone() => new()
        {
            _current = _current,
            _min = _min,
            _max = _max,
            EnableEvents = EnableEvents
        };
    }
}
