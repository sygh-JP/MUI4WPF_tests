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
		// ToolTip のインスタンスごとに BitmapImage として画像生成するのは時間とメモリの無駄なので、よく使うアイコン画像は起動時にキャッシュしておく。

		private static readonly BitmapImage f_errorImage24p = null;
		private static readonly BitmapImage f_warningImage24p = null;
		private static readonly BitmapImage f_infoImage24p = null;
		private static readonly BitmapImage f_helpImage24p = null;

		public static bool IsModernStyle { get; set; }

		static BalloonToolTipContent()
		{
			// Visual Studio 2010 に付属する ImageLibrary の .ico ファイルから抽出したラスター画像。

			f_errorImage24p = new BitmapImage(new Uri("pack://application:,,,/MyWpfCommon;component/Images/Annotation_Error_24.png"));
			f_warningImage24p = new BitmapImage(new Uri("pack://application:,,,/MyWpfCommon;component/Images/Annotation_Warning_24.png"));
			f_infoImage24p = new BitmapImage(new Uri("pack://application:,,,/MyWpfCommon;component/Images/Annotation_Info_24.png"));
			f_helpImage24p = new BitmapImage(new Uri("pack://application:,,,/MyWpfCommon;component/Images/Annotation_Help_24.png"));

			// デフォルトでベクトル ベースの Modern スタイルとする。
			IsModernStyle = true;
		}

		public void SetIconImage(MessageBoxImage mbImage)
		{
			// TODO: もしツールチップごとにカスタム画像を設定する機能を付ける場合、オーバーロードを定義する。
			if (IsModernStyle)
			{
				this.imageIcon.Visibility = System.Windows.Visibility.Collapsed;
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
			else
			{
				this.pathIcon.Visibility = System.Windows.Visibility.Collapsed;
				switch (mbImage)
				{
					//case MessageBoxImage.Stop:
					//case MessageBoxImage.Hand:
					case MessageBoxImage.Error:
						this.imageIcon.Source = f_errorImage24p;
						this.imageIcon.Visibility = System.Windows.Visibility.Visible;
						break;
					//case MessageBoxImage.Exclamation:
					case MessageBoxImage.Warning:
						this.imageIcon.Source = f_warningImage24p;
						this.imageIcon.Visibility = System.Windows.Visibility.Visible;
						break;
					//case MessageBoxImage.Asterisk:
					case MessageBoxImage.Information:
						this.imageIcon.Source = f_infoImage24p;
						this.imageIcon.Visibility = System.Windows.Visibility.Visible;
						break;
					case MessageBoxImage.Question:
						this.imageIcon.Source = f_helpImage24p;
						this.imageIcon.Visibility = System.Windows.Visibility.Visible;
						break;
					case MessageBoxImage.None:
					default:
						this.imageIcon.Source = null;
						this.imageIcon.Visibility = System.Windows.Visibility.Collapsed;
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
		ToolTip f_balloonToolTip = new ToolTip();
		// バルーン型ツールチップを隠すディレイ タイマー。
		DispatcherTimer f_balloonToolTipDelayHideTimer = new DispatcherTimer(DispatcherPriority.Normal);

		BalloonToolTipContent f_tipRootPanel = new BalloonToolTipContent();

		public BalloonToolTipProxy()
		{
			this.f_balloonToolTip.Content = this.f_tipRootPanel;
			this.f_balloonToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
			this.f_balloonToolTip.HorizontalOffset = 4;
			this.f_balloonToolTip.VerticalOffset = 4;

			this.f_balloonToolTipDelayHideTimer.Interval = new TimeSpan(0, 0, DefaultBalloonToolTipOpeningPeriodInSec);
			this.f_balloonToolTipDelayHideTimer.Tick += (s, e) =>
			{
				// 指定時間が経過したら、バルーン型ツールチップを隠してタイマーを停止。
				this.f_balloonToolTip.IsOpen = false;
				this.f_balloonToolTipDelayHideTimer.Stop();
			};
		}

		public void SetIconImage(MessageBoxImage mbImage)
		{
			this.f_tipRootPanel.SetIconImage(mbImage);
		}

		public UIElement PlacementTarget
		{
			get { return this.f_balloonToolTip.PlacementTarget; }
			set { this.f_balloonToolTip.PlacementTarget = value; }
		}

		public string MainText
		{
			get { return this.f_tipRootPanel.MainText; }
			set { this.f_tipRootPanel.MainText = value; }
		}

		public string SubText
		{
			get { return this.f_tipRootPanel.SubText; }
			set { this.f_tipRootPanel.SubText = value; }
		}


		public void ShowBalloon()
		{
			// バルーンとヒントは似て非なるもの。
			// http://msdn.microsoft.com/ja-jp/library/windows/desktop/aa511495.aspx
			// http://msdn.microsoft.com/ja-jp/library/windows/desktop/aa511451.aspx

			// タイマーをリセットして、エラーを示すバルーン（!=ヒント）を明示的に表示する。
			this.f_balloonToolTipDelayHideTimer.Stop();
			this.f_balloonToolTip.IsOpen = true;
			this.f_balloonToolTipDelayHideTimer.Start();
		}
	}
}
