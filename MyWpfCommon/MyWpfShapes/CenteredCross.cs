using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MyWpfShapes
{
	public class CenteredCross : Shape
	{
		GeometryGroup allGeo = new GeometryGroup();
		LineGeometry lineGeoX = new LineGeometry();
		LineGeometry lineGeoY = new LineGeometry();
		RotateTransform xform = new RotateTransform();

		#region Dependency properties

		public static readonly DependencyProperty CenterProperty =
			EllipseGeometry.CenterProperty.AddOwner(
			typeof(CenteredCross),
			new FrameworkPropertyMetadata(new Point(0, 0),
			EllipsePropertyChanged));

		public static readonly DependencyProperty RadiusXProperty =
			EllipseGeometry.RadiusXProperty.AddOwner(
			typeof(CenteredCross),
			new FrameworkPropertyMetadata(0.0,
			EllipsePropertyChanged));

		public static readonly DependencyProperty RadiusYProperty =
			EllipseGeometry.RadiusYProperty.AddOwner(
			typeof(CenteredCross),
			new FrameworkPropertyMetadata(0.0,
			EllipsePropertyChanged));

		public static readonly DependencyProperty RotationAngleProperty =
			ArcSegment.RotationAngleProperty.AddOwner(
			typeof(CenteredCross),
			new FrameworkPropertyMetadata(0.0,
			TransformPropertyChanged));

		static void EllipsePropertyChanged(DependencyObject obj,
			DependencyPropertyChangedEventArgs args)
		{
			(obj as CenteredCross).EllipsePropertyChanged(args);
		}

		static void TransformPropertyChanged(DependencyObject obj,
			DependencyPropertyChangedEventArgs args)
		{
			(obj as CenteredCross).TransformPropertyChanged(args);
		}

		void EllipsePropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			lineGeoX.StartPoint = new Point(Center.X - RadiusX, Center.Y);
			lineGeoX.EndPoint = new Point(Center.X + RadiusX, Center.Y);
			lineGeoY.StartPoint = new Point(Center.X, Center.Y - RadiusY);
			lineGeoY.EndPoint = new Point(Center.X, Center.Y + RadiusY);
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

		public CenteredCross()
		{
			lineGeoX.Transform = xform;
			lineGeoY.Transform = xform;
			allGeo.Children.Add(lineGeoX);
			allGeo.Children.Add(lineGeoY);
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
				return allGeo;
			}
		}
	}
}
