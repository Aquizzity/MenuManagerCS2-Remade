using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuManager;

internal static class Control
{
    public static List<PlayerInfo> menus = new List<PlayerInfo>();
    private static MenuManagerCore? hPlugin;

    // Re-added missing method: CloseMenu
    public static void CloseMenu(CCSPlayerController player)
    {
        CounterStrikeSharp.API.Modules.Menu.MenuManager.CloseActiveMenu(player);
        menus.RemoveAll(m => m.GetPlayer() == player);
    }

    // Re-added missing method: HasOpenedMenu (Corrected Parameter Count)
    public static bool HasOpenedMenu(CCSPlayerController player)
    {
        return menus.Any(menu => menu.GetPlayer() == player && !menu.Closed());
    }

    // Re-added missing method: PlaySound
    public static void PlaySound(CCSPlayerController player, string sound)
    {
        if (!string.IsNullOrEmpty(sound))
        {
            player.ExecuteClientCommand($"play {sound}");
        }
    }

    // Re-added missing method: Clear
    public static void Clear()
    {
        menus.Clear();
    }

    // Re-added missing method: AddMenu
    public static void AddMenu(CCSPlayerController player, ButtonMenu inst)
    {
        menus.Add(new PlayerInfo(player, inst, 0.0f, 0, 0, inst.Title));
    }

    // Re-added missing method: AddMenuAll
    public static void AddMenuAll(ButtonMenu inst)
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (player != null && player.IsValid && !player.IsBot && player.Connected == PlayerConnectedState.PlayerConnected)
                AddMenu(player, inst);
        }
    }

    public static void OnPluginTick()
    {
        if (menus.Count > 0)
        {
            for (int i = 0; i < menus.Count; i++)
            {
                var menu = menus[i];
                if (menu == null)
                {
                    menus.RemoveAt(i);
                    i--;
                    continue;
                }

                var player = menu.GetPlayer();
                if (!Misc.IsValidPlayer(player))
                {
                    menus.RemoveAt(i);
                    i--;
                    continue;
                }

                PlayerButtons buttons = player.Buttons;

                if (!hPlugin.Config.MoveWhileOpenMenu)
                    player.PlayerPawn.Value.VelocityModifier = 0.0f;

                if (!menu.IsEqualButtons(buttons.ToString()))
                {
                    if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.UpButton))
                        menu.MoveUp();
                    else if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.DownButton))
                        menu.MoveDown();
                    else if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.LeftButton))
                        menu.MoveUp(Control.GetPlugin().Config.MenuLinesCount);
                    else if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.RightButton))
                        menu.MoveDown(Control.GetPlugin().Config.MenuLinesCount);
                    else if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.SelectButton))
                        menu.OnSelect();
                    else if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.BackButton) && menu.menu.BackAction != null)
                        menu.menu.BackAction(player);

                    if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.ExitButton) || menu.Closed())
                    {
                        menu.Close(true);
                        if (!hPlugin.Config.MoveWhileOpenMenu)
                            player.PlayerPawn.Value.VelocityModifier = menu.GetMod();
                        menus.RemoveAt(i);
                        i--;
                        continue;
                    }
                }

                string menuText = menu.GetText();
                menuText = $"<font color='{Control.GetPlugin().Config.ButtonsConfig.DefaultTextColor}'>{menuText}</font>";
                player.PrintToCenterHtml(menuText, 1);
            }
        }
    }

    internal static void Init(MenuManagerCore _hPlugin)
    {
        hPlugin = _hPlugin;
    }

    internal static MenuManagerCore GetPlugin()
    {
        return hPlugin ?? throw new InvalidOperationException("hPlugin is not initialized");
    }
}
