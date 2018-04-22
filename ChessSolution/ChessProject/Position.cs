using System;
using System.Collections.Generic;

namespace ChessProject
{
	public class Position
	{
		/// Format specifications:
		/// - Row (R) and Column (C) are 0..7 integers. Rows are bottom-to-top and columns are left-to-right.
		/// White player starts at rows 0,1 and black player starts at rows 6,7.
		/// - Location (L) is 0..63 integer, L=R*8+C and optimized L=(R<<3)|C.
		/// Optimized operations: R+1:C is L+8, and R:C+1 is L+1, etc.
		/// Optimized comparisons: R<2 is L<16, and R>5 is L>47, C>0 is L&7>0, and C<7 is L&7<7.
		/// - List is 64-bit integer, each 7-bit chunk is either L location or value 64 sentinel or following zeroes.
		/// Location chunks must be grouped before the sentinel, from LSB, up to 8 locations, then a sentinel, then zeroed bits.
		/// - Bitmask (B) is 1<<L integer, an encoded location.
		/// Optimized operations: R+1:C is B<<8, and R:C+1 is B<<1, etc.
		/// Optimized comparisons: R<2 is B<(1<<16), and R>5 is B>(1<<47), C>0 is B&0b...11111110≠0, and C<7 is B&0b...01111111≠0.
		/// - Bitmap is a set of bitwise-ored B locations.

		/// Possible optimisation: use 16-bit shorts for rooks and bishops.

		public ulong WhitePawnsList;
		public ulong WhiteKnigthsList;
		public ulong WhiteBishopsList;
		public ulong WhiteRooksList;
		public ulong WhiteQueensList;
		public byte WhiteKingLocation;

		public ulong WhitePawns;
		public ulong WhiteKnights;
		public ulong WhiteBishops;
		public ulong WhiteRooks;
		public ulong WhiteQueens;

		public ulong BlackPawnsList;
		public ulong BlackKnightsList;
		public ulong BlackBishopsList;
		public ulong BlackRooksList;
		public ulong BlackQueensList;
		public byte BlackKingLocation;

		public ulong BlackPawns;
		public ulong BlackKnights;
		public ulong BlackBishops;
		public ulong BlackRooks;
		public ulong BlackQueens;

		public bool WhiteInCheck;
		public bool BlackInCheck;

		public ulong WhiteOccupied;
		public ulong BlackOccupied;
		public ulong Occupied;

		/// Missing fields: 50-moves counter, castling, en passant capture, who's turn.
		/// Missing methods: equality, hashing.

		public Position ()
		{
		}

		static Position ()
		{
		}

		public static readonly ulong[] PrecomputedBlackPawnsMakeCheck = GetBlackPawnsMakeCheck ();
		public static readonly ulong[] PrecomputedWhitePawnsMakeCheck = GetWhitePawnsMakeCheck ();
		public static readonly ulong[] PrecomputedKnightsMakeCheck = GetKnightsMakeCheck ();

		public static ulong[] GetBlackPawnsMakeCheck ()
		{
			ulong[] r = new ulong[64];
			for (int L = 0; L < 64; L++) {
				int R = L / 8;
				int C = L % 8;

				ulong x = 0;
				if (R < 6 && C > 0)
					x |= MakeBitmask (R + 1, C - 1);
				if (R < 6 && C < 7)
					x |= MakeBitmask (R + 1, C + 1);
				r [L] = x;
			}
			return r;
		}

		public static ulong[] GetWhitePawnsMakeCheck ()
		{
			ulong[] r = new ulong[64];
			for (int L = 0; L < 64; L++) {
				int R = L / 8;
				int C = L % 8;

				ulong x = 0;
				if (R > 1 && C > 0)
					x |= MakeBitmask (R + 1, C - 1);
				if (R > 1 && C < 7)
					x |= MakeBitmask (R + 1, C + 1);
				r [L] = x;
			}
			return r;
		}

		public static ulong[] GetKnightsMakeCheck ()
		{
			ulong[] r = new ulong[64];
			for (int L = 0; L < 64; L++) {
				int R = L / 8;
				int C = L % 8;

				ulong x = 0;
				if (R < 6 && C > 0)
					x |= MakeBitmask (R + 2, C - 1);
				if (R < 6 && C < 7)
					x |= MakeBitmask (R + 2, C + 1);
				if (R < 7 && C > 1)
					x |= MakeBitmask (R + 1, C - 2);
				if (R < 7 && C < 6)
					x |= MakeBitmask (R + 1, C + 2);
				if (R > 0 && C > 1)
					x |= MakeBitmask (R - 1, C - 2);
				if (R > 0 && C < 6)
					x |= MakeBitmask (R - 1, C + 2);
				if (R > 1 && C > 0)
					x |= MakeBitmask (R - 2, C - 1);
				if (R > 1 && C < 7)
					x |= MakeBitmask (R - 2, C + 1);
				r [L] = x;
			}
			return r;
		}

		public static int MakeLocation (int R, int C)
		{
			return R * 8 + C;
		}

		public static ulong MakeBitmask (int R, int C)
		{
			return 1UL << MakeLocation (R, C);
		}

		public bool GetWhiteInCheck ()
		{
			int L = this.WhiteKingLocation;

			if ((PrecomputedBlackPawnsMakeCheck [L] & this.BlackPawns) != 0)
				return true;
			if ((PrecomputedKnightsMakeCheck [L] & this.BlackKnights) != 0)
				return true;

			ulong B = 1UL << L;
			ulong RooksQueens = this.BlackRooks | this.BlackQueens;
			ulong mB;

			/// leftwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (minC: 1).ToString ("X16"));
			//0xFEFEFEFEFEFEFEFE
			mB = B;
			while ((mB & 0xFEFEFEFEFEFEFEFE) != 0 && ((mB >> 1) & this.Occupied) == 0)
				mB >>= 1;
			if ((mB & RooksQueens) != 0)
				return true;

			/// rightwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (maxC: 6).ToString ("X16"));
			//0x7F7F7F7F7F7F7F7F
			mB = B;
			while ((mB & 0x7F7F7F7F7F7F7F7F) != 0 && ((mB << 1) & this.Occupied) == 0)
				mB <<= 1;
			if ((mB & RooksQueens) != 0)
				return true;

			/// downwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (minR: 1).ToString ("X16"));
			//0xFFFFFFFFFFFFFF00
			mB = B;
			while ((mB & 0xFFFFFFFFFFFFFF00) != 0 && ((mB >> 8) & this.Occupied) == 0)
				mB >>= 8;
			if ((mB & RooksQueens) != 0)
				return true;

			/// upwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (maxR: 6).ToString ("X16"));
			//0x00FFFFFFFFFFFFFF
			mB = B;
			while ((mB & 0x00FFFFFFFFFFFFFF) != 0 && ((mB >> 8) & this.Occupied) == 0)
				mB <<= 8;
			if ((mB & RooksQueens) != 0)
				return true;

			ulong BishopsQueens = this.BlackBishops | this.BlackQueens;

			/// upwards-leftwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (maxR: 6, minC: 1).ToString ("X16"));
			//0x00FEFEFEFEFEFEFE
			mB = B;
			while ((mB & 0x00FEFEFEFEFEFEFE) != 0 && ((mB << 7) & this.Occupied) == 0)
				mB <<= 7;
			if ((mB & BishopsQueens) != 0)
				return true;

			/// upwards-rightwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (maxR: 6, maxC: 6).ToString ("X16"));
			//0x007F7F7F7F7F7F7F
			mB = B;
			while ((mB & 0x007F7F7F7F7F7F7F) != 0 && ((mB << 9) & this.Occupied) == 0)
				mB <<= 9;
			if ((mB & BishopsQueens) != 0)
				return true;

			/// downwards-leftwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (minR: 1, minC: 1).ToString ("X16"));
			//0xFEFEFEFEFEFEFE00
			mB = B;
			while ((mB & 0xFEFEFEFEFEFEFE00) != 0 && ((mB >> 9) & this.Occupied) == 0)
				mB >>= 9;
			if ((mB & BishopsQueens) != 0)
				return true;

			/// downwards-rightwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (minR: 1, maxC: 6).ToString ("X16"));
			//0x7F7F7F7F7F7F7F00
			mB = B;
			while ((mB & 0x7F7F7F7F7F7F7F00) != 0 && ((mB >> 7) & this.Occupied) == 0)
				mB >>= 7;
			if ((mB & BishopsQueens) != 0)
				return true;

			return false;
		}

		public bool GetBlackInCheck ()
		{
			int L = this.BlackKingLocation;

			if ((PrecomputedWhitePawnsMakeCheck [L] & this.WhitePawns) != 0)
				return true;
			if ((PrecomputedKnightsMakeCheck [L] & this.WhiteKnights) != 0)
				return true;

			ulong B = 1UL << L;
			ulong RooksQueens = this.WhiteRooks | this.WhiteQueens;
			ulong mB;

			/// leftwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (minC: 1).ToString ("X16"));
			//0xFEFEFEFEFEFEFEFE
			mB = B;
			while ((mB & 0xFEFEFEFEFEFEFEFE) != 0 && ((mB >> 1) & this.Occupied) == 0)
				mB >>= 1;
			if ((mB & RooksQueens) != 0)
				return true;

			/// rightwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (maxC: 6).ToString ("X16"));
			//0x7F7F7F7F7F7F7F7F
			mB = B;
			while ((mB & 0x7F7F7F7F7F7F7F7F) != 0 && ((mB << 1) & this.Occupied) == 0)
				mB <<= 1;
			if ((mB & RooksQueens) != 0)
				return true;

			/// downwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (minR: 1).ToString ("X16"));
			//0xFFFFFFFFFFFFFF00
			mB = B;
			while ((mB & 0xFFFFFFFFFFFFFF00) != 0 && ((mB >> 8) & this.Occupied) == 0)
				mB >>= 8;
			if ((mB & RooksQueens) != 0)
				return true;

			/// upwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (maxR: 6).ToString ("X16"));
			//0x00FFFFFFFFFFFFFF
			mB = B;
			while ((mB & 0x00FFFFFFFFFFFFFF) != 0 && ((mB >> 8) & this.Occupied) == 0)
				mB <<= 8;
			if ((mB & RooksQueens) != 0)
				return true;

			ulong BishopsQueens = this.WhiteBishops | this.WhiteQueens;

			/// upwards-leftwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (maxR: 6, minC: 1).ToString ("X16"));
			//0x00FEFEFEFEFEFEFE
			mB = B;
			while ((mB & 0x00FEFEFEFEFEFEFE) != 0 && ((mB << 7) & this.Occupied) == 0)
				mB <<= 7;
			if ((mB & BishopsQueens) != 0)
				return true;

			/// upwards-rightwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (maxR: 6, maxC: 6).ToString ("X16"));
			//0x007F7F7F7F7F7F7F
			mB = B;
			while ((mB & 0x007F7F7F7F7F7F7F) != 0 && ((mB << 9) & this.Occupied) == 0)
				mB <<= 9;
			if ((mB & BishopsQueens) != 0)
				return true;

			/// downwards-leftwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (minR: 1, minC: 1).ToString ("X16"));
			//0xFEFEFEFEFEFEFE00
			mB = B;
			while ((mB & 0xFEFEFEFEFEFEFE00) != 0 && ((mB >> 9) & this.Occupied) == 0)
				mB >>= 9;
			if ((mB & BishopsQueens) != 0)
				return true;

			/// downwards-rightwards
			//Console.WriteLine ("0x" + Position.MakeBitmapRanges (minR: 1, maxC: 6).ToString ("X16"));
			//0x7F7F7F7F7F7F7F00
			mB = B;
			while ((mB & 0x7F7F7F7F7F7F7F00) != 0 && ((mB >> 7) & this.Occupied) == 0)
				mB >>= 7;
			if ((mB & BishopsQueens) != 0)
				return true;

			return false;
		}

		public static ulong MakeBitmapRanges (int minR = 0, int maxR = 7, int minC = 0, int maxC = 7)
		{
			ulong r = 0;
			for (int L = 0; L < 64; L++) {
				int R = L / 8;
				int C = L % 8;

				if (minR <= R && R <= maxR && minC <= C && C <= maxC)
					r |= 1UL << L;
			}
			return r;
		}

		public void Invert ()
		{
			InvertListBitmap (this.WhitePawnsList, out this.WhitePawnsList, out this.WhitePawns);
			InvertListBitmap (this.WhiteKnigthsList, out this.WhiteKnigthsList, out this.WhiteKnights);
			InvertListBitmap (this.WhiteBishopsList, out this.WhiteBishopsList, out this.WhiteBishops);
			InvertListBitmap (this.WhiteRooksList, out this.WhiteRooksList, out this.WhiteRooks);
			InvertListBitmap (this.WhiteQueensList, out this.WhiteQueensList, out this.WhiteQueens);
			this.WhiteKingLocation = (byte)(63 - this.WhiteKingLocation);

			InvertListBitmap (this.BlackPawnsList, out this.BlackPawnsList, out this.BlackPawns);
			InvertListBitmap (this.BlackKnightsList, out this.BlackKnightsList, out this.BlackKnights);
			InvertListBitmap (this.BlackBishopsList, out this.BlackBishopsList, out this.BlackBishops);
			InvertListBitmap (this.BlackRooksList, out this.BlackRooksList, out this.BlackRooks);
			InvertListBitmap (this.BlackQueensList, out this.BlackQueensList, out this.BlackQueens);
			this.BlackKingLocation = (byte)(63 - this.BlackKingLocation);

			var w = this.WhiteInCheck;
			var b = this.BlackInCheck;
			this.WhiteInCheck = b;
			this.BlackInCheck = w;

			this.WhiteOccupied = this.WhitePawns | this.WhiteKnights | this.WhiteBishops | this.WhiteRooks | this.WhiteQueens | (1UL << this.WhiteKingLocation);
			this.BlackOccupied = this.BlackPawns | this.BlackKnights | this.BlackBishops | this.BlackRooks | this.BlackQueens | (1UL << this.BlackKingLocation);
			this.Occupied = this.WhiteOccupied | this.BlackOccupied;
		}

		public Position Inverted ()
		{
			Position r = new Position ();

			InvertListBitmap (this.WhitePawnsList, out r.WhitePawnsList, out r.WhitePawns);
			InvertListBitmap (this.WhiteKnigthsList, out r.WhiteKnigthsList, out r.WhiteKnights);
			InvertListBitmap (this.WhiteBishopsList, out r.WhiteBishopsList, out r.WhiteBishops);
			InvertListBitmap (this.WhiteRooksList, out r.WhiteRooksList, out r.WhiteRooks);
			InvertListBitmap (this.WhiteQueensList, out r.WhiteQueensList, out r.WhiteQueens);
			r.WhiteKingLocation = (byte)(63 - this.WhiteKingLocation);

			InvertListBitmap (this.BlackPawnsList, out r.BlackPawnsList, out r.BlackPawns);
			InvertListBitmap (this.BlackKnightsList, out r.BlackKnightsList, out r.BlackKnights);
			InvertListBitmap (this.BlackBishopsList, out r.BlackBishopsList, out r.BlackBishops);
			InvertListBitmap (this.BlackRooksList, out r.BlackRooksList, out r.BlackRooks);
			InvertListBitmap (this.BlackQueensList, out r.BlackQueensList, out r.BlackQueens);
			r.BlackKingLocation = (byte)(63 - this.BlackKingLocation);

			r.WhiteInCheck = this.BlackInCheck;
			r.BlackInCheck = this.WhiteInCheck;

			r.WhiteOccupied = r.WhitePawns | r.WhiteKnights | r.WhiteBishops | r.WhiteRooks | r.WhiteQueens | (1UL << r.WhiteKingLocation);
			r.BlackOccupied = r.BlackPawns | r.BlackKnights | r.BlackBishops | r.BlackRooks | r.BlackQueens | (1UL << r.BlackKingLocation);
			r.Occupied = r.WhiteOccupied | r.BlackOccupied;

			return r;
		}

		public static ulong InvertList (ulong list)
		{
			ulong r = 64;
			/// Optimisation: when arrived at a sentinel, its equal 64, before that its greater than 64.
			while (list > 64) {
				r = (r << 7) | (63 - (list & 63));
				/// Optimisation: lists are guaranteed to have a sentinel.
				list >>= 7;
			}
			return r;
		}

		public static void InvertListBitmap (ulong list, out ulong rlist, out ulong rbitmap)
		{
			rlist = 64;
			rbitmap = 0;
			/// Optimisation: when arrived at a sentinel, its equal 64, before that its greater than 64.
			while (list > 64) {
				ulong rL = 63 - (list & 63);
				rlist = (rlist << 7) | rL;
				rbitmap |= 1UL << (int)rL;
				/// Optimisation: lists are guaranteed to have a sentinel.
				list >>= 7;
			}
		}

		public void Progress ()
		{
			throw new NotImplementedException ();
		}

		public static readonly Position Zero = GetZeroPosition ();

		public static Position GetZeroPosition ()
		{
			Position r = new Position ();

			r.WhitePawnsList = MakeList (8, 9, 10, 11, 12, 13, 14, 15);
			r.WhiteKnigthsList = MakeList (1, 6);
			r.WhiteBishopsList = MakeList (2, 5);
			r.WhiteRooksList = MakeList (0, 7);
			r.WhiteQueensList = MakeList (3);
			r.WhiteKingLocation = 4;

			r.WhitePawns = MakeBitmap (r.WhitePawnsList);
			r.WhiteKnights = MakeBitmap (r.WhiteKnigthsList);
			r.WhiteBishops = MakeBitmap (r.WhiteBishopsList);
			r.WhiteRooks = MakeBitmap (r.WhiteRooksList);
			r.WhiteQueens = MakeBitmap (r.WhiteQueensList);

			r.BlackPawnsList = MakeList (48, 49, 50, 51, 52, 53, 54, 55);
			r.BlackKnightsList = MakeList (57, 62);
			r.BlackBishopsList = MakeList (58, 61);
			r.BlackRooksList = MakeList (56, 63);
			r.BlackQueensList = MakeList (59);
			r.BlackKingLocation = 60;

			r.BlackPawns = MakeBitmap (r.BlackPawnsList);
			r.BlackKnights = MakeBitmap (r.BlackKnightsList);
			r.BlackBishops = MakeBitmap (r.BlackBishopsList);
			r.BlackRooks = MakeBitmap (r.BlackRooksList);
			r.BlackQueens = MakeBitmap (r.BlackQueensList);

			r.WhiteInCheck = false;
			r.BlackInCheck = false;

			r.WhiteOccupied = r.WhitePawns | r.WhiteKnights | r.WhiteBishops | r.WhiteRooks | r.WhiteQueens | (1UL << r.WhiteKingLocation);
			r.BlackOccupied = r.BlackPawns | r.BlackKnights | r.BlackBishops | r.BlackRooks | r.BlackQueens | (1UL << r.BlackKingLocation);
			r.Occupied = r.WhiteOccupied | r.BlackOccupied;

			return r;
		}

		public static ulong MakeList (params uint[] locations)
		{
			if (locations.Length > 8)
				throw new ArgumentException ("MakeList: can only encode up to 8 locations");
			Array.Reverse (locations);
			ulong r = 64;
			foreach (var L in locations) {
				if (L > 63)
					throw new ArgumentException ("MakeList: only locations 0..63 are valid");
				r = (r << 7) | L;
			}
			return r;
		}

		public static ulong MakeBitmap (ulong list)
		{
			ulong r = 0;
			/// Optimisation: when arrived at a sentinel, its equal 64, before that its greater than 64.
			while (list > 64) {
				/// Optimisation: bitwise-shift rhs is interpreted modulo 64, implicit truncation.
				r |= 1UL << (int)list;
				/// Optimisation: lists are guaranteed to have a sentinel.
				list >>= 7;
			}
			return r;
		}

		public Position[] GenerateAllLegalMoves ()
		{
			var generated = new List<Position> ();

			foreach (int L in IterateList(this.WhitePawnsList)) {
				int mL = L + 8;
				ulong B = 1UL << L;
				ulong mB = B << 8;

				/// one move ahead
				if ((mB & this.Occupied) == 0) {
					if (L < 48) {
						/// normal move
						Position p = (Position)this.MemberwiseClone ();
						p.MovePawn (L, mL, B, mB);
						p.PostMove ();
						generated.Add (p);
					} else {
						/// promotion, move to last row
						Position p = (Position)this.MemberwiseClone ();
						p.MovePawnPromoteQueen (L, mL, B, mB);
						p.PostMove ();
						generated.Add (p);
					}

					/// en passant double move
					/// Optimisation: already checked if mid square is not occupied.
					mL = L + 16;
					if (L < 16) {
						mB = B << 16;
						if ((mB & this.Occupied) == 0) {
							Position p = (Position)this.MemberwiseClone ();
							p.MovePawnEnPassant (L, mL, B, mB);
							p.PostMove ();
							generated.Add (p);
						}
					}

					/// capture left-ahead
					mL = L + 7;
					mB = B << 7;
					if ((mB & this.BlackOccupied) != 0) {
						Position p = (Position)this.MemberwiseClone ();
						p.MovePawn (L, mL, B, mB);
						p.Capture (mL, mB);
						p.PostMove ();
						generated.Add (p);
					}
					/// capture right-ahead
					mL = L + 9;
					mB = B << 9;
					if ((mB & this.BlackOccupied) != 0) {
						Position p = (Position)this.MemberwiseClone ();
						p.MovePawn (L, mL, B, mB);
						p.Capture (mL, mB);
						p.PostMove ();
						generated.Add (p);
					}
				}
			}

			return generated.ToArray ();
		}

		public void MovePawn (int L, int mL, ulong B, ulong mB)
		{
			this.WhitePawnsList = ListAdd (ListRemove (this.WhitePawnsList, L), mL);
			this.WhitePawns ^= B ^ mB;
			this.WhiteOccupied ^= B ^ mB;
			this.Occupied ^= B ^ mB;
		}

		public void MovePawnEnPassant (int L, int mL, ulong B, ulong mB)
		{
			this.WhitePawnsList = ListAdd (ListRemove (this.WhitePawnsList, L), mL);
			this.WhitePawns = this.WhitePawns ^ B ^ mB;
			this.WhiteOccupied ^= B ^ mB;
			this.Occupied ^= B ^ mB;
		}

		public void MovePawnPromoteQueen (int L, int mL, ulong B, ulong mB)
		{
			/// Possible issue: list can hold only 8 locations, if player attempts to gain 9th queen or 10th horse
			/// then give him the other piece (horse or queen).
			this.WhitePawnsList = ListRemove (this.WhitePawnsList, L);
			this.WhitePawns ^= B;
			this.WhiteQueensList = ListAdd (this.WhiteQueensList, mL);
			this.WhiteQueens ^= mB;
			this.WhiteOccupied ^= B ^ mB;
			this.Occupied ^= B ^ mB;
		}

		public void MovePawnPromoteKnight (int L, int mL, ulong B, ulong mB)
		{
			throw new NotImplementedException ();
		}

		public void Capture (int L, ulong B)
		{
			if ((B & this.BlackPawns) != 0) {
				this.BlackPawnsList = ListRemove (this.BlackPawnsList, L);
				this.BlackPawns ^= B;
				this.BlackOccupied ^= B;
				this.Occupied ^= B;
			} else if ((B & this.BlackKnights) != 0) {
				this.BlackKnightsList = ListRemove (this.BlackKnightsList, L);
				this.BlackKnights ^= B;
				this.BlackOccupied ^= B;
				this.Occupied ^= B;
			} else if ((B & this.BlackBishops) != 0) {
				this.BlackBishopsList = ListRemove (this.BlackBishopsList, L);
				this.BlackBishops ^= B;
				this.BlackOccupied ^= B;
				this.Occupied ^= B;
			} else if ((B & this.BlackRooks) != 0) {
				this.BlackRooksList = ListRemove (this.BlackRooksList, L);
				this.BlackRooks ^= B;
				this.BlackOccupied ^= B;
				this.Occupied ^= B;
			} else if ((B & this.BlackQueens) != 0) {
				this.BlackQueensList = ListRemove (this.BlackQueensList, L);
				this.BlackQueens ^= B;
				this.BlackOccupied ^= B;
				this.Occupied ^= B;
			} else
				throw new ArgumentException ("Capture: selected square contains a king or nothing");
		}

		public void PostMove ()
		{
			this.WhiteInCheck = this.GetWhiteInCheck ();
			this.BlackInCheck = this.GetBlackInCheck ();
		}

		public static IEnumerable<int> IterateList (ulong list)
		{
			while (list > 64) {
				yield return (int)(list & 63);
				list >>= 7;
			}
		}

		public static ulong ListAdd (ulong list, int L)
		{
			/// Optimisation: assumes list+1 is within capacity of 8.
			/// Possible issue: list is pre-appended like a stack, not queue.
			return (list << 7) | (ulong)L;
		}

		public static ulong ListRemove (ulong list, int L)
		{
			/// Possible optimisation: list is guaranteed to contain L, no checking for sentinel.
			/// Possible optimisation: upon finding L, remaining half-list is merged without chopping.
			/// Possible issue: function reverses the list order.

//			for i in range(0,64-7-7,7):
//				print("else if (((list >> {}) & 63) == L)".format(i))
//				print("    return (list & {}) | ((list >> {}) << {});".format((1<<i)-1, i+7, i))

			ulong r = 64;
			while (list > 64) {
				ulong x = list & 63;
				if (x != (ulong)L) {
					r = (r << 7) | x;
				}
				list >>= 7;
			}
			return r;
		}

		public static void ListRemoveBitmap (ulong list, int removeL, out ulong rlist, out ulong rbitmap)
		{
			rlist = 64;
			rbitmap = 0;
			/// Optimisation: when arrived at a sentinel, its equal 64, before that its greater than 64.
			while (list > 64) {
				ulong L = list & 63;
				if ((int)L != removeL) {
					rlist = (rlist << 7) | L;
					rbitmap |= 1UL << (int)L;
				}
				/// Optimisation: lists are guaranteed to have a sentinel.
				list >>= 7;
			}
		}

	}
}
