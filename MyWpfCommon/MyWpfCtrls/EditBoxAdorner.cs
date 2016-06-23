using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Data;

namespace MyWpfCtrls
{
	class EditBoxAdorner : Adorner
	{
		public EditBoxAdorner(UIElement adornedElement, UIElement adorningElement)
			: base(adornedElement)
		{
			_textBox = adorningElement as TextBox;
			Debug.Assert(_textBox != null, "No TextBox!");

			_visualChildren = new VisualCollection(this);

			BuildTextBox();
		}

		#region Public Methods

		public void UpdateVisibilty(bool isVisisble)
		{
			_isVisible = isVisisble;
			InvalidateMeasure();
		}

		#endregion

		#region Protected Methods

		protected override Size MeasureOverride(Size constraint)
		{
			_textBox.IsEnabled = _isVisible;
			if (_isVisible)
			{
				AdornedElement.Measure(constraint);
				_textBox.Measure(constraint);
				return new Size(AdornedElement.DesiredSize.Width + _extraWidth, _textBox.DesiredSize.Height);
			}
			else
			{
				return new Size(0, 0);
			}
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			if (_isVisible)
			{
				_textBox.Arrange(new Rect(-5, -3, finalSize.Width, finalSize.Height));
			}
			else
			{
				_textBox.Arrange(new Rect(0, 0, 0, 0));
			}
			return finalSize;
		}

		protected override int VisualChildrenCount { get { return _visualChildren.Count; } }

		protected override Visual GetVisualChild(int index) { return _visualChildren[index]; }

		#endregion

		#region Private Methods

		private void BuildTextBox()
		{
			_canvas = new Canvas();
			_canvas.Children.Add(_textBox);
			_visualChildren.Add(_canvas);

			Binding binding = new Binding("Text");
			binding.Mode = BindingMode.TwoWay;
			binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			binding.Source = this.AdornedElement;   // TextBlock

			_textBox.SetBinding(TextBox.TextProperty, binding);
			_textBox.LayoutUpdated += new EventHandler(OnTextBoxLayoutUpdated);
		}

		private void OnTextBoxLayoutUpdated(object sender, EventArgs e)
		{
			if (_isVisible)
				_textBox.Focus();
		}

		#endregion

		#region Private Fields

		private VisualCollection _visualChildren;
		private TextBox _textBox;
		private bool _isVisible;
		private Canvas _canvas;
		private const double _extraWidth = 15;

		#endregion
	}
}
