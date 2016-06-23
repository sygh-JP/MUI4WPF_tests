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

namespace MyWpfCtrls
{
	/// <summary>
	/// MetroIconHeartbeat.xaml の相互作用ロジック
	/// </summary>
	public partial class MetroIconHeartbeat : UserControl
	{
		public MetroIconHeartbeat()
		{
			InitializeComponent();

			this.HeartFontSize = DefaultFontSizePresetSmall;
			this.HeartOpacity = 1.0;
			this.IsSolidHeart = false;

			//this.Loaded += (s, e) => { this.RaiseBeatEvent(); }; // テスト コード。
		}

		// Verdana フォント基準。
		public const double DefaultFontSizePresetLarge = 28;
		public const double DefaultFontSizePresetSmall = 20;

		public double HeartFontSize
		{
			get
			{
				return this.textblockHeart.FontSize;
			}
			set
			{
				//double half = value / 2;
				//this.heartScaleTransform.CenterX = half;
				//this.heartScaleTransform.CenterY = half;
				this.textblockHeart.FontSize = value;
			}
		}

		public double HeartOpacity
		{
			get
			{
				return this.textblockHeart.Opacity;
			}
			set
			{
				this.textblockHeart.Opacity = value;
			}
		}

		/// <summary>
		/// 中実のハート 2665H「♥」か否か。false の場合は中空のハート 2661H「♡」になる。
		/// </summary>
		public bool IsSolidHeart
		{
			get
			{
				return this.textblockHeart.Text == "♥";
			}
			set
			{
				this.textblockHeart.Text = value ? "♥" : "♡";
			}
		}

		// カスタム イベントでルーティング イベントをサポートために RoutedEvent を登録する。
		public static readonly RoutedEvent GradBeatEvent = EventManager.RegisterRoutedEvent(
			"GradBeat", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MetroIconHeartbeat));

		public event RoutedEventHandler GradBeat
		{
			add { this.AddHandler(GradBeatEvent, value); }
			remove { this.RemoveHandler(GradBeatEvent, value); }
		}

		public void RaiseGradBeatEvent()
		{
			RoutedEventArgs newEventArgs = new RoutedEventArgs(GradBeatEvent);
			RaiseEvent(newEventArgs);
		}

		// ハートの不透明度がグラデーションでアニメーションするタイプ。
		public void AnimateOpacity()
		{
			this.IsSolidHeart = true;
			this.HeartFontSize = MyWpfCtrls.MetroIconHeartbeat.DefaultFontSizePresetLarge;
			this.RaiseGradBeatEvent();
		}

		// ハートの不透明度がフリップするタイプ。
		public void FlipOpacityByCounter(uint counter)
		{
			this.IsSolidHeart = true;
			this.HeartFontSize = MyWpfCtrls.MetroIconHeartbeat.DefaultFontSizePresetLarge;
			this.HeartOpacity = counter % 2 == 0 ? 0.5 : 1.0;
		}

		// ハートの大きさがフリップするタイプ。また、中空から中実になる。
		public void FlipSizeByCounter(uint counter)
		{
			this.IsSolidHeart = true;
			this.HeartFontSize =
				counter % 2 == 0
				? DefaultFontSizePresetLarge
				: DefaultFontSizePresetSmall;
		}

		// サブスレッドからの直接呼び出しに対応。
		public void InvokeFlipSizeByCounter(uint counter)
		{
			// Dispatcher.Invoke() するか、それとも直接呼び出すかどうかを分岐する DispatcherObject.CheckAccess() 呼び出しは不要らしい。
			// インテリセンスにも表示されない、隠しメソッドらしい？
			this.Dispatcher.Invoke(() =>
			{
				this.FlipSizeByCounter(counter);
			});
		}

#if false
		public string ToolTipText
		{
			get
			{
				return this.textblockHeart.ToolTip.ToString();
			}
			set
			{
				this.textblockHeart.ToolTip = value;
			}
		}
#endif
	}
}
