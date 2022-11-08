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

	List<DataPoint> data;
	void initChart() {
		int i, j;
		data = new List<DataPoint>();
		foreach (int v in Enumerable.Range(0, 100)) {
			if(v.inlist(10,30,50,70))
				data.Add(new DataPoint(v, double.NaN));
			else data.Add(new DataPoint(v, Math.Sin(v/10.0)*100));
		}
		pic.SetData(data, false, "kg");
		pic.legend = "Sin(x)";
		pic.RePaint();
	}

	private void button1_Click(object sender, EventArgs e) {
		var arr = "1 1 2 2 10 10 10 10 10 11 11 11 11.1 12".Split(' ').Select(x => new Chart.DataPoint(x.toDouble(), 0)).ToList();
		Chart.Util.PointsInRange_Binsearch(arr, 10, 11, out int from, out int to);
		F.pr($"{from}, {to}");
	}
}
