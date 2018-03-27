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

namespace ModernUIApp1.Pages
{
	/// <summary>
	/// UserControl1.xaml の相互作用ロジック
	/// </summary>
	public partial class UserControl1 : UserControl, FirstFloor.ModernUI.Windows.IContent
	{
		public UserControl1()
		{
			InitializeComponent();

			this.Loaded += UserControl1_Loaded;
			this.Unloaded += UserControl1_Unloaded;

			this.buttonOpenImage.Click += async (s, e) =>
			{
				await this.OpenImageByFileDialogAsync();
			};
		}

		[System.Diagnostics.Conditional("DEBUG")]
		static void DebugWriteLineMemberName([System.Runtime.CompilerServices.CallerMemberName] string member = "")
		{
			System.Diagnostics.Debug.WriteLine(member);
		}

		private void UserControl1_Loaded(object sender, RoutedEventArgs e)
		{
			DebugWriteLineMemberName();
		}

		private void UserControl1_Unloaded(object sender, RoutedEventArgs e)
		{
			DebugWriteLineMemberName();
		}

		public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
		{
			DebugWriteLineMemberName();
		}

		public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
		{
			DebugWriteLineMemberName();
		}

		public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
		{
			DebugWriteLineMemberName();
			// MUI4WPF の ModernWindow.MenuLinkGroups の場合、FirstFloor.ModernUI.Presentation.Link に指定した
			// UserControl の Loaded イベントはページが表示されたときだけでなく非表示になったときも発生してしまう。
			// ナビゲーション系のイベントを捕まえる場合、FirstFloor.ModernUI.Windows.IContent を実装すればよいらしい。
			// https://github.com/firstfloorsoftware/mui/wiki/Handle-navigation-events
			// ちなみに、ここでメッセージボックスを表示している間、メインウィンドウ上には Indeterminate のプログレスバーが
			// 勝手に表示されるようになっている。

			// HACK: Look & Feel を統一するため、メッセージボックスを表示したときに、アイコン種別に応じて警告音なども再生したほうがよい。
#if false
			MyWpfHelpers.MyModernDialogHack.ShowMessage("FirstFloor.ModernUI.Windows.IContent.OnNavigatedTo", null);
#endif
		}

		public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
		{
			DebugWriteLineMemberName();
		}

		private async Task OpenImageByFileDialogAsync()
		{
			var fileDlg = new Microsoft.Win32.OpenFileDialog();
			fileDlg.Filter = "All Image Files|*.png;*.gif;*.bmp;*.dib;*.jpg;*.jpeg;*.tif;*.tiff|"
				+ "PNG Files|*.png|"
				+ "GIF Files|*.gif|"
				+ "BMP Files|*.bmp;*.dib|"
				+ "JPEG Files|*.jpg;*.jpeg|"
				+ "TIFF Files|*.tif;*.tiff|"
				+ "All Files|*.*";

			if (fileDlg.ShowDialog() == true)
			{
				try
				{
					this.IsEnabled = false;

					var filePath = fileDlg.FileName;

					// サブスレッドで画像ファイル読み込みを行なう。
					var srcImage = await Task.Run(() =>
					{
						try
						{
							// フリーズしていないビットマップは別スレッドで操作できない。
							return MyWpfHelpers.MyWpfImageHelper.CreateBitmapFromSharableFileStream(filePath, true, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
						}
						finally
						{
							// メモリ リーク対策。async/await を使う場合はなくてもよさそう？
							MyMiscHelpers.MyThreadHelper.InvokeShutdownCurrentThreadDispatcher();
						}
					});

					var dpiScaleFactor = MyMiscHelpers.MyVisualHelper.GetDpiScaleFactor(this);
					// 高 DPI 環境でもラスター画像だけは dot-by-dot で表示するため、画像サイズ（デバイス ピクセル）を論理ピクセルに変換する。
					// BitmapSource.Width, BitmapSource.Height は使わない。
					this.image1.Width = srcImage.PixelWidth / dpiScaleFactor.X;
					this.image1.Height = srcImage.PixelHeight / dpiScaleFactor.Y;
					this.image1.Source = srcImage;
				}
				catch (Exception ex)
				{
					MyWpfHelpers.MyModernDialogHack.ShowMessage(ex.Message, null, MessageBoxImage.Error);
				}
				finally
				{
					GC.Collect();
					Console.WriteLine("Total GC Memory = {0} KB", GC.GetTotalMemory(true) / 1024);

					this.IsEnabled = true;
				}
			}
		}
	}
}
