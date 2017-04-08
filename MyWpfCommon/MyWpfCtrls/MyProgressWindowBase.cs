﻿using System;
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


	public class MyProgressViewModel : MyBindingHelpers.MyNotifyPropertyChangedBase
	{
		public MyProgressViewModel()
		{
			this.StopCommand = new MyBindingHelpers.MyDelegateCommand();
		}

		/// <summary>
		/// 停止（中断）コマンド。実際のイベントの割り付けはユーザーコード側で行なう。
		/// </summary>
		public MyBindingHelpers.MyDelegateCommand StopCommand { get; private set; }

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
		/// ProgressBar にバインドするときは、0~100 の範囲。TaskbarItemInfo にバインドするときは、0.0~1.0 の範囲とする。
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

	public class MyTaskbarProgressViewModel : MyProgressViewModel
	{
		// Windows 7 以降の OS で、なおかつ Aero テーマ使用時に利用可能なタスク バーへのプログレス表示を想定。
		// Windows 8 以降はクラシック テーマが廃止されているので、プログレス表示は確実になされることが保証されるが、
		// Windows 10 Preview Build 10074 ではアプリを最小化したりして非アクティブになっているときのみプログレスが表示されるようになっている。
		// この仕様だと、たとえばユーザーがアプリ A でメイン作業をしている間にアプリ B にてバックグラウンドで実行されているタスク進捗率をちら見で確認する、
		// というようなおまけ用途程度にしか使えない。
		// Windows 10 Preview Build 10130 では、アクティブ・非アクティブにかかわらずタスク バーにプログレスが表示されるようになっているが、
		// これまでは左から右へ進行していたプログレスが、下から上へのプログレスになっている。
		// また、これまで Normal のプログレス色は緑だったが、Win10 では白色になっている。
		// いずれもこれまでと比べて直感性や視認性に欠ける気がする。
		// 正直残念すぎる仕様変更だが、タスク バーでの進捗率表示機能は、メインとしては使わないほうがいいかもしれない。
		// 必ずアプリのウィンドウ UI 上に従来通りのプログレス バーを明示的に設けておいたほうがいい。
		// WPF だとバインディングを使えばいいので連動は比較的簡単だが……
		// → 最終的に Windows 10 正式版ではこれまで同様のまともな仕様になった模様。

		System.Windows.Shell.TaskbarItemProgressState _progressState = System.Windows.Shell.TaskbarItemProgressState.None;

		public System.Windows.Shell.TaskbarItemProgressState ProgressState
		{
			get { return this._progressState; }
			set { base.SetSingleProperty(ref this._progressState, value); }
		}
	}
}
