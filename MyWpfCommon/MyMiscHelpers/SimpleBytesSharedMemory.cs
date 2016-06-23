using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMiscHelpers
{
	/// <summary>
	/// バイト配列のみを扱う単純な共有メモリ（メモリ マップト ファイル）。
	/// using ステートメントで使うことはまずないので、今回は IDisposable 実装は省略。
	/// </summary>
	public class SimpleBytesSharedMemory
	{
		const string RWLockMutexNamePostfix = "_Mutex";

		MemoryMappedFile mmf;
		System.Threading.Mutex sharedMemoryRWLockMutex;

		readonly bool isSharedMemoryServer;
		readonly int sharedMemorySizeInBytes;

		public SimpleBytesSharedMemory(bool isServer, string name, int sizeInBytes)
		{
			this.isSharedMemoryServer = isServer;
			this.sharedMemorySizeInBytes = sizeInBytes;
			try
			{
				if (isSharedMemoryServer)
				{
					this.CreateNewSharedMemory(name, sizeInBytes);
				}
				else
				{
					this.OpenExistingSharedMemory(name);
				}
			}
#if false
			catch (FileNotFoundException ex)
			{
				this.mmf = null;
				throw;
			}
#endif
			catch (Exception)
			{
				this.mmf = null;
				throw;
			}

			// 失敗すれば例外が再スローされる。
		}

		private void OpenExistingSharedMemory(string name)
		{
			this.mmf = MemoryMappedFile.OpenExisting(name);
			this.sharedMemoryRWLockMutex = new System.Threading.Mutex(false, name + RWLockMutexNamePostfix);
		}

		private void CreateNewSharedMemory(string name, int sizeInBytes)
		{
			this.mmf = MemoryMappedFile.CreateNew(name, sizeof(byte) * sizeInBytes);
			this.sharedMemoryRWLockMutex = new System.Threading.Mutex(false, name + RWLockMutexNamePostfix);
		}

		public void Close()
		{
			// 共有メモリを作成したアプリが終了すれば、OS によって自動的に解放されるはずだが、
			// 一応明示的に解放するメソッドを書いておいたほうがよい。
			try
			{
				if (this.mmf != null)
				{
					this.mmf.Dispose();
					this.mmf = null;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

			try
			{
				if (this.sharedMemoryRWLockMutex != null)
				{
					// HACK: ReleaseMutex() で ApplicationException 例外が発生する模様。
					// 「オブジェクト同期メソッドは、コードの非同期ブロックから呼び出されました。」というメッセージ。
					try
					{
						this.sharedMemoryRWLockMutex.ReleaseMutex();
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
					}
					this.sharedMemoryRWLockMutex.Close();
					this.sharedMemoryRWLockMutex = null;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		public bool Read(byte[] localBitsDataArray)
		{
			if (this.mmf == null)
			{
				return false;
			}
			Debug.Assert(localBitsDataArray.Count() == this.sharedMemorySizeInBytes);

			try
			{
				this.sharedMemoryRWLockMutex.WaitOne();
				using (var stream = this.mmf.CreateViewStream(0, 0))
				{
					using (var binReader = new BinaryReader(stream))
					{
						for (int i = 0; i < localBitsDataArray.Count(); ++i)
						{
							localBitsDataArray[i] = binReader.ReadByte();
						}
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return false;
			}
			finally
			{
				this.sharedMemoryRWLockMutex.ReleaseMutex();
			}
		}

		public bool Write(byte[] localBitsDataArray)
		{
			if (this.mmf == null)
			{
				return false;
			}
			Debug.Assert(localBitsDataArray.Count() == this.sharedMemorySizeInBytes);

			try
			{
				this.sharedMemoryRWLockMutex.WaitOne();
				using (var stream = this.mmf.CreateViewStream(0, 0))
				{
					using (var binWriter = new BinaryWriter(stream))
					{
						binWriter.Write(localBitsDataArray);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return false;
			}
			finally
			{
				this.sharedMemoryRWLockMutex.ReleaseMutex();
			}
		}
	}
}
