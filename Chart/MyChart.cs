using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Q.Chart;

public sealed class MyChart : Control {
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
	public int uiinvalid = 0;
	public List<Series> series = new List<Series>();
	public Config C = new Config();

	public MyChart() {
		MouseMove += MyChart_MouseMove;
		Paint += MyChart_Paint;
		Resize += MyChart_Resize;
		MouseDown += MyChart_MouseDown;
		MouseUp += MyChart_MouseUp;
		MouseWheel += MyChart_MouseMove;
		DoubleBuffered = true;
		W = xaxis.ScreenWorH = Math.Max(1,Width);
		H = yaxis.ScreenWorH = Math.Max(1,Height);
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
		if (uiinvalid != 0 && Width >= 10 && Height >= 10) {
			if ((uiinvalid & 4) != 0) {
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
				PaintChart();
				Invalidate();
			}
			else if ((uiinvalid & 1) != 0) {
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

	public void SetData(Config c) {
		c.Init();
		xaxis.datamin = yaxis.datamin = double.PositiveInfinity;
		xaxis.datamax = yaxis.datamax = double.NegativeInfinity;
		foreach (var s in c.series) {
			foreach (var v in s.data) {
				xaxis.UpdateDataMinMax(v.x);
				yaxis.UpdateDataMinMax(v.y);
			}
		}
		xaxis.isDate = c.isDate;
		this.series = c.series;
		this.C = c;
		SetDminDmax();
	}

	void SetDminDmax() {
		if (C._Xhasminmax) {
			xaxis.Dmin = C.Xmin;
			xaxis.Dmax = C.Xmax;
		}
		else {
			xaxis.Dmin = xaxis.datamin;
			xaxis.Dmax = xaxis.datamax;
		}
		if (C._Yhasminmax) {
			yaxis.Dmin = C.Ymin;
			yaxis.Dmax = C.Ymax;
		}
		else {
			yaxis.Dmin = yaxis.datamin;
			yaxis.Dmax = yaxis.datamax;
			if (C.Ypadding > 0) {
				double availY = H - font1CharHeight - 45, pad;
				if (C.Ypadding < 0.5)
					pad = (yaxis.Dmax - yaxis.Dmin) * C.Ypadding;
				else pad = (availY / (availY - C.Ypadding * 2) - 1) / 2 * (yaxis.Dmax - yaxis.Dmin);
				if (pad < 0 || availY < pad * 2) pad = 0;
				yaxis.Dmin = yaxis.Dmin - pad;
				yaxis.Dmax = yaxis.Dmax + pad;
			}
		}
		if (xaxis.Dmin > xaxis.Dmax) {
			xaxis.Dmin = 0;
			xaxis.Dmax = 1;
		}
		else if (xaxis.Dmax - xaxis.Dmin == 0) {
			xaxis.Dmin--;
			xaxis.Dmax++;
		}
		if (yaxis.Dmin > yaxis.Dmax) {
			yaxis.Dmin = 0;
			yaxis.Dmax = 1;
		}
		else if (yaxis.Dmax - yaxis.Dmin == 0) {
			yaxis.Dmin--;
			yaxis.Dmax++;
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
		if (Width <= 0 || Height <= 0)
			return;
		lastmouse.X = -1;
		focusSeries = -1;
		if (W <= 1 || H <= 1)
			Timer_Tick(null, null);
		uiinvalid |= 4;
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
		if(mousedownpos.X >= 0 && C.enablePan) {
			if (Math.Abs(lastmouse.X - e.X) >= 1) {
				int deltaX = e.X - mousedownpos.X;
				double vv = deltaX / (double)rGrid.Width * (xaxis.mousedownDmin - xaxis.mousedownDmax);
				if (C._Xhaspanminmax) {
					if (vv < 0 & xaxis.mousedownDmin + vv < C.Xpanmin)
						vv = C.Xpanmin - xaxis.mousedownDmin;
					else if (vv > 0 && xaxis.mousedownDmax + vv > C.Xpanmax)
						vv = C.Xpanmax - xaxis.mousedownDmax;
				}
				xaxis.Dmin = xaxis.mousedownDmin + vv;
				xaxis.Dmax = xaxis.mousedownDmax + vv;
				uiinvalid |= 1;
			}
			if (Math.Abs(lastmouse.Y - e.Y) >= 1) {
				int deltaY = e.Y - mousedownpos.Y;
				double vv = deltaY / (double)rGrid.Height * (yaxis.mousedownDmax - yaxis.mousedownDmin);
				if (C._Yhaspanminmax) {
					if (vv < 0 & yaxis.mousedownDmin + vv < C.Ypanmin)
						vv = C.Ypanmin - yaxis.mousedownDmin;
					else if (vv > 0 && yaxis.mousedownDmax + vv > C.Ypanmax)
						vv = C.Ypanmax - yaxis.mousedownDmax;
				}
				yaxis.Dmin = yaxis.mousedownDmin + vv;
				yaxis.Dmax = yaxis.mousedownDmax + vv;
				uiinvalid |= 1;
			}
		}
		if(e.Delta != 0 && C.enablePan) {
			uiinvalid |= 1;
			focusPointPos = -1;
			if (ModifierKeys == Keys.Control) {
				double vv = (yaxis.Dmax - yaxis.Dmin) * 0.1 * (e.Delta > 0 ? -2 : 2);
				double ratio = (e.Y - rGrid.Top) / (double)rGrid.Bottom;
				yaxis.Dmin -= vv * ratio;
				yaxis.Dmax += vv * (1 - ratio);
				if (C._Yhaspanminmax) {
					if (yaxis.Dmin < C.Ypanmin)
						yaxis.Dmin = C.Ypanmin;
					if (yaxis.Dmax > C.Ypanmax)
						yaxis.Dmax = C.Ypanmax;
				}
			}
			else {
				double vv = (xaxis.Dmax - xaxis.Dmin) * 0.1 * (e.Delta > 0 ? -2 : 2);
				double ratio = (e.X - rGrid.Left) / (double)rGrid.Width;
				xaxis.Dmin -= vv * ratio;
				xaxis.Dmax += vv * (1 - ratio);
				if (C._Xhaspanminmax) {
					if (xaxis.Dmin < C.Xpanmin)
						xaxis.Dmin = C.Xpanmin;
					if (xaxis.Dmax > C.Xpanmax)
						xaxis.Dmax = C.Xpanmax;
				}
			}
		}
		lastmouse = e.Location;
		uiinvalid |= 2;
		int i, cx, cy, found = -1, foundSeries = -1;
		double dist, minDist = -1;
		if (C.enableHover) {
			for (i = 0; i < series.Count; i++) {
				var s = series[i];
				Util.PointsInRange_Binsearch(s.data, xaxis.c2d(e.X - HOVERRANGE), xaxis.c2d(e.X + HOVERRANGE), out int i0, out int i1);
				for (; i0 <= i1; i0++) {
					if (double.IsNaN(s.data[i0].y))
						continue;
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
		}
		if(focusPointPos != found || focusSeries != foundSeries) {
			focusPointPos = found;
			focusSeries = foundSeries;
			uiinvalid |= 2;
		}
	}

	void MyChart_MouseUp(object sender, MouseEventArgs e) {
		mousedownpos.X = -1;
	}

	void MyChart_MouseDown(object sender, MouseEventArgs e) {
		mousedownpos = e.Location;
		xaxis.mousedownDmin = xaxis.Dmin;
		xaxis.mousedownDmax = xaxis.Dmax;
		yaxis.mousedownDmin = yaxis.Dmin;
		yaxis.mousedownDmax = yaxis.Dmax;
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
		if (W < 100 || H < 100) return;
		CalculateGrid();
		pen.Width = 1;
		pen.Color = Color.FromArgb(0xcc,0xcc,0xcc);
		foreach(var ytick in yaxis.tickarr) {
			if (ytick.value < yaxis.Dmin || ytick.value > yaxis.Dmax)
				continue;
			j = yaxis.d2c(ytick.value);
			sdc.DrawString(ytick.strvalue, font1, Brushes.Black, 1 + (Ylabelmaxw - ytick.W), j - ytick.H / 2);
			if(C.drawGrid) sdc.DrawLine(pen,rGrid.Left,j,rGrid.Right,j);
		}
		foreach(var xtick in xaxis.tickarr) {
			if (xtick.value < xaxis.Dmin || xtick.value > xaxis.Dmax)
				continue;
			j = xaxis.d2c(xtick.value);
			sdc.DrawString(xtick.strvalue, font1, Brushes.Black, j-xtick.W/2, H-5-font1CharHeight);
			if(C.drawGrid) sdc.DrawLine(pen,j,rGrid.Top,j,rGrid.Bottom);
		}
		sdc.SmoothingMode = SmoothingMode.HighQuality;
		int cx0, cy0, cx1, cy1;
		var R = C._drawLineReducer;
		R.Init(sdc, bmp, pen, rGrid);
		foreach (var s in series) {
			pen.Color = s.color;
			cx0 = int.MaxValue;
			cy0 = 0;
			Util.PointsInRange2_Binsearch(s.data, xaxis.c2d(rGrid.Left), xaxis.c2d(rGrid.Right), out int i0, out int i1);
			for (; i0 <= i1; i0++) {
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
		sdc.DrawRectangle(pen, rGrid);
		MyChart_MouseMove(null, new MouseEventArgs(MouseButtons.None, 0, lastmouse.X, lastmouse.Y, 0));
		uiinvalid = 0;
		PaintOverlay();
	}

	void PaintOverlay() {
		sdcover.Clear(Color.Transparent);
		int i;
		bool inGrid = false;
		if (series.Count == 0)
			return;
		if (C.enableHover && lastmouse.X >= rGrid.Left && lastmouse.X <= rGrid.Right && lastmouse.Y >= rGrid.Top && lastmouse.Y <= rGrid.Bottom) {
			pen.Width = 1;
			pen.Color = Color.Blue;
			sdcover.DrawLine(pen, lastmouse.X, rGrid.Top + 1, lastmouse.X, rGrid.Bottom - 1);
			inGrid = true; 
			if (C.showHoverXPos) {
				double x = xaxis.c2d(lastmouse.X);
				string msg;
				if (xaxis.isDate) msg = Util.FromTs(x).ToString("HH:mm:ss");
				else msg = Util.ToDec(x, xaxis._nowtickdec);
				var wh = sdcover.MeasureString(msg, font1);
				Rectangle rect = new Rectangle(lastmouse.X+1,0,(int)wh.Width,(int)wh.Height-3);
				if (rect.Right >= W) rect.X = lastmouse.X - 2 - (int)wh.Width;
				sdcover.FillRectangle(Brushes.White, rect);
				sdcover.DrawString(msg, font1, Brushes.Black, rect.Left, rect.Top);
			}
		}
		if (focusSeries >= 0) {
			var s = series[focusSeries];
			if (focusPointPos >= 0 && focusPointPos < s.data.Count && s.data[focusPointPos].x >= xaxis.Dmin && s.data[focusPointPos].x <= xaxis.Dmax && s.data[focusPointPos].y >= yaxis.Dmin && s.data[focusPointPos].y <= yaxis.Dmax) {
				var v = s.data[focusPointPos];
				pen.Width = 5;
				pen.Color = Color.FromArgb(0x60, s.color);
				sdcover.DrawEllipse(pen, new Rectangle(xaxis.d2c(v.x) - 10, yaxis.d2c(v.y) - 10, 20, 20));
				string msg = null;
				if(C.isDate)
					msg = $"{Util.FromTs(v.x):yyyy-MM-dd HH:mm:ss}, {Util.ToDec(v.y, s.dec)} {s.unit}";
				else msg = $"{v.x}, {Util.ToDec(v.y, s.dec)} {s.unit}";
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
			var wh = sdcover.MeasureString(s.legend + (C.enableHover?" = ":""), font1);
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
			sdcover.DrawString(s.legend+(C.enableHover?" = ":""), font1, Brushes.Black, legrect.Left + legwid - s._legendwid, legrect.Top);
			sdcover.DrawString(s._hovervalue, font1, Brushes.Black, legrect.Left + legwid + hovwid - s._hovervaluewid, legrect.Top);
			ypos += rowhei;
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
		this.x = Util.ToTs(x);
		this.y = y;
	}
}
