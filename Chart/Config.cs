using System.Drawing;

namespace Q.Chart;
public class Config {
	public List<Series> series;
	public double Xmin, Xmax, Xpanmin, Xpanmax;
	public bool isDate;
}

public class Series {
	public List<DataPoint> data;
	public string legend, unit;
	public byte dec;
	public Color color;
	//private
	public string _hovervalue;
	public int _legendwid, _hovervaluewid;
}
