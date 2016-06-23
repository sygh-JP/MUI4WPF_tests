using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MyWpfCtrls
{
#if false
	// 結局 OnRender() を直接使っても、ImageSource として渡すタイミングで Pbgra32 変換が入るらしい。一度変換されるとデータ自体はキャッシュされる模様。
	// Image.Source プロパティを変更するのではなく、描画・表示専用の WriteableBitmap のバッファをフレームごとに書き換えていくしかない？

	public class MyCustomImageElement : FrameworkElement
	{
		private WriteableBitmap _source;
		public WriteableBitmap Source
		{
			get { return this._source; }
			set
			{
				this._source = value;
				this.InvalidateVisual();
			}
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			if (this.Source != null)
			{
				drawingContext.DrawImage(this.Source, new Rect(0, 0, this.Source.PixelWidth, this.Source.PixelHeight));
			}
		}
	}
#endif

	// WPF の ScrollViewer 上でマウス ホイールを操作すると、垂直方向のスクロール バーのみ反応する。
	// むしろ邪魔なので無効化する。
	// http://blog.ramondeklein.nl/index.php/2009/07/24/scrollviewer-always-handles-the-mousewheel/

	public class MyCustomScrollViewer : ScrollViewer
	{
		private ScrollBar verticalScrollbar;

		public override void OnApplyTemplate()
		{
			// Call base class
			base.OnApplyTemplate();

			// Obtain the vertical scrollbar
			this.verticalScrollbar = this.GetTemplateChild("PART_VerticalScrollBar") as ScrollBar;
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			// Only handle this message if the vertical scrollbar is in use
			if ((this.verticalScrollbar != null) && (this.verticalScrollbar.Visibility == Visibility.Visible) && this.verticalScrollbar.IsEnabled)
			{
				// Perform default handling
				//base.OnMouseWheel(e);
			}
		}
	}
}
