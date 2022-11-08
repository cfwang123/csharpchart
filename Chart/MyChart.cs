using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Q.Chart;

public sealed partial class MyChart : PictureBox {
	Bitmap bmp, bmpover;
	Graphics sdc, sdcover;
	Pen pen;
	Font font1;
	SolidBrush brush;
	int W, H, font1CharHeight;
	Timer timer;
	Point lastmouse = new Point(-1, -1), mousedownpos = new Point(-1, -1);
	Rectangle rGrid;
	int focusPointPos = 2;
	int Ylabelmaxw;
	public Axis xaxis = new Axis(), yaxis = new Axis() { isY = true };
	List<DataPoint> data = new List<DataPoint>();
	int uiinvalid = 0;
	public string legend = "", unit = "";
	public Color seriesColor;

	public MyChart() {
		InitializeComponent();
		MouseMove += MyChart_MouseMove;
		Paint += MyChart_Paint;
		Resize += MyChart_Resize;
		MouseDown += MyChart_MouseDown;
		MouseUp += MyChart_MouseUp;
		MouseWheel += MyChart_MouseMove;
		W = xaxis.ScreenWorH = Width;
		H = yaxis.ScreenWorH = Height;
		bmp = new Bitmap(W,H);
		bmpover = new Bitmap(W,H);
		sdc = Graphics.FromImage(bmp);
		sdcover = Graphics.FromImage(bmpover);
		font1 = new Font("宋体", 12);
		font1CharHeight = (int)sdc.MeasureString("X轠", font1).Height;
		pen = new Pen(Brushes.Black);
		brush = new SolidBrush(Color.Black);
		timer = new Timer() {
			Interval = 16,
			Enabled = true,
		};
		timer.Tick += Timer_Tick;
	}

	void Timer_Tick(object sender, EventArgs e) {
		if((uiinvalid & 1) != 0) {
			PaintChart();
			Invalidate();
		}
		else if((uiinvalid & 2) != 0) {
			PaintOverlay();
			Invalidate();
		}
		uiinvalid = 0;
	}

	protected override void Dispose(bool disposing) {
		timer.Dispose();
		if (disposing && (components != null)) {
			components.Dispose();
		}
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
		bool ch = false, lay = true;
		if(mousedownpos.X >= 0) {
			if (Math.Abs(lastmouse.X - e.X) >= 3) {
				int deltaX = e.X - mousedownpos.X;
				double vv = deltaX / (double)rGrid.Width * (xaxis.mousedownDmax - xaxis.mousedownDmin);
				xaxis.Dmin = xaxis.mousedownDmin - vv;
				xaxis.Dmax = xaxis.mousedownDmax - vv;
				ch = true;
			}
		}
		else if(e.Delta != 0) {
			ch = true;
			double vv = (xaxis.Dmax - xaxis.Dmin) * 0.1 * (e.Delta>0?-2:2);
			double ratio = (e.X - rGrid.Left) / (double)rGrid.Width;
			xaxis.Dmin -= vv*ratio;
			xaxis.Dmax += vv*(1-ratio);
			focusPointPos = -1;
		}
		lastmouse = e.Location;
		int i, len, cx, cy, found = -1;
		Util.PointsInRange_Binsearch(data, xaxis.c2d(e.X - HOVERRANGE), xaxis.c2d(e.X + HOVERRANGE), out int i0, out int i1);
		double dist, minDist = -1;
		for (; i0 <= i1; i0++) {
			cx = Math.Abs(xaxis.d2c(data[i0].x) - e.X);
			cy = Math.Abs(yaxis.d2c(data[i0].y) - e.Y);
			if (cx + cy > HOVERRANGE * 3 / 2)
				continue;
			dist = Math.Sqrt(cx * cx + cy * cy);
			if (dist <= HOVERRANGE && minDist < 0 || dist < minDist) {
				found = i0;
				minDist = dist;
			}
		}
		if(focusPointPos != found) {
			focusPointPos = found;
			lay = true;
		}
		if (ch) uiinvalid |= 1;
		else if (lay) uiinvalid |= 2;
	}

	void MyChart_MouseUp(object sender, MouseEventArgs e) {
		mousedownpos = new Point(-1, 0);
	}

	void MyChart_MouseDown(object sender, MouseEventArgs e) {
		mousedownpos = e.Location;
		xaxis.mousedownDmin = xaxis.Dmin;
		xaxis.mousedownDmax = xaxis.Dmax;
	}

	public void SetData(List<DataPoint> data, bool isDate = false, string unit = "") {
		this.data = data;
		if (data.Count > 0) {
			xaxis.Dmin = yaxis.Dmin = double.PositiveInfinity;
			xaxis.Dmax = yaxis.Dmax = double.NegativeInfinity;
			foreach(var v in data) {
				xaxis.UpdateMinMax(v.x);
				yaxis.UpdateMinMax(v.y);
			}
			if (xaxis.Dmax - xaxis.Dmin == 0) {
				xaxis.Dmin--;
				xaxis.Dmax++;
			}
			if(yaxis.Dmax - yaxis.Dmin == 0) {
				yaxis.Dmin--;
				yaxis.Dmax++;
			}
		}
		else {
			xaxis.Dmin = yaxis.Dmin = 0;
			xaxis.Dmax = yaxis.Dmax = 1;
		}
		xaxis.isDate = isDate;
		this.unit = unit;
		seriesColor = Color.FromArgb(unchecked((int)0xff6699ff));
	}

	void CalculateGrid() {
		xaxis.genTicks(xaxis.Dmax, xaxis.Dmin, 0, sdc, font1);
		yaxis.genTicks(yaxis.Dmax, yaxis.Dmin, 0, sdc, font1);
		Ylabelmaxw = yaxis.tickarr.Max(v => v.W);
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
		int cx0=int.MaxValue, cy0=0, cx1, cy1;
		pen.Color = seriesColor;
		sdc.Clip = new Region(rGrid);
		sdc.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
		var R = new DrawLineReducer(sdc,pen);
		for (i = 0; i < data.Count; i++) {
			var p = data[i];
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
		sdc.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
		pen.Width = 3;
		pen.Color = Color.FromArgb(0x66,0x66,0x66);
		sdc.ResetClip();
		sdc.DrawRectangle(pen, rGrid);
		PaintOverlay();
	}

	void PaintOverlay() {
		sdcover.Clear(Color.Transparent);
		string leg = $"{legend} = ";
		if (lastmouse.X >= rGrid.Left && lastmouse.X <= rGrid.Right && lastmouse.Y >= rGrid.Top && lastmouse.Y <= rGrid.Bottom) {
			pen.Width = 1;
			pen.Color = Color.Blue;
			sdcover.DrawLine(pen, lastmouse.X, rGrid.Top + 1, lastmouse.X, rGrid.Bottom - 1);
			double dx = xaxis.c2d(lastmouse.X);
			if (Util.GetInterpolatedY(data, xaxis.c2d(lastmouse.X), out double y))
				leg += y.ToString("0.000") + " " + unit;
		}
		if(focusPointPos.is_between(0, data.Count-1) && data[focusPointPos].x.is_between(xaxis.Dmin,xaxis.Dmax) && data[focusPointPos].y.is_between(yaxis.Dmin,yaxis.Dmax)) {
			var v = data[focusPointPos];
			pen.Width = 5;
			pen.Color = Color.FromArgb(0x60, Color.Blue);
			sdcover.DrawEllipse(pen, new Rectangle(xaxis.d2c(v.x) - 10, yaxis.d2c(v.y) - 10, 20, 20));
		}
		var wh = sdcover.MeasureString(leg, font1);
		pen.Width = 1;
		pen.Color = Color.FromArgb(0xff,0xe4,0xe4,0xe4);
		sdcover.DrawRectangle(pen, rGrid.Right - wh.Width - 5 - 20, rGrid.Top + 8, 20, 15);
		brush.Color = seriesColor;
		sdcover.FillRectangle(brush, rGrid.Right - wh.Width - 5 - 20 + 2, rGrid.Top + 8 + 4, 16, 9);
		var legrect = new RectangleF(rGrid.Right - wh.Width - 5, rGrid.Top + 5, wh.Width, wh.Height);
		sdcover.FillRectangle(Brushes.White, legrect);
		sdcover.DrawString(leg, font1, Brushes.Black, legrect.Left, legrect.Top);
	}
}

public static class Util {
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
	const double MERGEANGLE = Math.PI / 18;
	const int MERGEMAXXDIFF = 3;

	public DrawLineReducer(Graphics sdc, Pen pen) : this() {
		this.sdc = sdc;
		this.pen = pen;
	}

	public void Finish() {
		if(lines.Count > 0) {
			sdc.DrawLine(pen, lines[0], lines[1], lines[lines.Count - 4], lines[lines.Count - 3]);
			lines.Clear();
		}
	}

	public void AddLine(int x0, int y0, int x1, int y1) {
		if(lines.Count == 0) {
			if (x1 - x0 >= MERGEMAXXDIFF) {
				sdc.DrawLine(pen, x0, y0, x1, y1);
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
				sdc.DrawLine(pen, lines[0], lines[1], lines[lines.Count - 2], lines[lines.Count - 1]);
				lines.Clear();
				if(x1 - x0 >= MERGEMAXXDIFF) {
					sdc.DrawLine(pen, x0, y0, x1, y1);
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
		double delta = (max - min) / tickcount;
		int dec = -(int)Math.Floor(Math.Log10(delta));
		if (cTickDecimals > 0 && dec > cTickDecimals)
			dec = cTickDecimals;
		double magn = Math.Pow(10, -dec);
		double norm = delta / magn; // 1.0 ~ 10.0
		double size;
		/*
			0.1 ~ 0.15, 10 ticks , 0.005
			dec = 3
			magn = 0.001
			norm = 5
		 */
		//0.025 -> -2 0.01
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
			if (isDate) tickstr = F.EPOCH.AddMilliseconds(start).ToString("HH:mm:ss");
			else tickstr = start.ToString("f" + dec);
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
		this.x = (x - F.EPOCH).TotalMilliseconds;
		this.y = y;
	}
}
