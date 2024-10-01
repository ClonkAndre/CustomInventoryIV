using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

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
        private uint lastLoadedEpisode;

        public Dictionary<int, IntPtr> loadedWeaponTextures;

        private InventoryPool inventoryPool;
        private BasicInventory basicInventory;

        private bool blockPlayerAbilityToCollectPickup;
        private Vector3 blockPosition;
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
            loadedWeaponTextures = new Dictionary<int, IntPtr>(32);
            inventoryPool = new InventoryPool();

            Uninitialize += Main_Uninitialize;
            Initialized += Main_Initialized;
            GameLoad += Main_GameLoad;
            OnImGuiRendering += Main_OnImGuiRendering;
            Tick += Main_Tick;
        }
        #endregion

        #region Functions
        private string GetWeaponName(eWeaponType type)
        {
            switch (type)
            {
                case eWeaponType.WEAPON_BASEBALLBAT: return "Baseball Bat";
                case eWeaponType.WEAPON_POOLCUE: return "Pool Cue";
                case eWeaponType.WEAPON_KNIFE: return "Knife";
                case eWeaponType.WEAPON_GRENADE: return "Grenade";
                case eWeaponType.WEAPON_MOLOTOV: return "Molotov";
                case eWeaponType.WEAPON_PISTOL: return "Pistol";
                case eWeaponType.WEAPON_DEAGLE: return "Deagle";
                case eWeaponType.WEAPON_SHOTGUN: return "Shotgun";
                case eWeaponType.WEAPON_BARETTA: return "Baretta";
                case eWeaponType.WEAPON_MICRO_UZI: return "Micro SMG";
                case eWeaponType.WEAPON_MP5: return "SMG";
                case eWeaponType.WEAPON_AK47: return "Assault Rifle";
                case eWeaponType.WEAPON_M4: return "Carbine Rifle";
                case eWeaponType.WEAPON_SNIPERRIFLE: return "Sniper Rifle";
                case eWeaponType.WEAPON_M40A1: return "Combat Sniper";
                case eWeaponType.WEAPON_RLAUNCHER: return "Rocket Launcher";
                case eWeaponType.WEAPON_FTHROWER: return "Flame Thrower";
                case eWeaponType.WEAPON_MINIGUN: return "Minigun";
                case eWeaponType.WEAPON_EPISODIC_1: return "Grenade Launcher";
                case eWeaponType.WEAPON_EPISODIC_2: return "Assault Shotgun";
                case eWeaponType.WEAPON_EPISODIC_3: return null;
                case eWeaponType.WEAPON_EPISODIC_4: return "Pool Cue";
                case eWeaponType.WEAPON_EPISODIC_5: return null;
                case eWeaponType.WEAPON_EPISODIC_6: return "Sawn-Off Shotgun";
                case eWeaponType.WEAPON_EPISODIC_7: return "Automatic 9mm";
                case eWeaponType.WEAPON_EPISODIC_8: return "Pipe Bomb";
                case eWeaponType.WEAPON_EPISODIC_9: return "Pistol .44";
                case eWeaponType.WEAPON_EPISODIC_10: return "Explosive Automatic Shotgun";
                case eWeaponType.WEAPON_EPISODIC_11: return "Automatic Shotgun";
                case eWeaponType.WEAPON_EPISODIC_12: return "Assault SMG";
                case eWeaponType.WEAPON_EPISODIC_13: return "Gold SMG";
                case eWeaponType.WEAPON_EPISODIC_14: return "Advanced MG";
                case eWeaponType.WEAPON_EPISODIC_15: return "Advanced Sniper";
                case eWeaponType.WEAPON_EPISODIC_16: return "Sticky Bomb";
                case eWeaponType.WEAPON_EPISODIC_17: return null;
                case eWeaponType.WEAPON_EPISODIC_18: return null;
                case eWeaponType.WEAPON_EPISODIC_19: return null;
                case eWeaponType.WEAPON_EPISODIC_20: return null;
                case eWeaponType.WEAPON_EPISODIC_21: return "Parachute";
                case eWeaponType.WEAPON_EPISODIC_22: return null;
                case eWeaponType.WEAPON_EPISODIC_23: return null;
                case eWeaponType.WEAPON_EPISODIC_24: return null;
                case eWeaponType.WEAPON_CAMERA: return "Camera";
                case eWeaponType.WEAPON_OBJECT: return "Object";
            }

            return null;
        }

        private int CreateWeaponPickupAtPosition(Vector3 pos, int weaponType, int ammo)
        {
            GET_WEAPONTYPE_MODEL(weaponType, out uint model);

            Vector3 spawnPos = NativeWorld.GetGroundPosition(pos) + new Vector3(0f, 0f, GENERATE_RANDOM_FLOAT_IN_RANGE(0.01f, 0.2f));
            CREATE_PICKUP_ROTATE(model, (uint)ePickupType.PICKUP_TYPE_WEAPON, (uint)ammo, spawnPos, new Vector3(90f, 0f, GENERATE_RANDOM_FLOAT_IN_RANGE(0f, 90f)), out int pickup);

            // DOES_PICKUP_EXIST
            // REMOVE_PICKUP

            return pickup;
        }
        #endregion

        private void Main_Uninitialize(object sender, EventArgs e)
        {
            inventoryPool.Clear();
        }
        private void Main_Initialized(object sender, EventArgs e)
        {
            basicInventory = new BasicInventory(8);
            basicInventory.OnPopupItemClick += BasicInventory_OnPopupItemClick;
            basicInventory.OnItemClick += BasicInventory_OnItemClick;
            basicInventory.Name = "TestInventory";
            basicInventory.IsVisible = true;

            inventoryPool.Add(basicInventory);
        }

        private void Main_GameLoad(object sender, EventArgs e)
        {
            if (loadedWeaponTextures.Count != 0)
            {
                // Destroy loaded textures if changing episode
                if (lastLoadedEpisode != IVGame.CurrentEpisode)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        IntPtr txt = loadedWeaponTextures[i];
                        ImGuiIV.ReleaseTexture(ref txt);
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
                        loadedWeaponTextures.Add(result, txtPtr);
                    }
                }
            }
        }

        private void Main_OnImGuiRendering(IntPtr devicePtr, ImGuiIV_DrawingContext ctx)
        {
            inventoryPool.ProcessDrawing(ctx);
        }

        private void BasicInventory_OnPopupItemClick(BasicInventory sender, BasicInventoryItem item, string popupItemName)
        {
            if (sender.Name == "TestInventory")
            {
                if (popupItemName == "Drop")
                {
                    int weaponType = Convert.ToInt32(item.Tags["WeaponType"]);

                    GET_AMMO_IN_CHAR_WEAPON(playerPedHandle, weaponType, out int ammo);

                    REMOVE_WEAPON_FROM_CHAR(playerPedHandle, weaponType);

                    GET_CHAR_COORDINATES(playerPedHandle, out Vector3 pos);

                    // Disable ability for local player to any pickups
                    DISABLE_LOCAL_PLAYER_PICKUPS(true);
                    blockPlayerAbilityToCollectPickup = true;
                    blockPosition = pos;

                    // Creates a weapon pickup at the players location
                    CreateWeaponPickupAtPosition(pos, weaponType, ammo);

                    // Removes the item out of the inventory
                    basicInventory.RemoveItem(item);
                }
            }
        }
        private void BasicInventory_OnItemClick(BasicInventory sender, BasicInventoryItem item, int itemIndex)
        {
            if (itemIndex == 0)
            {
                sender.Resize(12);
            }
            else if (itemIndex == 1)
            {
                sender.Resize(8);
            }
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            playerPedHandle = playerPed.GetHandle();
            GET_CHAR_COORDINATES(playerPedHandle, out Vector3 pos);

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
                    item.Tags.Add("WeaponType", weaponType);

                    item.PopupMenuItems.Add("Drop");

                    item.TopLeftText = GetWeaponName(type);
                    item.BottomLeftText = "0 Bullets";
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
                            item.BottomLeftText = string.Format("{0} Bullets", ammo0);
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
