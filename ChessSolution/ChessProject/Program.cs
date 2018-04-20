using System;

namespace ChessProject
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var p = Position.Zero.Inverted ().Inverted ();
			p.Invert ();
			p.Invert ();
			Visualizer.DrawPosition (p);
		}

	}
}
