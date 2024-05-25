using System;
using BoneLib;
using Fusion5vs5Gamemode.Utilities;
using MelonLoader;
using SLZ.UI;
using SafeActions = BoneLib.SafeActions;

namespace Fusion5vs5Gamemode.Client.UI;

public static class BuyMenu
{
    // Root Menu
    private static RadialMenu.RadialSubMenu _BuyMenu;

    // Weapon Category
    private static RadialMenu.RadialSubMenu _Pistols;
    private static RadialMenu.RadialSubMenu _SMGs;
    private static RadialMenu.RadialSubMenu _Shotguns;

    private static RadialMenu.RadialSubMenu _Rifles;
    // private static RadialSubMenu _SniperRifles;
    // private static RadialSubMenu _Utilities;

    // Pistols
    private static RadialMenu.RadialMenuItem EDER22;
    private static RadialMenu.RadialMenuItem _1911;
    private static RadialMenu.RadialMenuItem P350;

    // SMGs
    private static RadialMenu.RadialMenuItem UMP;
    private static RadialMenu.RadialMenuItem MP5;
    private static RadialMenu.RadialMenuItem Vector;
    private static RadialMenu.RadialMenuItem UZI;

    // Shotguns
    private static RadialMenu.RadialMenuItem FAB;
    private static RadialMenu.RadialMenuItem M4;
    private static RadialMenu.RadialMenuItem M590A1;

    // Rifles
    private static RadialMenu.RadialMenuItem MK18;
    private static RadialMenu.RadialMenuItem AKM;
    private static RadialMenu.RadialMenuItem PDRC;
    private static RadialMenu.RadialMenuItem M16;

    // Sniper Rifles

    // Utilities

    public static Action<string>? OnBuyMenuItemClicked;

    static BuyMenu()
    {
        _BuyMenu = new RadialMenu.RadialSubMenu("Buy", PageItem.Directions.WEST);

        _Pistols = _BuyMenu.CreateSubMenu("Pistols", PageItem.Directions.SOUTHWEST)!;
        EDER22 = new RadialMenu.RadialMenuItem("EDER22", PageItem.Directions.WEST,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.Eder22));
        _1911 = new RadialMenu.RadialMenuItem("1911", PageItem.Directions.EAST,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.M1911));
        P350 = new RadialMenu.RadialMenuItem("P350", PageItem.Directions.NORTH,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.P350));
        _Pistols.Add(EDER22);
        _Pistols.Add(_1911);
        _Pistols.Add(P350);
        _BuyMenu.Add(_Pistols);

        _SMGs = _BuyMenu.CreateSubMenu("SMGs", PageItem.Directions.WEST)!;
        UMP = new RadialMenu.RadialMenuItem("UMP", PageItem.Directions.WEST,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.UMP));
        MP5 = new RadialMenu.RadialMenuItem("MP5", PageItem.Directions.NORTH,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.MP5));
        Vector = new RadialMenu.RadialMenuItem("Vector", PageItem.Directions.EAST,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.Vector));
        UZI = new RadialMenu.RadialMenuItem("UZI", PageItem.Directions.SOUTHWEST,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.UZI));
        _SMGs.Add(UMP);
        _SMGs.Add(MP5);
        _SMGs.Add(Vector);
        _SMGs.Add(UZI);
        _BuyMenu.Add(_SMGs);

        _Shotguns = _BuyMenu.CreateSubMenu("Shotguns", PageItem.Directions.NORTHEAST)!;
        FAB = new RadialMenu.RadialMenuItem("FAB", PageItem.Directions.WEST,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.FAB));
        M4 = new RadialMenu.RadialMenuItem("M4", PageItem.Directions.EAST,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.M4));
        M590A1 = new RadialMenu.RadialMenuItem("590A1", PageItem.Directions.NORTH,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.M590A1));
        _Shotguns.Add(FAB);
        _Shotguns.Add(M4);
        _Shotguns.Add(M590A1);
        _BuyMenu.Add(_Shotguns);

        _Rifles = _BuyMenu.CreateSubMenu("Rifles", PageItem.Directions.NORTH)!;
        MK18 = new RadialMenu.RadialMenuItem("MK18", PageItem.Directions.NORTH,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.MK18Naked));
        AKM = new RadialMenu.RadialMenuItem("AKM", PageItem.Directions.EAST,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.AKM));
        PDRC = new RadialMenu.RadialMenuItem("PDRC", PageItem.Directions.WEST,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.PDRC));
        M16 = new RadialMenu.RadialMenuItem("M16", PageItem.Directions.SOUTHWEST,
            () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.M16IronSights));
        _Rifles.Add(MK18);
        _Rifles.Add(AKM);
        _Rifles.Add(PDRC);
        _Rifles.Add(M16);
        _BuyMenu.Add(_Rifles);
    }

    public static void AddBuyMenu()
    {
        RadialMenu.AddRootMenu(_BuyMenu);
        if (RadialMenu.IsInRootLevel && RadialMenu.IsActive)
        {
            RadialMenu.DeactivateRadialMenu();
            RadialMenu.ActivateRadialMenu();
        }
    }

    public static void RemoveBuyMenu()
    {
        RadialMenu.RemoveRootMenu(_BuyMenu);
        if (RadialMenu.IsInRootLevel && RadialMenu.IsActive)
        {
            RadialMenu.DeactivateRadialMenu();
            RadialMenu.ActivateRadialMenu();
        }
        else
        {
            RadialMenu.RadialSubMenu? currentSubMenu = RadialMenu.CurrentCustomSubMenu;
            if (currentSubMenu == null)
                return;

            MelonLogger.Msg($"Current Custom SubMenu: {currentSubMenu.Name}");

            if (currentSubMenu == _BuyMenu || RadialMenu.IsSubMenuInsideChildren(_BuyMenu, currentSubMenu))
            {
                MelonLogger.Msg("We are in a sub menu of BuyMenu.");
                if (RadialMenu.IsActive)
                {
                    RadialMenu.DeactivateRadialMenu();
                    RadialMenu.ReturnToRootLevel();
                    RadialMenu.ActivateRadialMenu();
                }
                else
                {
                    RadialMenu.ReturnToRootLevel();
                }
            }
        }
    }

    internal static void Internal_OnBuyMenuItemClicked(string barcode)
    {
        SafeActions.InvokeActionSafe(OnBuyMenuItemClicked, barcode);
    }
}