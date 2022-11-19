using System;
using System.Collections.Generic;
using System.Windows.Forms;
﻿using System.Drawing;

namespace Q.Chart {
	public sealed class Config {
		public List<Series> series;
		public List<YAxisOption> yaxes;
		public List<TickLabel> cXTickLabels;
		public List<TickLabel> cYTickLabels;
		public double Xmin, Xmax, Ymin, Ymax, Ypadding = 0.05;
		public bool isDate, enableHover = true, enableCross = true, showHoverXPos = true, drawGridX = true, drawGridY = true;
		public PanOption panX = new PanOption(), panY = new PanOption();
		//private
		public DrawLineReducer2 _drawLineReducer = new DrawLineReducer2();
		public bool _Xhasminmax;
		public void _init() {
			_Xhasminmax = Xmax > Xmin;
			panX._hasminmax = panX.Dmax > panX.Dmin;
			panY._hasminmax = panY.Dmax > panY.Dmin;
			if (yaxes == null) yaxes = new List<YAxisOption>();
			if (yaxes.Count == 0)
				yaxes.Add(new YAxisOption {
					Ymin = Ymin,
					Ymax = Ymax,
					unit = series[0].unit,
					cTickLabels = cYTickLabels,
				});
			foreach (var s in series)
				s._init();
		}
	}
	
	public class PanOption {
		public bool enablePan = true, enableZoom = true;
		public double Dmin = 0, Dmax = 0, Dmindelta = 0.00001;
		//private
		public bool _hasminmax = false;
	}
	
	public sealed class YAxisOption {
		public string title = "", unit = "";
		public double Ymin, Ymax;
		public List<TickLabel> cTickLabels;
	}
	
	public sealed class Series {
		public List<DataPoint> data;
		public string legend = "", unit = "";
		public byte dec, yaxis;
		public Color color = Color.Transparent;
		public LineOption line = new LineOption();
		public PointOption point = new PointOption();
		public BarOption bar = new BarOption();
		public delegate string HoverMessageFunc(int pos, DataPoint p);
		public HoverMessageFunc hovermsg;
		//private
		public string _hovervalue;
		public int _legendwid, _hovervaluewid;
		public void _init() {
			if (line.color.A == 0) line.color = color;
			if (point.color.A == 0) point.color = color;
			if (bar.color.A == 0) bar.color = color;
		}
	}
	
	public class LineOption {
		public bool show = true;
		public float lineWidth = 1;
		public Color color = Color.Transparent;
	}
	
	public class PointOption {
		public bool show = false;
		public float radius = 3, lineWidth = 1;
		public Color color = Color.Transparent, fillColor = Color.White;
	}
	
	public class BarOption {
		public bool show = false;
		public float width = 1;
		public Color color = Color.Transparent;
	}
	
	public struct TickLabel {
		public double value;
		public string strvalue;
		public int W, H;
		public TickLabel(double value, string strvalue) {
			this.value = value;
			this.strvalue = strvalue;
			W = H = 0;
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
}