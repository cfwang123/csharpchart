using System.ComponentModel;
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
	Font font;
	SolidBrush brush;
	int W, H, font1CharWidth, font1CharHeight;
	Timer timer;
	Point lastmouse = new Point(-1, -1), mousedownpos = new Point(-1, -1);
	Rectangle rGrid;
	int focusSeries = 0, focusPointPos = 2;
	public Axis xaxis = new Axis();
	public List<Axis> yaxes = new List<Axis>() { new Axis { isY = true } };
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
		H = Math.Max(1,Height);
		bmp = new Bitmap(W, H);
		bmpover = new Bitmap(W, H);
		sdc = Graphics.FromImage(bmp);
		sdcover = Graphics.FromImage(bmpover);
		font = new Font("宋体",12);
		font1CharWidth = (int)(sdc.MeasureString("00000", font).Width / 5);
		font1CharHeight = (int)sdc.MeasureString("X靐", font).Height;
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
				H = Height;
				foreach (var v in yaxes)
					v.ScreenWorH = H;
				bmp = new Bitmap(W,H);
				bmpover = new Bitmap(W,H);
				sdc = Graphics.FromImage(bmp);
				sdcover = Graphics.FromImage(bmpover);
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

	public Font ChartFont {
		get { return font; }
		set {
			if (value != null) {
				font.Dispose();
				font = value;
				uiinvalid |= 1;
			}
		}
	}

	public void SetData(Config c) {
		c._init();
		yaxes.Clear();
		foreach (var v in c.yaxes) {
			bool hasminmax = v.Ymax > v.Ymin;
			yaxes.Add(new Axis {
				isY = true,
				datamin = hasminmax ? v.Ymin : double.PositiveInfinity,
				datamax = hasminmax ? v.Ymax : double.NegativeInfinity,
				_hasminmax = hasminmax,
				cDmin = v.Ymin,
				cDmax = v.Ymax,
				cTicks = v.cTickLabels,
				title = v.title,
				unit = v.unit,
				ScreenWorH = H,
			});
		}
		xaxis.datamin = double.PositiveInfinity;
		xaxis.datamax = double.NegativeInfinity;
		xaxis.cTicks = c.cXTickLabels;
		foreach (var s in c.series) {
			var y = yaxes[s.yaxis];
			foreach (var v in s.data) {
				xaxis.UpdateDataMinMax(v.x);
				y.UpdateDataMinMax(v.y);
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
		if (xaxis.Dmin > xaxis.Dmax) {
			xaxis.Dmin = 0;
			xaxis.Dmax = 1;
		}
		else if (xaxis.Dmax - xaxis.Dmin == 0) {
			xaxis.Dmin--;
			xaxis.Dmax++;
		}
		int i;
		for (i = 0; i < yaxes.Count; i++) {
			var y = yaxes[i];
			if (y._hasminmax) {
				y.Dmin = C.yaxes[i].Ymin;
				y.Dmax = C.yaxes[i].Ymax;
			}
			else {
				y.Dmin = y.datamin;
				y.Dmax = y.datamax;
				if (C.Ypadding > 0) {
					double availY = H - font1CharHeight - 45, pad = 0;
					if (C.Ypadding < 0.5)
						pad = (y.Dmax - y.Dmin) * C.Ypadding;
					if (pad < 0 || availY < pad * 2) pad = 0;
					y.Dmin = y.Dmin - pad;
					y.Dmax = y.Dmax + pad;
				}
			}
			if (y.Dmin > y.Dmax) {
				y.Dmin = 0;
				y.Dmax = 1;
			}
			else if (y.Dmax - y.Dmin == 0) {
				y.Dmin--;
				y.Dmax++;
			}
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
		font.Dispose();
		base.Dispose(disposing);
	}

	void MyChart_Resize(object sender, EventArgs e) {
		if (Width <= 0 || Height <= 0)
			return;
		lastmouse.X = -1;
		focusSeries = -1;
		uiinvalid |= 4;
		if (W <= 1 || H <= 1)
			Timer_Tick(null, null);
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
		if(mousedownpos.X >= 0 && (C.panX.enablePan || C.panY.enablePan)) {
			if (C.panX.enablePan && Math.Abs(lastmouse.X - e.X) >= 1) {
				int deltaX = e.X - mousedownpos.X;
				double vv = deltaX / (double)rGrid.Width * (xaxis.mousedownDmin - xaxis.mousedownDmax);
				if (C.panX._hasminmax) {
					if (vv < 0 & xaxis.mousedownDmin + vv < C.panX.Dmin)
						vv = C.panX.Dmin - xaxis.mousedownDmin;
					else if (vv > 0 && xaxis.mousedownDmax + vv > C.panX.Dmax)
						vv = C.panX.Dmax - xaxis.mousedownDmax;
				}
				xaxis.Dmin = xaxis.mousedownDmin + vv;
				xaxis.Dmax = xaxis.mousedownDmax + vv;
				uiinvalid |= 1;
			}
			if (C.panY.enablePan && Math.Abs(lastmouse.Y - e.Y) >= 1) {
				int deltaY = e.Y - mousedownpos.Y;
				foreach (var y in yaxes) {
					double vv = deltaY / (double)rGrid.Height * (y.mousedownDmax - y.mousedownDmin);
					if (C.panY._hasminmax && yaxes.Count == 1) {
						if (vv < 0 & y.mousedownDmin + vv < C.panY.Dmin)
							vv = C.panY.Dmin - y.mousedownDmin;
						else if (vv > 0 && y.mousedownDmax + vv > C.panY.Dmax)
							vv = C.panY.Dmax - y.mousedownDmax;
					}
					y.Dmin = y.mousedownDmin + vv;
					y.Dmax = y.mousedownDmax + vv;
				}
				uiinvalid |= 1;
			}
		}
		if(e.Delta != 0 && (C.panX.enableZoom || C.panY.enableZoom)) {
			uiinvalid |= 1;
			focusPointPos = -1;
			if (ModifierKeys == Keys.Control) {
				if (C.panY.enableZoom) {
					foreach (var y in yaxes) {
						double vv = (y.Dmax - y.Dmin) * 0.1 * (e.Delta > 0 ? -2 : 2);
						if (e.Delta > 0 && C.panY.Dmindelta > 0 && y.Dmax - y.Dmin + vv < C.panY.Dmindelta)
							vv = C.panY.Dmindelta - (y.Dmax - y.Dmin);
						double ratio = 1 - (e.Y - rGrid.Top) / (double)rGrid.Bottom;
						y.Dmin -= vv * ratio;
						y.Dmax += vv * (1 - ratio);
						if (C.panY._hasminmax && yaxes.Count == 1) {
							if (y.Dmin < C.panY.Dmin)
								y.Dmin = C.panY.Dmin;
							if (y.Dmax > C.panY.Dmax)
								y.Dmax = C.panY.Dmax;
						}
					}
				}
			}
			else if(C.panX.enableZoom) {
				double vv = (xaxis.Dmax - xaxis.Dmin) * 0.1 * (e.Delta > 0 ? -2 : 2);
				if (e.Delta > 0 && C.panX.Dmindelta > 0 && xaxis.Dmax - xaxis.Dmin + vv < C.panX.Dmindelta)
					vv = C.panX.Dmindelta - (xaxis.Dmax - xaxis.Dmin);
				if (vv != 0) {
					double ratio = (e.X - rGrid.Left) / (double)rGrid.Width;
					xaxis.Dmin -= vv * ratio;
					xaxis.Dmax += vv * (1 - ratio);
					if (C.panX._hasminmax) {
						if (xaxis.Dmin < C.panX.Dmin)
							xaxis.Dmin = C.panX.Dmin;
						if (xaxis.Dmax > C.panX.Dmax)
							xaxis.Dmax = C.panX.Dmax;
					}
				}
			}
		}
		lastmouse = e.Location;
		if(C.enableCross || C.enableHover) uiinvalid |= 2;
		int i, cx, cy, found = -1, foundSeries = -1;
		double dist, minDist = -1, dx, dy;
		if (C.enableHover && rGrid.Contains(lastmouse.X, lastmouse.Y)) {
			for (i = 0; i < series.Count; i++) {
				var s = series[i];
				var y = yaxes[s.yaxis];
				if (s.line.show || s.point.show) {
					Util.PointsInRange_Binsearch(s.data, xaxis.c2d(e.X - HOVERRANGE), xaxis.c2d(e.X + HOVERRANGE), out int i0, out int i1);
					for (; i0 <= i1; i0++) {
						if (double.IsNaN(s.data[i0].y))
							continue;
						cx = Math.Abs(xaxis.d2c(s.data[i0].x) - e.X);
						cy = Math.Abs(y.d2c(s.data[i0].y) - e.Y);
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
				else if (s.bar.show) {
					double r = s.bar.width / 2;
					Util.PointsInRange_Binsearch(s.data, xaxis.c2d(e.X) - r, xaxis.c2d(e.X) + r, out int i0, out int i1);
					dx = xaxis.c2d(e.X);
					dy = y.c2d(e.Y);
					for (; i0 <= i1; i0++) {
						if (double.IsNaN(s.data[i0].y))
							continue;
						if (Math.Abs(s.data[i0].x - dx) <= r) {
							if (s.data[i0].y >= 0) {
								if (dy >= 0 && dy <= s.data[i0].y) {
									found = i0;
									foundSeries = i;
									break;
								}
							}
							else {
								if (dy < 0 && dy >= s.data[i0].y) {
									found = i0;
									foundSeries = i;
									break;
								}
							}
						}
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
		foreach (var y in yaxes) {
			y.mousedownDmin = y.Dmin;
			y.mousedownDmax = y.Dmax;
		}
	}

	void CalculateGrid() {
		xaxis.genTicks(xaxis.Dmax, xaxis.Dmin, 0, sdc, font);
		foreach (var y in yaxes)
			y.genTicks(y.Dmax, y.Dmin, 0, sdc, font);
		int i, j, rleft = 5, rright = W - 15;
		for (i = 0; i < yaxes.Count; i++) {
			var y = yaxes[i];
			y._isAtLeft = i % 2 == 0;
			y._drawSmallGrid = i >= 2;
		}
		for (i = 0; i < yaxes.Count; i++) {
			var y = yaxes[i];
			if (!y._isAtLeft) continue;
			y._left = 0;
			for (j = i + 1; j < yaxes.Count; j++) {
				if (!yaxes[j]._isAtLeft) continue;
				y._left += yaxes[j]._tickmaxW + 6;
			}
			if (i == 0)
				rleft = y._left + y._tickmaxW;
		}
		for (i = 1; i < yaxes.Count; i++) {
			var y = yaxes[i];
			if (y._isAtLeft) continue;
			y._left = W - 5 - y._tickmaxW;
			for (j = i + 1; j < yaxes.Count; j++) {
				if (!yaxes[j]._isAtLeft) continue;
				y._left -= yaxes[j]._tickmaxW + 6;
			}
			if (i == 1)
				rright = y._left - 1;
		}
		rGrid = new Rectangle(rleft, font1CharHeight, rright - rleft + 1, H - font1CharHeight * 2);
		xaxis.Cmin = rGrid.Left;
		xaxis.Cmax = rGrid.Right;
		foreach (var y in yaxes) {
			y.Cmin = rGrid.Bottom;
			y.Cmax = rGrid.Top;
		}
	}

	void DrawOneBar(Graphics sdc, int cx0, int cy0, int cx1, int cy1) {
		if(cy0 > cy1) {
			int t = cy0;
			cy0 = cy1;
			cy1 = t;
		}
		if (cx0 < rGrid.Left) cx0 = rGrid.Left;
		if (cx1 > rGrid.Right) cx1 = rGrid.Right;
		if (cy0 < rGrid.Top) cy0 = rGrid.Top;
		if (cy1 > rGrid.Bottom) cy1 = rGrid.Bottom;
		sdc.FillRectangle(brush, cx0, cy0, cx1 - cx0 + 1, cy1 - cy0 + 1);
	}

	public void PaintChart() {
		int i, j;
		sdc.Clear(Color.White);
		if (W < 100 || H < 100) return;
		CalculateGrid();
		pen.Width = 1;
		pen.Color = Color.FromArgb(0xcc,0xcc,0xcc);
		var f = font;
		foreach (var y in yaxes) {
			foreach (var ytick in y.tickarr) {
				if (ytick.value < y.Dmin || ytick.value > y.Dmax)
					continue;
				j = y.d2c(ytick.value);
				sdc.DrawString(ytick.strvalue, f, Brushes.Black, y._isAtLeft ? (y._left + y._tickmaxW - ytick.W) : (y._left + (y._drawSmallGrid ? 6 : 0)), j - ytick.H / 2);
				if (C.drawGridY) {
					if (!y._drawSmallGrid) {
						if(y._isAtLeft) sdc.DrawLine(pen, rGrid.Left, j, rGrid.Right, j);
					}
					else if (y._isAtLeft) sdc.DrawLine(pen, y._left + y._tickmaxW - 5, j, y._left + y._tickmaxW + 5, j);
					else sdc.DrawLine(pen, y._left, j, y._left + 10, j);
				}
			}
			if(y.title != "") {
				var w = sdc.MeasureString(y.title, f).Width;
				sdc.DrawString(y.title, f, Brushes.Black, y._left + (y._isAtLeft ? (y._tickmaxW - w) : 0), 0);
			}
			if (y._drawSmallGrid) {
				if (y._isAtLeft) {
					sdc.DrawLine(pen, y._left + y._tickmaxW + 5, rGrid.Top, y._left + y._tickmaxW + 5, rGrid.Bottom);
					sdc.DrawLine(pen, y._left + y._tickmaxW - 5, rGrid.Top, y._left + y._tickmaxW + 5, rGrid.Top);
					sdc.DrawLine(pen, y._left + y._tickmaxW - 5, rGrid.Bottom, y._left + y._tickmaxW + 5, rGrid.Bottom);
				}
				else {
					sdc.DrawLine(pen, y._left, rGrid.Top, y._left, rGrid.Bottom);
					sdc.DrawLine(pen, y._left, rGrid.Top, y._left + 10, rGrid.Top);
					sdc.DrawLine(pen, y._left, rGrid.Bottom, y._left + 10, rGrid.Bottom);
				}
			}
		}
		foreach(var xtick in xaxis.tickarr) {
			if (xtick.value < xaxis.Dmin || xtick.value > xaxis.Dmax)
				continue;
			j = xaxis.d2c(xtick.value);
			sdc.DrawString(xtick.strvalue, f, Brushes.Black, j-xtick.W/2, H-font1CharHeight);
			if(C.drawGridX) sdc.DrawLine(pen,j,rGrid.Top,j,rGrid.Bottom);
		}
		sdc.SmoothingMode = SmoothingMode.HighQuality;
		int cx0, cy0, cx1, cy1;
		var R = C._drawLineReducer;
		//bar
		foreach (var s in series) {
			if (!s.bar.show)
				continue;
			var y = yaxes[s.yaxis];
			var r = s.bar.width / 2;
			brush.Color = s.bar.color;
			brush.Color = Color.FromArgb(100, brush.Color);
			Util.PointsInRange_Binsearch(s.data, xaxis.c2d(rGrid.Left) - r, xaxis.c2d(rGrid.Right) + r, out int i0, out int i1);
			for (; i0 <= i1; i0++) {
				var p = s.data[i0];
				cx0 = xaxis.d2c(p.x - r);
				cx1 = xaxis.d2c(p.x + r);
				cy0 = y.d2c(0);
				cy1 = y.d2c(p.y);
				if (!double.IsNaN(p.y))
					DrawOneBar(sdc, cx0,cy0,cx1,cy1);
			}
		}
		//line
		R.Init(sdc, bmp, pen, rGrid);
		foreach (var s in series) {
			if (!s.line.show)
				continue;
			var y = yaxes[s.yaxis];
			pen.Color = s.line.color;
			pen.Width = s.line.lineWidth;
			cx0 = int.MaxValue;
			cy0 = 0;
			Util.PointsInRange2_Binsearch(s.data, xaxis.c2d(rGrid.Left), xaxis.c2d(rGrid.Right), out int i0, out int i1);
			for (; i0 <= i1; i0++) {
				var p = s.data[i0];
				cx1 = xaxis.d2c(p.x);
				cy1 = y.d2c(p.y);
				bool valid = !double.IsNaN(p.y);
				if (valid) {
					if (cx0 != int.MaxValue && xaxis.cisNeedDrawLine(cx0, cx1) && y.cisNeedDrawLine(cy0, cy1)) {
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
		//Point
		foreach (var s in series) {
			if (!s.point.show)
				continue;
			var y = yaxes[s.yaxis];
			var r = s.point.radius;
			brush.Color = s.point.fillColor;
			pen.Color = s.point.color;
			pen.Width = s.point.lineWidth;
			Util.PointsInRange_Binsearch(s.data, xaxis.c2d(rGrid.Left), xaxis.c2d(rGrid.Right), out int i0, out int i1);
			for (; i0 <= i1; i0++) {
				var p = s.data[i0];
				cx1 = xaxis.d2c(p.x);
				cy1 = y.d2c(p.y);
				bool valid = !double.IsNaN(p.y);
				if (valid && y.cisBetween(cy1)) {
					sdc.FillEllipse(brush, cx1 - r, cy1 - r, r + r, r + r);
					sdc.DrawEllipse(pen, cx1 - r, cy1 - r, r + r, r + r);
				}
			}
		}
		sdc.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
		pen.Width = 3;
		pen.Color = Color.FromArgb(0x66, 0x66, 0x66);
		sdc.DrawRectangle(pen, rGrid);
		MyChart_MouseMove(null, new MouseEventArgs(MouseButtons.None, 0, lastmouse.X, lastmouse.Y, 0));
		PaintOverlay();
	}

	void PaintOverlay() {
		sdcover.Clear(Color.Transparent);
		int i;
		bool inGrid = false;
		var f = font;
		if (series.Count == 0)
			return;
		if (lastmouse.X >= rGrid.Left && lastmouse.X <= rGrid.Right && lastmouse.Y >= rGrid.Top && lastmouse.Y <= rGrid.Bottom) {
			inGrid = true;
			if (C.enableCross) {
				pen.Width = 1;
				pen.Color = Color.Blue;
				sdcover.DrawLine(pen, lastmouse.X, rGrid.Top + 1, lastmouse.X, rGrid.Bottom - 1);
				if (C.showHoverXPos) {
					double x = xaxis.c2d(lastmouse.X);
					string msg;
					if (xaxis.isDate) msg = Util.FromTs(x).ToString("HH:mm:ss");
					else msg = Util.ToDec(x, xaxis._nowtickdec);
					var wh = sdcover.MeasureString(msg, f);
					Rectangle rect = new Rectangle(lastmouse.X+1,0,(int)wh.Width,Math.Min((int)wh.Height-3,rGrid.Top-2));
					if (rect.Right >= rGrid.Right) rect.X = lastmouse.X - 2 - (int)wh.Width;
					sdcover.FillRectangle(Brushes.White, rect);
					sdcover.DrawString(msg, f, Brushes.Black, rect.Left, rect.Top);
				}
			}
		}
		if (focusSeries >= 0) {
			var s = series[focusSeries];
			var y = yaxes[s.yaxis];
			if (focusPointPos >= 0 && focusPointPos < s.data.Count && s.data[focusPointPos].x >= xaxis.Dmin && s.data[focusPointPos].x <= xaxis.Dmax && s.data[focusPointPos].y >= y.Dmin && s.data[focusPointPos].y <= y.Dmax) {
				var v = s.data[focusPointPos];
				if(s.line.show || s.point.show) {
					pen.Width = 5;
					pen.Color = Color.FromArgb(0x60, s.color);
					sdcover.DrawEllipse(pen, new Rectangle(xaxis.d2c(v.x) - 10, y.d2c(v.y) - 10, 20, 20));
				}
				string msg = null;
				if (s.hovermsg != null) msg = s.hovermsg(focusPointPos, v);
				else if(C.isDate)
					msg = $"{Util.FromTs(v.x):yyyy-MM-dd HH:mm:ss}, {Util.ToDec(v.y, s.dec)} {s.unit}";
				else if(s.line.show || s.point.show) msg = $"{v.x}, {Util.ToDec(v.y, s.dec)} {s.unit}";
				else msg = $"{Util.ToDec(v.y, s.dec)} {s.unit}";
				var wh = sdcover.MeasureString(msg, f);
				Rectangle rect = new Rectangle(lastmouse.X + 10, lastmouse.Y + 10, (int)wh.Width + 4, (int)wh.Height + 4);
				if (lastmouse.X + 10 + wh.Width + 4 >= rGrid.Right) rect.X = lastmouse.X - 10 - (int)wh.Width;
				if (lastmouse.Y + 10 + wh.Height + 4 >= rGrid.Bottom) rect.Y = lastmouse.Y - 10 - (int)wh.Height;
				pen.Width = 1;
				pen.Color = Color.Black;
				if (s.bar.show) {
					brush.Color = Color.FromArgb(100,s.bar.color);
					DrawOneBar(sdcover, xaxis.d2c(v.x - s.bar.width / 2), y.d2c(0), xaxis.d2c(v.x+s.bar.width/2), y.d2c(v.y));
				}
				sdcover.FillRectangle(Brushes.White, rect);
				sdcover.DrawRectangle(pen, rect);
				sdcover.DrawString(msg, f, Brushes.Black, rect.Left + 2, rect.Top + 2);
			}
		}
		int legwid = 0, hovwid = 0, rowhei = 0;
		for (i = 0; i < series.Count; i++) {
			var s = series[i];
			if (inGrid && Util.GetInterpolatedY(s.data, xaxis.c2d(lastmouse.X), out double y) && C.enableCross)
				s._hovervalue = Util.ToDec(y, s.dec) + " " + s.unit;
			else s._hovervalue = "";
			var wh = sdcover.MeasureString(s.legend + (C.enableCross?" = ":""), f);
			s._legendwid = (int)wh.Width;
			s._hovervaluewid = (int)sdcover.MeasureString(s._hovervalue, f).Width;
			rowhei = (int)wh.Height;
			legwid = Math.Max(legwid, s._legendwid);
			hovwid = Math.Max(hovwid, s._hovervaluewid);
		}
		int boxW, boxH;
		float ypos = rGrid.Top;
		boxW = font1CharWidth - 2;
		boxH = Math.Min(15, rowhei);
		for (i = 0; i < series.Count; i++) {
			var s = series[i];
			pen.Width = 1;
			pen.Color = Color.FromArgb(0xff,0xe4,0xe4,0xe4);
			sdcover.DrawRectangle(pen, rGrid.Right - legwid - hovwid - 5 - 20, ypos+(rowhei - boxH)/2+3, 20, boxH);
			brush.Color = s.color;
			sdcover.FillRectangle(brush, rGrid.Right - legwid - hovwid - 25, ypos+(rowhei - boxH)/2+7, 16, boxH - 6);
			var legrect = new RectangleF(rGrid.Right - legwid - hovwid - 5, ypos + 5, legwid + hovwid, rowhei);
			sdcover.FillRectangle(Brushes.White, legrect);
			sdcover.DrawString(s.legend+(C.enableCross?" = ":""), f, Brushes.Black, legrect.Left + legwid - s._legendwid, legrect.Top);
			sdcover.DrawString(s._hovervalue, f, Brushes.Black, legrect.Left + legwid + hovwid - s._hovervaluewid, legrect.Top);
			ypos += rowhei;
		}
	}
}
