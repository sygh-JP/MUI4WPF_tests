using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MyWpfShapes
{
	public class CenteredEllipse : Shape
	{
		EllipseGeometry elipGeo = new EllipseGeometry();
		RotateTransform xform = new RotateTransform();

		#region Dependency properties

		public static readonly DependencyProperty CenterProperty =
			EllipseGeometry.CenterProperty.AddOwner(
			typeof(CenteredEllipse),
			new FrameworkPropertyMetadata(new Point(0, 0),
			EllipsePropertyChanged));

		public static readonly DependencyProperty RadiusXProperty =
			EllipseGeometry.RadiusXProperty.AddOwner(
			typeof(CenteredEllipse),
			new FrameworkPropertyMetadata(0.0,
			EllipsePropertyChanged));

		public static readonly DependencyProperty RadiusYProperty =
			EllipseGeometry.RadiusYProperty.AddOwner(
			typeof(CenteredEllipse),
			new FrameworkPropertyMetadata(0.0,
			EllipsePropertyChanged));

		public static readonly DependencyProperty RotationAngleProperty =
			ArcSegment.RotationAngleProperty.AddOwner(
			typeof(CenteredEllipse),
			new FrameworkPropertyMetadata(0.0,
			TransformPropertyChanged));

		static void EllipsePropertyChanged(DependencyObject obj,
			DependencyPropertyChangedEventArgs args)
		{
			(obj as CenteredEllipse).EllipsePropertyChanged(args);
		}

		static void TransformPropertyChanged(DependencyObject obj,
			DependencyPropertyChangedEventArgs args)
		{
			(obj as CenteredEllipse).TransformPropertyChanged(args);
		}

		void EllipsePropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			elipGeo.Center = Center;
			elipGeo.RadiusX = RadiusX;
			elipGeo.RadiusY = RadiusY;
			InvalidateMeasure();
		}

		void TransformPropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			xform.Angle = RotationAngle;
			xform.CenterX = Center.X;
			xform.CenterY = Center.Y;
			InvalidateMeasure();
		}

		#endregion

		public CenteredEllipse()
		{
			elipGeo.Transform = xform;
		}

		#region Public CLR properties

		public Point Center
		{
			set { SetValue(CenterProperty, value); }
			get { return (Point)GetValue(CenterProperty); }
		}

		public double RadiusX
		{
			set { SetValue(RadiusXProperty, value); }
			get { return (double)GetValue(RadiusXProperty); }
		}

		public double RadiusY
		{
			set { SetValue(RadiusYProperty, value); }
			get { return (double)GetValue(RadiusYProperty); }
		}

		public double RotationAngle
		{
			set { SetValue(RotationAngleProperty, value); }
			get { return (double)GetValue(RotationAngleProperty); }
		}

		#endregion

		// Required DefiningGeometry override
		protected override Geometry DefiningGeometry
		{
			get
			{
				return elipGeo;
			}
		}
	}
}
