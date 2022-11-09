using Q.Chart;
using System.Drawing;
using System.Linq;

namespace Q;

public partial class Form1 : Form {
	public Form1() {
		InitializeComponent();
	}

	private void Form1_Load(object sender, EventArgs e) {
		initChart();
	}

	private void timer1_Tick(object sender, EventArgs e) {
		if (emove.Checked) {
			pic.xaxis.Dmax += 10;
			pic.xaxis.Dmin += 10;
			pic.PaintChart();
			pic.Invalidate();
		}
	}

	void initChart() {
		int i, j;
		var arr = new List<Series>();
		List<DataPoint> data = new List<DataPoint>(), data2 = new List<DataPoint>();
		foreach (int v in Enumerable.Range(0, 100)) {
			if(v == 50)
				data.Add(new DataPoint(v, double.NaN));
			else data.Add(new DataPoint(v, Math.Sin(v/10.0)*100));
			data2.Add(new DataPoint(v, Math.Cos(v/10.0)*100));
		}
		arr.Add(new Series {
			data = data,
			legend = "sin(x)",
			unit = "kg",
			color = Color.Red,
			dec = 3,
		});
		arr.Add(new Series {
			data = data2,
			legend = "cos(x)",
			unit = "kg",
			color = Color.Blue,
			dec = 3,
		});
		pic.SetData(arr);
		pic.RePaint();
	}

	private void button1_Click(object sender, EventArgs e) {
	}
}
