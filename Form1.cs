using Q.Chart;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Q;

public partial class Form1 : Form {
	Action timerfunc;
	public Form1() {
		InitializeComponent();
	}

	private void Form1_Load(object sender, EventArgs e) {
		demo1();
	}

	private void timer1_Tick(object sender, EventArgs e) {
		timerfunc?.Invoke();
	}

	private void button1_Click(object sender, EventArgs e) {
		timerfunc = null;
		demo1();
	}

	private void button2_Click(object sender, EventArgs e) {
		var s = new Stopwatch();
		s.Start();
		timerfunc = null;
		demo2();
		//App.pr($"draw in {(s.ElapsedTicks / (double)Stopwatch.Frequency)}s");
	}

	private void button3_Click(object sender, EventArgs e) {
		demo3();
	}

	void demo1() {
		List<DataPoint> data = new List<DataPoint>(), data2 = new List<DataPoint>();
		int i;
		for (i = 0; i < 1000; i++) {
			data.Add(new DataPoint(i, Math.Sin(i/10.0)*100));
			data2.Add(new DataPoint(i, Math.Cos(i/10.0)*100));
		}
		pic.SetData(new Config {
			series = new List<Series> {
				new Series {
					data = data,
					legend = "sin(x)",
					unit = "kg",
					color = Color.Red,
					dec = 3,
				},
				new Series {
					data = data2,
					legend = "cos(x)",
					unit = "kg",
					color = Color.Blue,
					dec = 3,
				},
			},
			isDate = false,
			Xpanmin = -1000,
			Xpanmax = 2000,
		});
		pic.RePaint();
	}

	Random rand = new Random();
	void demo2() {
		List<DataPoint> data = new List<DataPoint>();
		int i;
		double val = 0;
		Random rand = new Random(0);
		for (i = 0; i < 500000; i++) {
			data.Add(new DataPoint(i, val));
			val += rand.NextDouble() - 0.5;
		}
		pic.SetData(new Config {
			series = new List<Series> {
				new Series {
					data = data,
					legend = "f(x)",
					unit = "m",
					color = Color.Red,
					dec = 3,
				},
			},
			isDate = false,
		});
		pic.RePaint();
	}

	void demo3() {
		List<DataPoint> data = new List<DataPoint>();
		var dnow = DateTime.Now;
		dnow = new DateTime(dnow.Year, dnow.Month, dnow.Day, dnow.Hour, dnow.Minute, dnow.Second);
		int i;
		for (i = 0; i < 60; i++) {
			data.Add(new DataPoint(dnow.AddSeconds(-60+i), Math.Sin(i/10.0)*100));
		}
		pic.SetData(new Config {
			series = new List<Series> {
				new Series {
					data = data,
					legend = "Pressure",
					unit = "kpa",
					color = Color.Red,
					dec = 3,
				},
			},
			isDate = true,
			Xpanmin = data[0].x,
			Xpanmax = data[data.Count - 1].x,
		});
		pic.RePaint();
		var lastpos = data[data.Count - 1];
		var nextUpdate = Environment.TickCount + 300;
		timerfunc = () => {
			if (Environment.TickCount - nextUpdate >= 0) {
				lastpos.y += rand.NextDouble() * 100 - 50;
				pic.series[0].data.Add(lastpos);
				pic.yaxis.UpdateMinMax(lastpos.y);
				nextUpdate = Environment.TickCount + 300;
			}
			lastpos.x = Util.ToTs(DateTime.Now);
			double delta = lastpos.x - pic.C.Xpanmax;
			if (pic.xaxis.Dmax >= pic.C.Xpanmax) {
				pic.C.Xpanmax = lastpos.x;
				pic.xaxis.Dmax += delta;
				pic.xaxis.Dmin += delta;
				pic.uiinvalid |= 1;
			}
		};
	}
}
