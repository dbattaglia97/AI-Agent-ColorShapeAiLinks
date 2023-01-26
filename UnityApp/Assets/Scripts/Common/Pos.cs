/// @file
/// @brief This file contains the ::ColorShapeLinks.Common.Pos struct.
///
/// @author Nuno Fachada
/// @date 2019, 2020
/// @copyright [MPLv2](http://mozilla.org/MPL/2.0/)
using System;

namespace ColorShapeLinks.Common
{
    /// <summary>Represents a board position.</summary>
    public struct Pos
    {
        /// <summary>Board row.</summary>
        public readonly int row;

        /// <summary>Board column.</summary>
        public readonly int col;

        /// <summary>
        /// Creates a new board position.
        /// </summary>
        /// <param name="row">Board row.</param>
        /// <param name="col">Board column.</param>
        public Pos(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        public override int GetHashCode()
        {
            // Use the HashCode method provided by the System.Tuple class to
            // generate a hash code based on the values of the i and j attributes.
            return Tuple.Create(row, col).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            // Check if the object is null or is not of the same type as this instance.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // Cast the object to a Pos instance.
            Pos other = (Pos)obj;

            // Check if the i and j attributes of this instance and the other object are equal.
            return row == other.row && col == other.col;
        }






        /// <summary>
        /// Provides a string representation of the board position in the form
        /// "(row,col)".
        /// </summary>
        /// <returns>A string representation of the board position.</returns>
        public override string ToString() => $"({row},{col})";
    }
}
