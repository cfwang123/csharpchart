using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Q.Chart;

public sealed partial class MyChart : Control {
	public static readonly DateTime EPOCH = new DateTime(1970, 1, 1);
	Bitmap bmp, bmpover;
	Graphics sdc, sdcover;
	Pen pen;
	Font font1;
	SolidBrush brush;
	int W, H, font1CharWidth, font1CharHeight;
	Timer timer;
	Point lastmouse = new Point(-1, -1), mousedownpos = new Point(-1, -1);
	Rectangle rGrid;
	int focusSeries = 0, focusPointPos = 2;
	int Ylabelmaxw;
	public Axis xaxis = new Axis(), yaxis = new Axis() { isY = true };
	int uiinvalid = 0;
	public List<Series> series = new List<Series>();

	public MyChart() {
		MouseMove += MyChart_MouseMove;
		Paint += MyChart_Paint;
		Resize += MyChart_Resize;
		MouseDown += MyChart_MouseDown;
		MouseUp += MyChart_MouseUp;
		MouseWheel += MyChart_MouseMove;
		DoubleBuffered = true;
		W = Math.Max(1,xaxis.ScreenWorH = Width);
		H = Math.Max(1,yaxis.ScreenWorH = Height);
		bmp = new Bitmap(W, H);
		bmpover = new Bitmap(W, H);
		sdc = Graphics.FromImage(bmp);
		sdcover = Graphics.FromImage(bmpover);
		font1 = new Font("宋体", 12);
		font1CharWidth = (int)sdc.MeasureString("0", font1).Width;
		font1CharHeight = (int)sdc.MeasureString("X轠", font1).Height;
		pen = new Pen(Brushes.Black);
		brush = new SolidBrush(Color.Black);
		timer = new Timer() {
			Interval = 15,
			Enabled = true,
		};
		timer.Tick += Timer_Tick;
	}

	void Timer_Tick(object sender, EventArgs e) {
		if (uiinvalid != 0) {
			if ((uiinvalid & 1) != 0) {
				PaintChart();
				Invalidate();
			}
			else if ((uiinvalid & 2) != 0) {
				PaintOverlay();
				Invalidate();
			}
			uiinvalid = 0;
		}
	}

	protected override void Dispose(bool disposing) {
		timer.Dispose();
		bmp.Dispose();
		sdc.Dispose();
		bmpover.Dispose();
		sdcover.Dispose();
		brush.Dispose();
		pen.Dispose();
		font1.Dispose();
		base.Dispose(disposing);
	}

	void MyChart_Resize(object sender, EventArgs e) {
		if (W <= 0 || H <= 0)
			return;
		if(bmp != null) {
			bmp.Dispose();
			sdc.Dispose();
			bmpover.Dispose();
			sdcover.Dispose();
		}
		W = xaxis.ScreenWorH = Width;
		H = yaxis.ScreenWorH = Height;
		bmp = new Bitmap(W,H);
		bmpover = new Bitmap(W,H);
		sdc = Graphics.FromImage(bmp);
		sdcover = Graphics.FromImage(bmpover);
		font1CharHeight = (int)sdc.MeasureString("X轠", font1).Height;
		RePaint();
	}

	void MyChart_Paint(object sender, PaintEventArgs e) {
		e.Graphics.DrawImage(bmp, 0, 0);
		e.Graphics.DrawImage(bmpover, 0, 0);
	}

	public void RePaint() {
		PaintChart();
		Invalidate();
	}

	const int HOVERRANGE = 10;
	void MyChart_MouseMove(object sender, MouseEventArgs e) {
		if (series.Count == 0)
			return;
		if(mousedownpos.X >= 0) {
			if (Math.Abs(lastmouse.X - e.X) >= 1) {
				int deltaX = e.X - mousedownpos.X;
				double vv = deltaX / (double)rGrid.Width * (xaxis.mousedownDmax - xaxis.mousedownDmin);
				xaxis.Dmin = xaxis.mousedownDmin - vv;
				xaxis.Dmax = xaxis.mousedownDmax - vv;
				uiinvalid |= 1;
			}
		}
		else if(e.Delta != 0) {
			uiinvalid |= 1;
			double vv = (xaxis.Dmax - xaxis.Dmin) * 0.1 * (e.Delta>0?-2:2);
			double ratio = (e.X - rGrid.Left) / (double)rGrid.Width;
			xaxis.Dmin -= vv*ratio;
			xaxis.Dmax += vv*(1-ratio);
			focusPointPos = -1;
		}
		lastmouse = e.Location;
		uiinvalid |= 2;
		int i, len, cx, cy, found = -1, foundSeries = -1;
		double dist, minDist = -1;
		for (i = 0; i < series.Count; i++) {
			var s = series[i];
			Util.PointsInRange_Binsearch(s.data, xaxis.c2d(e.X - HOVERRANGE), xaxis.c2d(e.X + HOVERRANGE), out int i0, out int i1);
			for (; i0 <= i1; i0++) {
				cx = Math.Abs(xaxis.d2c(s.data[i0].x) - e.X);
				cy = Math.Abs(yaxis.d2c(s.data[i0].y) - e.Y);
				if (cx + cy > HOVERRANGE * 3 / 2)
					continue;
				dist = Math.Sqrt(cx * cx + cy * cy);
				if (dist <= HOVERRANGE && minDist < 0 || dist < minDist) {
					found = i0;
					foundSeries = i;
					minDist = dist;
				}
			}
		}
		if(focusPointPos != found || focusSeries != foundSeries) {
			focusPointPos = found;
			focusSeries = foundSeries;
			uiinvalid |= 2;
		}
	}

	void MyChart_MouseUp(object sender, MouseEventArgs e) {
		mousedownpos = new Point(-1, 0);
	}

	void MyChart_MouseDown(object sender, MouseEventArgs e) {
		mousedownpos = e.Location;
		xaxis.mousedownDmin = xaxis.Dmin;
		xaxis.mousedownDmax = xaxis.Dmax;
	}

	public void SetData(List<Series> series, bool isDate = false) {
		xaxis.Dmin = yaxis.Dmin = double.PositiveInfinity;
		xaxis.Dmax = yaxis.Dmax = double.NegativeInfinity;
		foreach (var s in series) {
			foreach (var v in s.data) {
				xaxis.UpdateMinMax(v.x);
				yaxis.UpdateMinMax(v.y);
			}
		}
		if (xaxis.Dmin > xaxis.Dmax) {
			xaxis.Dmin = yaxis.Dmin = 0;
			xaxis.Dmax = yaxis.Dmax = 1;
		}
		else {
			if (xaxis.Dmax - xaxis.Dmin == 0) {
				xaxis.Dmin--;
				xaxis.Dmax++;
			}
			if (yaxis.Dmax - yaxis.Dmin == 0) {
				yaxis.Dmin--;
				yaxis.Dmax++;
			}
		}
		xaxis.isDate = isDate;
		this.series = series;
	}

	void CalculateGrid() {
		xaxis.genTicks(xaxis.Dmax, xaxis.Dmin, 0, sdc, font1);
		yaxis.genTicks(yaxis.Dmax, yaxis.Dmin, 0, sdc, font1);
		Ylabelmaxw = yaxis.tickarr.Count > 0 ? yaxis.tickarr.Max(v => v.W) : 0;
		rGrid = new Rectangle(Ylabelmaxw+5, 20, W - Ylabelmaxw - 20, H - font1CharHeight - 25);
		xaxis.Cmin = rGrid.Left;
		xaxis.Cmax = rGrid.Right;
		yaxis.Cmin = rGrid.Bottom;
		yaxis.Cmax = rGrid.Top;
	}

	public void PaintChart() {
		int i, j;
		sdc.Clear(Color.White);
		CalculateGrid();
		pen.Width = 1;
		pen.Color = Color.FromArgb(0xcc,0xcc,0xcc);
		//sdc.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
		foreach(var ytick in yaxis.tickarr) {
			j = yaxis.d2c(ytick.value);
			sdc.DrawString(ytick.strvalue, font1, Brushes.Black, 1 + (Ylabelmaxw - ytick.W), j - ytick.H / 2);
			if (ytick.value <= yaxis.Dmin || ytick.value >= yaxis.Dmax) continue;
			sdc.DrawLine(pen,rGrid.Left,j,rGrid.Right,j);
		}
		foreach(var xtick in xaxis.tickarr) {
			j = xaxis.d2c(xtick.value);
			sdc.DrawString(xtick.strvalue, font1, Brushes.Black, j-xtick.W/2, H-5-font1CharHeight);
			if (xtick.value == xaxis.Dmin || xtick.value >= xaxis.Dmax) continue;
			sdc.DrawLine(pen,j,rGrid.Top,j,rGrid.Bottom);
		}
		sdc.Clip = new Region(rGrid);
		sdc.SmoothingMode = SmoothingMode.HighQuality;
		int cx0, cy0, cx1, cy1;
		foreach (var s in series) {
			pen.Color = s.color;
			cx0 = int.MaxValue;
			cy0 = 0;
			var R = new DrawLineReducer(sdc, pen, rGrid);
			Util.PointsInRange2_Binsearch(s.data, xaxis.c2d(rGrid.Left), xaxis.c2d(rGrid.Right), out int i0, out int i1);
			if (i0 > 0) i0--;
			if (i1 < s.data.Count - 1) i1 = Math.Min(i1 + 2, s.data.Count - 1);
			for (; i0 < i1; i0++) {
				var p = s.data[i0];
				cx1 = xaxis.d2c(p.x);
				cy1 = yaxis.d2c(p.y);
				bool valid = !double.IsNaN(p.y);
				if (valid) {
					if (cx0 != int.MaxValue && xaxis.cisNeedDrawLine(cx0, cx1) && yaxis.cisNeedDrawLine(cy0, cy1)) {
						R.AddLine(cx0, cy0, cx1, cy1);
					}
					cx0 = cx1;
					cy0 = cy1;
				}
				else {
					R.Finish();
					cx0 = int.MaxValue;
				}
			}
			R.Finish();
		}
		sdc.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
		pen.Width = 3;
		pen.Color = Color.FromArgb(0x66, 0x66, 0x66);
		sdc.ResetClip();
		sdc.DrawRectangle(pen, rGrid);
		PaintOverlay();
	}

	void PaintOverlay() {
		sdcover.Clear(Color.Transparent);
		int i, len;
		bool inGrid = false;
		if (series.Count == 0)
			return;
		if (lastmouse.X >= rGrid.Left && lastmouse.X <= rGrid.Right && lastmouse.Y >= rGrid.Top && lastmouse.Y <= rGrid.Bottom) {
			pen.Width = 1;
			pen.Color = Color.Blue;
			sdcover.DrawLine(pen, lastmouse.X, rGrid.Top + 1, lastmouse.X, rGrid.Bottom - 1);
			inGrid = true;
		}
		if (focusSeries >= 0) {
			var s = series[focusSeries];
			if (focusPointPos >= 0 && focusPointPos < s.data.Count && s.data[focusPointPos].x >= xaxis.Dmin && s.data[focusPointPos].x <= xaxis.Dmax && s.data[focusPointPos].y >= yaxis.Dmin && s.data[focusPointPos].y <= yaxis.Dmax) {
				var v = s.data[focusPointPos];
				pen.Width = 5;
				pen.Color = Color.FromArgb(0x60, s.color);
				sdcover.DrawEllipse(pen, new Rectangle(xaxis.d2c(v.x) - 10, yaxis.d2c(v.y) - 10, 20, 20));
				string msg = $"{v.x}, {Util.ToDec(v.y, s.dec)}";
				var wh = sdcover.MeasureString(msg, font1);
				Rectangle rect = new Rectangle(lastmouse.X + 10, lastmouse.Y + 10, (int)wh.Width + 4, (int)wh.Height + 4);
				if (lastmouse.X + 10 + wh.Width + 4 >= rGrid.Right) rect.X = lastmouse.X - 10 - (int)wh.Width;
				if (lastmouse.Y + 10 + wh.Height + 4 >= rGrid.Bottom) rect.Y = lastmouse.Y - 10 - (int)wh.Height;
				pen.Width = 1;
				pen.Color = Color.Black;
				sdcover.FillRectangle(Brushes.White, rect);
				sdcover.DrawRectangle(pen, rect);
				sdcover.DrawString(msg, font1, Brushes.Black, rect.Left + 2, rect.Top + 2);
			}
		}
		float ypos = rGrid.Top + 8;
		int legwid = 0, hovwid = 0, rowhei = 0;
		for (i = 0; i < series.Count; i++) {
			var s = series[i];
			if (inGrid && Util.GetInterpolatedY(s.data, xaxis.c2d(lastmouse.X), out double y))
				s._hovervalue = Util.ToDec(y, s.dec) + " " + s.unit;
			else s._hovervalue = "";
			var wh = sdcover.MeasureString(s.legend + " = ", font1);
			s._legendwid = (int)wh.Width;
			s._hovervaluewid = (int)sdcover.MeasureString(s._hovervalue, font1).Width;
			rowhei = (int)wh.Height;
			legwid = Math.Max(legwid, s._legendwid);
			hovwid = Math.Max(hovwid, s._hovervaluewid);
		}
		for (i = 0; i < series.Count; i++) {
			var s = series[i];
			pen.Width = 1;
			pen.Color = Color.FromArgb(0xff,0xe4,0xe4,0xe4);
			sdcover.DrawRectangle(pen, rGrid.Right - legwid - hovwid - 5 - 20, ypos, 20, 15);
			brush.Color = s.color;
			sdcover.FillRectangle(brush, rGrid.Right - legwid - hovwid - 5 - 20 + 2, ypos + 4, 16, 9);
			var legrect = new RectangleF(rGrid.Right - legwid - hovwid - 5, ypos - 3, legwid + hovwid, rowhei);
			sdcover.FillRectangle(Brushes.White, legrect);
			sdcover.DrawString(s.legend+" = ", font1, Brushes.Black, legrect.Left + legwid - s._legendwid, legrect.Top);
			sdcover.DrawString(s._hovervalue, font1, Brushes.Black, legrect.Left + legwid + hovwid - s._hovervaluewid, legrect.Top);
			ypos += rowhei;
		}
	}
}

public static class Util {
	static readonly string[] decfmts = new string[] {
		"0",
		"0.0",
		"0.00",
		"0.000",
		"0.0000",
		"0.00000",
		"0.000000",
		"0.0000000",
		"0.00000000",
		"0.000000000",
		"0.0000000000",
		"0.00000000000",
	};
	public static string ToDec(double d, int dec = 0) {
		return d.ToString(decfmts[dec >= 0 && dec < decfmts.Length ? dec : 0]);
	}

	public static bool GetInterpolatedY(List<DataPoint> data, double x, out double y) {
		int s = 0, e = data.Count - 1, m, i0;
		while (s <= e) {
			m = (s + e) / 2;
			if (data[m].x >= x) e = m - 1;
			else s = m + 1;
		}
		y = 0;
		i0 = e;
		if (i0 < 0 || data[i0].x > x || double.IsNaN(data[i0].y)) return false;
		else if (data[i0].x == x) {
			y = data[i0].y;
			return true;
		}
		else if (i0 + 1 >= data.Count || double.IsNaN(data[i0 + 1].y)) return false;
		else {
			y = data[i0].y + (x - data[i0].x) / (data[i0 + 1].x - data[i0].x) * (data[i0 + 1].y - data[i0].y);
			return true;
		}
	}

	public static void PointsInRange2_Binsearch(List<DataPoint> data, double x0, double x1, out int from, out int to) {
		int s=0,e=data.Count-1,m;
		while (s <= e) {
			m = (s + e) / 2;
			if (data[m].x >= x0 || double.IsNaN(data[m].y)) e = m - 1;
			else s = m + 1;
		}
		from = s;
		s = from;
		e = data.Count - 1;
		while (s <= e) {
			m = (s + e) / 2;
			if (data[m].x <= x1 || double.IsNaN(data[m].y)) s = m + 1;
			else e = m - 1;
		}
		to = e;
	}

	public static void PointsInRange_Binsearch(List<DataPoint> data, double x0, double x1, out int from, out int to) {
		int s=0,e=data.Count-1,m;
		while (s <= e) {
			m = (s + e) / 2;
			if (data[m].x >= x0) e = m - 1;
			else s = m + 1;
		}
		from = s;
		s = from;
		e = data.Count - 1;
		while (s <= e) {
			m = (s + e) / 2;
			if (data[m].x <= x1) s = m + 1;
			else e = m - 1;
		}
		to = e;
	}
}

public struct DrawLineReducer {
	public Graphics sdc;
	public Pen pen;
	public List<int> lines = new List<int>();
	public double maxAngle, minAngle;
	public Rectangle rGrid;
	const double MERGEANGLE = Math.PI / 18;
	const int MERGEMAXXDIFF = 3;

	public DrawLineReducer(Graphics sdc, Pen pen, Rectangle rGrid) : this() {
		this.sdc = sdc;
		this.pen = pen;
		this.rGrid = rGrid;
	}

	public void Finish() {
		if(lines.Count > 0) {
			DrawLine(lines[0], lines[1], lines[lines.Count - 2], lines[lines.Count - 1]);
			lines.Clear();
		}
	}

	void DrawLine(float x0, float y0, float x1, float y1) {
		if(x0 < rGrid.Left) {
			y0 = y0 + (rGrid.Left - x0) / (x1 - x0) * (y1 - y0);
			x0 = rGrid.Left;
		}
		if(y0 < rGrid.Top) {
			x0 = x0 + (rGrid.Top - y0) / (y1 - y0) * (x1 - x0);
			y0 = 0;
		}
		if(x1 > rGrid.Right) {
			y1 = y1 - (x1 - rGrid.Right) / (x1 - x0) * (y1 - y0);
			x1 = rGrid.Right;
		}
		if(y1 > rGrid.Bottom) {
			x1 = x1 - (y1 - rGrid.Bottom) / (y1 - y0) * (x1 - x0);
			y1 = rGrid.Bottom;
		}
		sdc.DrawLine(pen, x0, y0, x1, y1);
	}

	public void AddLine(int x0, int y0, int x1, int y1) {
		if(lines.Count == 0) {
			if (x1 - x0 >= MERGEMAXXDIFF) {
				DrawLine(x0, y0, x1, y1);
				return;
			}
			else {
				double angle = Math.Atan2(y1 - y0, x1 - x0);
				lines.Add(x0);
				lines.Add(y0);
				lines.Add(x1);
				lines.Add(y1);
				maxAngle = minAngle = angle;
			}
		}
		else {
			double angle = Math.Atan2(y1 - y0, x1 - x0);
			if(x1 - lines[0] >= MERGEMAXXDIFF || angle < minAngle && angle < maxAngle - MERGEANGLE || angle > maxAngle && angle > minAngle + MERGEANGLE) {
				DrawLine(lines[0], lines[1], lines[lines.Count - 2], lines[lines.Count - 1]);
				lines.Clear();
				if(x1 - x0 >= MERGEMAXXDIFF) {
					DrawLine(x0, y0, x1, y1);
				}
				else {
					lines.Add(x0);
					lines.Add(y0);
					lines.Add(x1);
					lines.Add(y1);
					maxAngle = minAngle = angle;
				}
			}
			else {
				lines.Add(x0);
				lines.Add(y0);
				lines.Add(x1);
				lines.Add(y1);
				if (angle < minAngle) minAngle = angle;
				if (angle > maxAngle) maxAngle = angle;
			}
		}
	}
}

public sealed class Axis {
	public bool isDate, isY;
	public int ScreenWorH, Cmin, Cmax = 1;
	public double Dmin, Dmax = 1, mousedownDmin, mousedownDmax;
	public short tickcount;
	public List<TickLabel> tickarr, cTicks;

	public bool cisNeedDrawLine(int c1, int c2) {
		if (isY) {
			if (c1 < Cmax) return c2 >= Cmax;
			else return c1 <= Cmin;
		}
		else {
			if (c1 < Cmin) return c2 >= Cmin;
			else return c1 <= Cmax;
		}
	}

	public bool cisBetween(int c) {
		if(isY) return c >= Cmax && c <= Cmin;
		return c >= Cmin && c <= Cmax;
	}

	public int d2c(double d) {
		return (int)(Cmin + (d - Dmin) / (Dmax - Dmin) * (Cmax - Cmin));
	}

	public double c2d(int c) {
		return (Dmin + (c - Cmin) / (double)(Cmax - Cmin) * (Dmax - Dmin));
	}

	public void UpdateMinMax(double xy) {
		if (xy < Dmin) Dmin = xy;
		if (xy > Dmax) Dmax = xy;
	}

	public void genTicks(double max, double min, short cTickDecimals, Graphics sdc, Font font1) {
		if (cTicks != null) {
			tickarr = cTicks;
			for(int i = 0; i < tickarr.Count; i++) {
				var wh = sdc.MeasureString(tickarr[i].strvalue, font1);
				tickarr[i] = new TickLabel {
					value = tickarr[i].value,
					strvalue = tickarr[i].strvalue,
					W = (int)Math.Ceiling(wh.Width),
					H = (int)Math.Ceiling(wh.Height),
				};
			}
			return;
		}
		tickcount = (short)(int)(0.3 * Math.Sqrt(ScreenWorH));
		double delta;
		int dec;
		float charW = sdc.MeasureString("0000000000", font1).Width / 10 + 2;
		while (true) {
			delta = (max - min) / tickcount;
			dec = -(int)Math.Floor(Math.Log10(delta));
			int tickwid = (int)(Math.Max(Util.ToDec(min,dec).Length, Util.ToDec(max, dec).Length) * charW) + 5;
			if (tickcount > 2 && tickwid * tickcount > Math.Abs(Cmax - Cmin))
				tickcount--;
			else break;
		}

		if (cTickDecimals > 0 && dec > cTickDecimals)
			dec = cTickDecimals;
		double magn = Math.Pow(10, -dec);
		double norm = delta / magn; // 1.0 ~ 10.0
		double size;
		if (norm < 1.5) size = 1;
		else if (norm < 3) {
			size = 2;
			if (norm > 2.25 && (cTickDecimals == 0 || (dec + 1) <= cTickDecimals)) {
				dec++;
				size = 2.5;
			}
		}
		else if (norm < 7.5) size = 5;
		else {
			size = 10;
			dec--;
		}
		if (dec < 0) dec = 0;
		double ticksize = size * magn;
		tickarr = new List<TickLabel>();
		double start = Math.Floor(min / ticksize) * ticksize, v;
		if (start < min)
			start += ticksize;
		while(start < max) {
			string tickstr;
			if (isDate) tickstr = MyChart.EPOCH.AddMilliseconds(start).ToString("HH:mm:ss");
			else tickstr = Util.ToDec(start, dec);
			var wh = sdc.MeasureString(tickstr, font1);
			tickarr.Add(new TickLabel {
				value = start,
				strvalue = tickstr,
				W = (int)Math.Ceiling(wh.Width),
				H = (int)Math.Ceiling(wh.Height),
			});
			start += ticksize;
		}
	}
}

public struct TickLabel {
	public double value;
	public string strvalue;
	public int W = 0, H = 0;
	public TickLabel(double value, string strvalue) {
		this.value = value;
		this.strvalue = strvalue;
	}
}

public struct DataPoint {
	public double x, y;
	public DataPoint(double x, double y) {
		this.x = x;
		this.y = y;
	}
	public DataPoint(DateTime x, double y) {
		this.x = (x - MyChart.EPOCH).TotalMilliseconds;
		this.y = y;
	}
}
