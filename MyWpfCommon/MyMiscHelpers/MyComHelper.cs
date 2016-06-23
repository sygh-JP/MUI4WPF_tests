using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMiscHelpers
{
	/// <summary>
	/// COM 関連のヘルパーメソッドを静的に提供するクラス。
	/// </summary>
	public static class MyComHelper
	{
		/// <summary>
		/// ユーザー定義のカスタム インターフェイスが使用してよいファシリティ。
		/// </summary>
		internal const uint FACILITY_ITF = 4;

		/// <summary>
		/// COM の HRESULT リターンコードを生成する。MAKE_HRESULT マクロ相当。
		/// </summary>
		/// <param name="severity">失敗の場合 true、成功の場合 false。</param>
		/// <param name="facility"></param>
		/// <param name="code"></param>
		/// <returns></returns>
		public static uint MakeHResult(bool severity, uint facility, uint code)
		{
			return (facility << 16) | code | (severity ? 0x80000000 : 0);
		}

		/// <summary>
		/// SUCCEEDED マクロ相当。
		/// </summary>
		/// <param name="hresult"></param>
		/// <returns></returns>
		public static bool IsHResultSucceeded(int hresult)
		{
			return hresult >= 0;
		}

		/// <summary>
		/// FAILED マクロ相当。
		/// </summary>
		/// <param name="hresult"></param>
		/// <returns></returns>
		public static bool IsHResultFailed(int hresult)
		{
			return hresult < 0;
		}
	}
}
