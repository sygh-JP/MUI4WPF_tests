using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MyCommonTypes
{
	// class などの参照型だけでなく struct などの値型もデータ バインディングのソースに使える。

	public struct TimeSpansRatio
	{
		public TimeSpan Numerator;
		public TimeSpan Denominator;

		public TimeSpansRatio(TimeSpan numerator, TimeSpan denominator)
		{
			this.Numerator = numerator;
			this.Denominator = denominator;
		}

		static double CalcTimeSpansRatio(TimeSpan numerator, TimeSpan denominator)
		{
			// (0.0 / 0.0) は NaN となる。
			return numerator.TotalSeconds / denominator.TotalSeconds;
		}

		static double SafeClampRatio(double x)
		{
			return Double.IsNaN(x) ? 0.0 : MyMiscHelpers.MyGenericsHelper.Clamp(x, 0.0, 1.0);
		}

		public static TimeSpansRatio CreateZero()
		{
			return new TimeSpansRatio(TimeSpan.Zero, TimeSpan.Zero);
		}

		static TimeSpan TruncateMilliseconds(TimeSpan src)
		{
			return new TimeSpan(0, 0, (int)src.TotalSeconds);
		}

		public static TimeSpansRatio TruncateMilliseconds(TimeSpansRatio src)
		{
			return new TimeSpansRatio(TruncateMilliseconds(src.Numerator), TruncateMilliseconds(src.Denominator));
		}

		public double Ratio
		{
			get { return SafeClampRatio(CalcTimeSpansRatio(this.Numerator, this.Denominator)); }
		}

		public override string ToString()
		{
			return String.Format("{0} / {1}", this.Numerator, this.Denominator);
		}
		public override bool Equals(object obj)
		{
			if (obj is TimeSpansRatio)
			{
				var src = (TimeSpansRatio)obj;
				return this.Numerator.Equals(src.Numerator) && this.Denominator.Equals(src.Denominator);
			}
			return false;
		}
		public override int GetHashCode()
		{
			return this.Numerator.GetHashCode() ^ this.Denominator.GetHashCode();
		}
	}
}
