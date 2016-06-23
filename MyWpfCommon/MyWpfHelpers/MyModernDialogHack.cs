using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace MyWpfHelpers
{
	public enum MessageBoxDefaultButton
	{
		Button1,
		Button2,
		Button3,
		//Button4,
	}

	public class MyModernCommonData
	{
		public Geometry ErrorIconGeometry { get; private set; }
		public Geometry WarningIconGeometry { get; private set; }
		public Geometry InformationIconGeometry { get; private set; }
		public Geometry QuestionIconGeometry { get; private set; }

		public static Brush GetAccentBrush()
		{
			// もしここで取得したオブジェクトをキャッシュして使いまわす場合、テーマ（アクセント カラー）の変更には追従しないので注意。
			// テーマの変更にも追従させる場合、メソッドを都度呼び出す必要がある。
			// なお、Application.Resources を使うと、App.xaml に記述した MergedDictionaries から検索してくれる模様。
			// EXE 側のリソースと DLL 側のリソースの両方を検索対象にすることができる。

			//var accent1Brush = (Brush)System.Windows.Application.Current.Resources["ButtonBackgroundPressed"]; // 不透明。
			var accent1Brush = (Brush)System.Windows.Application.Current.Resources["WindowBorderActive"]; // 不透明。
			//var accent2Brush = (Brush)System.Windows.Application.Current.Resources["WindowBorder"]; // 半透明。
			return accent1Brush;
		}

		public MyModernCommonData()
		{
		}

		public void Create()
		{
			// UIElement はサブスレッドでは操作できない。
			var dispatcher = Application.Current.Dispatcher;
			if (dispatcher.CheckAccess())
			{
				this.CreateImpl();
			}
			else
			{
				dispatcher.Invoke(() => this.CreateImpl());
			}
		}

		private void CreateImpl()
		{
			// いまさら OS 標準のアイコンをシェルの DLL などから拾ってくるよりは、ベクトル データ（Path）で Modern アイコンを作ってしまう。
			// 下記に XAML 形式でアイコン データが公開されているので、そちらを使う。商用利用も可能らしい。詳しくは同梱されているライセンス ファイルを参照。
			// http://modernuiicons.com/

			// ちなみに System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon() を使ってシステム アイコンのラスター画像を取得する方法がある。

			// Path を直接使いまわす方法は NG。
			// ダイアログを再作成する際に、以前使っていた Path のホスト コントロールへの参照が GC 回収されずに残っている場合、
			// UIElement ホストが複数存在することになり、ランタイム エラーになってしまう。
			// UIElement ではない Geometry の再利用にとどめる。Image と BitmapImage の関係も同様。
#if false
			var errorIconPath = new Path()
			{
				Style = (Style)System.Windows.Application.Current.Resources["StopIconPathStyle"],
			};
			this.ErrorIconGeometry = errorIconPath.Data;
			var warningIconPath = new Path()
			{
				Style = (Style)System.Windows.Application.Current.Resources["WarningIconPathStyle"],
			};
			this.WarningIconGeometry = warningIconPath.Data;
			var infoIconPath = new Path()
			{
				Style = (Style)System.Windows.Application.Current.Resources["InformationCircleIconPathStyle"],
			};
			this.InformationIconGeometry = infoIconPath.Data;
			var questionIconPath = new Path()
			{
				Style = (Style)System.Windows.Application.Current.Resources["QuestionIconPathStyle"],
			};
			this.QuestionIconGeometry = questionIconPath.Data;
			// Geometry のみを取り出して、ダミーの Path は捨てる。もっと効率的ないい方法があるかもしれない。
#else
			var resErrorIconGeometry = (StreamGeometry)System.Windows.Application.Current.Resources["ModernStopIconGeometryKey"];
			this.ErrorIconGeometry = resErrorIconGeometry;
			var resWarningIconGeometry = (StreamGeometry)System.Windows.Application.Current.Resources["ModernWarningIconGeometryKey"];
			this.WarningIconGeometry = resWarningIconGeometry;
			var resInformationIconGeometry = (StreamGeometry)System.Windows.Application.Current.Resources["ModernInformationCircleIconGeometryKey"]; // 円付きのほうがよさげ。
			this.InformationIconGeometry = resInformationIconGeometry;
			var resQuestionIconGeometry = (StreamGeometry)System.Windows.Application.Current.Resources["ModernQuestionIconGeometryKey"];
			this.QuestionIconGeometry = resQuestionIconGeometry;
#endif
			// もともと Path.Data に指定されているのは、StreamGeometry 用のミニ言語（SVG のようなもの）なので、
			// リソース ディクショナリ内で StreamGeometry を直接定義して、それを使用すればよいらしい。
			// ちなみに PathGeometry.Figures 用のミニ言語とは似ているが若干仕様が異なるので注意。
			// 詳しくは「パス マークアップ構文」を参照のこと。
			// http://msdn.microsoft.com/ja-jp/library/ms751808%28v=vs.100%29.aspx
			// http://msdn.microsoft.com/ja-jp/library/ms752293%28v=vs.100%29.aspx
		}
	}

	/// <summary>
	/// リフレクションで MUI4WPF の ModernDialog をハックしたコードを管理するヘルパークラス。
	/// </summary>
	public static class MyModernDialogHack
	{

		public static readonly MyModernCommonData CommonData = new MyModernCommonData();

		static MyModernDialogHack()
		{
			lock (CommonData)
			{
				// 静的コンストラクタは MyModernDialogHack クラスの初回利用時に実行される。
				// メインスレッド以外から実行される場合を考慮。
				// 複数のスレッドから同時アクセスがあった場合でも、静的コンストラクタは1回だけ呼び出されることが保証されるのか？
				// やはりメインスレッドで明示的かつ確実に実行できるようにしたほうがよいかも。
				CommonData.Create();
			}
		}

		// MUI4WPF (1.0.5) の元のコード。
#if false
		/// <summary>
		/// Displays a messagebox.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="title">The title.</param>
		/// <param name="button">The button.</param>
		/// <returns></returns>
		public static MessageBoxResult ShowMessage(string text, string title, MessageBoxButton button)
		{
			var dlg = new ModernDialog
			{
				Title = title,
				Content = new BBCodeBlock { BBCode = text, Margin = new Thickness(0, 0, 0, 8) },
				MinHeight = 0,
				MinWidth = 0,
				MaxHeight = 480,
				MaxWidth = 640,
			};

			dlg.Buttons = GetButtons(dlg, button);
			dlg.ShowDialog();
			return dlg.dialogResult;
		}
#endif

		/// <summary>
		/// Displays a messagebox.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="title">The title.</param>
		/// <param name="button">The button.</param>
		/// <param name="image">The image.</param>
		/// <param name="defaultButton">The default button number from left side.</param>
		/// <param name="buttonTexts">The list of button label texts for localization or customization.</param>
		/// <returns></returns>
		public static MessageBoxResult ShowMessage(string text, string title, MessageBoxButton button, MessageBoxImage image = MessageBoxImage.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1, List<string> buttonTexts = null)
		{
			// NOTE: WPF 標準の MessageBox.Show() のように、サブスレッドから直接呼び出せるようにするためには、Dispatcher を経由する必要がある。
			// HACK: メッセージ ボックスを表示している間はサブスレッドの実行が一時停止するのは同じだが、モードレスではなくモーダルになる。

			var dispatcher = Application.Current.Dispatcher;
			if (dispatcher.CheckAccess())
			{
				return ShowMessageImpl(text, title, button, image, defaultButton, buttonTexts);
			}
			else
			{
				var result = MessageBoxResult.None;
				dispatcher.Invoke(() => { result = ShowMessageImpl(text, title, button, image, defaultButton, buttonTexts); });
				return result;
			}
		}

		private static MessageBoxResult ShowMessageImpl(string text, string title, MessageBoxButton button, MessageBoxImage image = MessageBoxImage.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1, List<string> buttonTexts = null)
		{
			var dlg = CreateDefaultDialog(text, title, image);
			// WPF 標準は OK のみの場合も Esc キーや Alt+F4 で閉じることができる。その場合も結果は OK 扱いになる。None ではない。
			// OKCancel, YesNoCancel を Esc キーや Alt+F4 で閉じると結果は Cancel 扱いになる。None ではない。
			// YesNo を Alt+F4 で閉じることはできない。
			// WPF でシステム メニュー コマンドを無効化する場合、GetSystemMenu() と EnableMenuItem() を P/Invoke して SC_CLOSE を無効化する他なさげ。
			// なお、無効化ではなくメニュー コマンド自体を削除する場合、RemoveMenu() を使う。
			if (button == MessageBoxButton.OK)
			{
				dlg.KeyDown += (s, e) => { if (e.Key == Key.Escape) { dlg.Close(); } };
				// いかなる方法で閉じられた場合でも OK を返す。
				dlg.Closed += (s, e) => { SetDialogResult(dlg, MessageBoxResult.OK); };
			}
			else if (button == MessageBoxButton.OKCancel)
			{
				// システム コマンドで閉じられたときの対処。Cancel 扱いとする。
				dlg.Closed += (s, e) => { if (GetDialogResult(dlg) != MessageBoxResult.OK) { SetDialogResult(dlg, MessageBoxResult.Cancel); } };
			}
			else if (button == MessageBoxButton.YesNo)
			{
				// MUI4WPF デフォルトではなぜか Esc キーを押すと No 扱いになる。余計な機能なので Esc は無視する。
				dlg.KeyDown += (s, e) => { if (e.Key == Key.Escape) { e.Handled = true; } };
				dlg.SourceInitialized += (s, e) =>
				{
					// Close システム メニューの削除。
					IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(dlg).Handle;
					MyMiscHelpers.MyWin32InteropHelper.RemoveSystemMenuLast2Items(hwnd);
				};
				// システム メニューを無効化しても、Alt+F4 は依然として使用可能。なので Esc 同様に無視する。
				dlg.Closing += (s, e) =>
				{
					var resultYN = GetDialogResult(dlg);
					if (resultYN != MessageBoxResult.Yes && resultYN != MessageBoxResult.No)
					{
						e.Cancel = true;
					}
				};
			}
			else if (button == MessageBoxButton.YesNoCancel)
			{
				// MUI4WPF デフォルトではなぜか Esc キーを押すと No と Cancel 間のフォーカスが往復する。余計な機能なので Esc は無視する。
				dlg.KeyDown += (s, e) => { if (e.Key == Key.Escape) { dlg.Close(); } };
				// システム コマンドで閉じられたときの対処。Cancel 扱いとする。
				dlg.Closed += (s, e) =>
				{
					var resultYNC = GetDialogResult(dlg);
					if (resultYNC != MessageBoxResult.Yes && resultYNC != MessageBoxResult.No)
					{ SetDialogResult(dlg, MessageBoxResult.Cancel); }
				};
			}
			// WPF 標準のように、Ctrl+C でメッセージ内容をクリップボードにコピーできるようにする。
			dlg.KeyDown += (s, e) =>
			{
				if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.C)
				{ Clipboard.SetText(FormatMessageContentsForClipboard(dlg, text, title)); }
			};
			// TODO: Win32 の MB_DEFBUTTON1, MB_DEFBUTTON2 などに相当するパラメータを指定できるようにしたほうがよい。

			dlg.Buttons = EnumButtons(dlg, button);

			if (buttonTexts != null && buttonTexts.Count() != dlg.Buttons.Count())
			{
				throw new ArgumentException("buttonTexts parameter is invalid!!");
			}

			dlg.Loaded += (s, e) =>
			{
				AdjustTabNavigation(dlg);
				// なぜか MUI4WPF のデフォルトのボタン テキストはすべて小文字になっている。
				// 普通 [OK] はすべて大文字だし、その他も先頭は大文字にするものだが……
				// また、[Yes]/[No] へのアクセス キー（Alt+Y/Alt+N）がない。
				// どのみちローカライズするときにはなんらかのパラメータを受け取ってカスタム処理を記述する必要がある。
				// ダイアログの標準ボタン群を使う場合は OS の言語に合わせるという従来の方針でもよいが……
				if (button == MessageBoxButton.OK)
				{
					System.Diagnostics.Debug.Assert(buttonTexts == null || buttonTexts.Count() == 1);
					dlg.OkButton.Content = buttonTexts != null ? buttonTexts[0] : "OK";
					dlg.OkButton.IsDefault = true;
					dlg.OkButton.Focus();
				}
				else if (button == MessageBoxButton.OKCancel)
				{
					System.Diagnostics.Debug.Assert(buttonTexts == null || buttonTexts.Count() == 2);
					dlg.CancelButton.Content = buttonTexts != null ? buttonTexts[1] : "Cancel";
					dlg.OkButton.Content = buttonTexts != null ? buttonTexts[0] : "OK";
					dlg.OkButton.IsDefault = true;
					dlg.OkButton.Focus();
				}
				else if (button == MessageBoxButton.YesNo)
				{
					System.Diagnostics.Debug.Assert(buttonTexts == null || buttonTexts.Count() == 2);
					dlg.NoButton.Content = buttonTexts != null ? buttonTexts[1] : "_No";
					dlg.YesButton.Content = buttonTexts != null ? buttonTexts[0] : "_Yes";
					dlg.YesButton.IsDefault = true;
					dlg.YesButton.Focus();
				}
				else if (button == MessageBoxButton.YesNoCancel)
				{
					System.Diagnostics.Debug.Assert(buttonTexts == null || buttonTexts.Count() == 3);
					dlg.CancelButton.Content = buttonTexts != null ? buttonTexts[2] : "Cancel";
					dlg.NoButton.Content = buttonTexts != null ? buttonTexts[1] : "_No";
					dlg.YesButton.Content = buttonTexts != null ? buttonTexts[0] : "_Yes";
					dlg.YesButton.IsDefault = true;
					dlg.YesButton.Focus();
				}
			};
			if (Application.Current.MainWindow == null || Application.Current.MainWindow.Visibility != Visibility.Visible)
			{
				dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				dlg.ShowInTaskbar = true;
			}
			dlg.ShowDialog();
			return GetDialogResult(dlg);
		}

		public static MessageBoxResult ShowMessage(string text, string title, MessageBoxImage image = MessageBoxImage.None, string buttonText = null)
		{
			var dispatcher = Application.Current.Dispatcher;
			if (dispatcher.CheckAccess())
			{
				return ShowMessageImpl(text, title, image, buttonText);
			}
			else
			{
				var result = MessageBoxResult.None;
				dispatcher.Invoke(() => { result = ShowMessageImpl(text, title, image, buttonText); });
				return result;
			}
		}

		private static MessageBoxResult ShowMessageImpl(string text, string title, MessageBoxImage image = MessageBoxImage.None, string buttonText = null)
		{
			var dlg = CreateDefaultDialog(text, title, image);
			// いかなる方法で閉じられた場合でも None を返す。
			dlg.KeyDown += (s, e) => { if (e.Key == Key.Escape) { dlg.Close(); } };
			dlg.KeyDown += (s, e) =>
			{
				if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.C)
				{ Clipboard.SetText(FormatMessageContentsForClipboard(dlg, text, title)); }
			};
			// ModernDialog のコンストラクタで、Buttons には Close ボタンが割り当てられている。
			dlg.Loaded += (s, e) =>
			{
				AdjustTabNavigation(dlg);
				dlg.CloseButton.Content = buttonText ?? "_Close";
				dlg.CloseButton.IsDefault = true;
				dlg.CloseButton.Focus();
			};
			if (Application.Current.MainWindow == null || Application.Current.MainWindow.Visibility != Visibility.Visible)
			{
				dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				dlg.ShowInTaskbar = true;
			}
			dlg.ShowDialog();
			//return GetDialogResult(dlg); // 分かりきっているので実行しない。
			return MessageBoxResult.None;
		}

		private static string GetButtonsStringForClipboard(ModernDialog dlg)
		{
			string output = "";
			foreach (var btn in dlg.Buttons)
			{
				output += "[" + btn.Content.ToString() + "] ";
			}
			return output;
		}

		private static string FormatMessageContentsForClipboard(ModernDialog dlg, string text, string title)
		{
			const string StrSeparator = "---------------------------";
			const string StrCrLf = "\r\n";
			return StrSeparator + StrCrLf
				+ title + StrCrLf
				+ StrSeparator + StrCrLf
				+ text + StrCrLf
				+ StrSeparator + StrCrLf
				+ GetButtonsStringForClipboard(dlg) + StrCrLf
				+ StrSeparator + StrCrLf
				;
		}

		private static void AdjustTabNavigation(ModernDialog dlg)
		{
			// MUI4WPF デフォルトではなぜかボタン以外にキーボード フォーカスを受け取る余計な要素がある。その調整。
			var btnContainerList = new List<ItemsControl>();
			MyWpfControlHelper.SearchAllVisualChildrenRecursively(dlg, btnContainerList, (c) => { return c.ItemsSource == dlg.Buttons; });
			//System.Diagnostics.Debug.WriteLine(btnContainerList.Count());
			if (btnContainerList.Count() == 1)
			{
				KeyboardNavigation.SetTabNavigation(btnContainerList[0], KeyboardNavigationMode.Cycle);
				KeyboardNavigation.SetIsTabStop(btnContainerList[0], false);
			}
		}

		private static ModernDialog CreateDefaultDialog(string text, string title, MessageBoxImage image)
		{
			if (title == null)
			{
				title = MyWpfHelpers.MyWpfMiscHelper.GetAppName();
			}

			// WPF 標準メッセージボックス（Win32 API）の場合、一定の幅までは可変だが、それ以上は固定となる。
			// BBCodeBlock に Width を指定せず、MinWidth や MaxWidth を指定するだけでは、折り返しがうまく効かない模様。

#if false
			var codeBlock = new BBCodeBlock { BBCode = text, Margin = new Thickness(8, 8, 0, 8), VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap, Width = 340 };
#else
			var codeBlock = new TextBlock { Text = text, Margin = new Thickness(8, 8, 0, 8), VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap, Width = 340 };
#endif

			// StackPanel だと、BBCodeBlock（の内部の TextBlock）の自動折り返し（TextWrapping プロパティ）が効かない。
			var textAreaPanel = new DockPanel() { UseLayoutRounding = true };

			// WPF 標準のように、MessageBoxImage を指定できるようにする。
			const double size = 76;
			var iconPath = new Path()
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Width = size,
				Height = size,
			};
			switch (image)
			{
				// Dark テーマの背景にも、Light テーマの背景にも映える色を使う。
				case MessageBoxImage.Error:
					iconPath.Data = CommonData.ErrorIconGeometry;
					iconPath.Fill = Brushes.Crimson;
					textAreaPanel.Children.Add(iconPath);
					break;
				case MessageBoxImage.Warning:
					iconPath.Data = CommonData.WarningIconGeometry;
					iconPath.Fill = Brushes.Orange;
					textAreaPanel.Children.Add(iconPath);
					break;
				case MessageBoxImage.Information:
					iconPath.Data = CommonData.InformationIconGeometry;
					iconPath.Fill = Brushes.DeepSkyBlue;
					textAreaPanel.Children.Add(iconPath);
					break;
				case MessageBoxImage.Question:
					iconPath.Data = CommonData.QuestionIconGeometry;
					iconPath.Fill = Brushes.DodgerBlue;
					textAreaPanel.Children.Add(iconPath);
					break;
				default:
					break;
			}
			textAreaPanel.Children.Add(codeBlock);

			// MUI4WPF のオリジナル実装では、高さ制限がかなりきついが、
			// WPF 標準のメッセージボックス（というか Win32 メッセージボックス）は一応ワークエリアの高さまで伸ばせるはずなので、修正しておく。
			return new ModernDialog
			{
				Title = title,
				Content = textAreaPanel,
				Width = Double.NaN,
				Height = Double.NaN,
				SizeToContent = System.Windows.SizeToContent.WidthAndHeight,
				MinHeight = 120,
				MaxHeight = System.Windows.SystemParameters.WorkArea.Height,
			};
		}

		// リフレクションを使ってカプセル化を破壊する。本来は禁じ手。内部シンボル名はソースコードを直接解析した結果得たもの。
		// MUI4WPF の非公開内部シンボル名が変更されてしまうとコードが不正になるという欠点がある。
		// 修正コードが MUI4WPF 本体にコミットされれば不要になるはず……プルリクエストでも出すか？
		// 
		// MFC, Windows Forms, WPF 標準のメッセージ ボックス（ただの MessageBox() Win32 API ベース）のように、
		// ボタン テキストのローカライズやカスタマイズを実現するためには、
		// どのみちインスタンス化する際になんらかのフック処理を挟めるような仕組みを別途用意する必要がある。
		// MessageBox() ベースではメッセージ フックを使った泥臭いやり方しかないが、MUI4WPF ではデリゲートを作れば簡単に実現できるはず。
		// いっそ追加パラメータを用意する方法でもいい。
		#region Methods with Reflection

		private static IEnumerable<Button> EnumButtons(ModernDialog dlg, MessageBoxButton button)
		{
			var getButtonsMethod = typeof(ModernDialog).GetMethod("GetButtons",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			System.Diagnostics.Debug.Assert(getButtonsMethod != null);
			return (IEnumerable<Button>)getButtonsMethod.Invoke(null, new object[] { dlg, button });
		}

		//const string InternalFieldNameOfDialogResult = "dialogResult"; // Ver.1.0.5 まで。
		const string InternalFieldNameOfDialogResult = "messageBoxResult"; // Ver.1.0.6 から。

		private static MessageBoxResult GetDialogResult(ModernDialog dlg)
		{
			var dialogResultField = typeof(ModernDialog).GetField(InternalFieldNameOfDialogResult,
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			System.Diagnostics.Debug.Assert(dialogResultField != null);
			return (MessageBoxResult)dialogResultField.GetValue(dlg);
		}

		private static void SetDialogResult(ModernDialog dlg, MessageBoxResult value)
		{
			var dialogResultField = typeof(ModernDialog).GetField(InternalFieldNameOfDialogResult,
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			System.Diagnostics.Debug.Assert(dialogResultField != null);
			dialogResultField.SetValue(dlg, value);
		}

		#endregion
	}
}
