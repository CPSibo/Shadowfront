using Godot;
using System;

namespace Shadowfront.Backend.Board.BoardPieces.Behaviors
{
    public partial class BoardPieceHealth : BoardPieceBehavior
    {
        private float _maxHealth;

        /// <summary>
        /// Maximum allowed value for <see cref="CurrentHealth"/>.
        /// </summary>
        [Export]
        public float MaxHealth
        {
            get => _maxHealth;
            set => _maxHealth = value;
        }

        private float _minHealth;

        /// <summary>
        /// Minimum allowed value for <see cref="CurrentHealth"/>.
        /// </summary>
        [Export]
        public float MinHealth
        {
            get => _minHealth;
            set => _minHealth = value;
        }

        private float _currentHealth;

        /// <summary>
        /// The current health.
        /// </summary>
        /// <remarks>
        /// <para>Will be clamped between <see cref="MinHealth"/> and <see cref="MaxHealth"/>.</para>
        /// </remarks>
        public float CurrentHealth
        {
            get => _currentHealth;
            set => SetCurrentHealth(value);
        }

        /// <summary>
        /// Whether the current health is approx. the same as the min health.
        /// </summary>
        public bool CurrentHealthIsAtMinHealth => Mathf.IsEqualApprox(_currentHealth, MinHealth);

        /// <summary>
        /// Whether the current health is approx. the same as the max health.
        /// </summary>
        public bool CurrentHealthIsAtMaxHealth => Mathf.IsEqualApprox(_currentHealth, MaxHealth);

        public override void _Ready()
        {
            base._Ready();

            _currentHealth = _maxHealth;
        }

        /// <summary>
        /// Attemps to set the current health to the given value.
        /// </summary>
        /// <remarks>
        /// <para><paramref name="newValue"/> will be clamped between <see cref="MinHealth"/> and <see cref="MaxHealth"/></para>
        /// </remarks>
        /// <param name="newValue">The desired value for current health</param>
        public void SetCurrentHealth(float newValue)
        {
            // Ensure our new health is between our min and max health.
            var clampedHealth = Math.Clamp(newValue, MinHealth, MaxHealth);

            // If we aren't really changing the value, just return.
            if (Mathf.IsEqualApprox(clampedHealth, _currentHealth))
                return;

            var previousHealth = _currentHealth;

            _currentHealth = clampedHealth;

            EventBus.Emit(new BoardPiece_HealthChangedEvent(_boardPiece, previousHealth, _currentHealth));

            CheckIfHealthAtBounds(previousHealth);
        }

        /// <summary>
        /// Attemps to add an ammount to the current health.
        /// </summary>
        /// <remarks>
        /// <para><paramref name="newValue"/> will be clamped between <see cref="MinHealth"/> and <see cref="MaxHealth"/></para>
        /// </remarks>
        /// <param name="newValue">The desired difference from the current health</param>
        public void AddCurrentHealth(float difference)
        {
            var newValue = _currentHealth + difference;

            SetCurrentHealth(newValue);
        }

        /// <summary>
        /// Attemps to multiply the current health by the given value.
        /// </summary>
        /// <remarks>
        /// <para><paramref name="newValue"/> will be clamped between <see cref="MinHealth"/> and <see cref="MaxHealth"/></para>
        /// </remarks>
        /// <param name="newValue">The desired value by which to multiply the current health</param>
        public void MultiplyByCurrentHealth(float scale)
        {
            var newValue = _currentHealth * scale;

            SetCurrentHealth(newValue);
        }

        /// <summary>
        /// Sets the max health to the given value.
        /// </summary>
        /// <remarks>
        /// <para>Current health will be re-clamped to the new bounds.</para>
        /// </remarks>
        /// <param name="newValue">The desired max health</param>
        public void SetMaxHealth(float newValue)
        {
            // If we aren't really changing the value, just return.
            if (Mathf.IsEqualApprox(_maxHealth, newValue))
                return;

            // Ensure our max health doesn't go below our min health.
            newValue = Math.Max(_minHealth, newValue);

            var previousMaxHealth = _maxHealth;

            _maxHealth = newValue;

            EventBus.Emit(new BoardPiece_MaxHealthChangedEvent(_boardPiece, previousMaxHealth, _maxHealth));

            // If the ceiling's been lowered below our current health...
            if (_maxHealth < _currentHealth)
                // Set our current health to the new ceiling.
                SetCurrentHealth(_maxHealth);
        }

        /// <summary>
        /// Adds to the max health.
        /// </summary>
        /// <remarks>
        /// <para>Current health will be re-clamped to the new bounds.</para>
        /// </remarks>
        /// <param name="difference">The ammount to add to max health</param>
        public void AddToMaxHealth(float difference)
        {
            var newValue = _maxHealth + difference;

            SetMaxHealth(newValue);
        }

        /// <summary>
        /// Multiples the max health by the given value.
        /// </summary>
        /// <remarks>
        /// <para>Current health will be re-clamped to the new bounds.</para>
        /// </remarks>
        /// <param name="scale">The value by which to multiply the max health</param>
        public void MultiplyByMaxHealth(float scale)
        {
            var newValue = _maxHealth * scale;

            SetMaxHealth(newValue);
        }

        /// <summary>
        /// Sets the min health to the given value.
        /// </summary>
        /// <remarks>
        /// <para>Current health will be re-clamped to the new bounds.</para>
        /// </remarks>
        /// <param name="newValue">The desired min health</param>
        public void SetMinHealth(float newValue)
        {
            // If we aren't really changing the value, just return.
            if (Mathf.IsEqualApprox(_minHealth, newValue))
                return;

            // Ensure our min health doesn't go above our max health.
            newValue = Math.Min(_maxHealth, newValue);

            var previousMinHealth = _minHealth;

            _minHealth = newValue;

            EventBus.Emit(new BoardPiece_MinHealthChangedEvent(_boardPiece, previousMinHealth, _minHealth));

            // If the floor's been raised above our current health...
            if (_minHealth > _currentHealth)
                // Set our current health to the new floor.
                SetCurrentHealth(_minHealth);
        }

        /// <summary>
        /// Adds to the min health.
        /// </summary>
        /// <remarks>
        /// <para>Current health will be re-clamped to the new bounds.</para>
        /// </remarks>
        /// <param name="difference">The ammount to add to min health</param>
        public void AddToMinHealth(float difference)
        {
            var newValue = _maxHealth + difference;

            SetMinHealth(newValue);
        }

        /// <summary>
        /// Multiples the min health by the given value.
        /// </summary>
        /// <remarks>
        /// <para>Current health will be re-clamped to the new bounds.</para>
        /// </remarks>
        /// <param name="scale">The value by which to multiply the min health</param>
        public void MultiplyByMinHealth(float scale)
        {
            var newValue = _maxHealth * scale;

            SetMinHealth(newValue);
        }

        /// <summary>
        /// Checks if the current health is the same as the min or max health.
        /// </summary>
        /// <param name="previousHealth"></param>
        private void CheckIfHealthAtBounds(float previousHealth)
        {
            if (CurrentHealthIsAtMinHealth)
                EventBus.Emit(new BoardPiece_HealthAtMinEvent(_boardPiece, previousHealth, _currentHealth));

            if (CurrentHealthIsAtMaxHealth)
                EventBus.Emit(new BoardPiece_HealthAtMaxEvent(_boardPiece, previousHealth, _currentHealth));
        }
    }

    public readonly record struct BoardPiece_HealthChangedEvent(BoardPiece BoardPiece, float PreviousHealth, float NewHealth) : IEventType;

    public readonly record struct BoardPiece_HealthAtMinEvent(BoardPiece BoardPiece, float PreviousHealth, float NewHealth) : IEventType;

    public readonly record struct BoardPiece_HealthAtMaxEvent(BoardPiece BoardPiece, float PreviousHealth, float NewHealth) : IEventType;

    public readonly record struct BoardPiece_MaxHealthChangedEvent(BoardPiece BoardPiece, float PreviousMaxHealth, float NewMaxHealth) : IEventType;

    public readonly record struct BoardPiece_MinHealthChangedEvent(BoardPiece BoardPiece, float PreviousMinHealth, float NewMinHealth) : IEventType;
}
