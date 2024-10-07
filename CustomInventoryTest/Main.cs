using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using CCL.GTAIV;

using CustomInventoryIV;
using CustomInventoryIV.Inventories;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

namespace CustomInventoryTest
{
    public class Main : Script
    {

        #region Variables
        private int playerPedHandle;
        private int lastPlayerWeapon;
        private uint lastLoadedEpisode;

        public Dictionary<int, CITexture> loadedWeaponTextures;

        private InventoryPool inventoryPool;
        private BasicInventory basicInventory;

        private bool blockPlayerAbilityToCollectPickup;
        private Vector3 blockPosition;

        private Stopwatch tabKeyWatch;
        #endregion

        #region Classes
        public class Texture
        {
            public string FileName;
            public IntPtr Ptr;

            public Texture(string fileName, IntPtr ptr)
            {
                FileName = fileName;
                Ptr = ptr;
            }

            public override string ToString()
            {
                return string.Format("FileName: {0}, Pointer: {1}", FileName, Ptr);
            }
        }
        #endregion

        #region Constructor
        public Main()
        {
            loadedWeaponTextures = new Dictionary<int, CITexture>(32);
            inventoryPool = new InventoryPool();

            tabKeyWatch = new Stopwatch();

            Uninitialize += Main_Uninitialize;
            Initialized += Main_Initialized;
            GameLoad += Main_GameLoad;
            OnImGuiRendering += Main_OnImGuiRendering;
            Tick += Main_Tick;
        }
        #endregion

        #region Methods
        private void DropItem(BasicInventory inventory, BasicInventoryItem item, float range = 0.0F)
        {
            if (inventory == null)
                return;
            if (item == null)
                return;

            int weaponType = Convert.ToInt32(item.Tags["WeaponType"]);

            GET_AMMO_IN_CHAR_WEAPON(playerPedHandle, weaponType, out int ammo);

            REMOVE_WEAPON_FROM_CHAR(playerPedHandle, weaponType);

            GET_CHAR_COORDINATES(playerPedHandle, out Vector3 pos);

            if (range != 0.0F)
                pos = pos.Around(range);

            // Disable ability for local player to any pickups
            DISABLE_LOCAL_PLAYER_PICKUPS(true);
            blockPlayerAbilityToCollectPickup = true;
            blockPosition = pos;

            // Creates a weapon pickup at the players location
            CreateWeaponPickupAtPosition(pos, weaponType, ammo);

            // Removes the item out of the inventory
            inventory.RemoveItem(item);
        }

        private void OpenInventory(bool wasOpenedUsingController)
        {
            basicInventory.IsVisible = true;

            if (wasOpenedUsingController)
                wasInventoryOpenedViaController = true;
        }
        private void CloseInventory()
        {
            basicInventory.IsVisible = false;
            wasInventoryOpenedViaController = false;
        }
        #endregion

        #region Functions
        public static float Lerp(float a, float b, float t)
        {
            // Clamp t between 0 and 1
            t = Math.Max(0.0f, Math.Min(1.0f, t));

            return a + (b - a) * t;
        }

        private int CreateWeaponPickupAtPosition(Vector3 pos, int weaponType, int ammo)
        {
            GET_WEAPONTYPE_MODEL(weaponType, out uint model);

            Vector3 spawnPos = NativeWorld.GetGroundPosition(pos) + new Vector3(0f, 0f, 0.05f);
            CREATE_PICKUP_ROTATE(model, (uint)ePickupType.PICKUP_TYPE_WEAPON, (uint)ammo, spawnPos, new Vector3(90f, 0f, GENERATE_RANDOM_FLOAT_IN_RANGE(0f, 90f)), out int pickup);

            // DOES_PICKUP_EXIST
            // REMOVE_PICKUP

            return pickup;
        }
        #endregion

        private bool wasInventoryOpenedViaController;
        public Vector2 itemSize;

        private void Main_Uninitialize(object sender, EventArgs e)
        {
            inventoryPool.Clear();
        }
        private void Main_Initialized(object sender, EventArgs e)
        {
            basicInventory = new BasicInventory("TestInventory", 8);
            basicInventory.OnItemDraggedOut += BasicInventory_OnItemDraggedOut;
            basicInventory.OnPopupItemClick += BasicInventory_OnPopupItemClick;
            basicInventory.OnItemClick += BasicInventory_OnItemClick;
            basicInventory.OnInventoryResized += BasicInventory_OnInventoryResized;
            basicInventory.ItemSize = new Vector2(128f, 100f);

            itemSize = basicInventory.ItemSize;

            inventoryPool.Add(basicInventory);
        }

        private void LoadTextures()
        {
            if (loadedWeaponTextures.Count != 0)
            {
                // Destroy loaded textures if changing episode
                if (lastLoadedEpisode != IVGame.CurrentEpisode)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        CITexture txt = loadedWeaponTextures[i];
                        ImGuiIV.ReleaseTexture(ref txt.Texture);
                    }
                    loadedWeaponTextures.Clear();
                }
            }

            // Set episode
            lastLoadedEpisode = IVGame.CurrentEpisode;

            string path = string.Format("{0}\\Icons\\Weapons\\{1}", ScriptResourceFolder, lastLoadedEpisode);

            // Create textures for the current episode
            string[] files = Directory.GetFiles(path, "*.dds", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                string fileName = Path.GetFileName(file);

                if (int.TryParse(fileName.Split('.')[0], out int result))
                {
                    // Create texture
                    if (ImGuiIV.CreateTextureFromFile(string.Format("{0}\\{1}", path, fileName), out IntPtr txtPtr, out int w, out int h, out eResult r))
                    {
                        loadedWeaponTextures.Add(result, new CITexture(txtPtr, new Size(w, h)));
                    }
                }
            }
        }
        private void Main_GameLoad(object sender, EventArgs e)
        {
            LoadTextures();
        }

        private void Main_OnImGuiRendering(IntPtr devicePtr, ImGuiIV_DrawingContext ctx)
        {
            inventoryPool.ProcessDrawing(ctx);
        }

        private void BasicInventory_OnItemDraggedOut(BasicInventory sender, BasicInventoryItem item, int itemIndex)
        {
            if (sender.Name == "TestInventory")
            {
                DropItem(sender, item);
            }
        }
        private void BasicInventory_OnPopupItemClick(BasicInventory sender, BasicInventoryItem item, string popupItemName)
        {
            if (sender.Name == "TestInventory")
            {
                if (popupItemName == "Drop")
                {
                    DropItem(sender, item);
                }
            }
        }
        private void BasicInventory_OnItemClick(BasicInventory sender, BasicInventoryItem item, int itemIndex)
        {
            if (item.Tags.ContainsKey("IsIncludedWeapon"))
            {
                int weaponType = Convert.ToInt32(item.Tags["WeaponType"]);
                lastPlayerWeapon = weaponType;
                SET_CURRENT_CHAR_WEAPON(playerPedHandle, weaponType, false);
            }

            if (wasInventoryOpenedViaController)
                CloseInventory();

            //if (itemIndex == 0)
            //{
            //    sender.Resize(12);
            //}
            //else if (itemIndex == 1)
            //{
            //    List<BasicInventoryItem> leftBehindItems = sender.Resize(8);

            //    if (leftBehindItems != null)
            //    {
            //        IVGame.Console.PrintError(string.Format("There are {0} left behind items:", leftBehindItems.Count));
            //        for (int i = 0; i < leftBehindItems.Count; i++)
            //        {
            //            BasicInventoryItem leftBehindItem = leftBehindItems[i];
            //            IVGame.Console.PrintWarning(leftBehindItem.TopLeftText);
            //        }
            //    }
            //    else
            //    {
            //        IVGame.Console.PrintError("There where no left behind items.");
            //    }
            //}
        }
        private void BasicInventory_OnInventoryResized(BasicInventory target, List<BasicInventoryItem> leftBehindItems)
        {
            if (leftBehindItems != null)
            {
                for (int i = 0; i < leftBehindItems.Count; i++)
                {
                    BasicInventoryItem item = leftBehindItems[i];

                    if (item.Tags.ContainsKey("IsIncludedWeapon"))
                        DropItem(target, item, i * 0.15f);
                }
            }
        }

        private float lerpValue;
        public int InventoryOpenTimeInMS = 150;
        private bool setCursorPos;

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        private void Main_Tick(object sender, EventArgs e)
        {
            if (loadedWeaponTextures.Count == 0)
                LoadTextures();

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            playerPedHandle = playerPed.GetHandle();
            GET_CHAR_COORDINATES(playerPedHandle, out Vector3 pos);
            GET_PED_BONE_POSITION(playerPedHandle, (uint)eBone.BONE_HEAD, Vector3.Zero, out Vector3 headPos);

            basicInventory.ItemSize = itemSize;

            bool wasOpenedViaController = NativeControls.IsUsingJoypad() && NativeControls.IsControllerButtonPressed(0, ControllerButton.BUTTON_BUMPER_LEFT);
            if (IsKeyPressed(Keys.Tab) || wasOpenedViaController)
            {
                if (tabKeyWatch.IsRunning)
                {
                    if (tabKeyWatch.ElapsedMilliseconds > InventoryOpenTimeInMS)
                    {
                        if (!basicInventory.IsVisible)
                        {
                            if (!setCursorPos)
                            {
                                GET_SCREEN_RESOLUTION(out int x, out int y);
                                SetCursorPos(x / 2, y / 2);
                                setCursorPos = true;
                            }

                            OpenInventory(wasOpenedViaController);
                        }
                    }
                }
                else
                {
                    tabKeyWatch.Start();
                }
            }
            else
            {
                if (tabKeyWatch.IsRunning)
                {
                    if (tabKeyWatch.ElapsedMilliseconds < InventoryOpenTimeInMS)
                    {
                        // Switch to last weapon or fist
                        GET_CURRENT_CHAR_WEAPON(playerPedHandle, out int currentWeapon);

                        if ((eWeaponType)currentWeapon == eWeaponType.WEAPON_UNARMED)
                        {
                            if (HAS_CHAR_GOT_WEAPON(playerPedHandle, lastPlayerWeapon))
                                SET_CURRENT_CHAR_WEAPON(playerPedHandle, lastPlayerWeapon, false);
                        }
                        else
                        {
                            lastPlayerWeapon = currentWeapon;
                            SET_CURRENT_CHAR_WEAPON(playerPedHandle, (int)eWeaponType.WEAPON_UNARMED, false);
                        }

                    }

                    tabKeyWatch.Reset();

                    if (!wasInventoryOpenedViaController)
                        basicInventory.IsVisible = false;
                }
            }

            if (basicInventory.IsVisible)
            {
                if (wasInventoryOpenedViaController && ImGuiIV.IsKeyDown(eImGuiKey.ImGuiKey_GamepadFaceUp))
                {
                    CloseInventory();
                }

                basicInventory.PositionAtWorldCoordinate(headPos);

                lerpValue = lerpValue + 0.02f;

                if (lerpValue > 1.0f)
                    lerpValue = 1.0f;

                IVTimer.TimeScale = Lerp(1.0f, 0.25f, lerpValue);
            }
            else
            {
                setCursorPos = false;

                lerpValue = lerpValue - 0.03f;

                if (lerpValue < 0.0f)
                    lerpValue = 0.0f;

                IVTimer.TimeScale = Lerp(1.0f, 0.25f, lerpValue);
            }

            if (blockPlayerAbilityToCollectPickup)
            {
                if (Vector3.Distance(pos, blockPosition) > 2.5f)
                {
                    DISABLE_LOCAL_PLAYER_PICKUPS(false);
                    blockPlayerAbilityToCollectPickup = false;
                }
                else
                {
                    DISABLE_LOCAL_PLAYER_PICKUPS(true);
                }
            }

            for (int i = 0; i < playerPed.WeaponData.Weapons.Length; i++)
            {
                GET_CHAR_WEAPON_IN_SLOT(playerPedHandle, i, out int weaponType, out int ammo0, out int ammo1);

                eWeaponType type = (eWeaponType)weaponType;

                switch (type)
                {
                    case eWeaponType.WEAPON_UNARMED:
                    case eWeaponType.WEAPON_WEAPONTYPE_LAST_WEAPONTYPE:
                    case eWeaponType.WEAPON_ARMOUR:
                    case eWeaponType.WEAPON_RAMMEDBYCAR:
                    case eWeaponType.WEAPON_RUNOVERBYCAR:
                    case eWeaponType.WEAPON_EXPLOSION:
                    case eWeaponType.WEAPON_UZI_DRIVEBY:
                    case eWeaponType.WEAPON_DROWNING:
                    case eWeaponType.WEAPON_FALL:
                    case eWeaponType.WEAPON_UNIDENTIFIED:
                    case eWeaponType.WEAPON_ANYMELEE:
                    case eWeaponType.WEAPON_ANYWEAPON:
                        continue;
                }

                uint nameHash = RAGE.AtStringHash(type.ToString());

                if (!basicInventory.ContainsItem(nameHash) && ammo0 != 0)
                {
                    BasicInventoryItem item = new BasicInventoryItem(nameHash);
                    item.Tags.Add("IsIncludedWeapon", null);
                    item.Tags.Add("WeaponType", weaponType);

                    item.PopupMenuItems.Add("Drop");
                    item.TopLeftText = NativeGame.GetCommonWeaponName(type);

                    if (loadedWeaponTextures.ContainsKey(weaponType))
                        item.Icon = loadedWeaponTextures[weaponType];

                    basicInventory.AddItem(item);
                }
                else
                {
                    BasicInventoryItem item = basicInventory.GetItem(nameHash);

                    if (item != null)
                    {
                        if (ammo0 != 0)
                        {
                            switch (type)
                            {
                                case eWeaponType.WEAPON_BASEBALLBAT:
                                case eWeaponType.WEAPON_KNIFE:
                                case eWeaponType.WEAPON_POOLCUE:
                                    item.BottomLeftText = "1x";
                                    break;

                                case eWeaponType.WEAPON_GRENADE:
                                case eWeaponType.WEAPON_MOLOTOV:
                                    item.BottomLeftText = string.Format("{0}x", ammo0);
                                    break;

                                case eWeaponType.WEAPON_RLAUNCHER:
                                    item.BottomLeftText = string.Format("{0} Rockets", ammo0);
                                    break;

                                default:
                                    item.BottomLeftText = string.Format("{0} Bullets", ammo0);
                                    break;
                            }
                        }
                        else
                        {
                            basicInventory.RemoveItem(nameHash);
                        }
                    }
                }

            }
        }

    }
}
