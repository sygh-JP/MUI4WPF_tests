using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MyWpfShapes
{
	public abstract class CenteredPieArcBase : Shape
	{
		ArcSegment arc = new ArcSegment();
		//LineSegment line1 = new LineSegment(); // 中心から円弧始点へ向かう線分（放射直線）。
		LineSegment line2 = new LineSegment(); // 円弧終点から中心へ向かう線分（放射直線）。
		PathFigure figure = new PathFigure();
		PathGeometry pathGeo = new PathGeometry();
		RotateTransform xform = new RotateTransform();

		void SetupPath(bool isPie)
		{
			arc.IsLargeArc = false; // 180 度は超えない。
			//arc.SweepDirection = SweepDirection.Counterclockwise;
			arc.SweepDirection = SweepDirection.Clockwise; // GDI+ など、2D グラフィックス座標系ではたいてい時計回りを正方向とする（Y 方向がスクリーン下向きである関係上）。
			if (isPie)
			{
				figure.Segments.Add(arc);
				figure.Segments.Add(line2);
				//figure.Segments.Add(line1);
				figure.IsClosed = true;
				// 開始と終了を自動的に閉じるので、線分は1つだけでよい。
			}
			else
			{
				figure.Segments.Add(arc);
				figure.IsClosed = false;
			}

			pathGeo.Figures.Add(figure);
		}

		void AssignTransformToGeometry()
		{
			pathGeo.Transform = xform;
		}

		#region Dependency properties

		public static readonly DependencyProperty CenterProperty =
			EllipseGeometry.CenterProperty.AddOwner(
			typeof(CenteredPieArcBase),
			new FrameworkPropertyMetadata(new Point(0, 0),
			EllipsePropertyChanged));

		public static readonly DependencyProperty RadiusXProperty =
			EllipseGeometry.RadiusXProperty.AddOwner(
			typeof(CenteredPieArcBase),
			new FrameworkPropertyMetadata(0.0,
			EllipsePropertyChanged));

		public static readonly DependencyProperty RadiusYProperty =
			EllipseGeometry.RadiusYProperty.AddOwner(
			typeof(CenteredPieArcBase),
			new FrameworkPropertyMetadata(0.0,
			EllipsePropertyChanged));

		public static readonly DependencyProperty StartAngleProperty =
			DependencyProperty.Register("StartAngle", typeof(double), typeof(CenteredPieArcBase),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
			EllipsePropertyChanged));

		public static readonly DependencyProperty SweepAngleProperty =
			DependencyProperty.Register("SweepAngle", typeof(double), typeof(CenteredPieArcBase),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
			EllipsePropertyChanged));

		public static readonly DependencyProperty RotationAngleProperty =
			ArcSegment.RotationAngleProperty.AddOwner(
			typeof(CenteredPieArcBase),
			new FrameworkPropertyMetadata(0.0,
			TransformPropertyChanged));

		static void EllipsePropertyChanged(DependencyObject obj,
			DependencyPropertyChangedEventArgs args)
		{
			(obj as CenteredPieArcBase).EllipsePropertyChanged(args);
		}

		static void TransformPropertyChanged(DependencyObject obj,
			DependencyPropertyChangedEventArgs args)
		{
			(obj as CenteredPieArcBase).TransformPropertyChanged(args);
		}

		void EllipsePropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			// GDI+ の Graphics.DrawPie(), FillPie(), DrawArc() に準じたパラメータ方式（開始角と掃引角の指定）とする。
			// WPF ジオメトリの内部実装的には、旧 GDI の Pie(), Arc() や Direct2D の ID2D1GeometrySink 同様に円弧の始点・終点を指定する方式も可能だが、
			// GDI+ 方式のほうが円・楕円と合わせて統一的に扱いやすくなる。
			double startRadian = this.StartAngle * Math.PI / 180;
			double endRadian = (this.StartAngle + this.SweepAngle) * Math.PI / 180; // 負の掃引角は考慮しない。

			//figure.StartPoint = Center;
			figure.StartPoint = new Point(Center.X + RadiusX * Math.Cos(startRadian), Center.Y + RadiusY * Math.Sin(startRadian));
			//line1.Point = new Point(Center.X + RadiusX * Math.Cos(startRadian), Center.Y - RadiusY * Math.Sin(startRadian));
			//line1.Point = figure.StartPoint;
			line2.Point = Center;
			arc.Size = new Size(RadiusX, RadiusY);
			arc.Point = new Point(Center.X + RadiusX * Math.Cos(endRadian), Center.Y + RadiusY * Math.Sin(endRadian));
			//arc.Point = new Point(Center.X + RadiusX * Math.Cos(endRadian), Center.Y - RadiusY * Math.Sin(endRadian));

			// Math.IEEERemainder() は動作仕様に注意。
			arc.IsLargeArc = (this.SweepAngle % 360.0 > 180.0);
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

		protected CenteredPieArcBase(bool isPie)
		{
			this.SetupPath(isPie);
			this.AssignTransformToGeometry();
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

		public double StartAngle
		{
			set { SetValue(StartAngleProperty, value); }
			get { return (double)GetValue(StartAngleProperty); }
		}

		public double SweepAngle
		{
			set { SetValue(SweepAngleProperty, value); }
			get { return (double)GetValue(SweepAngleProperty); }
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
				return pathGeo;
			}
		}
	}

	public class CenteredPie : CenteredPieArcBase
	{
		public CenteredPie()
			: base(true)
		{
		}
	}

	public class CenteredArc : CenteredPieArcBase
	{
		public CenteredArc()
			: base(false)
		{
		}
	}
}
