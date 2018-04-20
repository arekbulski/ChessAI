using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ChessProject
{
	public static class Visualizer
	{
		public static string folder = "graphics-brgfx/";

		public static Image WhitePawnPicture = Bitmap.FromFile (folder + "white-pawn.png");
		public static Image WhiteKnightPicture = Bitmap.FromFile (folder + "white-knight.png");
		public static Image WhiteBishopPicture = Bitmap.FromFile (folder + "white-bishop.png");
		public static Image WhiteRookPicture = Bitmap.FromFile (folder + "white-rook.png");
		public static Image WhiteQueenPicture = Bitmap.FromFile (folder + "white-queen.png");
		public static Image WhiteKingPicture = Bitmap.FromFile (folder + "white-king.png");

		public static Image BlackPawnPicture = Bitmap.FromFile (folder + "black-pawn.png");
		public static Image BlackKnightPicture = Bitmap.FromFile (folder + "black-knight.png");
		public static Image BlackBishopPicture = Bitmap.FromFile (folder + "black-bishop.png");
		public static Image BlackRookPicture = Bitmap.FromFile (folder + "black-rook.png");
		public static Image BlackQueenPicture = Bitmap.FromFile (folder + "black-queen.png");
		public static Image BlackKingPicture = Bitmap.FromFile (folder + "black-king.png");

		public static Font Ubuntu10 = new Font ("Ubuntu", 10);

		public static void DrawPosition (Position p)
		{
			Bitmap b = new Bitmap (512, 512);
			Graphics g = Graphics.FromImage (b);

			for (int L = 0; L < 64; L++) {
				int R = L / 8;
				int C = L % 8;
				ulong B = 1UL << L;

				Rectangle square = new Rectangle (C * 64, 512 - 64 - R * 64, 64, 64);
				bool blacksquare = ((L + R) & 1) == 0;

				g.FillRectangle (blacksquare ? Brushes.Gray : Brushes.WhiteSmoke, square);
				g.DrawString (L.ToString (), Ubuntu10, Brushes.DimGray, square);

				if ((p.WhitePawns & B) != 0)
					g.DrawImage (WhitePawnPicture, square);
				if ((p.WhiteKnights & B) != 0)
					g.DrawImage (WhiteKnightPicture, square);
				if ((p.WhiteBishops & B) != 0)
					g.DrawImage (WhiteBishopPicture, square);
				if ((p.WhiteRooks & B) != 0)
					g.DrawImage (WhiteRookPicture, square);
				if ((p.WhiteQueens & B) != 0)
					g.DrawImage (WhiteQueenPicture, square);
				if (p.WhiteKingLocation == L)
					g.DrawImage (WhiteKingPicture, square);
			
				if ((p.BlackPawns & B) != 0)
					g.DrawImage (BlackPawnPicture, square);
				if ((p.BlackKnights & B) != 0)
					g.DrawImage (BlackKnightPicture, square);
				if ((p.BlackBishops & B) != 0)
					g.DrawImage (BlackBishopPicture, square);
				if ((p.BlackRooks & B) != 0)
					g.DrawImage (BlackRookPicture, square);
				if ((p.BlackQueens & B) != 0)
					g.DrawImage (BlackQueenPicture, square);
				if (p.BlackKingLocation == L)
					g.DrawImage (BlackKingPicture, square);

			}

			b.Save ("temp1.png");
		}

	}
}
