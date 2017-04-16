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

// MUI4WPF の ModernDialog は相当貧弱なので（アイコンすら指定できない、タブ キーの動作がおかしい、OK ダイアログで Esc が効かない、etc.）、
// ハックしてカスタマイズするか、別の MyModernDialog を作るか、もしくは Windows API Code Pack などを活用して
// Vista 以降に実装されているタスク ダイアログを活用するなりしたほうがいい。
// WPF 標準の MessageBox は ModernDialog よりはマシだが、ただの Win32 P/Invoke なのでデザインがいまいち。訴求力がない。
// 
// MUI4WPF のアプリケーション プロジェクト テンプレート (VSIX) が公開されている。
// https://marketplace.visualstudio.com/items?itemName=KoenZwikstra.ModernUIforWPFTemplates
// しかし、このプロジェクト テンプレートを使って作成した場合、*.csproj に以下のタグが欠如しているために、
// プロジェクトに Visual Studio のウィザードで WPF のウィンドウやリソース ディクショナリなどを追加することができない。
// <ProjectTypeGuids>{60dc8134-eba5-43b8-bcc9-bb4bc16c2548};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>
// *.csproj の手動修正が必要。

namespace ModernUIApp1
{
	using AppearanceManager = FirstFloor.ModernUI.Presentation.AppearanceManager;
	using ThisAppSettings = global::ModernUIApp1.Properties.Settings;


	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : ModernWindow
	{
		//MyMiscHelpers.MyCustomLayeredWinProcManager customWinProc = new MyMiscHelpers.MyCustomLayeredWinProcManager();

		public MainWindow()
		{
#if true
			// Modern UI for WPF を使うと、TextBox や編集可能な ComboBox など、
			// 特定のコントロールがデフォルトで ClearType ではなく Grayscale アンチエイリアスになってしまう模様。
			// IDE の XAML プレビューではきちんと ClearType になっているが……
			// 強制的に ClearType を有効にしようとしても、TextBlock には若干効果があるようだが、TextBox と編集可能な ComboBox にはまったく効いていない？
			// WPF や Direct2D/DirectWrite では、完全不透明でない背景に ClearType テキストを描画しようとすると、
			// 自動的に Grayscale アンチエイリアスに変更されるらしいが……
			// どうも MUI4WPF のページ要素がアニメーションすることが関連している可能性がある。
			// 下記のように設定すると、ジャギーは低減されるがにじみがひどくなる。
			// TextOptions.TextFormattingMode="Ideal"
			// TextOptions.TextRenderingMode="ClearType"
			// TextOptions.TextHintingMode="Animated"

			MyWpfHelpers.MyWpfClearTypeHelper.EnableClearType(typeof(System.Windows.Controls.ComboBox));
			MyWpfHelpers.MyWpfClearTypeHelper.EnableClearType(typeof(System.Windows.Controls.TextBox));
			MyWpfHelpers.MyWpfClearTypeHelper.EnableClearType(typeof(System.Windows.Controls.TextBlock));
#endif

			InitializeComponent();

			// HACK: 起動直後に1回だけ設定するので、プロパティの変更に動的に追従するわけではない。
			//this.customWinProc.MinWindowWidth = Double.IsNaN(this.MinWidth) ? 0 : (int)this.MinWidth;
			//this.customWinProc.MinWindowHeight = Double.IsNaN(this.MinHeight) ? 0 : (int)this.MinHeight;

			// HACK: 起動直後に1回だけ設定するので、システム設定やユーザー設定の変更に追従するわけではない。
			MyWpfHelpers.MyWpfWindowHelper.ClampWindowSizeByDesktopWorkArea(this);

			// 前回終了時にユーザー設定ファイルに保存しておいたテーマ情報を初期値として与える場合はこのタイミングがよさげ。
			LoadThemeColorSettings();
		}

		IntPtr GetWindowHandle()
		{
			return new System.Windows.Interop.WindowInteropHelper(this).Handle;
		}

		static void LoadThemeColorSettings()
		{
			try
			{
				//ThisAppSettings.Default.Reload();

				//var colorMauve = Color.FromRgb(0x76, 0x60, 0x8a);
				var colorCyan = Color.FromRgb(0x1b, 0xa1, 0xe2);
				if (ThisAppSettings.Default.ModernAccentColor == Color.FromArgb(0, 0, 0, 0))
				{
					AppearanceManager.Current.AccentColor = colorCyan;
				}
				else
				{
					AppearanceManager.Current.AccentColor = ThisAppSettings.Default.ModernAccentColor;
				}
				if (ThisAppSettings.Default.ModernThemeSource == AppearanceManager.DarkThemeSource)
				{
					AppearanceManager.Current.ThemeSource = AppearanceManager.DarkThemeSource;
				}
				else
				{
					AppearanceManager.Current.ThemeSource = AppearanceManager.LightThemeSource;
				}
			}
			catch
			{
			}
		}

		static void SaveThemeColorSettings()
		{
			try
			{
				ThisAppSettings.Default.ModernAccentColor = AppearanceManager.Current.AccentColor;
				ThisAppSettings.Default.ModernThemeSource = AppearanceManager.Current.ThemeSource;
				ThisAppSettings.Default.Save();
			}
			catch
			{
			}
		}

		private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
		{
			MyWpfHelpers.MyModernWindowHack.AdjustSystemCommandButtons(this);

#if false
			var hWnd = this.GetWindowHandle();
			var strClose = MyMiscHelpers.MyWin32InteropHelper.GetSystemMenuItemLabelStringClose(hWnd);
			System.Diagnostics.Debug.WriteLine(String.Format("Close = \"{0}\"", strClose));
#endif

			// TODO: Add your code.
		}

		private void ModernWindow_Closed(object sender, EventArgs e)
		{
			// TODO: Add your code.

			SaveThemeColorSettings();

			//this.customWinProc.DetachCustomWndProc();
		}

		private void ModernWindow_SourceInitialized(object sender, EventArgs e)
		{
			// Modern UI for WPF 1.0.5 の時点では、ウィンドウのリサイズ時に画面が異常にちらつく不具合がある。
			// ModernWindow に、
			// AllowsTransparency="True"
			// Background="Transparent"
			// WindowStyle="None"
			// を設定してフルスペック レイヤード ウィンドウ化してやり、画面描画を完全にカスタマイズするとちらつきはなくなるが、
			// 代わりに最大化時にタスク バー領域まで覆ってしまう現象が発生するようになる。
			// 対処するためには、WM_GETMINMAXINFO メッセージをカスタム処理する必要がある。
			// なお、リサイズ時（拡大時）のウィンドウ外枠（おそらく Border）の右下ラインの追従が遅く、残像（ポリゴン）が見えるが、
			// これはおそらく GDI（Win32 ウィンドウ）相互運用の限界かと思われる。
			// ちなみに Visual Studio 2012 もリサイズ時にかなりちらついている。
			// MUI for WPF 1.0.5 とほとんど同じ実装になっているものと推測される。
			// また、WindowStyle に None を指定すると、最大化・最小化のアニメーションがなくなるが、
			// これは WS_CAPTION を追加してやるだけで復活可能。
			// ただし WS_CAPTION では閉じる際のアニメーションは復活できないが、どうしてもアニメーションが欲しければ WPF の機能で自前実装すればよい。
			// NOTE: MUI4WPF 1.0.6 ではこれらの問題が解消されている模様。したがって対処は不要。閉じる際のアニメーションも実行される。

			//this.AddCustomHook(); // 「リサイズ中に描画（レイアウト変更）しない」という方法では、このちらつき現象に対処するのは不可能。

			IntPtr hwnd = this.GetWindowHandle();
			System.Diagnostics.Debug.Assert(hwnd != IntPtr.Zero);
			//this.customWinProc.AttachCustomWndProc(hwnd);
			//MyMiscHelpers.MyWin32InteropHelper.AppendWindowStyleCaption(hwnd);
		}

#if false
		const int WM_SIZE = 0x0005;
		const int WM_PAINT = 0x000F;
		const int WM_ENTERSIZEMOVE = 0x0231;
		const int WM_EXITSIZEMOVE = 0x0232;
		const int WM_ERASEBKGND = 0x0014;

		// リサイズ中かどうか。
		bool isWindowResizing = false;

		// リサイズ終了後に投げる WM_SIZE メッセージのパラメータ。
		IntPtr lastResizeLParam;
		IntPtr lastResizeWParam;

		[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
		public static extern bool PostMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);


		void AddCustomHook()
		{
			var hsrc = System.Windows.Interop.HwndSource.FromVisual(this) as System.Windows.Interop.HwndSource;
			hsrc.AddHook(WndProc);
		}

		IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case WM_ERASEBKGND:
				case WM_PAINT:
					handled = true;
					break;
				case WM_ENTERSIZEMOVE:
					isWindowResizing = true;
					break;
				case WM_EXITSIZEMOVE:
					{
						isWindowResizing = false;
						PostMessage(hwnd, WM_SIZE, lastResizeWParam, lastResizeLParam);
						this.Opacity = 1;
						var fe = this.Content as FrameworkElement;
						fe.Visibility = System.Windows.Visibility.Visible;
					}
					break;
				case WM_SIZE:
					// サイズ変更中ならば、ハンドルして WPF に WM_SIZE を渡さない。
					if (isWindowResizing)
					{
						handled = true;

						// サイズ変更終了後に再度ポストするように、パラメータを保持。
						lastResizeLParam = lParam;
						lastResizeWParam = wParam;
						this.Opacity = 0.5;
						var fe = this.Content as FrameworkElement;
						fe.Visibility = System.Windows.Visibility.Collapsed;
					}
					break;
			}
			return IntPtr.Zero;
		}
#endif
	}
}
