using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyWpfHelpers
{
	public static class MyModernWindowHack
	{
		public static void AdjustSystemCommandButtons(ModernWindow window, bool disablesBackButton = false)
		{
			//var list = new List<FrameworkElement>();
			var list = new List<Button>();
			MyWpfHelpers.MyWpfControlHelper.SearchAllVisualChildrenRecursively(window, list,
				(v) =>
				{
					//if (v.GetType().GetProperty("ToolTip") != null && v.GetType().GetProperty("Command") != null)
					if (v.ToolTip != null && v.Command != null)
					{
						// Button であれば ToolTip や Command を持つことが静的に解決できるが、もし他の型に変更されたりした場合は対処できない。
						// その場合、dynamic を使って動的に解決する手がある。
						//dynamic dyn = v; // いったん動的型に変換。
						//string strToolTip = dyn.ToolTip;

						string strToolTip = v.ToolTip.ToString();
						string strName = v.Name;
						var cmd = v.Command as System.Windows.Input.RoutedCommand;
						if (cmd == null)
						{
							return false;
						}
						string strCmdName = cmd.Name;
						Debug.WriteLine("Binded Button Command Name = " + strCmdName);
						// Modern UI for WPF 1.0.5 の実装では、Minimize と Close の直親が同じであり、
						// また Maximize と Restore の直親が同じであり、それぞれ StackPanel と Grid らしい。
						// ちなみに Minimize と Close の Button には Name が割り当てられていない。
						// StackPanel は Grid の親にもなっているので、実際は StackPanel だけタブ ナビゲーションを無効化すればよい。
						// ……と思ったが、何かの拍子でキーボード フォーカスを受け取ってしまうことがあるらしい。
						// Windows ロックから復帰した直後？
						// なので、それぞれの親に対して明示的にタブ ナビゲーションを無効化する。
						// HACK: ウィンドウのほぼ全体を覆うパネルのようなものも存在し、不必要にフォーカスを受け取る模様。
						// 「ダークテーマ＋アクセントカラー赤」などにしないと分かりづらい。こちらも無効化する。

						// ツールチップは大文字で始めるように修正する。
						// HACK: ローカライズ対応。WPF オリジナルの Window 経由もしくは Win32 API 経由でローカライズされた文字列を取得できないか？
						if (strCmdName == "MinimizeWindow")
						{
							v.ToolTip = "Minimize";
							//KeyboardNavigation.SetTabNavigation(v, KeyboardNavigationMode.None);
							KeyboardNavigation.SetTabNavigation(v.Parent, KeyboardNavigationMode.None);
							return true;
						}
						if (strCmdName == "MaximizeWindow")
						{
							v.ToolTip = "Maximize";
							//KeyboardNavigation.SetTabNavigation(v, KeyboardNavigationMode.None);
							KeyboardNavigation.SetTabNavigation(v.Parent, KeyboardNavigationMode.None);
							return true;
						}
						if (strCmdName == "RestoreWindow")
						{
							v.ToolTip = "Restore";
							//KeyboardNavigation.SetTabNavigation(v, KeyboardNavigationMode.None);
							KeyboardNavigation.SetTabNavigation(v.Parent, KeyboardNavigationMode.None);
							return true;
						}
						if (strCmdName == "CloseWindow")
						{
							v.ToolTip = "Close";
							//KeyboardNavigation.SetTabNavigation(v, KeyboardNavigationMode.None);
							KeyboardNavigation.SetTabNavigation(v.Parent, KeyboardNavigationMode.None);
							return true;
						}
						if (strCmdName == "BrowseBack")
						{
							v.ToolTip = "Back";
							if (disablesBackButton)
							{
								v.Visibility = Visibility.Hidden;
							}
							return true;
						}
					}
					return false;
				}
				);
		}
	}
}
