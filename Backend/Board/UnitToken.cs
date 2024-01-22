using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shadowfront.Backend.Board
{
    public partial class UnitToken : DisposableRefCounted
    {
        [Signal]
        public delegate void HealthChangedEventHandler(UnitToken token, float previousHealth, float newHealth);

        [Signal]
        public delegate void HealthReachedZeroEventHandler(UnitToken token);

        public UnitResource UnitResource { get; private set; }

        public FactionResource Team { get; set; }

        public float Damage { get; set; }

        public float MaxHealth { get; set; }

        public int MaxMoveDistance { get; set; }

        public int MaxAttackDistance { get; set; }

        private float _health;

        public float Health
        {
            get => _health;

            protected set
            {
                var previousHealth  = _health;

                _health = value;

                EmitSignal(SignalName.HealthChanged, this, previousHealth, _health);

                if (_health <= 0)
                {
                    EmitSignal(SignalName.HealthReachedZero, this);
                }
            }
        }

        public UnitToken(UnitResource unitResource, FactionResource factionResource)
        {
            UnitResource = unitResource;
            Team = factionResource;

            MaxHealth = UnitResource.MaxHealth;
            Damage = UnitResource.Damage;
            MaxMoveDistance = UnitResource.MaxMoveDistance;
            MaxAttackDistance = UnitResource.MaxAttackDistance;

            Health = MaxHealth;
        }

        public void Attack(UnitToken target)
        {
            target.AddHealth(Damage * -1);
        }

        public void AddHealth(float amount)
        {
            Health = Math.Clamp(Health + amount, 0, MaxHealth);
        }
    }
}
