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

    public class IterativeDeepSearch : AbstractThinker
    {
        // Maximum Minimax search depth.
        private int maxDepth;
        private int heuristicMode;

        /// <summary>
        /// The default maximum search depth.
        /// </summary>
        public const int defaultMaxDepth = 0;
        private DateTime startTime;
        public TranspositionTableWIP TT;
        Zobrist z;
        private bool firstMove=true;
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
            // Try to get the maximum depth from the parameters
            /*if (!int.TryParse(str, out maxDepth))
            {
                // If not possible, set it to the default
                maxDepth = defaultMaxDepth;
            }*/
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
            return "IDS_" + "D" + maxDepth+"T"+heuristicMode;
        }

        /// @copydoc IThinker.Think
        /// <seealso cref="IThinker.Think"/>
        public override FutureMove Think(Board board, CancellationToken ct)
        {
            startTime= DateTime.Now;
            if(firstMove && !board[0,3].HasValue){
                firstMove=false;
                return new FutureMove(3,board.Turn.Shape());
            }
            // Invoke minimax, starting with zero depth
            FutureMove decision = IterativeDS(board, ct, board.Turn, board.Turn);

            // Return best move
            return decision;
        }

        private bool isTimeEnded(){ 
            return (DateTime.Now - startTime).TotalMilliseconds > TimeLimitMillis-20;
        }


        private FutureMove IterativeDS( Board board,CancellationToken ct, PColor player, PColor turn){

            FutureMove bestMove = FutureMove.NoMove;
            // Current board state
            int depth=1;
            while(!isTimeEnded() && depth<=maxDepth){
            //while(!isTimeEnded()){
                float alpha = float.NegativeInfinity;
                float beta = float.PositiveInfinity;
                float olda = alpha;
                float bestValue = float.NegativeInfinity;
                
                long zobHash= z.GetZobristHash(board);
                bool zKeyInTT = TT.ContainsKey(zobHash);
                if (zKeyInTT){
                    object[] tValues = TT.GetValues(zobHash);
                    var tDepth = (int)tValues[0];
                    var tFlag = (string)tValues[1];
                    var tValue = (float)tValues[2];
                    if (tDepth >= depth){
                        if (tFlag == "Lower") alpha = Math.Max(alpha, tValue);
                        else if (tFlag == "Upper") beta = Math.Min(beta, tValue);
                    }
                }
                for (int i = 0; i < board.cols; i++){
                    // If a cancellation request was made...
                    if (ct.IsCancellationRequested)
                    {
                            // ...set a "no move" and skip the remaining part of the algorithm
                            return FutureMove.NoMove;
                    }
                    if(isTimeEnded())goto ReturnBestMove;
                    bool breakLoops=false;
                    if(board.IsColumnFull(i))continue;
                    for (int j = 0; j < 2; j++){ 
                        
                        if(isTimeEnded()) goto ReturnBestMove;
                        PShape shape = (PShape)j;
                        float value;
                        // Skip unavailable shapes
                        if (board.PieceCount(turn, shape) == 0) continue;
                        // Test move, call minimax and undo move
                        board.DoMove(shape, i);
                        value = -NegamaxwithTT(board,turn.Other(),depth-1,-1*beta,-1*alpha);
                        board.UndoMove();
                        if(value > bestValue){
                            bestValue=value;
                            bestMove= new FutureMove(i,shape);
                        }
                        if(bestValue>alpha) alpha=bestValue;
                        if(bestValue>=beta){
                            breakLoops=true;
                            break;
                        }
                    }
                  if(breakLoops)break;
                }
                if (!zKeyInTT){
                  string flag;
                  if (bestValue <= olda) flag = "Upper";
                  else if (bestValue >= beta) flag = "Lower";
                  else flag = "Exact";
                  TT.Add(zobHash, depth, flag, bestValue);
                }  
                depth++;
              }
              ReturnBestMove:
                //UnityEngine.Debug.Log("Depth: "+depth);
                if(bestMove.Equals(FutureMove.NoMove)){
                    for (int i = 0; i < board.cols; i++){
                        if(board.IsColumnFull(i))continue;
                        for (int j = 0; j < 2; j++){ 
                            PShape shapeTemp = (PShape)j;
                            // Skip unavailable shapes
                            if (board.PieceCount(turn, shapeTemp) == 0) continue;
                            else bestMove= new FutureMove(i,shapeTemp);
                        }
                    }

                }
                return bestMove;
            }


            private float NegamaxwithTT (Board board, PColor turn, int depth,float alpha,float beta){
                float olda = alpha;
                Winner winner;
                float bestScore= float.NegativeInfinity;
                var tDepth=-1;
                long zobHash= z.GetZobristHash(board);
                bool zKeyInTT = TT.ContainsKey(zobHash);
                if (zKeyInTT){
                    object[] tValues = TT.GetValues(zobHash);
                    tDepth = (int)tValues[0];
                    var tFlag = (string)tValues[1];
                    var tValue = (float)tValues[2];
                    if (tDepth >= depth){
                      if (tFlag == "Exact") return tValue;
                      if (tFlag == "Lower") alpha = Math.Max(alpha, tValue);
                      else if (tFlag == "Upper") beta = Math.Min(beta, tValue);
                      if(alpha >= beta) return tValue;
                    }
                  }
              
                  // Otherwise, if it's a final board, return the appropriate
                  // evaluation
                  if ((winner = board.CheckWinner()) != Winner.None)
                  {
                      if (winner.ToPColor() == turn)
                      {
                          // AI player wins, return highest possible score
                          
                          return float.PositiveInfinity;
                          
                      }
                      else if (winner.ToPColor() == turn.Other())
                      {
                       
                          // Opponent wins, return lowest possible score
                         return float.NegativeInfinity;
                      }
                      else
                      {
                          // A draw, return zero
                          return 0f;
                      }
                  }
                  // If we're at maximum depth and don't have a final board, use
                  // the heuristic
                  else if (depth == 0)
                  {
                      if(heuristicMode==1)return Heuristic(board, turn);
                      else return Heuristic3(board,turn);
                  }
                  else{
                    if(isTimeEnded()) goto ReturnBestScore;
                    bestScore = float.NegativeInfinity;
                    for (int i = 0; i < board.cols; i++){
                      bool breakLoops=false;
                      if(board.IsColumnFull(i))continue;
                        for (int j = 0; j < 2; j++){  
                        if(isTimeEnded())goto ReturnBestScore;
                          PShape shape = (PShape)j;
                          if (board.PieceCount(turn, shape) == 0) continue;
                          board.DoMove(shape, i);
                          float score = -1 * NegamaxwithTT(board,turn.Other(),depth-1,-1*beta,-1*alpha);
                          board.UndoMove();
                          if(score > bestScore) bestScore=score;
                          if(bestScore>alpha) alpha=bestScore;
                          if(bestScore>=beta){
                            breakLoops=true;
                            break;
                          }
                        }
                      if(breakLoops)break;
                    }
                    if (!zKeyInTT || (zKeyInTT && depth > tDepth)){
                      string flag;
                      if (bestScore <= olda) flag = "Upper";
                      else if (bestScore >= beta) flag = "Lower";
                      else flag = "Exact";
                      TT.Add(zobHash, depth, flag, bestScore);
                    }   
                  }
                  ReturnBestScore:
                    return bestScore;
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
                if(canBeDoneNow){val1*=4f;val2*=4f;val3*=4f;}
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
                        if (piece.Value.shape == color.Shape()){
                            h += maxPoints - Dist(centerRow, centerCol, i, j);
                            
                        }
                        // Otherwise decrement the heuristic value using the
                        // same criteria
                        else{
                            h -= maxPoints - Dist(centerRow, centerCol, i, j);
                            }
                    }
                }
            }
            // Return the final heuristic score for the given board
            return h;
        }


    }
}