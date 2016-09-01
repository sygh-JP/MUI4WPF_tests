using FirstFloor.ModernUI.Windows.Controls;
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
using System.Windows.Shapes;

namespace MyWpfCtrls
{
	/// <summary>
	/// MyModernTaskDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class MyModernTaskDialog : ModernDialog
	{
		public MyModernTaskDialog()
		{
			InitializeComponent();

			// WPF 標準メッセージボックス（Win32 API）の場合、一定の幅までは可変だが、それ以上は固定となる。
			// BBCodeBlock に Width を指定せず、MinWidth や MaxWidth を指定するだけでは、折り返しがうまく効かない模様。
			// なお、Label だとアンダースコアが使えないので TextBlock を使う。

			// MUI4WPF のオリジナル実装では、高さ制限がかなりきついが、
			// WPF 標準のメッセージボックス（というか Win32 メッセージボックス）は一応ワークエリアの高さまで伸ばせるはずなので、修正しておく。
			// HACK: システム設定やユーザー設定の変更には追従していない。
			MaxHeight = System.Windows.SystemParameters.WorkArea.Height;

			this.IsVerificationCheckBoxVisible = false;
		}

		// MFC の CTaskDialog::SetVerificationCheckboxText(), SetVerificationCheckbox(), GetVerificationCheckboxState() のようなものを実装する。
		// コントロールのプロパティを直接制御せず、データバインディングを使ってもよいが、あまり旨味はなさそう。

		public string MainMessageText { set { this.textblockMainMessage.Text = value; } }

		public bool? VerificationCheckBoxState
		{
			get { return this.checkVerification.IsChecked; }
			set { this.checkVerification.IsChecked = value; }
		}

		public bool IsVerificationCheckBoxVisible
		{
			set { this.checkVerification.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
		}

		public string VerificationCheckBoxText
		{
			set { this.checkVerification.Content = value; }
		}

		// WPF 標準のように、アイコンを MessageBoxImage で指定できるようにする。
		public MessageBoxImage IconType
		{
			set
			{
				switch (value)
				{
					// Dark テーマの背景にも、Light テーマの背景にも映える色を使う。
					case MessageBoxImage.Error:
						this.pathIcon.Data = MyWpfHelpers.MyModernDialogHack.CommonData.ErrorIconGeometry;
						this.pathIcon.Fill = Brushes.Crimson;
						this.pathIcon.Visibility = System.Windows.Visibility.Visible;
						break;
					case MessageBoxImage.Warning:
						this.pathIcon.Data = MyWpfHelpers.MyModernDialogHack.CommonData.WarningIconGeometry;
						this.pathIcon.Fill = Brushes.Orange;
						this.pathIcon.Visibility = System.Windows.Visibility.Visible;
						break;
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
					default:
						this.pathIcon.Visibility = System.Windows.Visibility.Collapsed;
						break;
				}
			}
		}

		public static void PlaySound(MessageBoxImage image)
		{
			switch (image)
			{
				case MessageBoxImage.Error:
					System.Media.SystemSounds.Hand.Play();
					break;
				case MessageBoxImage.Warning:
					System.Media.SystemSounds.Exclamation.Play();
					break;
				case MessageBoxImage.Information:
					System.Media.SystemSounds.Asterisk.Play();
					break;
				case MessageBoxImage.Question:
					System.Media.SystemSounds.Question.Play();
					break;
				default:
					break;
			}
			// Beep は例えばモーダルダイアログ表示中に親ウィンドウ領域をクリックしようとしたときなどに再生される音。
			//System.Media.SystemSounds.Beep.Play();
		}
	}
}
