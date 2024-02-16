﻿using System;
using System.Collections;
using System.Collections.Generic;
using BoneLib;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.Data;
using MelonLoader;
using SLZ.Rig;
using SLZ.UI;
using UnityEngine;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Utilities
{
    public class RadialMenu
    {
        private static PopUpMenuView _PopUpMenu;
        private static Page _HomePage;
        internal static ActivatePopupParams? LastActivatePopupParams;

        internal static List<RadialSubMenu> RootMenus = new List<RadialSubMenu>();

        internal static Dictionary<RadialSubMenu, PageItem> _RootMenusAsPageItems =
            new Dictionary<RadialSubMenu, PageItem>();

        internal static List<PageItem> RootPageBackup = new List<PageItem>();

        internal static bool InRootLevel = true;
        internal static bool Active = false;
        public static bool IsInRootLevel => InRootLevel;
        public static bool IsActive => Active;
        public static RadialSubMenu CurrentCustomSubMenu { get; private set; }

        static RadialMenu()
        {
            PopUpMenuViewPatches.OnPopUpMenuActivate += OnPopUpMenuActivate;
            PopUpMenuViewPatches.OnPopUpMenuDeactivate += OnPopUpMenuDeacivate;
            Hooking.OnLevelInitialized += EmptyLists;
        }

        private static void EmptyLists(LevelInfo info)
        {
            RootPageBackup.Clear();
            _RootMenusAsPageItems.Clear();
        }

        private static void OnPopUpMenuActivate(Transform headTransform, Transform rootTransform,
            UIControllerInput controllerInput, BaseController controller)
        {
            Log(headTransform, rootTransform, controllerInput, controller);
            Active = true;
            LastActivatePopupParams = new ActivatePopupParams
            {
                headTransform = headTransform,
                rootTransform = rootTransform,
                controllerInput = controllerInput,
                controller = controller
            };

            if (_PopUpMenu == null || _HomePage == null)
            {
                RigManager rm = RigData.RigReferences.RigManager;
                _PopUpMenu = rm.uiRig.popUpMenu;
                _HomePage = _PopUpMenu.radialPageView.m_HomePage;
            }

            RenderRootMenus();
        }

        private static void OnPopUpMenuDeacivate()
        {
            Active = false;
        }

        private static void RenderRootMenus()
        {
            Log();
            if (RootMenus.Count == 0 || !InRootLevel)
            {
                return;
            }

            if (RootPageBackup.Count == 0)
            {
                RootPageBackup.AddRange(_HomePage.items.ToArray());
            }

            CurrentCustomSubMenu = null;

            foreach (var rootMenu in RootMenus)
            {
                if (!_RootMenusAsPageItems.TryGetValue(rootMenu, out PageItem pageItem))
                {
                    PageItem item = new PageItem(rootMenu.Name, rootMenu.Direction,
                        (Action)(() => { OnSubMenuClicked(rootMenu); }));
                    _RootMenusAsPageItems.Add(rootMenu, item);
                    _HomePage.items.Add(item);
                }
                else
                {
                    if (!_HomePage.items.Contains(pageItem))
                    {
                        _HomePage.items.Add(pageItem);
                    }
                }
            }
        }

        private static void OnSubMenuClicked(RadialSubMenu subMenuClicked)
        {
            Log(subMenuClicked);
            DeactivateRadialMenu();

            InRootLevel = false;
            CurrentCustomSubMenu = subMenuClicked;
            _HomePage.items.Clear();
            foreach (RadialMenuElement element in subMenuClicked)
            {
                if (element is RadialSubMenu subMenu)
                {
                    PageItem item = new PageItem(element.Name, element.Direction,
                        (Action)(() => { OnSubMenuClicked(subMenu); }));
                    _HomePage.items.Add(item);
                }
                else if (element is RadialMenuItem menuItem)
                {
                    PageItem item = new PageItem(menuItem.Name, menuItem.Direction,
                        menuItem.OnItemClicked);
                    _HomePage.items.Add(item);
                }
            }

            ActivateRadialMenu();
        }

        public static void ActivateRadialMenu()
        {
            Log();
            if (LastActivatePopupParams.HasValue)
            {
                ActivatePopupParams arg = LastActivatePopupParams.Value;
                _PopUpMenu.Activate(arg.headTransform, arg.rootTransform, arg.controllerInput, arg.controller);
            }
        }

        public static void DeactivateRadialMenu()
        {
            Log();
            ActivatePopupParams? arg = null;
            if (LastActivatePopupParams.HasValue)
            {
                arg = LastActivatePopupParams.Value;
            }

            _PopUpMenu.Deactivate();

            if (arg.HasValue)
            {
                LastActivatePopupParams = arg.Value;
            }
        }

        /// <summary>
        /// Places a custom menu on the specified side of the radial menu, replacing whatever usually exists on that side
        /// of the menu. It is not guaranteed that the created menu will stay here, as BONELAB will place its own menus
        /// inside of the radial menu. As a general rule, two custom menus are feasible, one on the south side and one
        /// on the north side. Menus on the west side of the radial menu will replaced by the "Utilities"
        /// menu while holding the spawn gun and any custom menus will be added back once the spawn gun is
        /// being let go of and menus on the north side will be replaced by the "Eject" button whenever holding a weapon
        /// with a magazine in it and any custom menus will be added back once the weapon is let go of or the magazine
        /// has been ejected out of the weapon. Considering this, it's wise to only place menus on those two sides.
        /// </summary>
        /// <param name="name">The display name of the new menu</param>
        /// <param name="direction">The direction of the new menu</param>
        /// <returns>A <see cref="RadialMenu"/> object to chain method calls with. This objects represents
        /// the root menu that you can then create sub menus and menu items in.</returns>
        public static RadialSubMenu AddRootMenu(string name, PageItem.Directions direction)
        {
            Log(name, direction);
            RadialSubMenu menu = new RadialSubMenu(name, direction);

            RadialSubMenu item = RootMenus.Find(e => e.Direction == menu.Direction);
            if (item != null)
            {
                RemoveRootMenu(item);
            }

            RootMenus.Add(menu);
            return menu;
        }

        public static RadialSubMenu AddRootMenu(RadialSubMenu menu)
        {
            Log(menu);

            RadialSubMenu item = RootMenus.Find(e => e.Direction == menu.Direction);
            if (item != null)
            {
                RemoveRootMenu(item);
            }

            RootMenus.Add(menu);
            return menu;
        }

        public static void RemoveRootMenu(RadialSubMenu menu)
        {
            Log(menu);

            if (menu == null)
            {
                return;
            }

            if (_RootMenusAsPageItems.TryGetValue(menu, out PageItem pageItem))
            {
                _RootMenusAsPageItems.Remove(menu);
                RootMenus.Remove(menu);
                if (InRootLevel)
                {
                    List<PageItem> toRemove = new List<PageItem>();
                    foreach (PageItem item in _HomePage.items)
                    {
                        if (item.Equals(pageItem))
                        {
                            toRemove.Add(item);
                        }
                    }

                    foreach (var item in toRemove)
                    {
                        _HomePage.items.Remove(item);
                    }
                }
            }
        }

        public abstract class RadialMenuElement
        {
            public string Name { get; set; }
            public PageItem.Directions Direction { get; set; }
        }

        public class RadialSubMenu : RadialMenuElement, IEnumerable<RadialMenuElement>
        {
            private List<RadialMenuElement> _Items = new List<RadialMenuElement>(7);
            public int Count => _Items.Count;

            internal RadialSubMenu Parent { get; private set; }
            internal RadialMenuItem BackButton;

            public RadialSubMenu(string name, PageItem.Directions direction)
            {
                Name = name;
                Direction = direction;

                BackButton = new RadialMenuItem("Back", PageItem.Directions.SOUTH,
                    () => { OnBackButtonClicked(this); });

                _Items.Add(BackButton);
            }

            public RadialSubMenu CreateSubMenu(string name, PageItem.Directions direction)
            {
                if (direction == PageItem.Directions.SOUTH)
                {
                    return null;
                }

                RadialSubMenu menu = new RadialSubMenu(name, direction);

                Add(menu);
                return menu;
            }

            public RadialMenuItem CreateItem(string name, PageItem.Directions direction, Action onItemPressed)
            {
                if (direction == PageItem.Directions.SOUTH)
                {
                    return null;
                }

                RadialMenuItem menu = new RadialMenuItem(name, direction, onItemPressed);

                Add(menu);
                return menu;
            }

            public void Clear()
            {
                foreach (var item in _Items)
                {
                    if (item is RadialSubMenu menu)
                    {
                        menu.Parent = null;
                    }
                }

                _Items.Clear();
            }

            public bool Remove(RadialMenuElement item)
            {
                if (item is RadialSubMenu menu)
                {
                    menu.Parent = null;
                }

                return _Items.Remove(item);
            }

            public void Add(RadialMenuElement item)
            {
                if (item.Direction == PageItem.Directions.SOUTH)
                {
                    return;
                }

                RadialMenuElement toDelete = _Items.Find(element => element.Direction == item.Direction);
                if (toDelete != null)
                {
                    Remove(toDelete);
                }

                if (item is RadialSubMenu menu)
                {
                    menu.Parent = this;
                }

                _Items.Add(item);
            }

            public void AddRange(IEnumerable<RadialMenuElement> collection)
            {
                foreach (var item in collection)
                {
                    Add(item);
                }
            }

            private static void OnBackButtonClicked(RadialSubMenu subMenu)
            {
                if (RootMenus.Contains(subMenu) || subMenu.Parent == null)
                {
                    DeactivateRadialMenu();

                    ReturnToRootLevel();

                    if (RootMenus.Contains(subMenu))
                    {
                        ActivateRadialMenu();
                    }
                }
                else if (subMenu.Parent != null)
                {
                    OnSubMenuClicked(subMenu.Parent);
                }
            }

            public IEnumerator<RadialMenuElement> GetEnumerator()
            {
                return _Items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class RadialMenuItem : RadialMenuElement
        {
            public Action OnItemClicked { get; set; }

            public RadialMenuItem(string name, PageItem.Directions direction, Action onItemClicked)
            {
                Name = name;
                Direction = direction;
                OnItemClicked = onItemClicked;
            }
        }

        public static void ReturnToRootLevel()
        {
            InRootLevel = true;
            _HomePage.items.Clear();

            foreach (var pageItem in RootPageBackup)
            {
                _HomePage.items.Add(pageItem);
            }
        }

        public static bool IsSubMenuInsideChildren(RadialSubMenu parent, RadialSubMenu descendant)
        {
            MelonLogger.Msg(
                $"Searching for {(descendant == null ? "a null descendant" : descendant.Name)} inside of {(parent == null ? "a null parent" : parent.Name)}.");
            try
            {
                foreach (var element in parent)
                {
                    if (element is RadialSubMenu menu)
                    {
                        if (menu.Equals(descendant))
                        {
                            return true;
                        }
                        else
                        {
                            if (IsSubMenuInsideChildren(menu, descendant))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                MelonLogger.Warning(
                    $"Aborting Sub Menu search of {(parent == null ? "a null parent" : parent.Name)} in {(descendant == null ? " a null descendant" : descendant.Name)}. Exception was thrown: {e}");
                return false;
            }
        }

        internal struct ActivatePopupParams
        {
            internal Transform headTransform;
            internal Transform rootTransform;
            internal UIControllerInput controllerInput;
            internal BaseController controller;
        }
    }
}