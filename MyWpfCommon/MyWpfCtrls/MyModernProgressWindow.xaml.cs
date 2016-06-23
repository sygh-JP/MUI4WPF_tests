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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyWpfCtrls
{
	/// <summary>
	/// MyModernProgressWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MyModernProgressWindow : MyProgressWindowBase
	{
		internal MyModernProgressWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// ファクトリ メソッド。
		/// </summary>
		/// <returns></returns>
		public static MyModernProgressWindow Create()
		{
			var temp = new MyModernProgressWindow();
			temp.Initialize();
			return temp;
		}
	}
}
