using Q.Chart;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Q;

public partial class Form1 : Form {
	Action timerfunc;
	Stopwatch s = new Stopwatch();
	public Form1() {
		InitializeComponent();
		timer1.Tick += (s, e) => timerfunc?.Invoke();
		edatanum.Items.AddRange(new string[] {
			"500000",
			"1000000",
			"2000000",
			"5000000",
			"10000000",
			"20000000",
		});
		edatanum.SelectedIndex = 0;
	}

	void RunDemo(Action demo) {
		timerfunc = null;
		s.Restart();
		demo();
		if(etimewatch.Checked)
			App.pr($"Draw in {(s.ElapsedTicks / (double)Stopwatch.Frequency)}s");
		s.Stop();
	}

	private void Form1_Load(object sender, EventArgs e) {
		RunDemo(demo1);
	}

	private void button1_Click(object sender, EventArgs e) {
		RunDemo(demo1);
	}

	private void button2_Click(object sender, EventArgs e) {
		RunDemo(demo2);
	}

	private void button3_Click(object sender, EventArgs e) {
		RunDemo(demo3);
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
			Ypadding = 20,
		});
		pic.RePaint();
	}

	void demo2() {
		List<DataPoint> data = new List<DataPoint>();
		int i;
		double val = 0;
		Random rand = new Random(0);
		int datanum = 1;
		int.TryParse(edatanum.Text, out datanum);
		for (i = 0; i <= datanum; i++) {
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
		var rand = new Random(0);
		timerfunc = () => {
			if (Environment.TickCount - nextUpdate >= 0) {
				lastpos.y += rand.NextDouble() * 100 - 50;
				pic.series[0].data.Add(lastpos);
				pic.yaxis.UpdateDataMinMax(lastpos.y, true);
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
