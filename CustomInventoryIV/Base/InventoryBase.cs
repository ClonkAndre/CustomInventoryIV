using System;
using System.Numerics;

using IVSDKDotNet;

namespace CustomInventoryIV.Base
{
    /// <summary>
    /// The base class for an inventory.
    /// <para>You can use this class to create your own inventory.</para>
    /// </summary>
    public abstract class InventoryBase
    {

        #region Variables and Properties
        // Variables
        private Guid id;
        private string name;
        internal bool isVisible;

        private Vector2 position;

        // Properties
        /// <summary>
        /// The unique id of this inventory.
        /// </summary>
        public Guid ID
        {
            get => id;
            private set => id = value;
        }
        public string Name
        {
            get => name;
            set => name = value;
        }
        public bool IsVisible
        {
            get => isVisible;
            set => isVisible = value;
        }

        public Vector2 Position
        {
            get => position;
            set => position = value;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="InventoryBase"/> class.
        /// </summary>
        /// <param name="name">The name of the inventory.</param>
        /// <param name="isVisible">Sets if this inventory should be visible or not when created.</param>
        public InventoryBase(string name, bool isVisible)
        {
            ID = Guid.NewGuid();
            Name = name;
            IsVisible = isVisible;
        }
        /// <summary>
        /// Creates a new instance of the <see cref="InventoryBase"/> class.
        /// </summary>
        /// <param name="name">The name of the inventory.</param>
        public InventoryBase(string name)
        {
            ID = Guid.NewGuid();
            Name = name;
            IsVisible = false;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InventoryBase"/> class.
        /// </summary>
        /// <param name="isVisible">Sets if this inventory should be visible or not when created.</param>
        public InventoryBase(bool isVisible)
        {
            ID = Guid.NewGuid();
            IsVisible = isVisible;
        }
        /// <summary>
        /// Creates a new instance of the <see cref="InventoryBase"/> class.
        /// </summary>
        public InventoryBase()
        {
            ID = Guid.NewGuid();
            IsVisible = false;
        }
        #endregion

        /// <summary>
        /// Responsible for drawing the inventory and all the items inside.
        /// </summary>
        public abstract void Draw(ImGuiIV_DrawingContext backgroundDrawList);

        public virtual void PushStyle()
        {

        }
        public virtual void PopStyle()
        {

        }

    }
}
