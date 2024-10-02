using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

using CustomInventoryIV.Base;

using IVSDKDotNet;
using IVSDKDotNet.Enums;

namespace CustomInventoryIV.Inventories
{
    public class BasicInventory : InventoryBase
    {

        #region Variables and Properties
        // Variables
        private BasicInventoryItem[] items;
        private int capacity;
        private int newLineAt;

        private bool isResizing;

        private Vector2 itemSize = new Vector2(128f);

        // Properties
        /// <summary>
        /// Gets how many items can be stored in this <see cref="BasicInventory"/>.
        /// </summary>
        public int Capacity
        {
            get => capacity;
            private set => capacity = value;
        }
        /// <summary>
        /// When to place an <see cref="BasicInventoryItem"/> in a new line.
        /// </summary>
        public int NewLineAt
        {
            get => newLineAt;
            set => newLineAt = value;
        }
        /// <summary>
        /// The size of every <see cref="BasicInventoryItem"/> in this <see cref="BasicInventory"/>.
        /// </summary>
        public Vector2 ItemSize
        {
            get => itemSize;
            set => itemSize = value;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="BasicInventory"/> class.
        /// </summary>
        /// <param name="capacity">The inventory size.</param>
        /// <param name="newLineAt">
        /// Defines the number at which a new line will be started.
        /// <para>Example: <paramref name="capacity"/> is set to 8, and <paramref name="newLineAt"/> is set to 4, this means a new line will be started at the 4th item.</para>
        /// </param>
        public BasicInventory(int capacity, int newLineAt)
        {
            items = new BasicInventoryItem[capacity];
            Capacity = capacity;
            NewLineAt = newLineAt;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BasicInventory"/> class.
        /// </summary>
        /// <param name="capacity">The inventory size.</param>
        public BasicInventory(int capacity)
        {
            items = new BasicInventoryItem[capacity];
            Capacity = capacity;
            NewLineAt = capacity / 2;
        }
        #endregion

        #region Methods
        private void DrawItem(int index)
        {
            BasicInventoryItem item = null;

            if (IsIndexValid(index))
                item = items[index];

            if ((index % NewLineAt) != 0)
                ImGuiIV.SameLine();
            else
                ImGuiIV.Spacing();

            if (ImGuiIV.BeginChild(string.Format("##BasicInventory_{0}_Child_{1}", ID, index), ItemSize, eImGuiChildFlags.None, eImGuiWindowFlags.NoScrollWithMouse | eImGuiWindowFlags.NoScrollbar))
            {
                ImGuiIV_DrawingContext ctx = ImGuiIV.GetWindowDrawList();
                Vector2 origin = ImGuiIV.GetCursorScreenPos();

                if (item == null)
                    ImGuiIV.BeginDisabled();

                ImGuiIV.PushStyleVar(eImGuiStyleVar.FramePadding, Vector2.Zero);

                if (ImGuiIV.Button(string.Format("{0}##BasicInventory_{1}_ChildButton_{2}", item == null ? "" : item.ButtonText, ID, index), ItemSize))
                    OnItemClick?.Invoke(this, item, index);

                ImGuiIV.PopStyleVar();

                if (item == null)
                    ImGuiIV.EndDisabled();

                // Drag & Drop Target
                if (ImGuiIV.BeginDragDropTarget())
                {
                    ImGuiIV_Payload payload = ImGuiIV.AcceptDragDropPayload(Name);
                    if (payload.IsValid)
                    {
                        int draggedItemIndex = Marshal.PtrToStructure<int>(payload.Data);

                        if (item == null)
                        {
                            // Get the item that was dragged
                            BasicInventoryItem draggedItem = GetItem(draggedItemIndex);

                            // Remove item the dragged item from their original slot
                            RemoveItem(draggedItemIndex);

                            // Insert dragged item at new position
                            InsertItem(index, draggedItem);
                        }
                        else
                        {
                            // Get the item that was dragged
                            BasicInventoryItem draggedItem = GetItem(draggedItemIndex);

                            // Remove item the dragged item from their original slot
                            RemoveItem(draggedItemIndex);

                            // Remove item where the dragged item was dragged on to
                            RemoveItem(index);

                            // Insert dragged item at new position
                            InsertItem(index, draggedItem);

                            // Insert the item that the dragged item was dragged on to at the slot the dragged item was before
                            InsertItem(draggedItemIndex, item);
                        }
                    }
                    ImGuiIV.EndDragDropTarget();
                }

                // Do stuff when item object is valid
                if (item != null)
                {
                    // Drag & Drop Source
                    if (ImGuiIV.BeginDragDropSource(eImGuiDragDropFlags.None))
                    {
                        int size = Marshal.SizeOf(index);
                        IntPtr ptr = Marshal.AllocHGlobal(size);
                        Marshal.StructureToPtr(index, ptr, true);

                        ImGuiIV.SetDragDropPayload(Name, ptr, (uint)size, eImGuiCond.None);
                        ImGuiIV.Image(item.Icon.Texture, new Vector2(item.Icon.GetWidth(), item.Icon.GetHeight()));
                        ImGuiIV.EndDragDropSource();
                    }

                    // Right-click popup menu
                    if (item.PopupMenuItems != null)
                    {
                        if (ImGuiIV.BeginPopupContextItem())
                        {
                            for (int i = 0; i < item.PopupMenuItems.Count; i++)
                            {
                                string popupMenuItem = item.PopupMenuItems[i];

                                if (ImGuiIV.Selectable(popupMenuItem))
                                    OnPopupItemClick?.Invoke(this, item, popupMenuItem);
                            }

                            ImGuiIV.EndPopup();
                        }
                    }

                    // Draw Icon
                    if (item.Icon != null)
                    {
                        float aspectRatio = item.Icon.GetAspectRatio();
                        SizeF s = new SizeF(item.Icon.GetWidth() / aspectRatio, item.Icon.GetHeight() / aspectRatio);

                        Vector2 centerPos = new Vector2(origin.X + ItemSize.X / 2f, origin.Y + ItemSize.Y / 2f);
                        ctx.AddImage(item.Icon.Texture, new RectangleF(centerPos.X - s.Width / 2f, centerPos.Y - s.Height / 2f, s.Width, s.Height), Color.White);
                    }

                    // Draw Top Left Text
                    if (!string.IsNullOrWhiteSpace(item.TopLeftText))
                    {
                        ImGuiIV.SetCursorScreenPos(origin + new Vector2(5f, 4f));
                        ImGuiIV.TextColored(item.TopLeftColor, item.TopLeftText);
                    }

                    // Draw Top Right Text
                    if (!string.IsNullOrWhiteSpace(item.TopRightText))
                    {
                        Vector2 textSize = ImGuiIV.CalcTextSize(item.TopRightText);
                        ImGuiIV.SetCursorScreenPos(origin + new Vector2(ItemSize.X - textSize.X, 0f) + new Vector2(-5f, 4f));
                        ImGuiIV.TextColored(item.TopRightColor, item.TopRightText);
                    }

                    // Draw Bottom Left Text
                    if (!string.IsNullOrWhiteSpace(item.BottomLeftText))
                    {
                        Vector2 textSize = ImGuiIV.CalcTextSize(item.BottomLeftText);
                        ImGuiIV.SetCursorScreenPos(origin + new Vector2(0f, ItemSize.X - textSize.Y) + new Vector2(5f, -4f));
                        ImGuiIV.TextColored(item.BottomLeftColor, item.BottomLeftText);
                    }

                    // Draw Bottom Right Text
                    if (!string.IsNullOrWhiteSpace(item.BottomRightText))
                    {
                        Vector2 textSize = ImGuiIV.CalcTextSize(item.BottomRightText);
                        ImGuiIV.SetCursorScreenPos(origin + new Vector2(ItemSize.X - textSize.X, ItemSize.Y - textSize.Y) - new Vector2(5f, 4f));
                        ImGuiIV.TextColored(item.BottomRightColor, item.BottomRightText);
                    }
                }

            }
            ImGuiIV.EndChild();
        }
        #endregion

        #region Functions
        /// <summary>
        /// Adds a <see cref="BasicInventoryItem"/> at the next available position in this <see cref="BasicInventory"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns><see langword="true"/> if added. Otherwise <see langword="false"/>.</returns>
        public bool AddItem(BasicInventoryItem item)
        {
            if (item == null)
                return false;
            //if (items.Count >= items.Length)
            //    return false;

            //items.Add(item);

            for (int i = 0; i < Capacity; i++)
            {
                BasicInventoryItem slot = items[i];

                if (slot == null)
                {
                    items[i] = item;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Inserts a <see cref="BasicInventoryItem"/> at the given <paramref name="index"/> in this <see cref="BasicInventory"/>.
        /// <para>Note: This will replace the item at the given <paramref name="index"/> with the one given by the <paramref name="item"/> parameter.</para>
        /// </summary>
        /// <param name="index">The index where to insert the item.</param>
        /// <param name="item">The item which will be inserted.</param>
        /// <returns><see langword="true"/> if inserted. Otherwise <see langword="false"/>.</returns>
        public bool InsertItem(int index, BasicInventoryItem item)
        {
            if (!IsIndexValid(index))
                return false;

            items[index] = item;
            return true;
        }

        /// <summary>
        /// Removes an <see cref="BasicInventoryItem"/> from this <see cref="BasicInventory"/> at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of where to remove an <see cref="BasicInventoryItem"/>.</param>
        /// <returns><see langword="true"/> if removed. Otherwise <see langword="false"/>.</returns>
        public bool RemoveItem(int index)
        {
            if (!IsIndexValid(index))
                return false;

            items[index] = null;

            //items.RemoveAt(index);
            return true;
        }
        /// <summary>
        /// Removes all <see cref="BasicInventoryItem"/> instances from this <see cref="BasicInventory"/> with the given <paramref name="hash"/>.
        /// </summary>
        /// <param name="hash">The hash of the <see cref="BasicInventoryItem"/> instances to remove.</param>
        /// <returns>The number of items removed.</returns>
        public bool RemoveItem(uint hash)
        {
            if (!ContainsItem(hash))
                return false;

            for (int i = 0; i < Capacity; i++)
            {
                BasicInventoryItem slot = items[i];

                if (slot == null)
                    continue;

                if (slot.Hash == hash)
                {
                    items[i] = null;
                    return true;
                }
            }

            //return items.RemoveAll(x => x.Hash == hash);
            return false;
        }
        /// <summary>
        /// Removes the given <see cref="BasicInventoryItem"/> <paramref name="item"/> from this <see cref="BasicInventory"/>.
        /// </summary>
        /// <param name="item">The <see cref="BasicInventoryItem"/> to remove.</param>
        /// <returns><see langword="true"/> if removed. Otherwise <see langword="false"/>.</returns>
        public bool RemoveItem(BasicInventoryItem item)
        {
            if (item == null)
                return false;

            for (int i = 0; i < Capacity; i++)
            {
                BasicInventoryItem slot = items[i];

                if (slot == null)
                    continue;

                if (slot == item)
                {
                    items[i] = null;
                    return true;
                }
            }

            //return items.Remove(item);
            return false;
        }

        /// <summary>
        /// Checks if a <see cref="BasicInventoryItem"/> with the given <paramref name="hash"/> exists in this <see cref="BasicInventory"/>.
        /// </summary>
        /// <param name="hash">The hash of a <see cref="BasicInventoryItem"/> to look for.</param>
        /// <returns><see langword="true"/> if exists. Otherwise <see langword="false"/>.</returns>
        public bool ContainsItem(uint hash)
        {
            return items.Any(x =>
            {
                if (x == null)
                    return false;

                return x.Hash == hash;
            });
        }

        /// <summary>
        /// Gets a <see cref="BasicInventoryItem"/> at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Where to get the item from.</param>
        /// <returns>The <see cref="BasicInventoryItem"/> if found. Otherwise <see langword="null"/>.</returns>
        public BasicInventoryItem GetItem(int index)
        {
            if (!IsIndexValid(index))
                return null;

            return items[index];
        }
        /// <summary>
        /// Gets a <see cref="BasicInventoryItem"/> which has this <paramref name="hash"/>.
        /// </summary>
        /// <param name="hash">The hash to look for.</param>
        /// <returns>The <see cref="BasicInventoryItem"/> if found. Otherwise <see langword="null"/>.</returns>
        public BasicInventoryItem GetItem(uint hash)
        {
            if (!ContainsItem(hash))
                return null;

            return items.Where(x =>
            {
                if (x == null)
                    return false;

                return x.Hash == hash;
            }).FirstOrDefault();
        }

        /// <summary>
        /// Changes the capacity of this <see cref="BasicInventory"/>.
        /// </summary>
        /// <param name="capacity">The new capacity of the <see cref="BasicInventory"/>.</param>
        /// <returns>If the new <paramref name="capacity"/> is less then the current capacity, this will return the items that where left behind. Otherwise, <see langword="null"/> is returned.</returns>
        public List<BasicInventoryItem> Resize(int capacity)
        {
            if (Capacity == capacity)
                return null;

            isResizing = true;

            Capacity = capacity;
            items = Helper.ResizeArray(items, Capacity, out List<BasicInventoryItem> leftBehindItems);

            if (leftBehindItems != null)
            {
                if (leftBehindItems.Count == 0)
                    leftBehindItems = null;
            }

            isResizing = false;

            return leftBehindItems;
        }

        private bool IsIndexValid(int index)
        {
            return index < Capacity;
        }
        #endregion

        #region Delegates and Events

        // Delegates
        /// <summary>
        /// Delegate for when the item was dragged out of this <see cref="BasicInventory"/>.
        /// </summary>
        /// <param name="sender">Which <see cref="BasicInventory"/> sent this event.</param>
        /// <param name="item">The target <see cref="BasicInventoryItem"/> which was dragged out. This can be null.</param>
        /// <param name="itemIndex">The index where the <see cref="BasicInventoryItem"/> is placed.</param>
        public delegate void ItemDraggedOutDelegate(BasicInventory sender, BasicInventoryItem item, int itemIndex);
        /// <summary>
        /// Delegate for when a item in this <see cref="BasicInventory"/> was clicked.
        /// </summary>
        /// <param name="sender">Which <see cref="BasicInventory"/> sent this event.</param>
        /// <param name="item">The target <see cref="BasicInventoryItem"/> which was clicked.</param>
        /// <param name="itemIndex">The index where the <see cref="BasicInventoryItem"/> is placed.</param>
        public delegate void ItemClickDelegate(BasicInventory sender, BasicInventoryItem item, int itemIndex);
        /// <summary>
        /// Delegate for when a popup item of <see cref="BasicInventoryItem"/> was clicked in this <see cref="BasicInventory"/>.
        /// </summary>
        /// <param name="sender">Which <see cref="BasicInventory"/> sent this event.</param>
        /// <param name="item">The target <see cref="BasicInventoryItem"/> of which a popup item was clicked.</param>
        /// <param name="popupItemName">The name of the popup item within the <see cref="BasicInventoryItem"/>.</param>
        public delegate void PopupItemClickDelegate(BasicInventory sender, BasicInventoryItem item, string popupItemName);

        // Events
        /// <summary>
        /// Gets raised when an item is dragged out of this <see cref="BasicInventory"/>.
        /// </summary>
        public event ItemDraggedOutDelegate OnItemDraggedOut;

        // Events
        /// <summary>
        /// Gets raised when a <see cref="BasicInventoryItem"/> was clicked in this <see cref="BasicInventory"/>.
        /// </summary>
        public event ItemClickDelegate OnItemClick;

        /// <summary>
        /// Gets raised when an item in the right-click popup within the <see cref="BasicInventoryItem"/> was clicked.
        /// </summary>
        public event PopupItemClickDelegate OnPopupItemClick;

        #endregion

        /// <inheritdoc/>
        public override void Draw(ImGuiIV_DrawingContext backgroundDrawList)
        {
            if (!IsVisible)
                return;

            // Push inventory style
            PushStyle();

            if (ImGuiIV.Begin(string.Format("##CustomInventory_{0}", ID), ref isVisible, eImGuiWindowFlags.NoDecoration | eImGuiWindowFlags.AlwaysAutoResize))
            {
                if (!isResizing)
                {
                    // Draw items
                    for (int i = 0; i < Capacity; i++)
                    {
                        DrawItem(i);
                    }

                    // Custom drag & drop handler outside window
                    Vector2 pos = ImGuiIV.GetWindowPos();
                    Vector2 size = ImGuiIV.GetWindowSize();
                    RectangleF rect = new RectangleF(pos.X, pos.Y, size.X, size.Y);

                    if (!ImGuiIV.IsMouseHoveringRect(rect, false) && ImGuiIV.IsDragDropActive())
                    {
                        ImGuiIV_Payload payload = ImGuiIV.AcceptDragDropPayload(Name, eImGuiDragDropFlags.AcceptNoDrawDefaultRect);
                        if (payload.IsValid)
                        {
                            int index = Marshal.PtrToStructure<int>(payload.Data);

                            // Raise event
                            OnItemDraggedOut?.Invoke(this, GetItem(index), index);

                            ImGuiIV.ClearDragDrop();
                            Marshal.FreeHGlobal(payload.Data);
                        }

                        Rectangle r = IVGame.Bounds;
                        backgroundDrawList.AddRect(new Vector2(r.X + 5f, r.Y + 5f), new Vector2(r.Right - 5f, r.Bottom - 5f), Color.Yellow, 0f, eImDrawFlags.None, 1f);
                    }
                }
            }
            ImGuiIV.End();

            PopStyle();
        }

        /// <inheritdoc/>
        public override void PushStyle()
        {
            ImGuiIV.PushStyleVar(eImGuiStyleVar.WindowBorderSize, 0f);
            ImGuiIV.PushStyleVar(eImGuiStyleVar.WindowRounding, 6f);
            ImGuiIV.PushStyleVar(eImGuiStyleVar.FrameRounding, 6f);

            ImGuiIV.PushStyleColor(eImGuiCol.WindowBg, new Vector4(0.08f, 0.08f, 0.08f, 0.5f));
            ImGuiIV.PushStyleColor(eImGuiCol.FrameBg, Color.FromArgb(100, Color.Black));
            ImGuiIV.PushStyleColor(eImGuiCol.Button, new Vector4(0.0f, 0.0f, 0.0f, 0.7f));
            ImGuiIV.PushStyleColor(eImGuiCol.ButtonHovered, new Vector4(0.12f, 0.12f, 0.12f, 0.9f));
            ImGuiIV.PushStyleColor(eImGuiCol.ButtonActive, new Vector4(0.15f, 0.15f, 0.15f, 0.9f));
        }
        /// <inheritdoc/>
        public override void PopStyle()
        {
            ImGuiIV.PopStyleVar(3);
            ImGuiIV.PopStyleColor(5);
        }

    }
}
