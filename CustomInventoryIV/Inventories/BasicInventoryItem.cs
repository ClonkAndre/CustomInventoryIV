using System;
using System.Collections.Generic;
using System.Drawing;

namespace CustomInventoryIV.Inventories
{
    public class BasicInventoryItem
    {
        #region Variables and Properties
        // Variables
        private Guid id;
        private uint hash;
        public Dictionary<string, object> Tags;

        private string buttonText;

        private string topLeftText;
        private Color topLeftColor;

        private string topRightText;
        private Color topRightColor;

        private string bottomLeftText;
        private Color bottomLeftColor;

        private string bottomRightText;
        private Color bottomRightColor;

        private CITexture icon;

        /// <summary>
        /// The item collection of the right-click popup menu of this <see cref="BasicInventoryItem"/>.
        /// </summary>
        public List<string> PopupMenuItems;

        // Properties
        /// <summary>
        /// The unique id of this item.
        /// </summary>
        public Guid ID
        {
            get => id;
            private set => id = value;
        }

        /// <summary>
        /// The hash of this item.
        /// </summary>
        public uint Hash
        {
            get => hash;
            set => hash = value;
        }

        /// <summary>
        /// The text of the button in the center.
        /// </summary>
        public string ButtonText
        {
            get => buttonText;
            set => buttonText = value;
        }

        /// <summary>
        /// The text at the top left corner of the item.
        /// </summary>
        public string TopLeftText
        {
            get => topLeftText;
            set => topLeftText = value;
        }
        /// <summary>
        /// The color of the text at the top left corner of the item.
        /// </summary>
        public Color TopLeftColor
        {
            get => topLeftColor;
            set => topLeftColor = value;
        }

        /// <summary>
        /// The text at the top right corner of the item.
        /// </summary>
        public string TopRightText
        {
            get => topRightText;
            set => topRightText = value;
        }
        /// <summary>
        /// The color of the text at the top right corner of the item.
        /// </summary>
        public Color TopRightColor
        {
            get => topRightColor;
            set => topRightColor = value;
        }

        /// <summary>
        /// The text at the bottom left corner of the item.
        /// </summary>
        public string BottomLeftText
        {
            get => bottomLeftText;
            set => bottomLeftText = value;
        }
        /// <summary>
        /// The color of the text at the bottom left corner of the item.
        /// </summary>
        public Color BottomLeftColor
        {
            get => bottomLeftColor;
            set => bottomLeftColor = value;
        }

        /// <summary>
        /// The text at the bottom right corner of the item.
        /// </summary>
        public string BottomRightText
        {
            get => bottomRightText;
            set => bottomRightText = value;
        }
        /// <summary>
        /// The color of the text at the bottom right corner of the item.
        /// </summary>
        public Color BottomRightColor
        {
            get => bottomRightColor;
            set => bottomRightColor = value;
        }

        /// <summary>
        /// The icon of this item.
        /// </summary>
        public CITexture Icon
        {
            get => icon;
            set => icon = value;
        }
        #endregion

        #region Constructor
        public BasicInventoryItem(uint hash)
        {
            ID = Guid.NewGuid();
            Hash = hash;
            Tags = new Dictionary<string, object>();

            TopLeftColor = Color.White;
            TopRightColor = Color.White;
            BottomLeftColor = Color.White;
            BottomRightColor = Color.White;

            ButtonText = string.Empty;

            // Lists
            PopupMenuItems = new List<string>();
        }
        public BasicInventoryItem()
        {
            ID = Guid.NewGuid();
            Hash = 0;
            Tags = new Dictionary<string, object>();

            TopLeftColor = Color.White;
            TopRightColor = Color.White;
            BottomLeftColor = Color.White;
            BottomRightColor = Color.White;

            ButtonText = string.Empty;

            // Lists
            PopupMenuItems = new List<string>();
        }
        #endregion

        #region Methods
        //internal void RaiseOnClick(BasicInventory sender, BasicInventoryItem item)
        //{
        //    OnClick?.Invoke(sender, item);
        //}
        //internal void RaiseOnPopupItemClick(BasicInventory sender, BasicInventoryItem item, string popupItemName)
        //{
        //    OnPopupItemClick?.Invoke(sender, item, popupItemName);
        //}
        #endregion
    }
}
