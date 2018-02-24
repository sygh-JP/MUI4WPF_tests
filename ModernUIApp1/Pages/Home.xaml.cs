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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModernUIApp1.Pages
{
	/// <summary>
	/// Interaction logic for Home.xaml
	/// </summary>
	public partial class Home : UserControl
	{
		bool _isFirstLoad = true;

		readonly MyWpfCtrls.MyProgressWindowSimpleWrapper _progressWindowWrapper = new MyWpfCtrls.MyProgressWindowSimpleWrapper();
		MyWpfCtrls.BalloonToolTipProxy _balloonToolTipProxy = null;

		volatile bool _isModalProgressStopped = false;

		public Home()
		{
			InitializeComponent();

			this._balloonToolTipProxy = new MyWpfCtrls.BalloonToolTipProxy();
			this._balloonToolTipProxy.PlacementTarget = this.textbox1;
			this._balloonToolTipProxy.SubText = "Enter key is pressed.";

			this.textbox1.KeyDown += (s, e) =>
			{
				if (e.Key == Key.Enter)
				{
					if (this.textbox1.Text.StartsWith("I", StringComparison.OrdinalIgnoreCase))
					{
						this._balloonToolTipProxy.SetIconImage(MessageBoxImage.Information);
						this._balloonToolTipProxy.MainText = "Information:";
					}
					else if (this.textbox1.Text.StartsWith("Q", StringComparison.OrdinalIgnoreCase))
					{
						this._balloonToolTipProxy.SetIconImage(MessageBoxImage.Question);
						this._balloonToolTipProxy.MainText = "Question:";
					}
					else if (this.textbox1.Text.StartsWith("W", StringComparison.OrdinalIgnoreCase))
					{
						this._balloonToolTipProxy.SetIconImage(MessageBoxImage.Warning);
						this._balloonToolTipProxy.MainText = "Warning:";
					}
					else if (this.textbox1.Text.StartsWith("E", StringComparison.OrdinalIgnoreCase))
					{
						this._balloonToolTipProxy.SetIconImage(MessageBoxImage.Error);
						this._balloonToolTipProxy.MainText = "Error:";
					}
					else
					{
						this._balloonToolTipProxy.SetIconImage(MessageBoxImage.None);
						this._balloonToolTipProxy.MainText = "None:";
					}
					this._balloonToolTipProxy.ShowBalloon();
				}
			};

			this.mbutton1.Click += (s, e) =>
			{
				// WPF 標準のメッセージ ボックス。CenterOwner でなく CenterScreen となる。
				// Win32 API の MessageBox() と同じだが、どうも単に P/Invoke しているだけらしい。
				// MFC の AfxMessageBox() は Center of Owner になるので、MFC に慣れていると違和感がある。
				// また、MessageBox.Show(string, ...) は、表示前に WPF アプリを非アクティブにすると、モードレスになってしまうという問題がある。
				// MessageBox.Show(Window, ...) を使ってオーナーを明示的に指定すれば回避できる。
				// 原則的にオーナーウィンドウを受け取るオーバーロードのほうを常に使用するべきだが、面倒。
				var ret = MessageBox.Show("test日本語テストα", "title", MessageBoxButton.YesNo);
				System.Diagnostics.Debug.WriteLine(ret);
			};
			this.mbutton2.Click += (s, e) =>
			{
				// MUI4WPF オリジナルの ModernDialog。タブ キーを押すとフォーカスが気持ち悪い動きをするなど、いろいろとグチャグチャで不完全。
				var ret = ModernDialog.ShowMessage("test日本語テストα", "title", MessageBoxButton.YesNo);
				System.Diagnostics.Debug.WriteLine(ret);
				// BBCode のテスト。他にもいろいろあるらしい。
				// https://mui.codeplex.com/wikipage?title=BBCode%20tag%20reference&referringTitle=Documentation
				// https://github.com/firstfloorsoftware/mui/wiki/BBCodeBlock-navigation-features
				// ちなみにエスケープが定義されていない模様。"[[" や "]]" で回避できないし、"\\[" も @"\[" も効かない。"\[" はそもそも C# では使えない。
				// [mm] [ms] [sec] などのようにテキスト中で単位を表記したりしたい場合に BBCode ごと消えてしまうので困るが……
				// とりあえず幅なしスペース U+200B やピリオド U+002E で回避する方法がある。
				// 1つでも BBCode の解析が失敗するような要素があれば、他のすべての BBCode もそのまま印字されるようになるらしい。
				// FirstFloor.ModernUI.Windows.Controls.BBCode.ParseException が内部的にスローされるが、呼び出し側には伝播しない。
				// HACK: BBCodeBlock の代わりに通常の TextBlock を使用するオプションをメソッドに付けられるとよい。というか BBCodeBlock をオプションとしたほうがよい。
				ret = ModernDialog.ShowMessage("test日本語テストα [b]Bold[/b]", "title", MessageBoxButton.OK);
				System.Diagnostics.Debug.WriteLine(ret);
				ModernDialog.ShowMessage("0.013[sec], 333[m], 42.195[km]", "title", MessageBoxButton.OK);
				ModernDialog.ShowMessage("0.013[sec.], 333[m], 42.195[km]", "title", MessageBoxButton.OK);
				ModernDialog.ShowMessage(String.Format("0.013[{0}sec], 333[m], 42.195[km]", (char)0x200b), "title", MessageBoxButton.OK);
				// 縦に長すぎると表示しきれない。
				ModernDialog.ShowMessage(Poem.AmenimoMakezuText, Poem.AmenimoMakezuTitle, MessageBoxButton.OK);
				ModernDialog.ShowMessage(Poem.Flatten(Poem.AmenimoMakezuText), Poem.AmenimoMakezuTitle, MessageBoxButton.OK);
			};
			// 独自に改良した ModernDialog Hack のテスト。
			this.button1.Click += (s, e) =>
			{
				var ret = MyWpfHelpers.MyModernDialogHack.ShowMessage("test日本語テストα", "title", MessageBoxButton.YesNo, MessageBoxImage.Warning,
					buttonTexts: new List<string>() { "はい(_Y)", "いいえ(_N)" });
				System.Diagnostics.Debug.WriteLine(ret);
			};
			this.button2.Click += (s, e) =>
			{
				MyWpfHelpers.MyModernDialogHack.ShowMessage(Poem.AmenimoMakezuText, Poem.AmenimoMakezuTitle, buttonText: "閉じる(_C)");
				// 自動折り返しのテスト。
				MyWpfHelpers.MyModernDialogHack.ShowMessage(Poem.Flatten(Poem.AmenimoMakezuText), Poem.AmenimoMakezuTitle, buttonText: "閉じる(_C)");
			};
			this.button3.Click += (s, e) =>
			{
				var taskDlg = MyWpfHelpers.MyModernDialogHack.CreateTaskDialog("Task dialog test", null, MessageBoxImage.Information);
				taskDlg.IsVerificationCheckBoxVisible = true;
				taskDlg.ShowDialog();
				System.Diagnostics.Debug.WriteLine(taskDlg.VerificationCheckBoxState);
			};
			this.button4.Click += (s, e) =>
			{
				this._isModalProgressStopped = false;
				var progWnd = new MyWpfCtrls.MyModernProgressWindow();
				progWnd.Owner = Application.Current.MainWindow;
				progWnd.Title = Application.Current.MainWindow.Title;
				progWnd.IsStopButtonVisible = true;
				progWnd.ProgressViewModel.Description = "Now waiting...";
				progWnd.ProgressViewModel.StopCommand.ExecuteHandler += (_) => { this._isModalProgressStopped = true; };
				progWnd.ProgressViewModel.IsIndeterminate = false;
				progWnd.Loaded += async (_, __) =>
				{
					const int stepMillisec = 30;
					const int movingPeriodMillisec = 3000;
					for (int i = 0; i < movingPeriodMillisec; i += stepMillisec)
					{
						progWnd.ProgressViewModel.ProgressValue = (double)i / movingPeriodMillisec;
						await Task.Delay(stepMillisec);
						if (this._isModalProgressStopped)
						{
							progWnd.UserDialogResult = null;
							progWnd.EnforcedClose();
							return; // タスク中断のエミュレート。
						}
					}
					progWnd.UserDialogResult = true;
					progWnd.EnforcedClose();
				};
				// モーダル ダイアログを表示。タスクは Loaded イベントにて非同期実行される。
				// HACK: ダイアログが表示される前にタスクが開始・終了するとおかしなことになる。
				// 回避するためには、Loaded イベントではなく ContentRendered イベントを使う。
				// もしくは、モードレスで対処。
				progWnd.ShowDialog();
				var dlgResult = progWnd.UserDialogResult;
				// C# 6.0 の ?. 演算子を使えば簡潔に書けるが、古いコンパイラでもコンパイルできるようにあえて冗長な書き方をする。
				var mainWnd = Application.Current.MainWindow as MainWindow;
				if (mainWnd != null)
				{
					mainWnd.FlashTaskbarButtonIfNotActive();
				}
				MyWpfHelpers.MyModernDialogHack.ShowMessage("DialogResult = " + MyMiscHelpers.MyGenericsHelper.ConvertToLiteralNullIfNull(dlgResult), null);
			};
			this.button5.Click += async (s, e) =>
			{
				Application.Current.MainWindow.IsEnabled = false;
				await ModelessProgressWindowTestAsync();
				Application.Current.MainWindow.IsEnabled = true;
			};
			this.button6.Click += this.ShowMessageAsync;
		}

		static async Task ModelessProgressWindowTestAsync()
		{
			var progWnd = new MyWpfCtrls.MyModernProgressWindow();
			progWnd.Owner = Application.Current.MainWindow;
			progWnd.Title = Application.Current.MainWindow.Title;
			progWnd.IsStopButtonVisible = false;
			progWnd.ProgressViewModel.Description = "Now waiting...";
			progWnd.ProgressViewModel.IsIndeterminate = true;

			// 別にモーダルでなくても OK。ただし、モードレスであれば親が操作できるので、待機中は他の UI の有効/無効を真面目に制御しなければならない。
			progWnd.Show();

			try
			{
				await Task.Delay(2000);
			}
			finally
			{
				progWnd.EnforcedClose();
			}
		}

		bool InvokeShowProgressWindow(string description)
		{
			// WPF 4.5 の Dispatcher.Invoke() は Action を受け取るオーバーロードがあるため、
			// 引数なし、戻り値なしのラムダ式を暗黙的に Action デリゲートに変換できるが、WPF 4.0 以前では不可能らしい。
			// 匿名メソッドを使うか、明示的に Action コンストラクタを呼んで Delegate として渡す必要があった。
			return this.Dispatcher.Invoke(() =>
			{
				return this._progressWindowWrapper.ShowProgressWindow<MyWpfCtrls.MyModernProgressWindow>(description);
			});
		}

		void InvokeHideProgressWindow()
		{
			this.Dispatcher.Invoke(() =>
			{
				this._progressWindowWrapper.HideProgressWindow();
			});
		}

		private async void ShowMessageAsync(object sender, RoutedEventArgs e)
		{
			// 戻り値 void が許されるのはイベント ハンドラーだけ。
			var button = sender as Button;
			System.Diagnostics.Debug.Assert(button != null);
			button.IsEnabled = false;

			// Task.Delay() は最初から非同期メソッドなので別に Task.Run() でラップする必要はないが、
			// サブスレッドから直接待機ウィンドウやメッセージ ダイアログを表示するテストのためにあえてラップする。
			System.Diagnostics.Debug.WriteLine("ThreadID = {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
			await Task.Run(async () =>
			{
				this.InvokeShowProgressWindow("Now waiting...");
				await Task.Delay(2000); // 別のサブスレッドが指定時間スリープし終えた後、このサブスレッドの処理が続行される。
				this.InvokeHideProgressWindow();
				// WPF 標準の MessageBox はサブスレッドからでも直接 Show() できる。ただしモーダルではなくモードレスになる。Win32 API と同じ。
				// サブスレッドの後続処理はダイアログが閉じられるまで一時停止する。
				// MUI4WPF オリジナルの ModernDialog では Dispatcher が必要。面倒なので Hack したほうがよい。
				var ret = MessageBox.Show("from subthread", "title");
				System.Diagnostics.Debug.WriteLine(ret);
#if false
				this.Dispatcher.Invoke(() =>
				{
					ModernDialog.ShowMessage("from subthread", "title", MessageBoxButton.OK);
				});
#else
				MyWpfHelpers.MyModernDialogHack.ShowMessage("from subthread", null);
				System.Diagnostics.Debug.WriteLine("ThreadID = {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
#endif
			});
			// await の既定の処理だと、Task.ConfigureAwait(true) が暗黙的に呼び出される。
			// 結果として、コンテキストの復帰が行なわれ、呼び出しスレッドに戻ってくる。
			System.Diagnostics.Debug.WriteLine("Completed.");
			button.IsEnabled = true;
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			// 初回ロード時のみに実行する処理を記述するためのフラグを制御する。
			// コンストラクタではタイミングが早すぎる場合、Loaded イベントを使うが、
			// タブ ページを使う場合、タブを切り替えるたびにタブ内の要素に対して Loaded イベントが発生するので注意。
			if (_isFirstLoad)
			{
				_isFirstLoad = false;

				// コンテキスト メニューは MUI4WPF の既定スタイルを適用しただけではハイライト色がカスタマイズされるだけ。
				// 明示的指定が必要。
				// TextBox に関しては XAML 側で一括指定するが、
				// ComboBox はコード ビハインドで対応。

				// 下記リソースはユーザー定義のリソース ディクショナリにて定義済みのはず。
				// 失敗すれば例外スローで強制終了。
				MyWpfHelpers.MyWpfControlHelper.ModifyAllComboBoxContextMenuAsStandard(this);
			}
		}
	}

	internal static class Poem
	{
		public const string AmenimoMakezuTitle = "雨ニモマケズ (常用漢字版)";

		public const string AmenimoMakezuText = @"
雨にも負けず
風にも負けず
雪にも夏の暑さにも負けぬ
丈夫なからだをもち
欲はなく
決していからず
いつも静かに笑っている
一日に玄米四合と
味噌と少しの野菜を食べ
あらゆることを
自分を勘定に入れずに
よく見聞きし分かり
そして忘れず
野原の松の林の陰の
小さな萱ぶきの小屋にいて
東に病気の子どもあれば
行って看病してやり
西に疲れた母あれば
行ってその稲の束を負い
南に死にそうな人あれば
行ってこわがらなくてもいいといい
北に喧嘩や訴訟があれば
つまらないからやめろといい
日照りのときは涙を流し
寒さの夏はおろおろ歩き
みんなにでくのぼうと呼ばれ
ほめられもせず
苦にもされず
そういうものに
わたしはなりたい

(宮沢賢治)
";
		public static string Flatten(string srcText)
		{
			return srcText.Replace('\n', ' ').Replace('\r', ' ').Trim(' ');
		}
	}
}
