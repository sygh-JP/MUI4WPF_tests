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
using System.Windows.Threading;


namespace MyWpfCtrls
{
	/// <summary>
	/// BalloonToolTipContent.xaml の相互作用ロジック
	/// </summary>
	public partial class BalloonToolTipContent : UserControl
	{
		public void SetIconImage(MessageBoxImage mbImage)
		{
			// 高 DPI 環境への対応を考慮し、ラスター画像ではなくベクトル画像を使う。

			// TODO: もしツールチップごとにカスタム画像を設定する機能を付ける場合、オーバーロードを定義する。
			{
				switch (mbImage)
				{
					//case MessageBoxImage.Stop:
					//case MessageBoxImage.Hand:
					case MessageBoxImage.Error:
						this.pathIcon.Data = MyWpfHelpers.MyModernDialogHack.CommonData.ErrorIconGeometry;
						this.pathIcon.Fill = Brushes.Crimson;
						this.pathIcon.Visibility = System.Windows.Visibility.Visible;
						break;
					//case MessageBoxImage.Exclamation:
					case MessageBoxImage.Warning:
						this.pathIcon.Data = MyWpfHelpers.MyModernDialogHack.CommonData.WarningIconGeometry;
						this.pathIcon.Fill = Brushes.Orange;
						this.pathIcon.Visibility = System.Windows.Visibility.Visible;
						break;
					//case MessageBoxImage.Asterisk:
					case MessageBoxImage.Information:
						this.pathIcon.Data = MyWpfHelpers.MyModernDialogHack.CommonData.InformationIconGeometry;
						this.pathIcon.Fill = Brushes.DeepSkyBlue;
						this.pathIcon.Visibility = System.Windows.Visibility.Visible;
						break;
					case MessageBoxImage.Question:
						this.pathIcon.Data = MyWpfHelpers.MyModernDialogHack.CommonData.QuestionIconGeometry;
						this.pathIcon.Fill = Brushes.DodgerBlue;
						this.pathIcon.Visibility = System.Windows.Visibility.Visible;
						break;
					case MessageBoxImage.None:
					default:
						this.pathIcon.Data = null;
						this.pathIcon.Fill = Brushes.Transparent;
						this.pathIcon.Visibility = System.Windows.Visibility.Collapsed;
						break;
				}
			}
		}

		public string MainText
		{
			get { return this.textblockMain.Text; }
			set { this.textblockMain.Text = value; }
		}

		public string SubText
		{
			get { return this.textblockSub.Text; }
			set { this.textblockSub.Text = value; }
		}

		public BalloonToolTipContent()
		{
			InitializeComponent();

			// Main は Sub よりも一定量だけフォントを大きくするため、XAML ではあえて指定していない。
			this.textblockMain.FontSize = this.FontSize + 2;
		}
	}

	/// <summary>
	/// あえて ToolTip を継承（is-a）せず、コンポジション（has-a）してラップする形をとる Proxy クラス。
	/// </summary>
	public class BalloonToolTipProxy
	{
		const int DefaultBalloonToolTipOpeningPeriodInSec = 5;

		// バルーン型ツールチップ。
		ToolTip _balloonToolTip = new ToolTip();
		// バルーン型ツールチップを隠すディレイ タイマー。
		DispatcherTimer _balloonToolTipDelayHideTimer = new DispatcherTimer(DispatcherPriority.Normal);

		BalloonToolTipContent _tipRootPanel = new BalloonToolTipContent();

		public BalloonToolTipProxy()
		{
			this._balloonToolTip.Content = this._tipRootPanel;
			this._balloonToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
			this._balloonToolTip.HorizontalOffset = 4;
			this._balloonToolTip.VerticalOffset = 4;

			this._balloonToolTipDelayHideTimer.Interval = new TimeSpan(0, 0, DefaultBalloonToolTipOpeningPeriodInSec);
			this._balloonToolTipDelayHideTimer.Tick += (s, e) =>
			{
				// 指定時間が経過したら、バルーン型ツールチップを隠してタイマーを停止。
				this._balloonToolTip.IsOpen = false;
				this._balloonToolTipDelayHideTimer.Stop();
			};
		}

		public void SetIconImage(MessageBoxImage mbImage)
		{
			this._tipRootPanel.SetIconImage(mbImage);
		}

		public UIElement PlacementTarget
		{
			get { return this._balloonToolTip.PlacementTarget; }
			set { this._balloonToolTip.PlacementTarget = value; }
		}

		public string MainText
		{
			get { return this._tipRootPanel.MainText; }
			set { this._tipRootPanel.MainText = value; }
		}

		public string SubText
		{
			get { return this._tipRootPanel.SubText; }
			set { this._tipRootPanel.SubText = value; }
		}


		public void ShowBalloon()
		{
			// バルーンとヒントは似て非なるもの。
			// http://msdn.microsoft.com/ja-jp/library/windows/desktop/aa511495.aspx
			// http://msdn.microsoft.com/ja-jp/library/windows/desktop/aa511451.aspx

			// タイマーをリセットして、エラーを示すバルーン（!=ヒント）を明示的に表示する。
			this._balloonToolTipDelayHideTimer.Stop();
			this._balloonToolTip.IsOpen = true;
			this._balloonToolTipDelayHideTimer.Start();
		}
	}
}
