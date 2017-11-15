using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyWpfProperties
{
	public static class CustomPropertyHelper
	{
		// 添付プロパティで任意の DependencyObject に外付けできるようにしておけば、欲しいプロパティを持たない場合も、いちいち依存関係プロパティを定義したりしないで済む。
		// たとえば ListView 全体を Disabled にするのではなく、内部 TextBox のみを ReadOnly にしたいとき、ホストする UserControl に独自の依存関係プロパティを毎回作るのは面倒。
		// 不可視のダミーコントロールを UserControl 内部に置いて、何らかの bool 型プロパティを ListView の各アイテムの IsReadOnly にバインディングする方法も考えたが、
		// 添付プロパティが一番簡単かつ汎用的。ほかには FrameworkElement.Tag をバインディングの中継に使う方法もあるが、もったいない。

		/// <summary>
		/// 子要素の IsReadOnly を親要素でまとめて設定する際などに使用することを想定している。
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.RegisterAttached(
			"IsReadOnly",
			typeof(bool),
			typeof(CustomPropertyHelper),
			new PropertyMetadata(false));

		public static bool GetIsReadOnly(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsReadOnlyProperty);
		}

		public static void SetIsReadOnly(DependencyObject obj, bool value)
		{
			obj.SetValue(IsReadOnlyProperty, value);
		}
	}
}
