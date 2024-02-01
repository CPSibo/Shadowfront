using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shadowfront.Backend
{
    public partial class GameObject : Node
    {
        public static Dictionary<Guid, GameObject> Registry { get; } = [];

        protected Guid _id = Guid.NewGuid();

        public Guid Id
        {
            get => _id;

            set
            {
                var previous = _id;

                _id = value;

                UpdateRegistry(this, previous, _id);
            }
        }

        public Guid? OwnerId { get; set; }

        public GameObject? OwnerObject { get; private set; }

        public override void _Ready()
        {
            base._Ready();

            UpdateRegistry(this, null, _id);
        }

        protected static void UpdateRegistry(GameObject gameObject, Guid? from, Guid to)
        {
            if(from is not null)
                Registry.Remove(from.Value);

            Registry.Add(to, gameObject);

            gameObject.UpdateChildrenOwnerId();
        }

        protected void UpdateChildrenOwnerId()
        {
            foreach (var child in GetChildren())
            {
                if (child is GameObject gameObject)
                    gameObject.OwnerId = _id;
            }
        }

        public IEnumerable<GameObject> GetOwnerChain()
        {
            if (OwnerObject is null)
                return Enumerable.Empty<GameObject>();

            return [OwnerObject, .. OwnerObject.GetOwnerChain()];
        }

        public bool OwnerChainContains(Guid id)
        {
            if (_id == id) return true;

            if (OwnerObject is null) return false;

            if (OwnerId == id) return true;

            var ownerChain = GetOwnerChain();

            return ownerChain
                .Select(f => f.Id)
                .Contains(id);
        }

        public bool OwnerChainContains(GameObject gameObject)
            => OwnerChainContains(gameObject.Id);

        public bool ChildChainContains(Guid id)
        {
            if (_id == id) return true;

            if(!Registry.TryGetValue(id, out var registryObject))
                return false;

            return registryObject.OwnerChainContains(_id);
        }

        public bool ChildChainContains(GameObject gameObject)
        {
            if (_id == gameObject.Id) return true;

            return gameObject.OwnerChainContains(_id);
        }
    }
}