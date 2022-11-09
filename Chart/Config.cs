using System.Drawing;

namespace Q.Chart;
public sealed class Config {
	public List<Series> series;
	public List<TickLabel> cXTickLabels;
	public List<TickLabel> cYTickLabels;
	public double Xmin, Xmax, Ymin, Ymax, Xpanmin, Xpanmax;
	public bool isDate;
	//private
	public bool _hasminmax, _haspanminmax;
	public void Init() {
		_hasminmax = Xmax > Xmin;
		_haspanminmax = Xpanmax > Xpanmin;
	}
}

public sealed class Series {
	public List<DataPoint> data;
	public string legend, unit;
	public byte dec;
	public Color color = Color.Transparent;
	//private
	public string _hovervalue;
	public int _legendwid, _hovervaluewid;
}
