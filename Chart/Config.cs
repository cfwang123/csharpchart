using System.Drawing;

namespace Q.Chart;
public sealed class Config {
	public List<Series> series;
	public List<TickLabel> cXTickLabels;
	public List<TickLabel> cYTickLabels;
	public double Xmin, Xmax, Ymin, Ymax, Xpanmin, Xpanmax, Ypanmin, Ypanmax, Ypadding = 20;
	public bool isDate, enablePan = true, enableHover = true, showHoverXPos = true, drawGrid = true;
	//private
	public DrawLineReducer2 _drawLineReducer = new DrawLineReducer2();
	public bool _Xhasminmax, _Yhasminmax, _Xhaspanminmax, _Yhaspanminmax;
	public void Init() {
		_Xhasminmax = Xmax > Xmin;
		_Yhasminmax = Ymax > Ymin;
		_Xhaspanminmax = Xpanmax > Xpanmin;
		_Yhaspanminmax = Ypanmax > Ypanmin;
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
