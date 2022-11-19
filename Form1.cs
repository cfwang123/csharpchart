using System;
using System.Collections.Generic;
using System.Windows.Forms;
﻿using Q.Chart;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q {
	
	public sealed partial class Form1 : Form {
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
			edatanum.Text = "10000000";
		}
	
		void RunDemo(Action demo) {
			timerfunc = null;
			s.Restart();
			demo();
			lbtime.Text = $"{(s.ElapsedTicks / (double)Stopwatch.Frequency)}s";
			s.Stop();
		}
	
		private void Form1_Load(object sender, EventArgs e) => RunDemo(demo_basic);
		private void button1_Click(object sender, EventArgs e) => RunDemo(demo_basic);
		private void bmultiy_Click(object sender, EventArgs e) => RunDemo(demo_multiy);
		private void button2_Click(object sender, EventArgs e) => RunDemo(demo_bigdata);
		private void button3_Click(object sender, EventArgs e) => RunDemo(demo_dateaxis);
		private void bbar_Click(object sender, EventArgs e) => RunDemo(demobar);
	
		void demo_basic() {
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
						dec = 3,
					},
					new Series {
						data = data2,
						legend = "cos(x)",
						unit = "kg",
						dec = 3,
						line = new LineOption { show = false },
						point = new PointOption {
							show = true,
							lineWidth = 1,
							fillColor = Color.White,
							radius = 2,
						},
					},
				},
				Ypadding = 0.1,
				panX = new PanOption {
					Dmin = -1000,
					Dmax = 1200,
					Dmindelta = 100,
				},
				panY = new PanOption {
					Dmin = -200,
					Dmax = 200,
					Dmindelta = 10,
				}
			});
			pic.RePaint();
		}
	
		void demobar() {
			pic.SetData(new Config {
				series = new List<Series> {
					new Series {
						data = new List<DataPoint> {
							new DataPoint(0, 10),
							new DataPoint(3, 30),
							new DataPoint(6, 50),
							new DataPoint(9, 90),
							new DataPoint(12, 60),
						},
						legend = "A",
						line = new LineOption{show = false},
						bar = new BarOption{show = true},
					},
					new Series {
						data = new List<DataPoint> {
							new DataPoint(1, 60),
							new DataPoint(4, 50),
							new DataPoint(7, 20),
							new DataPoint(10, 10),
							new DataPoint(13, 90),
						},
						legend = "B",
						line = new LineOption{show = false},
						bar = new BarOption{show = true},
					},
				},
				Xmin = -0.5,
				Xmax = 13.5,
				panX = new PanOption {
					enablePan = false,
					enableZoom = false,
				},
				panY = new PanOption {
					enablePan = false,
					enableZoom = false,
				},
				cXTickLabels = new List<TickLabel> {
					new TickLabel(0.5, "Jan"),
					new TickLabel(3.5, "Feb"),
					new TickLabel(6.5, "Mar"),
					new TickLabel(9.5, "Apr"),
					new TickLabel(12.5, "May"),
				},
				Ypadding = 0.1,
				drawGridX = false,
				enableCross = false,
			});
			pic.RePaint();
		}
	
		void demo_multiy() {
			var data = new List<DataPoint>();
			int i, j;
			var rand = new Random(0);
			double last = 0;
			for (i = 0; i < 1000; i++) {
				last += (rand.NextDouble() - 0.5) * 10;
				data.Add(new DataPoint(i, last));
			}
			pic.SetData(new Config {
				series = new List<Series> {
					new Series {
						data = data,
						legend = "A",
						dec = 3,
					},
					new Series {
						data = data,
						legend = "B",
						dec = 3,
						yaxis = 1,
					},
					new Series {
						data = data,
						legend = "C",
						dec = 3,
						yaxis = 2,
					},
					new Series {
						data = data,
						legend = "D",
						dec = 3,
						yaxis = 3,
					},
				},
				yaxes = new List<YAxisOption> {
					new YAxisOption {
						Ymin = -200,
						Ymax = 200,
						title = "A",
					},
					new YAxisOption {
						Ymin = -300,
						Ymax = 300,
						title = "B",
					},
					new YAxisOption {
						Ymin = -500,
						Ymax = 500,
						title = "C",
					},
					new YAxisOption {
						Ymin = -1000,
						Ymax = 1000,
						title = "D",
					},
				},
				Ypadding = 0.1,
			});
			pic.RePaint();
		}
	
		void demo_bigdata() {
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
	
		void demo_dateaxis() {
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
				panX = new PanOption {
					Dmin = data[0].x,
					Dmax = data[data.Count - 1].x,
				}
			});
			pic.RePaint();
			var lastpos = data[data.Count - 1];
			var nextUpdate = Environment.TickCount + 300;
			var rand = new Random(0);
			timerfunc = () => {
				if (Environment.TickCount - nextUpdate >= 0) {
					lastpos.y += rand.NextDouble() * 100 - 50;
					pic.series[0].data.Add(lastpos);
					pic.yaxes[0].UpdateDataMinMax(lastpos.y, true);
					nextUpdate = Environment.TickCount + 300;
				}
				lastpos.x = Util.ToTs(DateTime.Now);
				double delta = lastpos.x - pic.C.panX.Dmax;
				if (pic.xaxis.Dmax >= pic.C.panX.Dmax) {
					pic.C.panX.Dmax = lastpos.x;
					pic.xaxis.Dmax += delta;
					pic.xaxis.Dmin += delta;
					pic.uiinvalid |= 1;
				}
			};
		}
	}
}