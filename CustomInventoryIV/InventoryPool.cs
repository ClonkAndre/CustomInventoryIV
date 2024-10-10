using System;
using System.Collections.Generic;
using System.Linq;

using CustomInventoryIV.Base;

using IVSDKDotNet;

namespace CustomInventoryIV
{
    public class InventoryPool
    {

        #region Variables
        private List<InventoryBase> inventories;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="InventoryPool"/> class.
        /// </summary>
        public InventoryPool()
        {
            inventories = new List<InventoryBase>();
        }
        #endregion

        /// <summary>
        /// Adds a new inventory to this <see cref="InventoryPool"/>.
        /// </summary>
        /// <param name="item">The inventory to add.</param>
        public void Add(InventoryBase item)
        {
            inventories.Add(item);
        }

        /// <summary>
        /// Gets an inventory which got this <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id to search for.</param>
        /// <returns>The <see cref="InventoryBase"/> when found. Otherwise <see langword="null"/>.</returns>
        public InventoryBase Get(Guid id)
        {
            return inventories.Where(x => x.ID == id).FirstOrDefault();
        }

        /// <summary>
        /// Removes an inventory from this <see cref="InventoryPool"/>.
        /// </summary>
        /// <param name="item">The inventory to add.</param>
        /// <returns><see langword="true"/> if removed. Otherwise <see langword="false"/>.</returns>
        public bool Remove(InventoryBase item)
        {
            if (item == null)
                return false;

            return inventories.Remove(item);
        }
        /// <summary>
        /// Removes an inventory from this <see cref="InventoryPool"/>.
        /// </summary>
        /// <param name="index">The index to where to remove the inventory.</param>
        /// <returns><see langword="true"/> if removed. Otherwise <see langword="false"/>.</returns>
        public bool Remove(int index)
        {
            if (index < 0)
                return false;
            if (index > inventories.Count)
                return false;

            inventories.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes all added inventories of this <see cref="InventoryPool"/>.
        /// </summary>
        public void Clear()
        {
            inventories.Clear();
        }

        /// <summary>
        /// Changes the visibility of all inventories.
        /// </summary>
        /// <param name="visible">The new visibility.</param>
        public void ChangeVisibilityOfAllInventories(bool visible)
        {
            inventories.ForEach(x =>
            {
                x.IsVisible = visible;
            });
        }

        /// <summary>
        /// Handles the drawing of all added inventories in this <see cref="InventoryPool"/>.
        /// </summary>
        public void ProcessDrawing(ImGuiIV_DrawingContext backgroundDrawList)
        {
            inventories.ForEach(x => x.Draw(backgroundDrawList));
        }

    }
}
