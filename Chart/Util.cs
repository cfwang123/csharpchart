namespace Q.Chart;

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

	public static DateTime FromTs(double d) => MyChart.EPOCH.AddSeconds(d);
	public static double ToTs(DateTime d) => (d - MyChart.EPOCH).TotalSeconds;

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
		from = Math.Max(0, e);
		s = from;
		e = data.Count - 1;
		while (s <= e) {
			m = (s + e) / 2;
			if (data[m].x <= x1 || double.IsNaN(data[m].y)) s = m + 1;
			else e = m - 1;
		}
		to = Math.Min(data.Count - 1, s);
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
