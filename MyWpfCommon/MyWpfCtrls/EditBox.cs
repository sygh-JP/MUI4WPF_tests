using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

// http://msdn.microsoft.com/ja-jp/library/vstudio/ms745183(v=vs.90).aspx
// http://msdn.microsoft.com/ja-jp/library/ms745183(v=vs.90).aspx
// http://pro.art55.jp/?eid=908012
// http://pro.art55.jp/?eid=1029203

// Windows エクスプローラーや、従来のネイティブ Win32 のリスト コントロールなどのように、
// リスト中に編集可能なテキストを仕込むのに良い方法はないか探したが、
// 常時編集可能な TextBox を ListView に埋め込む方法のほうが単純でよさげ（その代わりコミット／キャンセルはない）。

namespace MyWpfCtrls
{
	public class EditBox : Control
	{
		#region Static Constructor

		static EditBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(EditBox), new FrameworkPropertyMetadata(typeof(EditBox)));
		}

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			TextBlock textBlock = GetTemplateChild("PART_TextBlockPart") as TextBlock;
			Binding binding = new Binding("Value");
			binding.Mode = BindingMode.TwoWay;
			binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			binding.Source = this;
			textBlock.SetBinding(TextBlock.TextProperty, binding);

			Debug.Assert(textBlock != null, "No TextBlock!");

			_textBox = new TextBox();
			_adorner = new EditBoxAdorner(textBlock, _textBox);
			AdornerLayer layer = AdornerLayer.GetAdornerLayer(textBlock);
			layer.Add(_adorner);

			_textBox.KeyDown += new KeyEventHandler(OnTextBoxKeyDown);
			_textBox.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(OnTextBoxLostKeyboardFocus);

			HookTemplateParentResizeEvent();
			HookItemsControlEvents();

			_listViewItem = GetDependencyObjectFromVisualTree(this, typeof(ListViewItem)) as ListViewItem;
		}

		#endregion

		#region Protected Methods

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);
			if (!IsEditing && IsParentSelected)
			{
				_canBeEdit = true;
			}
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			_isMouseWithinScope = false;
			_canBeEdit = false;
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);

			if (e.ChangedButton == MouseButton.Right || e.ChangedButton == MouseButton.Middle)
				return;

			if (!IsEditing)
			{
				if (!e.Handled && (_canBeEdit || _isMouseWithinScope))
				{
					IsEditing = true;
				}

				if (IsParentSelected)
					_isMouseWithinScope = true;
			}
		}

		#endregion

		#region Public Properties

		#region Value

		/// <summary>
		/// ValueProperty DependencyProperty.
		/// </summary>
		public static readonly DependencyProperty ValueProperty =
				DependencyProperty.Register(
						"Value",
						typeof(string),
						typeof(EditBox),
						new FrameworkPropertyMetadata(null));

		/// <summary>
		/// EditBoxの値
		/// </summary>
		public object Value
		{
			get { return GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		#endregion

		#region IsEditing

		public static DependencyProperty IsEditingProperty =
				DependencyProperty.Register(
						"IsEditing",
						typeof(bool),
						typeof(EditBox),
						new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsEditingPropertyChanged));

		private static void IsEditingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			EditBox editBox = (EditBox)sender;
			editBox._adorner.UpdateVisibilty((bool)e.NewValue);
		}

		/// <summary>
		/// true: 編集モード（依存関係プロパティ）
		/// </summary>
		public bool IsEditing
		{
			get { return (bool)GetValue(IsEditingProperty); }
			private set
			{
				SetValue(IsEditingProperty, value);
#if true
				_adorner.UpdateVisibilty(value);
				if (!IsEditing)
				{
					_listViewItem.Focus();
				}
#endif
			}
		}

		#endregion

		#region IsParentSelected

		/// <summary>
		/// ListViewItemが選択されているかどうか判定する。
		/// </summary>
		private bool IsParentSelected
		{
			get
			{
				if (_listViewItem == null)
					return false;
				else
					return _listViewItem.IsSelected;
			}
		}

		#endregion

		#endregion

		#region Private Methods

		private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (IsEditing && (e.Key == Key.Enter || e.Key == Key.F2))
			{
				IsEditing = false;
				_canBeEdit = false;
			}
		}

		private void OnTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			IsEditing = false;
		}

		private void OnCouldSwitchToNormalMode(object sender, RoutedEventArgs e)
		{
			IsEditing = false;
		}

		private void HookItemsControlEvents()
		{
			_itemsControl = GetDependencyObjectFromVisualTree(this, typeof(ItemsControl)) as ItemsControl;
			if (_itemsControl != null)
			{
				_itemsControl.SizeChanged += new SizeChangedEventHandler(OnCouldSwitchToNormalMode);
				_itemsControl.AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(OnScrollViewerChanged));
				_itemsControl.AddHandler(ScrollViewer.MouseWheelEvent, new RoutedEventHandler(OnCouldSwitchToNormalMode), true);
			}
		}

		private void OnScrollViewerChanged(object sender, RoutedEventArgs args)
		{
			if (IsEditing && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
			{
				IsEditing = false;
			}
		}

		private DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
		{
			DependencyObject parent = startObject;
			while (parent != null)
			{
				if (type.IsInstanceOfType(parent))
					break;
				else
					parent = VisualTreeHelper.GetParent(parent);
			}
			return parent;
		}

		private void HookTemplateParentResizeEvent()
		{
			FrameworkElement parent = TemplatedParent as FrameworkElement;
			if (parent != null)
			{
				parent.SizeChanged += new SizeChangedEventHandler(OnCouldSwitchToNormalMode);
			}
		}

		#endregion

		#region Private Fields

		private EditBoxAdorner _adorner;
		private FrameworkElement _textBox;
		private bool _canBeEdit;
		private bool _isMouseWithinScope;
		private ItemsControl _itemsControl;
		private ListViewItem _listViewItem;

		#endregion
	}
}
