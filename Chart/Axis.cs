using System.Drawing;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Q.Chart;

public sealed class Axis {
	public bool isDate, isY;
	public int ScreenWorH, Cmin, Cmax = 1;
	public double Dmin, Dmax = 1, datamax, datamin, mousedownDmin, mousedownDmax;
	public short tickcount;
	public List<TickLabel> tickarr = new List<TickLabel>(), cTicks;
	//private
	public int _nowtickdec;

	public bool cisNeedDrawLine(int c1, int c2) {
		if (isY) {
			if (c1 < Cmax) return c2 >= Cmax;
			else if (c1 <= Cmin) return true;
			else return c2 <= Cmin;
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

	public void UpdateDMinMax(double xy) {
		if (xy < Dmin) Dmin = xy;
		if (xy > Dmax) Dmax = xy;
	}

	public void UpdateDataMinMax(double xy, bool setD = false) {
		if (xy < datamin) datamin = xy;
		if (xy > datamax) datamax = xy;
		if (setD) {
			Dmin = datamin;
			Dmax = datamax;
		}
	}

	enum TickType {
		Second,Minute,Hour,Day,Month,Year
	}
	readonly static int[] TICKTYPESIZES = new int[] { 1, 60, 3600, 86400, 86400*30, 86400*365 };
	struct TickDefine {
		public float nums;
		public TickType unit;
		public double inSecond;
		public TickDefine(float nums, TickType unit) {
			this.nums = nums;
			this.unit = unit;
			inSecond = nums * TICKTYPESIZES[(int)unit];
		}
	}
	readonly static TickDefine[] ALLOWTICKS = new TickDefine[] {
		new TickDefine(1, TickType.Second),
		new TickDefine(2, TickType.Second),
		new TickDefine(5, TickType.Second),
		new TickDefine(10, TickType.Second),
		new TickDefine(30, TickType.Second),
		new TickDefine(1, TickType.Minute),
		new TickDefine(2, TickType.Minute),
		new TickDefine(5, TickType.Minute),
		new TickDefine(10, TickType.Minute),
		new TickDefine(30, TickType.Minute),
		new TickDefine(1, TickType.Hour),
		new TickDefine(2, TickType.Hour),
		new TickDefine(4, TickType.Hour),
		new TickDefine(8, TickType.Hour),
		new TickDefine(12, TickType.Hour),
		new TickDefine(1, TickType.Day),
		new TickDefine(2, TickType.Day),
		new TickDefine(3, TickType.Day),
		new TickDefine(5, TickType.Day),
		new TickDefine(10, TickType.Day),
		new TickDefine(1, TickType.Month),
		new TickDefine(2, TickType.Month),
		new TickDefine(3, TickType.Month),
		new TickDefine(6, TickType.Month),
		new TickDefine(1, TickType.Year),
		new TickDefine(2, TickType.Year),
		new TickDefine(5, TickType.Year),
		new TickDefine(10, TickType.Year),
		new TickDefine(20, TickType.Year),
		new TickDefine(50, TickType.Year),
		new TickDefine(100, TickType.Year),
	};
	public void genDateTicks(double max, double min, Graphics sdc, Font font1) {
		double delta = max - min;
		int i;
		int tickcount = (short)(int)(0.3 * Math.Sqrt(ScreenWorH));
		for (i = 0; i < ALLOWTICKS.Length-1; i++) {
			if (delta < ALLOWTICKS[i].inSecond * tickcount)
				break;
		}
		var t = ALLOWTICKS[i];
		DateTime c = Util.FromTs(min);
		switch (t.unit) {
			default:
			case TickType.Second:
				c = new DateTime(c.Year,c.Month,c.Day,c.Hour,c.Minute,(c.Second / (int)t.nums * (int)t.nums));
				break;
			case TickType.Minute:
				c = new DateTime(c.Year,c.Month,c.Day,c.Hour,(c.Minute / (int)t.nums * (int)t.nums),0);
				break;
			case TickType.Hour:
				c = new DateTime(c.Year,c.Month,c.Day,(c.Hour / (int)t.nums * (int)t.nums),0,0);
				break;
			case TickType.Day:
				c = c.Date;
				break;
			case TickType.Month:
				c = new DateTime(c.Year, (int)((c.Month - 1) / t.nums) * (int)t.nums + 1, 1);
				break;
			case TickType.Year:
				c = new DateTime(c.Year, 1, 1);
				break;
		}
		tickarr.Clear();
		double prev, v = double.NaN, carry = 0;
		string tickstr;
		do {
			prev = v;
			v = Util.ToTs(c);
			tickstr = DateTickFormat(c, delta, t);
			var wh = sdc.MeasureString(tickstr, font1);
			tickarr.Add(new TickLabel {
				value = v,
				strvalue = tickstr,
				W = (int)Math.Ceiling(wh.Width),
				H = (int)Math.Ceiling(wh.Height),
			});
			if(t.unit == TickType.Month) {
				if (t.nums < 1) {
					//0.25 month
				}
				else c = c.AddMonths((int)t.nums);
			}
			else if(t.unit == TickType.Year) {
				c = c.AddYears((int)t.nums);
			}
			else {
				c = c.AddSeconds(t.inSecond);
			}
		}
		while (v < max && v != prev && tickarr.Count < 100);
	}

	static string DateTickFormat(DateTime d, double span, TickDefine T) {
		if (T.inSecond < 60) return d.ToString("HH:mm:ss");
		else if (T.inSecond < 86400) {
			if (span < 86400 * 2) return d.ToString("HH:mm");
			else return d.ToString("MM-dd HH:mm");
		}
		else if (T.inSecond < 86400 * 30) return d.ToString("MM-dd");
		else if (T.inSecond < 86400 * 365) {
			if (span < 86400 * 365) return d.ToString("MM月");
			else return d.ToString("yyyy-MM");
		}
		else return d.ToString("yyyy");
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
		else if (isDate) {
			genDateTicks(max, min, sdc, font1);
			return;
		}
		tickcount = (short)(int)(0.3 * Math.Sqrt(ScreenWorH));
		double delta;
		int dec;
		float charW = sdc.MeasureString("0000000000", font1).Width / 10 + 2;
		while (true) {
			delta = (max - min) / tickcount;
			dec = -(int)Math.Floor(Math.Log10(delta));
			_nowtickdec = Math.Max(dec + 2, 0);
			int tickwid = (int)(Math.Max(Util.ToDec(min,dec).Length, Util.ToDec(max, dec).Length) * charW) + 5;
			if (!isY && tickcount > 2 && tickwid * tickcount > ScreenWorH - 20)
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
		tickarr.Clear();
		double start = Math.Floor(min / ticksize) * ticksize, v;
		if (start < min)
			start += ticksize;
		while(start < max) {
			string tickstr;
			if (isDate) tickstr = MyChart.EPOCH.AddSeconds(start).ToString("HH:mm:ss");
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
