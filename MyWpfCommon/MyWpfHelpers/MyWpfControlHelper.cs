using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace MyWpfHelpers
{
#if false
	public class CustomScrollViewer : ScrollViewer
	{
		public CustomScrollViewer()
		{
			this.Loaded += CustomScrollViewer_Loaded;
		}

		void CustomScrollViewer_Loaded(object sender, RoutedEventArgs e)
		{
			// http://msdn.microsoft.com/ja-jp/library/vstudio/aa970847.aspx

			var vertScrollBar = this.GetTemplateChild("VerticalScrollBar") as ScrollBar;
			vertScrollBar.Style = new Style(typeof(ScrollBar));
		}
	}
#endif

#if false
	public class CustomComboBox : ComboBox
	{
		static CustomComboBox()
		{
			// ComboBox の見栄えを採用する。
			DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomComboBox),
				   new FrameworkPropertyMetadata(typeof(ComboBox)));
		}

		public CustomComboBox()
		{
			this.Loaded += CustomComboBox_Loaded;
		}

		void CustomComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			// http://msdn.microsoft.com/ja-jp/library/vstudio/ms752094.aspx

			var innerTextBox = this.GetTemplateChild("PART_EditableTextBox") as TextBox;
			var innerPopup = this.GetTemplateChild("PART_Popup") as Popup;
			//var sv = this.GetTemplateChild("DropDownScrollViewer") as ScrollViewer;
			//sv.Resources.Add(SystemParameters.VerticalScrollBarWidthKey, (double)100.0);
			//sv.Style = new Style(typeof(ScrollViewer));
		}
	}
#endif


	public static class MyWpfControlHelper
	{
		public static void SearchAllVisualChildrenRecursively<T>(DependencyObject parent, List<T> outList)
			where T : DependencyObject
		{
			var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (var i = 0; i < childrenCount; ++i)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				var dep = child as T;
				if (dep != null)
				{
					outList.Add(dep);
				}
				SearchAllVisualChildrenRecursively<T>(child, outList);
				//Debug.WriteLine(child.GetType().ToString());
			}
		}

		public static void SearchAllVisualChildrenRecursively<T>(DependencyObject parent, List<T> outList, Func<T, bool> pred)
			where T : DependencyObject
		{
			var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (var i = 0; i < childrenCount; ++i)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				var dep = child as T;
				if (dep != null && pred(dep))
				{
					outList.Add(dep);
				}
				SearchAllVisualChildrenRecursively<T>(child, outList, pred);
				//Debug.WriteLine(child.GetType().ToString());
			}
		}

		public static void SearchAllLogicalChildrenRecursively<T>(DependencyObject parent, List<T> outList)
			where T : DependencyObject
		{
			foreach (var child in LogicalTreeHelper.GetChildren(parent))
			{
				var dep = child as T;
				if (dep != null)
				{
					outList.Add(dep);
					SearchAllLogicalChildrenRecursively<T>(dep, outList);
				}
				//Debug.WriteLine(child.GetType().ToString());
			}
		}

		public static void SearchAllLogicalChildrenRecursively<T>(DependencyObject parent, List<T> outList, Func<T, bool> pred)
			where T : DependencyObject
		{
			foreach (var child in LogicalTreeHelper.GetChildren(parent))
			{
				var dep = child as T;
				if (dep != null && pred(dep))
				{
					outList.Add(dep);
					SearchAllLogicalChildrenRecursively<T>(dep, outList, pred);
				}
				//Debug.WriteLine(child.GetType().ToString());
			}
		}

		public static void ModifyAllTextBoxStyle(UIElement parent, ContextMenu contextMenu, string resourceNameCaretBrush)
		{
			try
			{
				// ちなみに UIElement のコンストラクタを呼び出しただけでは Visual ツリーは作成されていないので注意。
				// 親の Loaded イベントで1回だけ呼び出すようにするなどの工夫が要る。
				var visualTextBoxList = new List<TextBox>();
				SearchAllVisualChildrenRecursively(parent, visualTextBoxList);
				foreach (var innerTextBox in visualTextBoxList)
				{
					var style = new Style(typeof(TextBox), innerTextBox.Style);
					style.Setters.Add(new Setter(TextBox.ContextMenuProperty, contextMenu));
					innerTextBox.Style = style;
					if (!String.IsNullOrEmpty(resourceNameCaretBrush))
					{
						innerTextBox.SetResourceReference(TextBox.CaretBrushProperty, resourceNameCaretBrush);
					}
				}

#if false
				var visualScrollBars = new List<ScrollBar>();
				SearchAllVisualChildrenRecursively(comboBox, visualScrollBars);
				foreach (var sb in visualScrollBars)
				{
					//sb.Template = new ControlTemplate(typeof(ScrollBar));
					//sb.Track.UpdateDefaultStyle();
				}
#endif
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
			}
		}

		public static void ModifyAllTextBoxIme(UIElement parent, bool? isImeSuspended, bool? isImeEnabled)
		{
			try
			{
				var visualTextBoxList = new List<TextBox>();
				SearchAllVisualChildrenRecursively(parent, visualTextBoxList);
				foreach (var innerTextBox in visualTextBoxList)
				{
					// IsInputMethodSuspended 添付プロパティの設定。
					if (isImeSuspended != null)
					{
						InputMethod.SetIsInputMethodSuspended(innerTextBox, isImeSuspended.Value);
					}
					// IsInputMethodEnabled 添付プロパティの設定。
					if (isImeEnabled != null)
					{
						InputMethod.SetIsInputMethodEnabled(innerTextBox, isImeEnabled.Value);
					}
				}
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
			}
		}

		public static void ModifyAllComboBoxInnerTextStyle(UIElement parent, ContextMenu contextMenu, string resourceNameCaretBrush)
		{
			try
			{
				var visualComboBoxList = new List<ComboBox>();
				SearchAllVisualChildrenRecursively(parent, visualComboBoxList);
				foreach (var innerComboBox in visualComboBoxList)
				{
					ModifyAllTextBoxStyle(innerComboBox, contextMenu, resourceNameCaretBrush);
				}
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
			}
		}

		// 通常の TextBox に関しては、XAML で Style を設定すれば対処可能だが、ComboBox 内部の TextBox に関してはそうはいかない。
		// コードビハインドを明示的に書く必要がある。

		public static void ModifyAllComboBoxContextMenuAsStandard(FrameworkElement target)
		{
			// 定義済みのリソースを使う。
			// MUI4WPF の ComboBox は、コンテキスト メニューに Modern スタイルが正しく適用されない。明示的に設定する必要がある。
			// また、Dark テーマのときにもキャレットが黒のままで見づらい。色を追従させる必要がある。
			ModifyAllComboBoxInnerTextStyle(target, target.FindResource("StandardTextBoxContextMenuKey") as ContextMenu, "WindowText");
		}

		public static bool UpdateAllBindingTargetsOfTextBoxText(UIElement parent)
		{
			return ScanAllBindingsOfTextBoxText(parent, (x) => x.UpdateTarget());
		}

		public static bool UpdateAllBindingSourcesOfTextBoxText(UIElement parent)
		{
			return ScanAllBindingsOfTextBoxText(parent, (x) => x.UpdateSource());
		}

		static bool ScanAllBindingsOfTextBoxText(UIElement parent, Action<BindingExpression> doFunc)
		{
			try
			{
				var visualTextBoxList = new List<TextBox>();
				SearchAllVisualChildrenRecursively(parent, visualTextBoxList);
				foreach (var innerTextBox in visualTextBoxList)
				{
					var bindExpress = innerTextBox.GetBindingExpression(TextBox.TextProperty);
					if (bindExpress != null)
					{
						doFunc(bindExpress);
					}
				}
				return true;
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
				return false;
			}
		}

		public static void UnselectAllListItemIfHitTestFailed(ListBox listbox, MouseButtonEventArgs e)
		{
			// WPF の ListBox, ListView は、アイテム以外の部分をクリックしても選択が解除されない。その対策を仕込む。
			// HACK: 添付ビヘイビアにしておくと再利用性がよさそう。

			// http://main.tinyjoker.net/Tech/CSharp/WPF/ListBox%A4%CE%A5%C0%A5%D6%A5%EB%A5%AF%A5%EA%A5%C3%A5%AF%A5%A4%A5%D9%A5%F3%A5%C8%A4%F2%A4%A6%A4%DE%A4%AF%BD%E8%CD%FD%A4%B9%A4%EB.html

#if false
			// 単一選択モードのみに対応。
			var item = listbox.ItemContainerGenerator.ContainerFromItem(listbox.SelectedItem) as UIElement;
			if (item != null && item.InputHitTest(e.GetPosition(item)) != null)
			{
				// 選択中のアイテム上でクリックされたとき。
			}
			else
			{
				// アイテム以外でクリックされたとき。
				listbox.UnselectAll();
			}
#else
			bool isClickedOnItem = false;
			foreach (var sel in listbox.SelectedItems)
			{
				var item = listbox.ItemContainerGenerator.ContainerFromItem(sel) as UIElement;
				if (item != null && item.InputHitTest(e.GetPosition(item)) != null)
				{
					isClickedOnItem = true;
					break;
				}
			}
			if (!isClickedOnItem)
			{
				listbox.UnselectAll();
			}
#endif
		}

		public static System.Windows.Rect GetWindowBoundsAsRect(Window window)
		{
			if (window.WindowState == System.Windows.WindowState.Maximized && !window.RestoreBounds.IsEmpty)
			{
				return window.RestoreBounds;
			}
			else if (window.WindowState == System.Windows.WindowState.Normal)
			{
				return new System.Windows.Rect(
					window.Left, window.Top,
					window.Width, window.Height);
			}
			else
			{
				return System.Windows.Rect.Empty;
			}
		}

		public static bool SetWindowBounds(Window window, System.Windows.Rect winBounds)
		{
			if (!Double.IsNaN(winBounds.Width) && winBounds.Width > 0 &&
				!Double.IsNaN(winBounds.Height) && winBounds.Height > 0)
			{
				window.Left = winBounds.X;
				window.Top = winBounds.Y;
				window.Width = winBounds.Width;
				window.Height = winBounds.Height;
				return true;
			}
			return false;
		}

		// WPF が扱うピクセルは、（高 DPI 環境にも容易に対応できるように）論理ピクセルとなっている。
		// Window のサイズはキリのいい数値とはかぎらないが、整数として切り捨てた値をやりとりする方法も用意しておく。
		// ただし Single.NaN or Double.NaN を int にキャストすると、Int32.MinValue になってしまうので要注意。

		public static System.Windows.Int32Rect GetWindowBoundsAsInt32Rect(Window window)
		{
			if (window.WindowState == System.Windows.WindowState.Maximized && !window.RestoreBounds.IsEmpty)
			{
				return new System.Windows.Int32Rect(
					(int)window.RestoreBounds.Left, (int)window.RestoreBounds.Top,
					(int)window.RestoreBounds.Width, (int)window.RestoreBounds.Height);
			}
			else if (window.WindowState == System.Windows.WindowState.Normal)
			{
				return new System.Windows.Int32Rect(
					(int)window.Left, (int)window.Top,
					(int)window.Width, (int)window.Height);
			}
			else
			{
				return System.Windows.Int32Rect.Empty;
			}
		}

		public static bool SetWindowBounds(Window window, System.Windows.Int32Rect winBounds)
		{
			if (winBounds.HasArea)
			{
				window.Left = winBounds.X;
				window.Top = winBounds.Y;
				window.Width = winBounds.Width;
				window.Height = winBounds.Height;
				return true;
			}
			return false;
		}

		public static void ShowOrActivateWindow(Window window)
		{
			if (window.Visibility != System.Windows.Visibility.Visible)
			{
				window.Show();
			}
			else if (window.WindowState == System.Windows.WindowState.Minimized)
			{
				System.Windows.SystemCommands.RestoreWindow(window);
			}
			else
			{
				window.Activate();
			}
		}

		public static void RestoreWindowIfMinimized(Window window)
		{
			// ウィンドウが最小化されていたら復元する。ただし、WindowState を Normal に戻すだけではダメ。
			// 最大化された状態で最小化されていた場合に対処できない。
			if (window.WindowState == System.Windows.WindowState.Minimized)
			{
				// わざわざ HWND を取得して P/Invoke を使う必要はない。WPF にユーティリティが用意されている。
				//var hwnd = (System.Windows.Interop.HwndSource.FromVisual(window) as System.Windows.Interop.HwndSource).Handle;
				//var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
				//MyMiscHelpers.User32DllMethodsInvoker.ShowWindow(hwnd, MyMiscHelpers.User32DllMethodsInvoker.CommandOfShowWindow.SW_RESTORE);
				System.Windows.SystemCommands.RestoreWindow(window);
			}
		}

		public static void ClampWindowSizeByPrimaryDesktopWorkArea(Window window)
		{
			var workAreaRect = System.Windows.SystemParameters.WorkArea;
			if (workAreaRect.Width < window.Width)
			{
				window.Width = workAreaRect.Width;
			}
			if (workAreaRect.Height < window.Height)
			{
				window.Height = workAreaRect.Height;
			}
		}
	}
}
