using System;
using System.Collections.Generic;
using System.Text;

namespace MyMiscHelpers
{
	/// <summary>
	/// ネイティブ DLL のロード元ディレクトリを追加するユーティリティ クラス。
	/// </summary>
	public static class DllSearcher
	{
		[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
		static extern bool SetDllDirectory(string lpPathName);
		// SetDllDirectory() は Windows XP SP1 以降で利用可能な Win32 API。

		/// <summary>
		/// ネイティブ DLL のロード元ディレクトリを追加します。
		/// </summary>
		/// <param name="path">ロード元ディレクトリ。</param>
		public static void AddPath(string path)
		{
			SetDllDirectory(path);
		}

		/// <summary>
		/// プラットフォームに合わせて x86 と x64 のネイティブ DLL のロード元ディレクトリを片方だけ追加します。
		/// </summary>
		/// <param name="x86path">x86 用 DLL ディレクトリ。</param>
		/// <param name="x64path">x64 用 DLL ディレクトリ。</param>
		/// <remarks>ARM には非対応。</remarks>
		public static void AddPath(string x86path, string x64path)
		{
			int bitWidth = IntPtr.Size * 8;
			switch (bitWidth)
			{
				case 64:
					AddPath(x64path);
					break;
				case 32:
					AddPath(x86path);
					break;
				default:
					throw new System.NotSupportedException();
			}
		}
	}
}
