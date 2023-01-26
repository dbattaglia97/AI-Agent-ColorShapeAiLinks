using System;
using System.Collections;
using System.Collections.Generic;
using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI; 


    /// <summary>
    /// Represents a move to be performed in the future.
    /// </summary>
    public class FullMove
    {
        /// <summary>
        /// The column where to drop the piece.
        /// </summary>
        public FutureMove move;

        /// <summary>
        /// The piece to use in the move.
        /// </summary>
        public float eval;

       


        /// <summary>
        /// Create a future move.
        /// </summary>
        /// <param name="column">The column where to drop the piece.</param>
        /// <param name="shape">The piece to use in the move.</param>
        public FullMove(FutureMove move, float eval)
        {
            this.move = move;
            this.eval = eval;
        }

        /// <summary>
        /// Provides a string representation of the future move in the form
        /// &quot;&lt;round|square&gt; piece at column &lt;col&gt;&quot;.
        /// </summary>
        /// <returns>A string representation of the future move.</returns>
        public override string ToString() =>
            $"{move.ToString().ToLower()} of value {eval}";
    }

