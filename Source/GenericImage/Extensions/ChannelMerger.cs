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

using System.Collections.Generic;
using DotImaging.Primitives2D;

namespace DotImaging
{
    /// <summary>
    /// Provides channel merge extensions.
    /// </summary>
    public static class ChannelMerger
    {
        /// <summary>
        /// Combines provided channels into single image with interleaved channels.
        /// </summary>
        /// <typeparam name="TSrcColor">Source color type.</typeparam>
        /// <typeparam name="TDepth">Channel depth type.</typeparam>
        /// <param name="channels">Channel collection.</param>
        /// <param name="channelIndices">Channel indicies. If null, all channels are taken.</param>
        /// <returns>Image.</returns>
        public static TSrcColor[,] MergeChannels<TSrcColor, TDepth>(this IList<Gray<TDepth>[,]> channels, params int[] channelIndices)
            where TSrcColor : struct, IColor<TDepth>
            where TDepth : struct
        {
            var area = new Rectangle(new Point(), channels[0].Size());
            return channels.MergeChannels<TSrcColor, TDepth>(area, channelIndices);
        }

        /// <summary>
        /// Combines provided channels into single image with interleaved channels.
        /// </summary>
        /// <typeparam name="TSrcColor">Source color type.</typeparam>
        /// <typeparam name="TDepth">Channel depth type.</typeparam>
        /// <param name="channels">Channel collection.</param>
        /// <param name="area">Working area.</param>
        /// <param name="channelIndices">Channel indicies. If null, all channels are taken.</param>
        /// <returns>Image.</returns>
        public static TSrcColor[,] MergeChannels<TSrcColor, TDepth>(this IList<Gray<TDepth>[,]> channels, Rectangle area, params int[] channelIndices)
            where TSrcColor : struct, IColor<TDepth>
            where TDepth : struct
        {
            TSrcColor[,] image = new TSrcColor[area.Height, area.Width];

            using (var im = image.Lock())
            {
                for (int i = 0; i < channelIndices.Length; i++)
                {
                    using (var ch = channels[i].Lock())
                    {
                        replaceChannel<TSrcColor, TDepth>(im, ch.GetSubRect(area), channelIndices[i]);
                    }
                }
            }
            return image;
        }

        /// <summary>
        /// Replaces the selected image channel with the specified channel.
        /// </summary>
        /// <typeparam name="TSrcColor">Source color type.</typeparam>
        /// <typeparam name="TDepth">Channel depth type.</typeparam>
        /// <param name="image">Image.</param>
        /// <param name="channel">Channel.</param>
        /// <param name="channelIndex">Index of a channel to replace.</param>
        public static void ReplaceChannel<TSrcColor, TDepth>(this TSrcColor[,] image, Gray<TDepth>[,] channel, int channelIndex)
            where TSrcColor : struct, IColor<TDepth>
            where TDepth : struct
        {
            using (var im = image.Lock())
            using (var ch = channel.Lock())
            {
                replaceChannel<TSrcColor, TDepth>(im, ch, channelIndex);
            }
        }

        private static unsafe void replaceChannel<TSrcColor, TDepth>(Image<TSrcColor> image, Image<Gray<TDepth>> channel, int channelIndex)
            where TSrcColor : struct, IColor<TDepth>
            where TDepth : struct
        {
            int width = image.Width;
            int height = image.Height;

            int channelSize = image.ColorInfo.ChannelSize;
            int colorSize = image.ColorInfo.Size;

            byte* srcPtr = (byte*)channel.ImageData;
            byte* dstPtr = (byte*)image.ImageData + channelIndex * image.ColorInfo.ChannelSize;;

            for (int row = 0; row < height; row++)
            {
                byte* srcColPtr = srcPtr;
                byte* dstColPtr = dstPtr;
                for (int col = 0; col < width; col++)
                {
                    /********** copy channel byte-per-byte ************/
                    for (int partIdx = 0; partIdx < channelSize; partIdx++)
                    {
                        dstPtr[partIdx] = srcColPtr[partIdx];
                    }

                    srcColPtr += channelSize; //move to the next column
                    dstColPtr += colorSize;
                    /********** copy channel byte-per-byte ************/
                }

                srcPtr += channel.Stride;
                dstPtr += image.Stride;
            }
        }
    }
}
