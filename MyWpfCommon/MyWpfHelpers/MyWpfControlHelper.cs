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

		public static void ModifyAllTextBoxStyle(UIElement parent, ContextMenu contextMenu)
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

		public static void ModifyAllComboBoxInnerTextStyle(UIElement parent, ContextMenu contextMenu)
		{
			try
			{
				var visualComboBoxList = new List<ComboBox>();
				SearchAllVisualChildrenRecursively(parent, visualComboBoxList);
				foreach (var innerComboBox in visualComboBoxList)
				{
					ModifyAllTextBoxStyle(innerComboBox, contextMenu);
				}
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
			}
		}

		public static void ModifyAllComboBoxContextMenuAsStandard(FrameworkElement target)
		{
			// 組み込みのリソースを使う。
			ModifyAllComboBoxInnerTextStyle(target, target.FindResource("StandardTextBoxContextMenuKey") as ContextMenu);
		}

		public static void UpdateAllBindingTargetsOfTextBoxText(UIElement parent)
		{
			try
			{
				var visualTextBoxList = new List<TextBox>();
				MyWpfHelpers.MyWpfControlHelper.SearchAllVisualChildrenRecursively(parent, visualTextBoxList);
				foreach (var innerTextBox in visualTextBoxList)
				{
					var bindExpress = innerTextBox.GetBindingExpression(TextBox.TextProperty);
					if (bindExpress != null)
					{
						bindExpress.UpdateTarget();
					}
				}
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
			}
		}
	}
}
