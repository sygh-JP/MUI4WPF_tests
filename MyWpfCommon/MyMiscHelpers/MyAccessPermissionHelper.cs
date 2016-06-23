using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// http://intre.net/item_6569.html

namespace MyMiscHelpers
{
	public static class MyAccessPermissionHelper
	{
		// 書き込み権限を持っているか否かを取得する。
		public static bool HasCurrentUserWriteAccessPermission(string path)
		{
			var rule = GetCurrentAccessRule(path);
			return ((rule != null) && ((rule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write));
		}

		// 現在のユーザーが持っている指定パスの FileSystemAccessRule を得る。
		public static FileSystemAccessRule GetCurrentAccessRule(string path)
		{
			var fileSecurity = File.GetAccessControl(path);
			var rules = fileSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier)).OfType<FileSystemAccessRule>();
			var currentIdentity = WindowsIdentity.GetCurrent();
			var sids = new[] { currentIdentity.User }.Concat(currentIdentity.Groups);

			// アクセス ルール内にユーザー SID がある？　無ければグループ SID も探す。
			return rules.FirstOrDefault(rule => sids.Contains(rule.IdentityReference));
		}

		public static void AppendStandardUsersAccountFullControlToDir(string dirPath)
		{
			// 事前にアクセス権の設定対象のフォルダーが作成されていることが前提。
			if (System.IO.Directory.Exists(dirPath) && HasCurrentUserWriteAccessPermission(dirPath))
			{
				using (var proc = new Process())
				{
					// icacls は Vista / Windows Server 2008 以降で cacls の代替として実装されている。
					// XP 以前にはバックポートされてないが、無視。
					// なお、Administrators グループのユーザーでディレクトリを作成すると、
					// 通常は Users グループのユーザーにはアクセス権が付与されない。
					// アクセス権の設定はフォルダー／ファイルの所有者であれば確実に成功するはずなので、まず所有者が自分自身であるかどうかを確認する。
					// 異なればアクセス権の設定はしない。
					// ……が、所有者の確認は System.Security.AccessControl.CommonSecurityDescriptor を使って取得するらしく、かなり面倒そうなので、
					// とりあえず書き込み権限を持っているか否かで判定する。
					// ファイルを作成するときは既定で親フォルダーのアクセス権が継承されるので設定は特に必要ないはず。
					proc.StartInfo.FileName = Environment.GetEnvironmentVariable("ComSpec");
					proc.StartInfo.CreateNoWindow = true;
					proc.StartInfo.UseShellExecute = false;
					proc.StartInfo.RedirectStandardInput = false;
					proc.StartInfo.RedirectStandardOutput = true;
					proc.StartInfo.Arguments = String.Format("/c icacls \"{0}\" /c /grant Users:(OI)(CI)(F)", dirPath);
					proc.Start();
				}
			}
		}
	}
}
