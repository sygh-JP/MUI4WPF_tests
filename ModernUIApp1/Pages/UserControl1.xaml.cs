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

			// HACK: Look&Feel を統一するため、メッセージボックスを表示したときに、アイコン種別に応じて警告音なども再生したほうがよい。
#if false
			MyWpfHelpers.MyModernDialogHack.ShowMessage("FirstFloor.ModernUI.Windows.IContent.OnNavigatedTo", null);
#endif
		}

		public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
		{
			DebugWriteLineMemberName();
		}
	}
}
