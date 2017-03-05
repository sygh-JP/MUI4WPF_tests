using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	/// UserControl2.xaml の相互作用ロジック
	/// </summary>
	public partial class UserControl2 : UserControl
	{
		public UserControl2()
		{
			InitializeComponent();

			this.listview1.MouseLeftButtonUp += (s, e) =>
			{
				MyWpfHelpers.MyWpfControlHelper.UnselectAllListItemIfHitTestFailed(this.listview1, e);
			};

			this.Loaded += UserControl2_Loaded;

			var list = new ObservableCollection<TestDataModel>();
			for (int i = 0; i < 30; ++i)
			{
				list.Add(new TestDataModel() { BoundingRect = new Int32Rect(10, 20, 30, 40), Area = i });
			}
			this.listview1.DataContext = list;
		}

		private void UserControl2_Loaded(object sender, RoutedEventArgs e)
		{
			//EnumSubVisuals(this.border1);
		}

		static void EnumSubVisuals(Visual parent)
		{
			var list = new List<Visual>();
			MyWpfHelpers.MyWpfControlHelper.SearchAllVisualChildrenRecursively(parent, list);
			System.Diagnostics.Debug.WriteLine("Visual Count = " + list.Count());
			foreach (var x in list)
			{
				System.Diagnostics.Debug.WriteLine("Type = " + x.GetType());
			}
		}
	}
}

namespace ModernUIApp1
{
	/// <summary>
	/// Model 兼 ViewModel。
	/// View へのバインディング後に動的にプロパティを変更する場合、INotifyPropertyChanged の実装が必要だが、
	/// 今回は簡単のため実装しない。
	/// </summary>
	class TestDataModel
	{
		public Int32Rect BoundingRect { get; set; }
		public int Area { get; set; }
	}
}
