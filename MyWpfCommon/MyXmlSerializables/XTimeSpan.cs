using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// ISerializable 向けに実装した SerializationInfo, StreamingContext を受け取るコンストラクタと GetObjectData() メソッドは、
// XmlSerializer を使うときは呼ばれない模様。
// というか XmlSerializer は ISerializable はおろか SerializableAttribute すら要求しないらしい。
// これらを要求するのは BinaryFormatter と SoapFormatter になる。

// public なプロパティは XmlSerializer によって自動でシリアライズされるが、データ型や階層に制限がある。
// TimeSpan は XmlSerializer でシリアライズ／逆シリアライズできないらしい。DateTime はできそうなのに……
// ユーザー定義クラス型の各プロパティをシリアライズすることはできる（XML 子要素として階層化される）。
// IXmlSerializable を実装して細かく制御する方法や、WCF で使われている DataContractSerializer を使う方法もあるが、ともに結構な手間がかかる。
// XML シリアライズ／逆シリアライズ専用に String 型の代替プロキシ プロパティを定義する方法もあるが、冗長で美しくないのが欠点。

// NOTE: private プロパティにすると XML に RW されない。


namespace MyXmlSerializables
{
	public sealed class XTimeSpan
	{
		[System.Xml.Serialization.XmlIgnore]
		public TimeSpan Value { get; set; }

		#region XmlSerializer
		// [System.Xml.Serialization.XmlElement("xxx")]
		// で要素のタグ名を明示的に指定することもできるが、面倒なだけなのでやらない。
		// Parse() メソッドの場合、文字列解析に失敗すると例外がスローされるが、あえて握りつぶさないでおく。
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public string TimeSpanString
		{
			get { return this.Value.ToString(); }
			set { this.Value = TimeSpan.Parse(value); }
		}
		#endregion

		public XTimeSpan()
		{
			this.Value = new TimeSpan();
		}
		public XTimeSpan(TimeSpan src)
		{
			this.Value = src;
		}
		public XTimeSpan(string src)
		{
			this.Value = TimeSpan.Parse(src);
		}
		public XTimeSpan(XTimeSpan src)
		{
			this.CopyFrom(src);
		}

		public void CopyFrom(XTimeSpan src)
		{
			this.Value = src.Value;
		}
		public XTimeSpan Clone()
		{
			var temp = new XTimeSpan();
			temp.CopyFrom(this);
			return temp;
		}

		public static explicit operator TimeSpan(XTimeSpan obj)
		{
			return obj.Value;
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}
		public override bool Equals(object obj)
		{
			var src = obj as XTimeSpan;
			return src != null ? this.Value.Equals(src.Value) : false;
		}
		public override int GetHashCode()
		{
			return this.Value.GetHashCode();
		}
	}
}
