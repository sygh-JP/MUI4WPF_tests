using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMiscHelpers
{
	public static class MyDotNetHelper
	{
		public static Process GetPreviousSameProcess()
		{
			var curProcess = Process.GetCurrentProcess();
			IEnumerable<Process> allProcesses = Process.GetProcessesByName(curProcess.ProcessName);

			foreach (Process checkProcess in allProcesses)
			{
				// 自分自身のプロセス ID は無視する。
				if (checkProcess.Id != curProcess.Id)
				{
					string prev = checkProcess.MainModule.FileName;
					string cur = curProcess.MainModule.FileName;
					if (String.Compare(prev, cur, true) == 0)
					{
						// 起動済みで同じフルパス名のプロセスを取得。
						return checkProcess;
					}
				}
			}
			return null;
		}

		public static System.Diagnostics.Process OpenByExplorer(string path)
		{
			return System.Diagnostics.Process.Start("explorer.exe", @"/select," + path);
		}

		/// <summary>
		/// ファイルのある場所を Windows エクスプローラーで開く。
		/// </summary>
		/// <param name="filePath"></param>
		/// <exception cref="System.IO.FileNotFoundException"></exception>
		public static void OpenFileDirectoryByExplorer(string filePath)
		{
			// 一応文字列フォーマット チェックも兼ねて、事前確認は行なう。
			if (System.IO.File.Exists(filePath))
			{
				OpenByExplorer(filePath);
			}
			else
			{
				throw new System.IO.FileNotFoundException();
			}
		}
	}


	public static class MyConfigHelper
	{
		public static System.Configuration.Configuration GetUserConfig()
		{
			// System.Windows.Forms.Application.UserAppDataPath プロパティ相当のはず。
			return System.Configuration.ConfigurationManager.OpenExeConfiguration(
				System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal);
		}

		public static string GetUserConfigFilePath()
		{
			return GetUserConfig().FilePath;
		}

		public static string GetUserConfigDirPath()
		{
			return System.IO.Path.GetDirectoryName(GetUserConfigFilePath());
		}
	}


	/// <summary>
	/// bool? に代わる簡易3値論理型。enum なので bool? と違い volatile 指定できる。
	/// </summary>
	public enum ThreeState
	{
		Invalid = -1,
		False = 0,
		True = 1,
	}

	public static class MyTypeHelper
	{
		public static bool? ToNullableBoolean(ThreeState val)
		{
			switch (val)
			{
				case ThreeState.True:
					return true;
				case ThreeState.False:
					return false;
				default:
					return null;
			}
		}

		public static ThreeState ToThreeState(bool val) { return val ? ThreeState.True : ThreeState.False; }
		public static ThreeState ToThreeState(bool? val) { return val.HasValue ? ToThreeState(val.Value) : ThreeState.Invalid; }
	}


	public abstract class MyDisposableBase : IDisposable
	{
		private bool isDisposed = false;

		~MyDisposableBase()
		{
			// リソースの解放。
			// マネージ オブジェクトは GC で寿命管理されているため、
			// C# のデストラクタはタイミング不定どころか終了時にすら呼ばれない可能性があることに注意。
			// デストラクタを記述する意味は、
			// 「Dispose() がもし呼ばれなかった場合、最終防衛ラインとして GC 回収されるタイミングでリソースを破棄します」という保証にしかならない。
			this.OnDispose(false);
		}

		protected abstract void OnDisposeManagedResources();

		protected abstract void OnDisposeUnmamagedResources();

		private void OnDispose(bool disposesManagedResources)
		{
			// よくある IDisposable 実装のサンプルでは、
			// protected virtual void Dispose(bool dispose) と宣言されている。
			// protected virtual な仮想メソッドを定義しているのは、
			// 派生クラスでもリソースを追加管理するようなときに備えるためらしい。
			// 派生クラスでオーバーライドする際には、base 経由で基底クラスの Dispose(bool) をきちんと呼び出すようにすればよい。

			lock (this)
			{
				if (this.isDisposed)
				{
					// 既に呼びだし済みであるならば何もしない。
					return;
				}

				if (disposesManagedResources)
				{
					// TODO: IDisposable 実装クラスなどのマネージ リソースの解放はココで行なう。
					this.OnDisposeManagedResources();
				}

				// TODO: IntPtr ハンドルなどのアンマネージ リソースの解放はココで行なう。
				this.OnDisposeUnmamagedResources();

				this.isDisposed = true;
			}
		}

		protected void ThrowExceptionIfDisposed()
		{
			if (this.isDisposed)
			{
				throw new ObjectDisposedException(this.GetType().ToString());
			}
		}

		/// <summary>
		/// IDisposable.Dispose() の実装。
		/// </summary>
		public void Dispose()
		{
			// リソースの解放。
			this.OnDispose(true);

			// このオブジェクトのデストラクタを GC 対象外とする。
			GC.SuppressFinalize(this);
		}
	}


	/// <summary>
	/// 多重起動を防止するためのミューテックス ラッパー。
	/// </summary>
	/// <remarks>
	/// using ステートメント内で使用するようなものではないので、IDisposable は実装していない。
	/// </remarks>
	public class MyMultibootPreventer
	{
		// 多重起動チェックに使う同期オブジェクト。
		private System.Threading.Mutex multibootPreventMutex = null;

		public bool TryAcquire(string mutexName)
		{
			this.multibootPreventMutex = new System.Threading.Mutex(false, mutexName);
			// ミューテックスの所有権を要求。
			if (!this.multibootPreventMutex.WaitOne(0, false))
			{
				// すでに起動していると判断する。必要に応じて呼び出し元プロセスを終了したりすること。
				this.multibootPreventMutex.Close();
				this.multibootPreventMutex = null;
				return false;
			}
			return true;
		}

		public void Release()
		{
			if (this.multibootPreventMutex != null)
			{
				this.multibootPreventMutex.ReleaseMutex();
				this.multibootPreventMutex.Close();
				this.multibootPreventMutex = null;
			}
		}
	}

	public static class MyAsyncHelper
	{
		public static async Task DoWrappedHeavyProcAsync(Action heavyProc, bool writesDebugMessage = false)
		{
			// ヘビーな処理はサブスレッドに逃がす。サブスレッドで発生した例外はタスクの戻り値を使って、スレッド境界を超えて伝播させる。
			// async/await を使えばサブスレッドからスローされた例外も間接的にキャッチできるが、
			// もしサブスレッドで直接ハンドルせずにフレームワークに例外の伝播を任せると、
			// デバッグ実行時にハンドルされない例外に対してブレークする設定になっていた場合に不都合。
			var resultEx = await Task.Run(() =>
			{
				try
				{
					heavyProc();
				}
				catch (Exception innerEx)
				{
					if (writesDebugMessage)
					{
						System.Diagnostics.Debug.WriteLine(innerEx.ToString());
					}
					return innerEx;
				}
				return null;
			});
			if (resultEx != null)
			{
				// 呼び出しスレッドで改めて再スロー。
				throw resultEx;
			}
		}
	}
}
