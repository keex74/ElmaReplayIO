using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElmaReplayIO
{
    /// <summary>
    /// Defines a picture in an Elma level.
    /// </summary>
    /// <param name="pictureName">The picture name.</param>
    /// <param name="textureName">The texture name.</param>
    /// <param name="maskName">The mask name.</param>
    /// <param name="position">The picture position.</param>
    /// <param name="distance">The Z-Order / distance.</param>
    /// <param name="clipping">The clipping type.</param>
    public class ElmaPicture(string pictureName, string textureName, string maskName, Position<double> position, int distance, int clipping)
    {
        /// <summary>
        /// Gets the picture name.
        /// </summary>
        public string PictureName { get; } = pictureName;

        /// <summary>
        /// Gets the texture name.
        /// </summary>
        public string TextureName { get; } = textureName;

        /// <summary>
        /// Gets the mask name.
        /// </summary>
        public string MaskName { get; } = maskName;

        /// <summary>
        /// Gets the picture position.
        /// </summary>
        public Position<double> Position { get; } = position;

        /// <summary>
        /// Gets the distance / z-order of the picture.
        /// </summary>
        public int Distance { get; } = distance;

        /// <summary>
        /// Gets the clipping type.
        /// </summary>
        public int Clipping { get; } = clipping;
    }
}
