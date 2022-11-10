using System.Drawing;

namespace Q.Chart;

public sealed class DrawLineReducer1 {
	public Graphics sdc;
	public Bitmap bmp;
	public Pen pen;
	public Rectangle rGrid;
	public void Init(Graphics sdc, Bitmap bmp, Pen pen, Rectangle rGrid) {
		this.sdc = sdc;
		this.pen = pen;
		this.rGrid = rGrid;
		this.bmp = bmp;
	}

	public bool hasLine;
	public int x0, y0, x1, y1;
	public double maxAngle, minAngle;
	const double MERGEANGLE = Math.PI / 36;
	const int MERGEMAXXDIFF = 3;

	public void Finish() {
		if(hasLine) {
			DrawLine(x0, y0, x1, y1);
			hasLine = false;
		}
	}

	void DrawLine(float x0, float y0, float x1, float y1) {
		if(x0 < rGrid.Left) {
			y0 = y0 + (rGrid.Left - x0) / (x1 - x0) * (y1 - y0);
			x0 = rGrid.Left;
		}
		if(x1 > rGrid.Right) {
			y1 = y1 - (x1 - rGrid.Right) / (x1 - x0) * (y1 - y0);
			x1 = rGrid.Right;
		}
		if(y0 < rGrid.Top) {
			x0 = x0 + (rGrid.Top - y0) / (y1 - y0) * (x1 - x0);
			y0 = rGrid.Top;
		}
		else if(y0 > rGrid.Bottom) {
			x0 = x0 - (y0 - rGrid.Bottom) / (y0 - y1) * (x0 - x1);
			y0 = rGrid.Bottom;
		}
		if(y1 < rGrid.Top) {
			x1 = x1 + (rGrid.Top - y1) / (y0 - y1) * (x0 - x1);
			y1 = rGrid.Top;
		}
		else if(y1 > rGrid.Bottom) {
			x1 = x1 - (y1 - rGrid.Bottom) / (y1 - y0) * (x1 - x0);
			y1 = rGrid.Bottom;
		}
		sdc.DrawLine(pen, x0, y0, x1, y1);
	}

	public void AddLine(int x0, int y0, int x1, int y1) {
		if(!hasLine) {
			if (x1 - x0 >= MERGEMAXXDIFF) {
				DrawLine(x0, y0, x1, y1);
				return;
			}
			else {
				double angle = Math.Atan2(y1 - y0, x1 - x0);
				this.x0 = x0;
				this.y0 = y0;
				this.x1 = x1;
				this.y1 = y1;
				hasLine = true;
				maxAngle = minAngle = angle;
			}
		}
		else {
			double angle = Math.Atan2(y1 - y0, x1 - x0);
			if(x1 - this.x0 >= MERGEMAXXDIFF || angle < minAngle && angle < maxAngle - MERGEANGLE || angle > maxAngle && angle > minAngle + MERGEANGLE) {
				DrawLine(this.x0, this.y0, this.x1, this.y1);
				hasLine = false;
				if(x1 - x0 >= MERGEMAXXDIFF) {
					DrawLine(x0, y0, x1, y1);
				}
				else {
					this.x0 = x0;
					this.y0 = y0;
					this.x1 = x1;
					this.y1 = y1;
					hasLine = true;
					maxAngle = minAngle = angle;
				}
			}
			else {
				this.x1 = x1;
				this.y1 = y1;
				hasLine = true;
				if (angle < minAngle) minAngle = angle;
				if (angle > maxAngle) maxAngle = angle;
			}
		}
	}
}

public sealed class DrawLineReducer2 {
	public Graphics sdc;
	public Bitmap bmp;
	public Pen pen;
	public Rectangle rGrid;
	public void Init(Graphics sdc, Bitmap bmp, Pen pen, Rectangle rGrid) {
		this.sdc = sdc;
		this.pen = pen;
		this.rGrid = rGrid;
		this.bmp = bmp;
	}

	public bool hasLine;
	public int x, y0, y1;
	public double maxAngle, minAngle;

	public void Finish() {
		if(hasLine) {
			DrawLine(x, y0, x, y1);
			hasLine = false;
		}
	}

	void UpdateY(int y) {
		if (y < this.y0) this.y0 = y;
		if (y > this.y1) this.y1 = y;
	}

	void UpdateY(int y0, int y1) {
		if (y0 < y1) {
			this.y0 = y0;
			this.y1 = y1;
		}
		else {
			this.y0 = y1;
			this.y1 = y0;
		}
	}

	public void AddLine(int x0, int y0, int x1, int y1) {
		if (!hasLine) {
			if (x0 == x1) {
				hasLine = true;
				this.x = x0;
				UpdateY(y0, y1);
			}
			else DrawLine(x0, y0, x1, y1);
		}
		else {
			if(x0 == this.x) {
				if(x0 == x1) {
					UpdateY(y0);
					UpdateY(y1);
				}
				else if(x1 == x0 + 1) {
					int tmpy = (y0 + y1) / 2;
					DrawLine(this.x, this.y0, this.x, this.y1);
					DrawLine(this.x, y0, this.x, tmpy);
					this.x = x1;
					if(tmpy > y0) {
						if (tmpy < y1) UpdateY(tmpy + 1, y1);
						else hasLine = false;
					}
					else {
						if (tmpy > y1) UpdateY(tmpy - 1, y1);
						else hasLine = false;
					}
					//UpdateY(tmpy, y1);
				}
				else {
					DrawLine(this.x, this.y0, this.x, this.y1);
					hasLine = false;
					DrawLine(x0, y0, x1, y1);
				}
			}
			else {
				DrawLine(this.x, this.y0, this.x, this.y1);
				hasLine = false;
				if (x0 == x1) {
					hasLine = true;
					this.x = x0;
					UpdateY(y0, y1);
				}
				else DrawLine(x0, y0, x1, y1);
			}
		}
	}

	void DrawLine(float x0, float y0, float x1, float y1) {
		if(x0 < rGrid.Left) {
			y0 = y0 + (rGrid.Left - x0) / (x1 - x0) * (y1 - y0);
			x0 = rGrid.Left;
		}
		if(x1 > rGrid.Right) {
			y1 = y1 - (x1 - rGrid.Right) / (x1 - x0) * (y1 - y0);
			x1 = rGrid.Right;
		}
		if(y0 < rGrid.Top) {
			x0 = x0 + (rGrid.Top - y0) / (y1 - y0) * (x1 - x0);
			y0 = rGrid.Top;
		}
		else if(y0 > rGrid.Bottom) {
			x0 = x0 - (y0 - rGrid.Bottom) / (y0 - y1) * (x0 - x1);
			y0 = rGrid.Bottom;
		}
		if(y1 < rGrid.Top) {
			x1 = x1 + (rGrid.Top - y1) / (y0 - y1) * (x0 - x1);
			y1 = rGrid.Top;
		}
		else if(y1 > rGrid.Bottom) {
			x1 = x1 - (y1 - rGrid.Bottom) / (y1 - y0) * (x1 - x0);
			y1 = rGrid.Bottom;
		}
		if(x0 == x1) {
			if (y0 < y1) for (; y0 <= y1; y0++) bmp.SetPixel((int)x0, (int)y0, pen.Color);
			else for (; y0 >= y1; y0--) bmp.SetPixel((int)x0, (int)y0, pen.Color);
		}
		else if (y0 == y1) {
			for (; x0 <= x1; x0++) bmp.SetPixel((int)x0, (int)y0, pen.Color);
		}
		else sdc.DrawLine(pen, x0, y0, x1, y1);
	}
}
