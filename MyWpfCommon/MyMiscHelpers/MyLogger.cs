using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

// TODO: ログ ファイルのパスや診断レベルも同時に管理するが、必ずスレッドセーフになるようにすること。

namespace MyLogHelpers
{
	#region Circular Logger

	/// <summary>
	/// 循環ログ管理の抽象基底クラス。
	/// </summary>
	public abstract class MyCircularLoggerBase : MyMiscHelpers.MyDisposableBase
	{
		protected override sealed void OnDisposeManagedResources()
		{
			// マネージ リソースの解放。
			this.SafeDisposeFileWriter();
		}

		protected override sealed void OnDisposeUnmamagedResources()
		{
			// 空の実装。
		}

		private DiagnosticLevel diagnosticLevel = DiagnosticLevel.Plain;

		private object lockObject = new Object();

		private System.IO.StreamWriter fileWriter = null;

		public string LogFileDirPath { get; private set; }
		public string LogFileBodyBase { get; private set; }
		public string LogFileExtension { get; private set; }
		public bool IsInitialized { get; private set; }

		/// <summary>
		/// 循環ログのためのパス情報などを初期化する。ただしファイルは作成されない。
		/// </summary>
		/// <param name="strLogFileDirPath">ログ ファイルを保存するディレクトリ パス。すでに存在していることが前提条件。</param>
		/// <param name="strLogFileBodyBase">ログ ファイルのファイル ボディ。英数字とアンダースコアのみ。末尾に日付や連番が付加される。</param>
		/// <param name="strLogFileExtension">ログ ファイルの拡張子。英数字のみで、また先頭のピリオド "." を含む。</param>
		/// <returns>引数が条件を満たし、初期化が成功したか否か。</returns>
		public bool Initialize(string strLogFileDirPath, string strLogFileBodyBase, string strLogFileExtension)
		{
			if (!Directory.Exists(strLogFileDirPath))
			{
				System.Diagnostics.Debug.Assert(false);
				return false;
			}
			// ボディは英数字とアンダースコアのみを許可する。
			var bodyCheckRegex = new Regex(@"^[A-Z\d_]+$", RegexOptions.IgnoreCase);
			//if (strLogFileBodyBase == String.Empty)
			if (!bodyCheckRegex.IsMatch(strLogFileBodyBase))
			{
				System.Diagnostics.Debug.Assert(false);
				return false;
			}
			// 拡張子にはアンダースコアなども使えるが、今回は許可しない。英数字のみ。
			var extCheckRegex = new Regex(@"^\.[A-Z\d]+$", RegexOptions.IgnoreCase);
			//if (!strLogFileExtension.StartsWith("."))
			if (!extCheckRegex.IsMatch(strLogFileExtension))
			{
				System.Diagnostics.Debug.Assert(false);
				return false;
			}
			// TODO: ベース ファイル名に連番を付けて真のファイル名を作成する。
			// すでに連番ファイルが存在する場合、更新日時が一番新しいファイルを使う。
			// 最新のファイルが規定サイズを超えている場合、次の連番ファイルを使う。
			// 次の連番ファイルがさらに規定サイズを超えている場合、その内容をクリアして使う（新規作成モードで開く）。
			// 連番が規定数に到達したら、先頭に戻る。
			// 規定サイズを超えているかどうかのチェックは、書き込み前に行なう。
			// ファイルを開いていない場合に書き込もうとしたときに Open する。
			// 起動中は明示的に Open/Close はせず、Flush のみを行なう。
			lock (this.lockObject)
			{
				MyMiscHelpers.MyGenericsHelper.SafeDispose(ref this.fileWriter);
				{
					this.LogFileDirPath = strLogFileDirPath;
					this.LogFileBodyBase = strLogFileBodyBase;
					this.LogFileExtension = strLogFileExtension;
					this.OnInitializePathInfo(); // カスタマイズ ポイント。
					IsInitialized = true;
					// ファイル自体はまだ開かない。
				}
			}
			return true;
		}

		/// <summary>
		/// 別スレッドでの書き込み中は Dispose() されないラッパー。
		/// </summary>
		/// <returns></returns>
		public bool SafeClose()
		{
			try
			{
				lock (this.lockObject)
				{
					this.SafeDisposeFileWriter();
					return true;
				}
			}
			catch (Exception err)
			{
				MyDebugLogger.DebugWriteException(err);
			}
			return false;
		}

		protected void SafeDisposeFileWriter()
		{
			MyMiscHelpers.MyGenericsHelper.SafeDispose(ref this.fileWriter);
		}

		protected void CreateFileWriterIfNull(string strLogFileFullPath, bool isAppendMode)
		{
			if (this.fileWriter == null)
			{
				this.fileWriter = MyLogHelper.CreateBomUtf8TextStreamWriter(strLogFileFullPath, isAppendMode);
			}
		}

		private bool WriteLineToLogFileAndFlushImpl(Func<string> createTextLine)
		{
			try
			{
				lock (this.lockObject)
				{
					if (!this.IsInitialized)
					{
						return false;
					}
					this.OnUpdateFileWriter(); // カスタマイズ ポイント。
					// StreamWriter の作成に失敗していれば、前段で例外がスローされているはず。
					System.Diagnostics.Debug.Assert(this.fileWriter != null);
					this.fileWriter.WriteLine(createTextLine());
					this.fileWriter.Flush();
					return true;
				}
			}
			catch (Exception err)
			{
				MyDebugLogger.DebugWriteException(err);
			}
			return false;
		}

		private bool WriteLineToLogFileAndFlush(string message, LogContentType type)
		{
			return WriteLineToLogFileAndFlushImpl(() =>
			{
				// 時刻を記入する。世界標準時ではなく現地時刻。
				// 時刻とメッセージの間にはタブ文字 0x09 を入れる TSV 形式。
				return String.Format("[{0}]\t[{1}]\t{2}",
					MyLogHelper.GetCurrentLocalDateTimeByIsoFormat(), MyLogHelper.GetLogContentTypeCode(type), message);
			});
		}

		private bool WriteLineToLogFileAndFlush(string message)
		{
			return WriteLineToLogFileAndFlushImpl(() =>
			{
				return String.Format("[{0}]\t{1}",
					MyLogHelper.GetCurrentLocalDateTimeByIsoFormat(), message);
			});
		}

		/// <summary>
		/// 古い期限切れログ ファイルの削除。
		/// </summary>
		/// <returns></returns>
		public bool DeleteExpiredLogFiles()
		{
			if (!this.IsInitialized)
			{
				System.Diagnostics.Debug.Assert(false);
				return false;
			}
			try
			{
				// 削除中はログ書き込み処理その他を念のためブロックしておく。
				lock (this.lockObject)
				{
					OnDeleteExpiredLogFiles(); // カスタマイズ ポイント。
					return true;
				}
			}
			catch (Exception err)
			{
				MyDebugLogger.DebugWriteException(err);
			}
			return false;
		}

		// 可変個引数と Caller Info の相性は悪いが、そもそも Debug.WriteLine() のオーバーロードのうち、
		// 可変個引数を受け取るもの WriteLine(string format, params object[] args) は第2引数に string 単体を渡して暗黙変換しようとすると
		// 意図せずに WriteLine(object value, string category) として解決されてしまうので、
		// String.Format() の結果を明示的に渡すようにしたほうがいいかも。
		public void WriteLine(string message, LogContentType type = LogContentType.Information, [System.Runtime.CompilerServices.CallerMemberName] string member = "")
		{
			try
			{
				string callerMessage = String.Format("CallerMemberName={0}; {1}", member, message);
				System.Diagnostics.Debug.WriteLine(callerMessage);
				this.WriteLineToLogFileAndFlush(callerMessage, type);
			}
			catch (Exception) { }
			// ログ出力は catch ブロック内で使用されることもありえるので、失敗しても絶対に例外を外部にスローしないようにしておく。
		}

		public async Task WriteLineAsync(string message, LogContentType type = LogContentType.Information, [System.Runtime.CompilerServices.CallerMemberName] string member = "")
		{
			await Task.Run(() =>
			{
				this.WriteLine(message, type, member);
			});
		}

		public void WriteSimpleLine(string message)
		{
			try
			{
				this.WriteLineToLogFileAndFlush(message);
			}
			catch (Exception) { }
		}

		public async Task WriteSimpleLineAsync(string message)
		{
			await Task.Run(() =>
			{
				this.WriteSimpleLine(message);
			});
		}

		// .NET 例外はスタック トレース情報を持てるので、Message プロパティよりも ToString() を使って文字列化したほうがいいかも
		// （スタック アンワインドのレベルに応じてデータ量が増えるが）。
		// HACK: 通常時は最低限の Message のみだが、アプリ設定で診断レベルを変更すると、詳細なスタック トレース情報をログに残せるようにできるとよい。
		public void WriteException(Exception exception, [System.Runtime.CompilerServices.CallerMemberName] string member = "")
		{
			try
			{
				string callerMessage = String.Format("CallerMemberName={0}, Exception={1}; {2}",
					member,
					exception.GetType().ToString(),
					(this.diagnosticLevel == DiagnosticLevel.Detail) ? exception.ToString() : exception.Message);
				System.Diagnostics.Debug.WriteLine(callerMessage);
				this.WriteLineToLogFileAndFlush(callerMessage, LogContentType.Error);
				// InnerException があれば、再帰で探索。
				if (exception.InnerException != null)
				{
					WriteException(exception.InnerException, member);
				}
			}
			catch (Exception) { }
		}

		protected abstract void OnInitializePathInfo();
		protected abstract void OnUpdateFileWriter();
		protected abstract void OnDeleteExpiredLogFiles();

		//protected abstract FileInfo CurrentLogFileInfo { get; }
		//protected abstract string CurrentLogFileFullPath { get; }
	}

	/// <summary>
	/// アプリケーション ログを管理するクラス。ログ ファイルはサイズ制限を持たない。指定した期日分（非連続でも可）のファイルが、リング バッファ的に管理される（有限個数）。
	/// </summary>
	public class MyDayBaseCircularLogger : MyCircularLoggerBase
	{
		public const uint LogDaysDefault = 7; // とりあえず1週間分とする。
		public const uint LogDaysLowerLimit = 5;
		public const uint LogDaysUpperLimit = 60; // とりあえず2ヶ月を上限とする。

		public static bool IsLogDaysInRange(uint logDays)
		{ return LogDaysLowerLimit <= logDays && logDays <= LogDaysUpperLimit; }

		readonly uint CurrentMaxLogDays = LogDaysDefault;

		string lastLogFileFullPath = null;
		DispatcherTimer dispatcherTimer;
		bool isExecutingTimerTasks = false;

		/// <summary>
		/// コンストラクタ。ログ管理期間などの定数値のみを初期化する。
		/// </summary>
		/// <param name="maxLogDays">もし異常値を設定しようとした場合は例外を投げず、強制クランプする。</param>
		public MyDayBaseCircularLogger(uint maxLogDays = LogDaysDefault)
		{
#if false
			if (!IsLogDaysInRange(maxLogDays))
			{
				throw new ArgumentOutOfRangeException("The maxLogDays argument is out of range!!");
			}
#endif
			this.CurrentMaxLogDays = MyMiscHelpers.MyGenericsHelper.Clamp<uint>(maxLogDays, LogDaysLowerLimit, LogDaysUpperLimit);
		}

		string GetLogFileFullPath(DateTime dateTime)
		{
			return System.IO.Path.Combine(this.LogFileDirPath,
				String.Format("{0}-{1}{2}", this.LogFileBodyBase, dateTime.ToString("yyyy-MM-dd"), this.LogFileExtension));
		}

		protected override void OnInitializePathInfo()
		{
			// 何もしない。
		}

		protected override void OnUpdateFileWriter()
		{
			var newPath = this.GetLogFileFullPath(DateTime.Now);
			if (newPath != this.lastLogFileFullPath)
			{
				// 日付が変わっているので、開いているファイルは閉じる。
				this.SafeDisposeFileWriter();
			}
			this.CreateFileWriterIfNull(newPath, true);
			this.lastLogFileFullPath = newPath;
		}

		protected override void OnDeleteExpiredLogFiles()
		{
			// 期限はファイル名を基準にチェックする。ファイルのタイムスタンプは使わない（日付が変わるタイミングがクリティカル セクションになるため）。
			// また、稼働中はずっとアプリが起動したままの状態になって、終了されることがないような用途にも備える必要がある。
			// TODO: 書き込み時にも毎回古い期限切れファイルのチェックを行なうとパフォーマンスに悪影響を与える可能性があるので、
			// 期限切れログの削除は起動直後に必ず1回、およびアプリのアイドル時に明示的に行なう。1日1回行なえばよいはず。24時間周期のタイマーを作る？

			// まず一定フォーマットの名前が付けられたログ ファイルを列挙する。
			// 規定数以内の新しいものを残して、それら以外は削除する。
			// 未来の日付の名前が付けられたファイルが意図的に作成されている場合は、そちらが優先されるので注意。
			// 一応西暦 10000 年以降にも対応できているはず。
			string strSearchPattern = this.LogFileBodyBase + "-*-??-??" + this.LogFileExtension; // * や ? が数字か否かのチェックはできない。
			var dayRegex = new Regex(@"-(\d{4,})-(\d\d)-(\d\d)$");

			var searchedFiles = System.IO.Directory.GetFiles(this.LogFileDirPath, strSearchPattern, System.IO.SearchOption.TopDirectoryOnly);
			// 最新のファイルを先頭に並べ替える。
			// ファイル ボディの書式が完全にマッチするものを対象とする。
			var comparer = new MyMiscHelpers.MyLogicalAscendingStringComparer();
			var sortedFiles =
				from path in searchedFiles.OrderByDescending(x => x, comparer)
				//orderby path descending // --> NG。機械的な序数順になってしまい、例えば 9999 年と 10000 年の大小比較が正しく行なわれない。
				where dayRegex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(path))
				select path;
			uint fileCounter = 0;
			// 1つでも削除に失敗したらエラーとみなし、処理を続行しない。
			foreach (string path in sortedFiles)
			{
#if false
				var matchObj = dayRegex.Match(System.IO.Path.GetFileNameWithoutExtension(path));
				if (matchObj.Groups.Count == 4)
#endif
				{
#if false
					// DateTime.Parse() は使わない（一応 "yyyy-MM-dd" の形式も解析できる模様）。
					var fileNameDateTime = new DateTime(
						Int32.Parse(matchObj.Groups[1].Value),
						Int32.Parse(matchObj.Groups[2].Value),
						Int32.Parse(matchObj.Groups[3].Value));
#endif
					++fileCounter;
					if (fileCounter > this.CurrentMaxLogDays)
					{
						// ファイル削除。
						System.IO.File.Delete(path);
					}
				}
			}
		}

		public void StartToMonitor()
		{
			// 1日置きの更新だと、24時間ずっと稼働していないといけない。
			// HACK: とりあえず1時間ごとにチェックするようにしているが、
			// 「毎日 X 時にチェックする」ような形のほうがよい。
			this.dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal);
			this.dispatcherTimer.Interval = new TimeSpan(1, 0, 0);
			this.dispatcherTimer.Tick += this.dispatcherTimer_Tick;
			this.dispatcherTimer.Start();
		}

		public void StopToMonitor()
		{
			if (this.dispatcherTimer != null)
			{
				this.dispatcherTimer.Stop();
				this.dispatcherTimer.Tick -= this.dispatcherTimer_Tick;
				this.dispatcherTimer = null;
			}
		}

		async void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			// ごく頻繁に呼ばれるわけではないので、再入防止はまず不要だが、念のため（時間を短縮してテストするときにも有用）。
			// また、UI スレッドをロックしないように、サブスレッドに削除処理をさせる。
			if (this.isExecutingTimerTasks)
			{
				return;
			}

			try
			{
				this.isExecutingTimerTasks = true;
				await Task.Run(() => { this.DeleteExpiredLogFiles(); });
			}
			catch { }
			finally
			{
				this.isExecutingTimerTasks = false;
			}
		}
	}

	/// <summary>
	/// アプリケーション ログを管理するクラス。ログ ファイルはサイズ制限を持つ。指定した個数分のファイルが、リング バッファ的に管理される（有限個数）。
	/// </summary>
	public class MySizeBaseCircularLogger : MyCircularLoggerBase
	{
		// HACK: ログの最大数および最大サイズは const ではなく readonly にして、コンストラクタで設定できるようにする。もし異常値を設定しようとした場合は例外を投げるだけでよい。

		private const uint MaxLogFileCount = 5; // HACK: [1～100] の範囲で設定可能にする？　テストがダルいので [1～10] にする？
		private const long MaxLogFileSizeInBytes = 1 * 1024; // HACK: 実際には 1MB くらいが妥当。

		private string[] logFilePathCandidates = new string[MaxLogFileCount];
		private FileInfo[] logFileInfos = new FileInfo[MaxLogFileCount];
		private uint currentSerialNumber = 0;

		private void IncrementSerialNumber()
		{
			this.currentSerialNumber = (this.currentSerialNumber + 1) % MaxLogFileCount;
		}

		private string CurrentLogFileFullPath
		{
			get
			{
				System.Diagnostics.Debug.Assert(this.logFilePathCandidates != null && this.logFilePathCandidates.Count() > this.currentSerialNumber);
				return this.logFilePathCandidates[this.currentSerialNumber];
			}
		}

		private FileInfo CurrentLogFileInfo
		{
			get
			{
				System.Diagnostics.Debug.Assert(this.logFileInfos != null && this.logFileInfos.Count() > this.currentSerialNumber
					&& this.logFileInfos[this.currentSerialNumber] != null);
				return this.logFileInfos[this.currentSerialNumber];
			}
		}

		protected override void OnInitializePathInfo()
		{
			DateTime? latestFileDate = null;
			uint latestIndex = 0;
			for (uint i = 0; i < MaxLogFileCount; ++i)
			{
				string filePath = System.IO.Path.Combine(this.LogFileDirPath,
					String.Format("{0}-{1:00}{2}", this.LogFileBodyBase, i, this.LogFileExtension));
				this.logFilePathCandidates[i] = filePath;
				var fileInfo = new FileInfo(filePath);
				fileInfo.Refresh();
				this.logFileInfos[i] = fileInfo;
				if (fileInfo.Exists)
				{
					// 時刻を比較する。世界標準時ではなく現地時刻。
					var fileDate = fileInfo.LastWriteTime;
					if (latestFileDate == null || fileDate > latestFileDate)
					{
						latestFileDate = fileDate;
						latestIndex = i;
					}
				}
			}
			// 最新のファイルを使う。
			this.currentSerialNumber = latestIndex;
		}

		protected override void OnUpdateFileWriter()
		{
			// ファイルが存在しない場合、FileInfo.Length プロパティは FileNotFoundException 例外を投げるので注意。

			bool isAppendMode = true;
			this.CurrentLogFileInfo.Refresh(); // キャッシュされた情報をクリアする。
			if (this.CurrentLogFileInfo.Exists && this.CurrentLogFileInfo.Length >= MaxLogFileSizeInBytes)
			{
				// 次の連番ファイルを使う。
				this.IncrementSerialNumber();
				this.CurrentLogFileInfo.Refresh(); // キャッシュされた情報をクリアする。
				if (this.CurrentLogFileInfo.Exists && this.CurrentLogFileInfo.Length >= MaxLogFileSizeInBytes)
				{
					// 新規作成する。
					isAppendMode = false;
				}
				// 開いているファイルは閉じておく。
				this.SafeDisposeFileWriter();
			}
			this.CreateFileWriterIfNull(this.CurrentLogFileFullPath, isAppendMode);
		}

		protected override void OnDeleteExpiredLogFiles()
		{
			// 何もしない。
		}
	}

	#endregion

	public enum LogContentType
	{
		Information,
		Warning,
		Error,
	}

	public enum DiagnosticLevel
	{
		Plain,
		Detail,
	}

	/// <summary>
	/// すべてのログ クラスの共通ヘルパーメソッドなどを定義するクラス。
	/// </summary>
	public static class MyLogHelper
	{
		public static char GetLogContentTypeCode(LogContentType type)
		{
			switch (type)
			{
				case LogContentType.Error:
					return 'E';
				case LogContentType.Warning:
					return 'W';
				default:
					return 'I';
			}
		}

		// UTF-8 の BOM を付ける場合、System.IO.File.CreateText() の代わりに使う。
		public static System.IO.StreamWriter CreateBomUtf8TextStreamWriter(string strFileName, bool isAppendMode = false)
		{
			// すでに別アプリ（テキスト エディタなど）において読み書きモードでログ ファイルを開いているときでも、ちゃんと書き込みモードで開ける。
			// FileStream と FileShare を明示的に使う必要はなさそう。
			//return new System.IO.StreamWriter(strFileName, isAppendMode, new System.Text.UTF8Encoding()); // NG。BOM は付かない。
			return new System.IO.StreamWriter(strFileName, isAppendMode, Encoding.UTF8); // OK。BOM が付く。
		}

		/// <summary>
		/// ISO 8601 の表記法に従った現地時刻文字列を返す。
		/// </summary>
		/// <returns></returns>
		public static string GetCurrentLocalDateTimeByIsoFormat(bool usesWhitespace = false)
		{
			return DateTime.Now.ToString(usesWhitespace
				? "yyyy-MM-dd HH:mm:ss.fffzzz"
				: "yyyy-MM-ddTHH:mm:ss.fffzzz"
				);
		}

		public static string GetEnvironmentInfoSummary()
		{
			return String.Format("EnvInfo: MachineName='{0}', ProcessorCount={1}, OSVersion='{2}', Is64bitOS={3}, DotNETFXVersion='{4}', UserName='{5}'",
				Environment.MachineName, Environment.ProcessorCount, Environment.OSVersion, Environment.Is64BitOperatingSystem, Environment.Version, Environment.UserName);
		}
	}


	/// <summary>
	/// デバッグ時のみ有効になるログ ヘルパー クラス。
	/// </summary>
	public static class MyDebugLogger
	{
		[System.Diagnostics.Conditional("DEBUG")]
		public static void DebugWriteLine(string message, [System.Runtime.CompilerServices.CallerMemberName] string member = "")
		{
			System.Diagnostics.Debug.WriteLine("CallerMemberName={0}; {1}", member, message);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void DebugWriteException(Exception exception, [System.Runtime.CompilerServices.CallerMemberName] string member = "")
		{
			System.Diagnostics.Debug.WriteLine("CallerMemberName={0}; {1}", member, exception.Message);
		}

		// Visual Studio IDE の出力ウィンドウに特定フォーマット（"ファイル名(行番号):"）で文字列をデバッグ出力すると、
		// ダブルクリックで該当ファイルの該当行にジャンプすることができる。
		// __FILE__ や __LINE__ が使える Visual C++ ではよく使う機能だが、VC# では Caller Info がないと難しかった。
		// ちなみに VC# では、コンストラクタはコンパイル時に ".ctor" という内部名になる模様。
		// なお、async でマークした非同期メソッドからリフレクション機能 System.Reflection.MethodBase.GetCurrentMethod().Name を呼び出すと、
		// コンパイラが展開した "MoveNext"（おそらく内部で使われるステートマシンの IEnumerator.MoveNext() 呼び出し）に変わってしまうが、
		// Caller Info ではきちんと C# コードからメソッドの名前が拾われる。
		[System.Diagnostics.Conditional("DEBUG")]
		public static void MarkDebugCallPoint(
			[System.Runtime.CompilerServices.CallerFilePath] string file = "",
			[System.Runtime.CompilerServices.CallerLineNumber] int line = 0,
			[System.Runtime.CompilerServices.CallerMemberName] string member = "")
		{
			System.Diagnostics.Debug.WriteLine("{0}({1}):{2}", file, line, member);
		}
	}
}
