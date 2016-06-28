using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MyWpfBehaviors
{
	// 添付ビヘイビアは対象コントロールの寿命に注意すること。所詮イベント ハンドラーの追加を自動化しているだけなので、
	// 動的に生成・破棄する要素に対して安易に添付ビヘイビアを付与すると、イベント ハンドラーのデリゲートが GC 回収対象にならないこともある。

#if false
	/// <summary>
	/// TextBox がフォーカスを受け取ったときに、テキストを全選択する添付ビヘイビア。
	/// </summary>
	public static class TextBoxSelectsAllOnGotFocusBehavior
	{
		public static readonly DependencyProperty SelectsAllOnGotFocusProperty =
			DependencyProperty.RegisterAttached(
				"SelectsAllOnGotFocus",
				typeof(bool),
				typeof(TextBoxSelectsAllOnGotFocusBehavior),
				new UIPropertyMetadata(false, SelectsAllOnGotFocusChanged)
			);

		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static bool GetSelectsAllOnGotFocus(DependencyObject obj)
		{
			return (bool)obj.GetValue(SelectsAllOnGotFocusProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static void SetSelectsAllOnGotFocus(DependencyObject obj, bool value)
		{
			obj.SetValue(SelectsAllOnGotFocusProperty, value);
		}

		private static void SelectsAllOnGotFocusChanged(DependencyObject sender, DependencyPropertyChangedEventArgs evt)
		{
			var textBox = sender as TextBox;
			if (textBox == null)
			{
				return;
			}

			textBox.GotFocus -= OnTextBoxGotFocus;
			if ((bool)evt.NewValue)
			{
				textBox.GotFocus += OnTextBoxGotFocus;
			}
		}

		private static void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
		{
			var textBox = sender as TextBox;
			Debug.Assert(textBox != null);

			textBox.Dispatcher.BeginInvoke((Action)(() => textBox.SelectAll()));
		}
	}

#else

	// WPF の TextBox や PasswordBox は、Win32 や HTML フォームとは違ってキーボード フォーカスを受け取っても全選択にならない。
	// それを解消するための添付ビヘイビア。
	// ただし、Win32 や HTML フォームのテキスト ボックスでは、Tab キーでフォーカスすると全選択になるが、
	// マウスによるフォーカスの場合は全選択にならないという仕様がある。
	// 一方で、Web ブラウザーのアドレス バーなどは、Tab キーでもマウスでも全選択になる。
	// 前者に合わせる場合、GotFocus あるいは GotKeyboardFocus イベントのみを使えばなんとかなる……と思っていたが、
	// 一瞬だけ全選択・ハイライトされるなどの現象が出て、あまり見た目がきれいな動作ではなく、結局 Win32 互換ではないし、
	// またときどき全選択が残ったままになることもあるので、
	// PreviewMouseLeftButtonDown イベントも使い、後者に合わせることにする。
	// ちなみにコンボ ボックスは Win32 も WPF も、Tab キーでもマウスでも全選択になる。

	/// <summary>
	/// TextBoxBase あるいは PasswordBox がフォーカスを受け取ったときに、テキストを全選択する添付ビヘイビア。
	/// </summary>
	public static class TextBoxSelectsAllOnGotFocusBehavior
	{
		public static readonly DependencyProperty SelectsAllOnGetFocusProperty =
			DependencyProperty.RegisterAttached(
				"SelectsAllOnGotFocus",
				typeof(bool),
				typeof(TextBoxSelectsAllOnGotFocusBehavior),
				new UIPropertyMetadata(OnPropertyChanged)
			);

		public static bool GetSelectsAllOnGotFocus(DependencyObject obj)
		{
			return (bool)obj.GetValue(SelectsAllOnGetFocusProperty);
		}

		public static void SetSelectsAllOnGotFocus(DependencyObject obj, bool value)
		{
			obj.SetValue(SelectsAllOnGetFocusProperty, value);
		}

		private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			// as 演算子で変換して null 判定する方法だと、XAML 上で TextBoxBase 派生クラス以外の要素にも適用可能（意味はないが）。
			// キャストで変換して失敗時に例外を発生させる方法だと、XAML 上で TextBoxBase 派生クラス以外の要素には適用できない。

			if (!(sender is TextBoxBase) && !(sender is PasswordBox)) { return; }

			var textBox = sender as UIElement;
			if (textBox == null) { return; }

			var newValue = (bool)e.NewValue;
			var oldValue = (bool)e.OldValue;
			if (oldValue != newValue)
			{
				//textBox.GotFocus -= textBox_GotFocus;
				textBox.GotKeyboardFocus -= textBox_GotKeyboardFocus;
				textBox.PreviewMouseLeftButtonDown -= textBox_PreviewMouseLeftButtonDown;
				if (newValue)
				{
					//textBox.GotFocus += textBox_GotFocus;
					textBox.GotKeyboardFocus += textBox_GotKeyboardFocus;
					textBox.PreviewMouseLeftButtonDown += textBox_PreviewMouseLeftButtonDown;
				}
			}
		}

		private static void textBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			// ジェネリクスはテンプレートとは違うのでこの場面では使えない。C# 4.0 以降の dynamic だと動的ダック タイピングできるが、今回は使わない。

			if (sender is TextBoxBase)
			{
				((TextBoxBase)sender).SelectAll();
			}
			else if (sender is PasswordBox)
			{
				((PasswordBox)sender).SelectAll();
			}
		}

		private static void textBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var textBox = sender as UIElement;
			if (textBox == null) { return; }

			if (!textBox.IsKeyboardFocused)
			{
				textBox.Focus();
				e.Handled = true;
			}
		}

		private static void textBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if (sender is TextBoxBase)
			{
				((TextBoxBase)sender).SelectAll();
			}
			else if (sender is PasswordBox)
			{
				((PasswordBox)sender).SelectAll();
			}
		}
	}
#endif

	/// <summary>
	/// TextBox での Enter キー押下時にバインディング ソースを自動更新する添付ビヘイビア。
	/// </summary>
	public static class TextBoxUpdatesBindingSourceOnEnterKeyDownBehavior
	{
		public static readonly DependencyProperty UpdatesBindingSourceOnEnterKeyDownProperty =
			DependencyProperty.RegisterAttached(
				"UpdatesBindingSourceOnEnterKeyDown",
				typeof(bool),
				typeof(TextBoxUpdatesBindingSourceOnEnterKeyDownBehavior),
				new UIPropertyMetadata(false, UpdatesBindingSourceOnEnterKeyDownChanged)
			);

		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static bool GetUpdatesBindingSourceOnEnterKeyDown(DependencyObject obj)
		{
			return (bool)obj.GetValue(UpdatesBindingSourceOnEnterKeyDownProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static void SetUpdatesBindingSourceOnEnterKeyDown(DependencyObject obj, bool value)
		{
			obj.SetValue(UpdatesBindingSourceOnEnterKeyDownProperty, value);
		}

		private static void UpdatesBindingSourceOnEnterKeyDownChanged(DependencyObject sender, DependencyPropertyChangedEventArgs evt)
		{
			var textBox = sender as TextBox;
			if (textBox == null)
			{
				return;
			}

			textBox.KeyDown -= UpdateBindingSourceOfTextBoxTextOnEnterKeyDown;
			if ((bool)evt.NewValue)
			{
				textBox.KeyDown += UpdateBindingSourceOfTextBoxTextOnEnterKeyDown;
			}
		}

		private static void UpdateBindingSourceOfTextBoxTextOnEnterKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter)
			{
				var textBox = sender as TextBox;
				Debug.Assert(textBox != null);
				if (textBox != null)
				{
					var binding = textBox.GetBindingExpression(TextBox.TextProperty);
					if (binding != null)
					{
						binding.UpdateSource();
					}
				}
			}
		}
	}

#if false
	/// <summary>
	/// Watermark を実現する添付ビヘイビア。
	/// </summary>
	public static class TextBoxPlaceholderTextBehavior
	{
		public static readonly DependencyProperty PlaceholderTextProperty =
			DependencyProperty.RegisterAttached(
				"PlaceholderText",
				typeof(string),
				typeof(TextBoxPlaceholderTextBehavior),
				new PropertyMetadata(null, OnPlaceholderChanged)
			);

		private static void OnPlaceholderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var textBox = sender as TextBox;
			if (textBox == null)
			{
				return;
			}

			var placeHolder = e.NewValue as string;
			var handler = CreateEventHandler(placeHolder);
			if (string.IsNullOrEmpty(placeHolder))
			{
				textBox.TextChanged -= handler;
			}
			else
			{
				textBox.TextChanged += handler;
				if (string.IsNullOrEmpty(textBox.Text))
				{
					textBox.Background = CreateVisualBrush(placeHolder, textBox.Background);
				}
			}
		}

		private static TextChangedEventHandler CreateEventHandler(string placeHolder)
		{
			// TextChanged イベントをハンドルし、TextBox が未入力のときだけ
			// プレースホルダーを表示するようにする。
			return (sender, e) =>
			{
				// 背景に Label を描画する VisualBrush を使って
				// プレースホルダーを実現。
				var textBox = (TextBox)sender;
				if (String.IsNullOrEmpty(textBox.Text))
				{
					textBox.Background = CreateVisualBrush(placeHolder, textBox.Background);
				}
				else
				{
					var vb = textBox.Background as VisualBrush;
					if (vb != null)
					{
						var inner = vb.Visual as Label;
						if (inner != null)
						{
							var orig = inner.Tag as Brush;
							textBox.Background = orig;
						}
					}
					//textBox.Background = new SolidColorBrush(Colors.Transparent);
					var origBackground = System.Windows.Application.Current.Resources["InputBackground"];
					textBox.Background = (Brush)origBackground;
					// InputBackground だけを考慮する方法だと、InputBackgroundHover が考慮されなくなる。
					// VisualBrush を使って Background を制御する方法は、結局破壊的なので制約が大きい。
				}
				// ちなみに Watermark を ComboBox にも付けたい場合、
				// WPF 4.5 の Windows 8 Aero2 テーマでは ComboBox.Background がカスタマイズできないため、
				// 別の方法を探る必要がある。
				// また、強制的に Transparent にしてしまうと、スタイルなどで背景色を設定していた場合はキャンセルされてしまう。
				// 動的なテーマ変更や ClearType とも相性が悪い。
				// Extended WPF Toolkit を使ったほうがよいか？
				// なお、Extended WPF TK 2.0.0 時点でビルド済みバイナリは .NET 4 向けのアセンブリしか提供されてないので、
				// Windows 8 上で実行すると Aero2 ではなく Aero になってしまうと思われる。.NET 4.5 にリターゲットしてビルドし直す必要がある。
			};
		}

		private static VisualBrush CreateVisualBrush(string placeHolder, Brush oldBackground)
		{
			var visual = new Label()
			{
				// 文字色は手動カスタマイズもしくはテーマに沿って自動変更できたほうがよいかも。
				Content = placeHolder,
				Padding = new Thickness(5, 1, 1, 1),
				Foreground = new SolidColorBrush(Colors.DarkGray),
				//HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
				Tag = oldBackground,
				// Tag でキャッシュしておく方法だと、テーマを変えたときにおかしくなる。
			};
			return new VisualBrush(visual)
			{
				Stretch = Stretch.None,
				TileMode = TileMode.None,
				AlignmentX = AlignmentX.Left,
				AlignmentY = AlignmentY.Center,
			};
		}

		public static void SetPlaceholderText(TextBox textBox, string placeHolder)
		{
			textBox.SetValue(PlaceholderTextProperty, placeHolder);
		}

		public static string GetPlaceholderText(TextBox textBox)
		{
			return textBox.GetValue(PlaceholderTextProperty) as string;
		}
	}
#endif

	// http://www.makcraft.com/blog/meditation/2014/02/02/to-scroll-from-the-viewmodel-on-wpf/
	// HACK: ビューがアクティブになっていないと反映されない模様。

	public enum ScrollJumpType
	{
		None,
		LineDown,
		LineUp,
		ToBottom,
		ToTop,
	}

	/// <summary>
	/// スクロールの移動をバインディングで行なうための添付ビヘイビア。
	/// </summary>
	public static class ScrollJumpBehavior
	{
		public static readonly DependencyProperty KickScrollProperty = DependencyProperty.RegisterAttached(
			"KickScroll", typeof(ScrollJumpType), typeof(ScrollJumpBehavior),
			new FrameworkPropertyMetadata
			{
				DefaultValue = ScrollJumpType.None,
				PropertyChangedCallback = KickScrollChanged,
				BindsTwoWayByDefault = true
			});

		public static ScrollJumpType GetKickScroll(DependencyObject obj)
		{
			return (ScrollJumpType)obj.GetValue(KickScrollProperty);
		}

		public static void SetKickScroll(DependencyObject obj, ScrollJumpType value)
		{
			obj.SetValue(KickScrollProperty, value);
		}

		private static void KickScrollChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var control = sender as Control;

			if (control == null) return;

			if ((ScrollJumpType)e.NewValue == ScrollJumpType.None) return;

			if (VisualTreeHelper.GetChildrenCount(control) == 0) return; // 子要素が取れる状態かを確認。

			var decorator = VisualTreeHelper.GetChild(control, 0) as Decorator;

			if (decorator == null) return;

			var scrollViewer = decorator.Child as ScrollViewer;

			if (scrollViewer != null)
			{
				switch ((ScrollJumpType)e.NewValue)
				{
					case ScrollJumpType.LineDown:
						scrollViewer.LineDown();
						break;

					case ScrollJumpType.LineUp:
						scrollViewer.LineUp();
						break;

					case ScrollJumpType.ToBottom:
						scrollViewer.ScrollToBottom();
						break;

					case ScrollJumpType.ToTop:
						scrollViewer.ScrollToTop();
						break;
				}
			}

			SetKickScroll(control, ScrollJumpType.None); // プロパティ値を None に戻す。
		}

	}
}
