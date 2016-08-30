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
	public class MyProgressWindowBase : Window
	{
		bool _isEnforcedClosing = false;

		public MyProgressWindowBase()
		{
			Initialize();
		}

		private void Initialize()
		{
			this.ProgressViewModel = new MyProgressViewModel();

			this.DataContext = this.ProgressViewModel;

			this.MouseLeftButtonDown += (s, e) => { this.DragMove(); };

			this.SourceInitialized += ProgressDialog_SourceInitialized;
			this.Closing += ProgressDialog_Closing;
		}

#if false
		/// <summary>
		/// ファクトリ メソッド。
		/// </summary>
		/// <returns></returns>
		public static MyProgressWindowBase Create<T>()
			where T : MyProgressWindowBase, new()
		{
			var temp = new T();
			temp.Initialize();
			return temp;
		}
#endif

		// Loaded イベントに非同期メソッドを直接バインドして、
		// ダイアログが画面表示される前にタスクが始まってしまうと ShowDialog() 内部で NullReferenceException が発生しておかしなことになることがある。
		// ContentRendered を使う回避方法がある。
		// http://qiita.com/hugo-sb/items/79e85baa1e99eec840d8

#if false
		async void ProgressDialog_Loaded(object sender, RoutedEventArgs e)
		{
			// タスク待機ダイアログの MainWorkStarted は、同期メソッドのデリゲートがバインドされていることを想定している。
			// 非同期メソッドを実行する場合、Task.Wait() で同期的に実行する。
			// HACK: 非同期メソッドのデリゲートを直接バインドすると実行時に例外が発生して、ダイアログの表示に失敗する。要改良。

			// マネージ サブスレッド上で、指定した処理を非同期に開始する。
			// 処理が正常終了した場合は true、失敗した場合は false、ユーザーによって中断された場合は null。
			this.DialogResult = await Task.Run(MainWorkStarted);
			// タスクが終了すれば、そのままウィンドウを閉じる。
			this.EnforcedClose();
		}
#endif

		void ProgressDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!this._isEnforcedClosing)
			{
				e.Cancel = true;
			}
		}

		void ProgressDialog_SourceInitialized(object sender, EventArgs e)
		{
			// 最小化以外は禁止しておく。
			// 実は WindowStyle を None にして、ShowInTaskbar を False にしてタイトル バーを消しても、
			// Alt+Space でシステム メニューを表示できてしまう。
			// ちなみにモーダル ウィンドウを表示しているアプリは最小化できない。モードレスであれば最小化可能。
			IntPtr hwnd = this.GetWindowHandle();
			System.Diagnostics.Debug.Assert(hwnd != IntPtr.Zero);
			MyMiscHelpers.MyWin32InteropHelper.DisableMaximizeButton(hwnd);
			MyMiscHelpers.MyWin32InteropHelper.DisableWindowResizing(hwnd);
			MyMiscHelpers.MyWin32InteropHelper.DisableWindowSystemMenu(hwnd);
			//MyMiscHelpers.MyWin32InteropHelper.AppendWindowStyleCaption(hwnd);
		}

		public MyProgressViewModel ProgressViewModel { get; private set; }

		// System.Windows.Window.DialogResult は設定したタイミングでウィンドウを閉じようとしたりする挙動をして使いづらいので、新たにユーザー定義する。
		// 値の設定はユーザーコードで行なう。ラムダ式のキャプチャを使えば別にダイアログ オブジェクトにフィールドやプロパティを用意する必要はないし、
		// また bool 型に制約する必要もないが、利便性のためにユーティリティとして用意しておく。利用するか否かは任意。
		public bool? UserDialogResult { get; set; }

		/// <summary>
		/// タスク開始に使う ContentRendered が一度でも発生したかどうか。こちらも制御はユーザーコードで行なう。
		/// </summary>
		public bool OnceContentRendered { get; set; }

		//public event Func<bool?> MainWorkStarted;

		private IntPtr GetWindowHandle()
		{
			return new System.Windows.Interop.WindowInteropHelper(this).Handle;
		}

		public void EnforcedClose()
		{
			this._isEnforcedClosing = true;
			this.Close();
		}

		public bool IsStopButtonVisible
		{
			get
			{
				return this.ProgressViewModel.StopButtonVisibility == Visibility.Visible;
			}
			set
			{
				this.ProgressViewModel.StopButtonVisibility = value ? Visibility.Visible : Visibility.Hidden;
			}
		}
	}


	public class MyProgressViewModel : MyWpfHelpers.MyNotifyPropertyChangedBase
	{
		public MyProgressViewModel()
		{
			this.StopCommand = new MyWpfHelpers.DelegateCommand();
		}

		/// <summary>
		/// 停止（中断）コマンド。実際のイベントの割り付けはユーザーコード側で行なう。
		/// </summary>
		public MyWpfHelpers.DelegateCommand StopCommand { get; private set; }

		string _description;
		Visibility _stopButtonVisibility = Visibility.Visible;
		double _progressValue = 0;
		bool _isIndeterminate = true;

		public string Description
		{
			get { return this._description; }
			set { base.SetSingleProperty(ref this._description, value); }
		}

		// 長時間かかるが途中で中断のできない処理を待機させる場合、
		// 単に停止ボタンを使えるかどうかをコマンドの CanExecute で制御するだけでもよいが、
		// UX を考えると常に使えないコントロールは最初から表示しないほうがいい。

		public Visibility StopButtonVisibility
		{
			get { return this._stopButtonVisibility; }
			set { base.SetSingleProperty(ref this._stopButtonVisibility, value); }
		}

		/// <summary>
		/// 0~100 の範囲。
		/// </summary>
		public double ProgressValue
		{
			get { return this._progressValue; }
			set { base.SetSingleProperty(ref this._progressValue, value); }
		}

		public bool IsIndeterminate
		{
			get { return this._isIndeterminate; }
			set { base.SetSingleProperty(ref this._isIndeterminate, value); }
		}
	}
}
