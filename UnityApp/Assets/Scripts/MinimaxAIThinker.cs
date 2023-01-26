/// @file
/// @brief This file contains the
/// ::ColorShapeLinks.Common.AI.Examples.MinimaxAIThinker class.
///
/// @author Nuno Fachada
/// @date 2020, 2021
/// @copyright [MPLv2](http://mozilla.org/MPL/2.0/)

using UnityEngine;
using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;

namespace ColorShapeLinks.Common.AI.Examples
{
    /// <summary>
    /// Sample AI thinker using a basic Minimax algorithm with a naive
    /// heuristic which previledges center board positions.
    /// </summary>
    /// <remarks>
    /// This is the same implementation used in the @ref minimax tutorial.
    /// </remarks>
    public class NegamaxTT : AbstractThinker
    {
        /// The default maximum search depth.
        /// </summary>
        public const int defaultMaxDepth = 0;
        private int heuristicMode;
        private int maxDepth;
        private DateTime startTime;
        public TranspositionTableWIP TT;
        Zobrist z;

        /// <summary>
        /// Setups up this thinker's maximum search depth.
        /// </summary>
        /// <param name="str">
        /// A string which should be convertible to a positive `int`.
        /// </param>
        /// <remarks>
        /// If <paramref name="str"/> is not convertible to a positive `int`,
        /// the maximum search depth is set to <see cref="defaultMaxDepth"/>.
        /// </remarks>
        /// <seealso cref="ColorShapeLinks.Common.AI.AbstractThinker.Setup"/>
        public override void Setup(string str)
        {
            TT = new TranspositionTableWIP();
            z = new Zobrist(6,7);
            int commaIndex = str.IndexOf(',');
            int.TryParse(str.Substring(0, commaIndex), out maxDepth);
            int.TryParse(str.Substring(commaIndex + 1),out heuristicMode);
            // If a non-positive integer was provided, reset it to the default
            if (maxDepth < 1) maxDepth = defaultMaxDepth;
        }

        /// <summary>
        /// Returns the name of this AI thinker which will include the
        /// maximum search depth.
        /// </summary>
        /// <returns>The name of this AI thinker.</returns>
        public override string ToString()
        {
            return base.ToString() + "D" + maxDepth+"T"+heuristicMode;
        }

        /// @copydoc IThinker.Think
        /// <seealso cref="IThinker.Think"/>
        public override FutureMove Think(Board board, CancellationToken ct)
        {
            startTime= DateTime.Now;
            // Invoke minimax, starting with zero depth
            (FutureMove move, float score) decision =
                Negamax(board, ct, board.Turn, board.Turn, 0,float.NegativeInfinity,float.PositiveInfinity);

            // Return best move
            return decision.move;
        }
        
        private bool isTimeEnded(){ 
            return (DateTime.Now - startTime).TotalMilliseconds > TimeLimitMillis-50;
        }

        /// <summary>
        /// A basic implementation of the Minimax algorithm.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <param name="player">
        /// Color of the AI controlling this thinker.
        /// </param>
        /// <param name="turn">
        /// Color of the player playing in this turn.
        /// </param>
        /// <param name="depth">Current search depth.</param>
        /// <returns>
        /// A value tuple with:
        /// <list type="bullet">
        /// <item>
        /// <term><c>move</c></term>
        /// <description>
        /// The best move from the perspective of who's playing in this turn.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>score</c></term>
        /// <description>
        /// The heuristic score associated with <c>move</c>.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        private (FutureMove move, float score) Negamax(
            Board board, CancellationToken ct,
            PColor player, PColor turn, int depth,float alpha,float beta)
        {
            // Move to return and its heuristic value
            (FutureMove move, float score) selectedMove=(FutureMove.NoMove,float.NegativeInfinity);

            // Current board state
            Winner winner;
            float olda=alpha;
            long zobHash= z.GetZobristHash(board);
            bool zKeyInTT = TT.ContainsKey(zobHash);

            // If a cancellation request was made...
            if (ct.IsCancellationRequested)
            {
                // ...set a "no move" and skip the remaining part of
                // the algorithm
                selectedMove = (FutureMove.NoMove, float.NaN);
            }
            // Otherwise, if it's a final board, return the appropriate
            // evaluation
            else if ((winner = board.CheckWinner()) != Winner.None)
            {
                if (winner.ToPColor() == turn)
                {
                    // AI player wins, return highest possible score
                    selectedMove = (FutureMove.NoMove, float.PositiveInfinity);
                }
                else if (winner.ToPColor() == turn.Other())
                {
                    // Opponent wins, return lowest possible score
                    selectedMove = (FutureMove.NoMove, float.NegativeInfinity);
                }
                else
                {
                    // A draw, return zero
                    selectedMove = (FutureMove.NoMove, 0f);
                }
            }
            // If we're at maximum depth and don't have a final board, use
            // the heuristic
            else if (depth == maxDepth)
            {
                if(heuristicMode==1)selectedMove = (FutureMove.NoMove, Heuristic(board, turn));
                else if (heuristicMode==2) selectedMove =  (FutureMove.NoMove, Heuristic2(board, turn));
                else selectedMove =  (FutureMove.NoMove, Heuristic3(board, turn));
            }
            else // Board not final and depth not at max...
            {
                // Test each column
                for (int i = 0; i < Cols; i++)
                {
                    // Skip full columns
                    if (board.IsColumnFull(i)) continue;

                    // Test shapes
                    for (int j = 0; j < 2; j++)
                    {
                        
                        // Get current shape
                        PShape shape = (PShape)j;

                        // Use this variable to keep the current board's score
                        float eval;

                        // Skip unavailable shapes
                        if (board.PieceCount(turn, shape) == 0) continue;

                        // Test move, call minimax and undo move
                        board.DoMove(shape, i);
                        eval = -Negamax(
                            board, ct, player, turn.Other(), depth + 1,-beta,-alpha).score;
                        board.UndoMove();

                        // If we're maximizing, is this the best move so far?
                        if (eval >= selectedMove.score)
                        {
                            // If so, keep it
                            selectedMove = (new FutureMove(i, shape), eval);
                        }
                        
                        if(selectedMove.score>alpha) alpha=selectedMove.score;
                        if(alpha>=beta) goto breakLoop;
                    }
                }
                breakLoop:
                    string flag2="yes";
                
  
            }
            // Return movement and its heuristic value
            return selectedMove;
        }

 /// <summary>
        /// Heuristic function which previledges center board positions.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <param name="board">The game board.</param>
        /// <param name="color">
        /// Perspective from which the board will be evaluated.
        /// </param>
        /// <returns>
        /// The heuristic value of the given <paramref name="board"/> from
        /// the perspective of the specified <paramref name="color"/.
        /// </returns>
        private float Heuristic(Board board, PColor turn)
        {
            float EvaluateSequenceOfFour(PColor player,int nWhite,int nRed, int nSquare,int nCircle,bool canBeDoneNow){
                float val1 =20 ;float val2 = 80;float val3 = 200;
                float addVal1 = 5;float addVal2 = 20;float addVal3 = 50;
                if(canBeDoneNow){val1*=2f;val2*=2f;val3*=2f;}
                float h=0;
                float tempH=0;
                if(nWhite>0 && nRed>0 && nSquare>0 && nCircle>0) return h;

                //Check on color white
                if(nWhite>0 && nRed==0){
                    switch (nWhite)
                    {
                      case 1:
                          h+=val1;
                          break;
                      case 2:
                          h+=val2;
                          break;
                      case 3:
                          h+=val3;
                          break;
                      default:
                          Console.WriteLine("Errore white");
                          break;
                    }
                    if(PColor.White != player) h = h*-1;
                }

                //Check on color red
                if(nRed>0 && nWhite==0){
                    switch (nRed)
                    {
                      case 1:
                          h+=val1;
                          
                          break;
                      case 2:
                          h+=val2;
                          break;
                      case 3:
                          h+=val3;
                          break;
                      default:
                          Console.WriteLine("Errore red");
                          break;
                    }
                    if(PColor.Red != player) h= h*-1;
                }

                //Check on shape square 
                if(nSquare>0 && nCircle==0){
                    switch (nSquare)
                    {
                      case 1:
                          tempH+=val1+addVal1;
                          
                          break;
                      case 2:
                          tempH+=val2+addVal2;
                          break;
                      case 3:
                          tempH+=val3+addVal3;
                          break;
                      default:
                          Console.WriteLine("Errore square");
                          break;
                    }
                    if(PShape.Square != player.Shape()){
                        h-=tempH ; 
                        if (board.PieceCount(player.Other(), player.Other().Shape()) <= 0) return 0;
                    }
                    else{ 
                        h+=tempH;
                        if (board.PieceCount(player, player.Shape()) <= 0) return 0;
                    }
                }


                //Check on shape circle 
                if(nCircle>0 && nSquare==0){
                    switch (nCircle)
                    {
                      case 1:
                          tempH+=val1+addVal1;
                          break;
                      case 2:
                          tempH+=val2+addVal2;
                          break;
                      case 3:
                          tempH+=val3+addVal3;
                          break;
                      default:
                          Console.WriteLine("Errore circle");
                          break;
                    }
                    if(PShape.Round != player.Shape()){
                        h-=tempH ; 
                        if (board.PieceCount(player.Other(), player.Other().Shape()) <= 0) return 0;
                    }
                    else{ 
                        h+=tempH;
                        if (board.PieceCount(player, player.Shape()) <= 0) return 0;
                    }
                }
                return h;
            }

            // Current heuristic value
            float eval = 0;
         
            foreach (IEnumerable<Pos> corridor in board.winCorridors){

                int nRed=0,nWhite=0,nSquare=0,nCircle=0;
                bool canBeDoneNow=true;
                if(corridor.Count() == 4 ){
                    
                    foreach (Pos p in corridor){
                        if(p.row-1>=0 && !board[p.row-1,p.col].HasValue){canBeDoneNow=false;}
                        Piece? piece = board[p.row,p.col];
                        if(piece.HasValue){
                            if (piece.Value.color == PColor.Red)nRed++;
                            else nWhite++;
                            if (piece.Value.shape == PShape.Square)nSquare++;
                            else nCircle++;
                        }
                    }
                    if(nWhite>0 || nRed>0 || nSquare>0 || nCircle>0) eval+=EvaluateSequenceOfFour( turn, nWhite, nRed,  nSquare, nCircle,canBeDoneNow);
                }
                else{
                    List<Pos> corridorList= corridor.ToList();
                    for(int i=0; i<=corridor.Count()- board.piecesInSequence ; i++){
                        nRed=0;nWhite=0;nSquare=0;nCircle=0;
                        canBeDoneNow=true;
                        for(int j=i;j<= i+board.piecesInSequence-1;j++){
                            Pos p= corridorList[j];
                            if(p.row-1>=0 && !board[p.row-1,p.col].HasValue){canBeDoneNow=false;}
                            Piece? piece = board[p.row,p.col];
                            if(piece.HasValue){
                                if (piece.Value.color == PColor.Red)nRed++;
                                else nWhite++;
                                if (piece.Value.shape == PShape.Square)nSquare++;
                                else nCircle++;
                            }
                        }
                        if(nWhite>0 || nRed>0 || nSquare>0 || nCircle>0) eval+=EvaluateSequenceOfFour( turn, nWhite, nRed,  nSquare, nCircle,canBeDoneNow);
                        Skip:
                            continue;
                    }
                }
                DaNonValutare:
                    continue;
            }
                           
            return eval;
        }



         private float Heuristic2(Board board, PColor color)
        {
            float[,] matrix = new float[6, 7]
	        {
		        { 3.0f, 4.0f, 5.0f, 7.0f, 5.0f, 4.0f, 3.0f },
		        { 4.0f, 6.0f, 8.0f, 10.0f, 8.0f, 6.0f, 4.0f },
		        { 5.0f, 8.0f, 11.0f, 13.0f, 11.0f, 8.0f, 5.0f },
		        { 5.0f, 8.0f, 11.0f, 13.0f, 11.0f, 8.0f, 5.0f },
		        { 4.0f, 6.0f, 8.0f, 10.0f, 8.0f, 6.0f, 4.0f },
		        { 3.0f, 4.0f, 5.0f, 7.0f, 5.0f, 4.0f, 3.0f }
	        };

            // Current heuristic value
            float h = 0;

            // Loop through the board looking for pieces
            for (int i = 0; i < board.rows; i++)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    Pos p= new Pos(i,j);
                    // Get piece in current board position
                    Piece? piece = board[i, j];

                    // Is there any piece there?
                    if (piece.HasValue)
                    {
                        // If the piece is of our color, increment the
                        // heuristic inversely to the distance from the center
                        if (piece.Value.color == color)
                            h += matrix[i,j];
                        // Otherwise decrement the heuristic value using the
                        // same criteria
                        else
                            h -= matrix[i,j];
                        // If the piece is of our shape, increment the
                        // heuristic inversely to the distance from the center
                        if (piece.Value.shape == color.Shape())
                            h += matrix[i,j]+i+j;
                        // Otherwise decrement the heuristic value using the
                        // same criteria
                        else
                            h -= matrix[i,j]+i+j;
                    }
                }
            }
            // Return the final heuristic score for the given board
            return h;
        }

                private float Heuristic3(Board board, PColor color)
        {
            // Distance between two points
            float Dist(float x1, float y1, float x2, float y2)
            {
                return (float)Math.Sqrt(
                    Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            }

            // Determine the center row
            float centerRow = board.rows / 2;
            float centerCol = board.cols / 2;

            // Maximum points a piece can be awarded when it's at the center
            float maxPoints = Dist(centerRow, centerCol, 0, 0);

            // Current heuristic value
            float h = 0;

            // Loop through the board looking for pieces
            for (int i = 0; i < board.rows; i++)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    // Get piece in current board position
                    Piece? piece = board[i, j];

                    // Is there any piece there?
                    if (piece.HasValue)
                    {
                        // If the piece is of our color, increment the
                        // heuristic inversely to the distance from the center
                        if (piece.Value.color == color)
                            h += maxPoints - Dist(centerRow, centerCol, i, j);
                        // Otherwise decrement the heuristic value using the
                        // same criteria
                        else
                            h -= maxPoints - Dist(centerRow, centerCol, i, j);
                        // If the piece is of our shape, increment the
                        // heuristic inversely to the distance from the center
                        if (piece.Value.shape == color.Shape())
                            h += maxPoints - Dist(centerRow, centerCol, i, j);
                        // Otherwise decrement the heuristic value using the
                        // same criteria
                        else
                            h -= maxPoints - Dist(centerRow, centerCol, i, j);
                    }
                }
            }
            // Return the final heuristic score for the given board
            return h;
        }
        
    }
}