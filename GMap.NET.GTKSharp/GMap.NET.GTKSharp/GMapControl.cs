using System;
using System.ComponentModel;
using System.Diagnostics;
using GMap.NET;
using GMap.NET.Internals;
using GMap.NET.MapProviders;
using GMap.NET.Projections;
using System.Drawing.Drawing2D;

namespace GMap.NET.GTKSharp
{
	[System.ComponentModel.ToolboxItem(true)]
	public class GMapControl : Gtk.DrawingArea
	{
		public GMapControl()
		{
			Core.SystemType = "GTKSharp";

			GMapImageProxy.Enable();

			Core.OnMapOpen().ProgressChanged += new ProgressChangedEventHandler(invalidatorEngage);

			this.AddEvents(Convert.ToInt32(Gdk.EventMask.ButtonPressMask));
			this.AddEvents(Convert.ToInt32(Gdk.EventMask.ButtonReleaseMask));
			this.AddEvents(Convert.ToInt32(Gdk.EventMask.ButtonMotionMask));
		}

		void invalidatorEngage(object sender, ProgressChangedEventArgs e)
		{
			InvalidateVisual();
		}

		public void InvalidateVisual()
		{
			/*redraw();
			if(Core.Refresh != null)
			{
				Core.Refresh.Set();
			}*/
		}

		public void redraw()
		{
			DrawGraphics(this.GdkWindow);
		}

		protected override bool OnButtonPressEvent(Gdk.EventButton ev)
		{
			Core.mouseDown = ApplyRotationInversion(Convert.ToInt32(ev.X), Convert.ToInt32(ev.Y));
			redraw();
			Core.BeginDrag(Core.mouseDown);
			// Insert button press handling code here.
			return base.OnButtonPressEvent(ev);
		}

		protected override bool OnButtonReleaseEvent(Gdk.EventButton evnt)
		{
			Core.EndDrag();
			return base.OnButtonReleaseEvent(evnt);
		}

		protected override bool OnMotionNotifyEvent(Gdk.EventMotion evnt)
		{
			Core.mouseCurrent = ApplyRotationInversion(Convert.ToInt32(evnt.X), Convert.ToInt32(evnt.Y));
			Core.Drag(Core.mouseCurrent);
			redraw();
			return base.OnMotionNotifyEvent(evnt);
		}

		protected override bool OnExposeEvent(Gdk.EventExpose ev)
		{
			base.OnExposeEvent(ev);
			redraw();
			return true;
		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);
		}

		protected override void OnSizeRequested(ref Gtk.Requisition requisition)
		{
			requisition.Height = 700;
			requisition.Width = 1000;
			Core.OnMapSizeChanged(requisition.Width, requisition.Height);
		}

		/// <summary>
		/// max zoom
		/// </summary>
		[Category("GMap.NET")]
		[Description("maximum zoom level of map")]
		public int MaxZoom {
			get {
				return Core.maxZoom;
			}
			set {
				Core.maxZoom = value;
			}
		}

		/// <summary>
		/// min zoom
		/// </summary>
		[Category("GMap.NET")]
		[Description("minimum zoom level of map")]
		public int MinZoom {
			get {
				return Core.minZoom;
			}
			set {
				Core.minZoom = value;
			}
		}

		/// <summary>
		/// map zooming type for mouse wheel
		/// </summary>
		[Category("GMap.NET")]
		[Description("map zooming type for mouse wheel")]
		public MouseWheelZoomType MouseWheelZoomType {
			get {
				return Core.MouseWheelZoomType;
			}
			set {
				Core.MouseWheelZoomType = value;
			}
		}

		/// <summary>
		/// enable map zoom on mouse wheel
		/// </summary>
		[Category("GMap.NET")]
		[Description("enable map zoom on mouse wheel")]
		public bool MouseWheelZoomEnabled {
			get {
				return Core.MouseWheelZoomEnabled;
			}
			set {
				Core.MouseWheelZoomEnabled = value;
			}
		}

		readonly Matrix rotationMatrixInvert = new Matrix();

		/// <summary>
		/// apply transformation if in rotation mode
		/// </summary>
		GPoint ApplyRotationInversion(int x, int y)
		{
			GPoint ret = new GPoint(x, y);

			//if(IsRotated)
			{
				System.Drawing.Point[] tt = new System.Drawing.Point[] { new System.Drawing.Point(x, y) };
				rotationMatrixInvert.TransformPoints(tt);
				var f = tt[0];

				ret.X = f.X;
				ret.Y = f.Y;
			}

			return ret;
		}

		/// <summary>
		/// gets map manager
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public GMaps Manager {
			get {
				return GMaps.Instance;
			}
		}
			
		/// <summary>
		/// current map center position
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public PointLatLng Position {
			get {
				return Core.Position;
			}
			set {
				Core.Position = value;

				if(Core.IsStarted)
				{
					//ForceUpdateOverlays();
				}
			}
		}


		double zoomReal;

		[Category("GMap.NET"), DefaultValue(0)]
		public double Zoom {
			get {
				return zoomReal;
			}
			set {
				if(zoomReal != value)
				{
					Debug.WriteLine("ZoomPropertyChanged: " + zoomReal + " -> " + value);

					if(value > MaxZoom)
					{
						zoomReal = MaxZoom;
					} else if(value < MinZoom)
					{
						zoomReal = MinZoom;
					} else
					{
						zoomReal = value;
					}

					double remainder = value % 1;
					/*if(ScaleModes.Integer == ScaleModes.Fractional && remainder != 0)
					{
						float scaleValue = (float)Math.Pow(2d, remainder);
						{
							//MapRenderTransform = scaleValue;
						}

						ZoomStep = Convert.ToInt32(value - remainder);
					} else
					*/
					{
						//MapRenderTransform = null;
						ZoomStep = (int)Math.Floor(value);
						//zoomReal = ZoomStep;
					}

					/*if(Core.IsStarted && !IsDragging)
					{
						ForceUpdateOverlays();
					}*/
				}
			}
		}


		/// <summary>
		/// map zoom level
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		internal int ZoomStep {
			get {
				return Core.Zoom;
			}
			set {
				if(value > MaxZoom)
				{
					Core.Zoom = MaxZoom;
				} else if(value < MinZoom)
				{
					Core.Zoom = MinZoom;
				} else
				{
					Core.Zoom = value;
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public GMapProvider MapProvider {
			get {
				return Core.Provider;
			}
			set {
				if(Core.Provider == null || !Core.Provider.Equals(value))
				{
					Debug.WriteLine("MapType: " + Core.Provider.Name + " -> " + value.Name);

					/*RectLatLng viewarea = SelectedArea;
					if(viewarea != RectLatLng.Empty)
					{
						Position = new PointLatLng(viewarea.Lat - viewarea.HeightLat / 2, viewarea.Lng + viewarea.WidthLng / 2);
					} else
					{
						viewarea = ViewArea;
					}*/

					Core.Provider = value;

					/*if(Core.IsStarted)
					{
						if(Core.zoomToArea)
						{
							// restore zoomrect as close as possible
							if(viewarea != RectLatLng.Empty && viewarea != ViewArea)
							{
								int bestZoom = Core.GetMaxZoomToFitRect(viewarea);
								if(bestZoom > 0 && Zoom != bestZoom)
								{
									Zoom = bestZoom;
								}
							}
						} else
						{
							ForceUpdateOverlays();
						}
					}*/
				}
			}
		}

		private bool showTileGridLines = false;

		/// <summary>
		/// shows tile gridlines
		/// </summary>
		[Category("GMap.NET")]
		[Description("shows tile gridlines")]
		public bool ShowTileGridLines {
			get {
				return showTileGridLines;
			}
			set {
				showTileGridLines = value;
				redraw();
			}
		}

		/// <summary>
		/// text on empty tiles
		/// </summary>
		public string EmptyTileText = "We are sorry, but we don't\nhave imagery at this zoom\nlevel for this region.";

		internal readonly Core Core = new Core();


		void DrawGraphics(Gdk.Window g)
		{
			//g.Clear();

			/*if(MapRenderTransform.HasValue)
			{
				#region -- scale --
				//if(!MobileMode)
				{
					var center = new GPoint(Allocation.Width / 2, Allocation.Height / 2);
					var delta = center;
					delta.OffsetNegative(Core.renderOffset);
					var pos = center;
					pos.OffsetNegative(delta);

					//g.ScaleTransform(MapRenderTransform.Value, MapRenderTransform.Value, MatrixOrder.Append);
					//g.TranslateTransform(pos.X, pos.Y, MatrixOrder.Append);

					DrawMap(g);
					//g.ResetTransform();

					//g.TranslateTransform(pos.X, pos.Y, MatrixOrder.Append);
					/*} else
				{
					DrawMap(g);
					g.ResetTransform();
				}*
					//OnPaintOverlays(g);
					#endregion
				}/* else
			{
				if(IsRotated)
				{
					#region -- rotation --

					g.TextRenderingHint = TextRenderingHint.AntiAlias;
					g.SmoothingMode = SmoothingMode.AntiAlias;

					g.TranslateTransform((float)(Core.Width / 2.0), (float)(Core.Height / 2.0));
					g.RotateTransform(-Bearing);
					g.TranslateTransform((float)(-Core.Width / 2.0), (float)(-Core.Height / 2.0));

					g.TranslateTransform(Core.renderOffset.X, Core.renderOffset.Y);

					DrawMap(g);

					g.ResetTransform();
					g.TranslateTransform(Core.renderOffset.X, Core.renderOffset.Y);

					OnPaintOverlays(g);

					#endregion
				} else
				{
					if(!MobileMode)
					{*/
			//g.TranslateTransform(Core.renderOffset.X, Core.renderOffset.Y);

			long x = Core.renderOffset.X;
			long y = Core.renderOffset.Y;


			//}
			DrawMap(g);
			OnPaintOverlays(g);
			//}
			//}
		}

		void DrawMap(Gdk.Window g)
		{
			if(Core.updatingBounds || MapProvider == EmptyProvider.Instance || MapProvider == null)
			{
				Debug.WriteLine("Core.updatingBounds");
				return;
			}

			Gdk.GC gc = new Gdk.GC(this.GdkWindow);
			long xoffset = Core.renderOffset.X;
			long yoffset = Core.renderOffset.Y;

			Core.tileDrawingListLock.AcquireReaderLock();
			Core.Matrix.EnterReadLock();

			try
			{
				foreach(var tilePoint in Core.tileDrawingList)
				{
					{
						Core.tileRect.Location = tilePoint.PosPixel;
						Core.tileRect.OffsetNegative(Core.compensationOffset);

						{
							bool found = false;

							Tile t = Core.Matrix.GetTileWithNoLock(Core.Zoom, tilePoint.PosXY);
							if(t.NotEmpty)
							{
								// render tile
								{
									foreach(GMapImage img in t.Overlays)
									{
										if(img != null && img.Img != null)
										{
											if(!found)
												found = true;

											g.DrawPixbuf(gc, img.Img, 0, 0, Convert.ToInt32(Core.tileRect.X + xoffset), Convert.ToInt32(Core.tileRect.Y + yoffset), Convert.ToInt32(Core.tileRect.Width), Convert.ToInt32(Core.tileRect.Height), Gdk.RgbDither.None, 0, 0);
										}
									}
								}
							} else if(true && MapProvider.Projection is MercatorProjection)
							{
								#region -- fill empty lines --
								int zoomOffset = 1;
								Tile parentTile = Tile.Empty;
								long Ix = 0;

								while(!parentTile.NotEmpty && zoomOffset < Core.Zoom && zoomOffset <= Core.LevelsKeepInMemmory)
								{
									Ix = (long)Math.Pow(2, zoomOffset);
									parentTile = Core.Matrix.GetTileWithNoLock(Core.Zoom - zoomOffset++, new GPoint((int)(tilePoint.PosXY.X / Ix), (int)(tilePoint.PosXY.Y / Ix)));
								}

								if(parentTile.NotEmpty)
								{
									long Xoff = Math.Abs(tilePoint.PosXY.X - (parentTile.Pos.X * Ix));
									long Yoff = Math.Abs(tilePoint.PosXY.Y - (parentTile.Pos.Y * Ix));

									// render tile 
									{
										foreach(GMapImage img in parentTile.Overlays)
										{
											if(img != null && img.Img != null && !img.IsParent)
											{
												if(!found)
													found = true;

												g.DrawPixbuf(gc, img.Img, Convert.ToInt32(Xoff * (img.Img.Width / Ix)), Convert.ToInt32(Yoff * (img.Img.Height / Ix)), Convert.ToInt32(Core.tileRect.X), Convert.ToInt32(Core.tileRect.Y), Convert.ToInt32(img.Img.Width / Ix), Convert.ToInt32(img.Img.Height / Ix), Gdk.RgbDither.None, 0, 0);
											}
										}
									}
								}
								#endregion
							}
							// add text if tile is missing
							if(!found)
							{
								lock(Core.FailedLoads)
								{
									var lt = new LoadTask(tilePoint.PosXY, Core.Zoom);
									if(Core.FailedLoads.ContainsKey(lt))
									{
										var ex = Core.FailedLoads[lt];
										//g.FillRectangle(EmptytileBrush, new RectangleF(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height));
										/*g.SetSourceColor(new Cairo.Color(255, 255, 255));
										g.Rectangle(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height);
										g.Fill();

										g.SetSourceColor(new Cairo.Color(0, 0, 0));
										//g.DrawString("Exception: " + ex.Message, MissingDataFont, Brushes.Red, new RectangleF(Core.tileRect.X + 11, Core.tileRect.Y + 11, Core.tileRect.Width - 11, Core.tileRect.Height - 11));
										g.SelectFontFace("Georgia", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
										g.SetFontSize(1.2);
										g.MoveTo(Core.tileRect.X + 11, Core.tileRect.Y + 11);
										g.ShowText("Exception: " + ex.Message);
*/

										//g.DrawString(EmptyTileText, MissingDataFont, Brushes.Blue, new RectangleF(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height), CenterFormat);
										/*g.SelectFontFace("Generic Sans Serif", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
										g.SetFontSize(11);
										g.MoveTo(Core.tileRect.X, Core.tileRect.Y);
										g.ShowText(EmptyTileText);
*/
										//g.DrawRectangle(EmptyTileBorders, (int)Core.tileRect.X, (int)Core.tileRect.Y, (int)Core.tileRect.Width, (int)Core.tileRect.Height);
										Cairo.Context cr = Gdk.CairoHelper.Create(g);
										cr.SelectFontFace("Purisa", Cairo.FontSlant.Normal, Cairo.FontWeight.Bold);
										cr.SetFontSize(13);
										cr.MoveTo(Core.tileRect.X, Core.tileRect.Y);
										cr.ShowText(EmptyTileText);
									}
								}
							}

							
							if(ShowTileGridLines)
							{
								g.DrawRectangle(gc, false, new Gdk.Rectangle(Convert.ToInt32(Core.tileRect.X + xoffset), Convert.ToInt32(Core.tileRect.Y + yoffset), Convert.ToInt32(Core.tileRect.Width), Convert.ToInt32(Core.tileRect.Height)));
								
								Cairo.Context cr = Gdk.CairoHelper.Create(g);
								cr.SelectFontFace("Purisa", Cairo.FontSlant.Normal, Cairo.FontWeight.Bold);
								cr.SetFontSize(13);

								string line1 = (tilePoint.PosXY == Core.centerTileXYLocation ? "CENTER: " : "TILE: ");
								string line2 = "" + tilePoint.PosXY + ", px:";
								string line3 = "" + tilePoint.PosPixel;

								AddText(cr, line1, xoffset, yoffset, 1, 3);
								AddText(cr, line2, xoffset, yoffset, 2, 3);
								AddText(cr, line3, xoffset, yoffset, 3, 3);
							}
						}
					}
				}
			} finally
			{
				Core.Matrix.LeaveReadLock();
				Core.tileDrawingListLock.ReleaseReaderLock();
			}
		}

		private void AddText(Cairo.Context cr, string text, double xoffset, double yoffset, Int32 line, Int32 fromlines)
		{
			Cairo.TextExtents ext = cr.TextExtents(text);

			double x = 128.0 - (ext.Width / 2 + ext.XBearing);
			double y = 128.0 - (ext.Height / 2 + ext.YBearing);

			cr.MoveTo(Core.tileRect.X + xoffset + x, Core.tileRect.Y + yoffset + y / fromlines * line);
			cr.ShowText(text);
		}
		
		protected virtual void OnPaintOverlays(Gdk.Window g)
		{
			if(g != null)
			{
				Gdk.GC gc = new Gdk.GC(this.GdkWindow);
				g.DrawLine(gc, Core.Width / 2 - 5, Core.Height / 2, Core.Width / 2 + 5, Core.Height / 2);
				g.DrawLine(gc, Core.Width / 2, Core.Height / 2 - 5, Core.Width / 2, Core.Height / 2 + 5);
			}
		}

	}

	public enum HelperLineOptions
	{
		DontShow = 0,
		ShowAlways = 1,
		ShowOnModifierKey = 2
	}
}

