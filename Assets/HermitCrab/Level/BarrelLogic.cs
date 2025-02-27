using System;

namespace HermitCrab.Level
{
    /// <summary>
    ///     Contains the core logic for a Barrel hazard.
    ///     This class is independent from UnityEngine and can be unit tested.
    /// </summary>
    public class BarrelLogic
    {
        public BarrelLogic(BarrelData data)
        {
            Config = data ?? throw new ArgumentNullException(nameof(data));
            IsActivated = false;
        }

        public bool IsActivated { get; private set; }

        /// <summary>
        ///     Exposes the barrel configuration.
        /// </summary>
        public BarrelData Config { get; }

        /// <summary>
        ///     Raised when the barrel is activated.
        /// </summary>
        public event Action OnActivated;

        /// <summary>
        ///     Call this method when the barrel receives any damage.
        ///     It will activate the barrel exactly once.
        /// </summary>
        public void ReceiveDamage()
        {
            if (!IsActivated)
            {
                IsActivated = true;
                OnActivated?.Invoke();
            }
        }
    }
}