using Godot;
using System;
using System.Collections.Generic;

namespace Shadowfront.Backend.Board.BoardPieces
{
    public partial class ObjectAttributes : Node
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        private Guid _ownerId;

        public Guid OwnerId
        {
            get => _ownerId;

            set
            {
                _ownerId = value;

                foreach(var (_, attr) in _values)
                {
                    attr.OwnerId = value;
                }
            }
        }

        private Dictionary<string, ObjectAttribute> _values = [];

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
                if (value is null)
                {
                    _values.Remove(key);
                    return;
                }

                var newValue = value;

                newValue.OwnerId = OwnerId;

                if(_values.ContainsKey(key))
                    _values[key] = newValue;

                _values.Add(key, newValue);
            }
        }

        public bool HasKey(string key)
            => _values.ContainsKey(key);
    }
}
