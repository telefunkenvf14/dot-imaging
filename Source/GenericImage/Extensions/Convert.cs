﻿#region Licence and Terms
// DotImaging Framework
// https://github.com/dajuric/dot-imaging
//
// Copyright © Darko Jurić, 2014 
// darko.juric2@gmail.com
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU Lesser General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU Lesser General Public License for more details.
// 
//   You should have received a copy of the GNU Lesser General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/lgpl.txt>.
//
#endregion

using DotImaging.Primitives2D;

namespace DotImaging
{
    /// <summary>
    /// Element conversion delegate.
    /// </summary>
    /// <typeparam name="TSrc">Source element type.</typeparam>
    /// <typeparam name="TDst">Destination element type.</typeparam>
    /// <param name="source">Source element.</param>
    /// <param name="destination">Destination element.</param>
    public delegate void Convert<TSrc, TDst>(TSrc source, ref TDst destination)
        where TSrc : struct
        where TDst : struct;

    /// <summary>
    /// Provides generic element conversion extensions.
    /// </summary>
    public static class ConvertExtensions
    {
        /// <summary>
        /// Converts each element of the array by using the specified conversion function.
        /// </summary>
        /// <typeparam name="TSrc">Source element type.</typeparam>
        /// <typeparam name="TDst">Destination element type.</typeparam>
        /// <param name="source">Source array.</param>
        /// <param name="convert">Conversion function.</param>
        /// <returns>Destination array.</returns>
        public static TDst[,] Convert<TSrc, TDst>(this TSrc[,] source, Convert<TSrc, TDst> convert)
            where TSrc : struct
            where TDst : struct
        {
            var area = new Rectangle(0, 0, source.GetLength(1), source.GetLength(0));
            return source.Convert<TSrc, TDst>(convert, area);
        }

        /// <summary>
        /// Converts each element of the array by using the specified conversion function.
        /// </summary>
        /// <typeparam name="TSrc">Source element type.</typeparam>
        /// <typeparam name="TDst">Destination element type.</typeparam>
        /// <param name="source">Source array.</param>
        /// <param name="convert">Conversion function.</param>
        /// <param name="area">Working area.</param>
        public static TDst[,] Convert<TSrc, TDst>(this TSrc[,] source, Convert<TSrc, TDst> convert, Rectangle area)
            where TSrc : struct
            where TDst : struct
        {
            TDst[,] destination = new TDst[area.Height, area.Width];
            var offset = area.Location;

            ParallelLauncher.Launch(thread =>
            {
                convert(source[thread.Y + offset.Y, thread.X + offset.X], ref destination[thread.Y, thread.X]);
            },
            area.Width, area.Height);

            return destination;
        }
    }
}
