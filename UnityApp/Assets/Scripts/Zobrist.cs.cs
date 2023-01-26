using System;
using System.Security.Cryptography;
using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using UnityEngine;
using System.Threading;

public class Zobrist
{	static int numberOfSquares;
	static int rows;
	static int columns;
	static long[][][] zArray;
	static System.Random random;
	public Zobrist(int cols,int rows)
	{	
		random= new System.Random();
		numberOfSquares= rows*cols;
		ZobristFillArray();
	}

	public static long Random64()
	{	
			//Working with ulong so that modulo works correctly with values > long.MaxValue
			ulong max=long.MaxValue;
			ulong min= (ulong)0;
            ulong uRange = (ulong)(max - min);

            //Prevent a modolo bias; see https://stackoverflow.com/a/10984975/238419
            //for more information.
            //In the worst case, the expected number of calls is 2 (though usually it's
            //much closer to 1) so this loop doesn't really hurt performance at all.
            ulong ulongRand;
            do
            {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) ;
    }

	public void ZobristFillArray()
    {
		// initialise zArray for 2 colours (red,white), with 2 piece types (square, circle) for each of 42 board squares
		zArray = new long[2][][];
		for (int i = 0; i < zArray.Length; i++) 
		{
			zArray[i] = new long[2][];
			for (int j = 0; j < zArray[i].Length; j++)
			{
				zArray[i][j] = new long[numberOfSquares];
			}
		}
		// fill each entry combination of colour, piece type and square with a different random number
		for (int colour = 0; colour < 2; colour++)
			for (int type = 0; type < 2; type++)
				for (int square = 0; square < numberOfSquares; square++)
				{
					zArray[colour][type][square] = Random64();
				}
	}

	public long GetZobristHash(Board board)
    {
		long returnKey = 0;
		for (int i = 0; i < board.rows; i++)
			for (int j = 0; j < board.cols; j++)
            {
				Piece? piece = board[i, j];
				if (piece.HasValue)
                    {
					int square = (board.cols * i) + j;
					if (piece.Value.color==PColor.Red && piece.Value.shape==PShape.Square) // red square
						returnKey ^= zArray[0][0][square];
					else if (piece.Value.color==PColor.White && piece.Value.shape==PShape.Square) // white square
						returnKey ^= zArray[1][0][square];
					else if (piece.Value.color==PColor.Red && piece.Value.shape==PShape.Round) // red round
						returnKey ^= zArray[0][1][square];
					else if (piece.Value.color==PColor.White && piece.Value.shape==PShape.Round) //white round
						returnKey ^= zArray[1][1][square];
					}
			}

		return returnKey;
    }
}