using System;
using Gtk;
using GMap.NET;
using GMap.NET.MapProviders;

public partial class MainWindow: Gtk.Window
{
	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();

		gmapcontrol2.Manager.Mode = AccessMode.ServerOnly;
		gmapcontrol2.MapProvider = GMapProviders.OpenStreetMap;
		gmapcontrol2.Position = new PointLatLng(48.332747, 14.308910);
		gmapcontrol2.MinZoom = 0;
		gmapcontrol2.MaxZoom = 24;
		gmapcontrol2.Zoom = 18;
	}
	
	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}

	protected void OnButton2Clicked(object sender, EventArgs e)
	{
		gmapcontrol2.redraw();
	}
}
