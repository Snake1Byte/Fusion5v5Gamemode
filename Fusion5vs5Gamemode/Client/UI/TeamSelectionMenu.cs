using System;
using SLZ.UI;
using static Fusion5vs5Gamemode.Utilities.RadialMenu;
using static Fusion5vs5Gamemode.Shared.Commons;

namespace Fusion5vs5Gamemode.Client.UI;

public static class TeamSelectionMenu
{
    // Root Menu
    private static readonly RadialSubMenu TeamsMenu;

    // Weapon Category

    public static Action? OnAttackersSelected;
    public static Action? OnDefendersSelected;
    public static Action? OnSpectatorsSelected;


    static TeamSelectionMenu()
    {
        Log();
        TeamsMenu = new RadialSubMenu("Teams", PageItem.Directions.NORTH);
        RadialMenuItem attackers = new RadialMenuItem("Attackers", PageItem.Directions.EAST,
            AttackersSelected);
        RadialMenuItem defenders = new RadialMenuItem("Defenders", PageItem.Directions.WEST,
            DefendersSelected);
        RadialMenuItem spectators = new RadialMenuItem("Spectators", PageItem.Directions.NORTH,
            SpectatorsSelected);
        TeamsMenu.Add(attackers);
        TeamsMenu.Add(defenders);
        TeamsMenu.Add(spectators);
    }

    public static void AddTeamsMenu()
    {
        Log();
        AddRootMenu(TeamsMenu);
    }

    public static void RemoveTeamsMenu()
    {
        Log();
        RemoveRootMenu(TeamsMenu);
    }

    private static void AttackersSelected()
    {
        Log();
        OnAttackersSelected?.Invoke();
    }

    private static void DefendersSelected()
    {
        Log();
        OnDefendersSelected?.Invoke();
    }

    private static void SpectatorsSelected()
    {
        Log();
        OnSpectatorsSelected?.Invoke();
    }
}