/*
 * Implementation converted to C# from Rust implementation: https://github.com/elmadev/elma-rust
 * Rust implementation published under MIT license, Copyright (c) 2016 Hexjelly
 * 
 * This code is also published under the MIT license.
 */

namespace ElmaReplayIO
{
    /// <summary>
    /// Defines a single frame of a <see cref="Ride"/> in a replay.
    /// </summary>
    /// <param name="bikePosition">The bike position.</param>
    /// <param name="leftWheelPosition">The left wheel position.</param>
    /// <param name="rightWheelPosition">The right wheel position.</param>
    /// <param name="headPosition">The head position.</param>
    /// <param name="bikeRotation">The rotation of the bike.</param>
    /// <param name="leftWheelRotation">The rotation of the left wheel.</param>
    /// <param name="rightWheelRotation">The rotation of the right wheel.</param>
    /// <param name="direction">The direction of travel of the bike.</param>
    /// <param name="throttleApplied">Whether throttle is applied.</param>
    /// <param name="backWheelSpeed">The speed of the back wheel.</param>
    /// <param name="collisionStrength">A collision strength, whatever that is.</param>
    public readonly struct Frame(Position<float> bikePosition, Position<short> leftWheelPosition, Position<short> rightWheelPosition, Position<short> headPosition, short bikeRotation, byte leftWheelRotation, byte rightWheelRotation, Direction direction, bool throttleApplied, byte backWheelSpeed, byte collisionStrength)
    {
        /// <summary>
        /// The bike position.
        /// </summary>
        public readonly Position<float> BikePosition = bikePosition;

        /// <summary>
        /// The left wheel position.
        /// </summary>
        public readonly Position<short> LeftWheelPosition = leftWheelPosition;

        /// <summary>
        /// The right wheel position.
        /// </summary>
        public readonly Position<short> RightWheelPosition = rightWheelPosition;

        /// <summary>
        /// The head position.
        /// </summary>
        public readonly Position<short> HeadPosition = headPosition;

        /// <summary>
        /// The bike rotation.
        /// </summary>
        public readonly short BikeRotation = bikeRotation;

        /// <summary>
        /// The left wheel rotation speed.
        /// </summary>
        public readonly byte LeftWheelRotation = leftWheelRotation;

        /// <summary>
        /// The right wheel rotation speed.
        /// </summary>
        public readonly byte RightWheelRotation = rightWheelRotation;

        /// <summary>
        /// The direction that the bike is travelling in.
        /// </summary>
        public readonly Direction Direction = direction;

        /// <summary>
        /// A value indicating whether throttle is applied.
        /// </summary>
        public readonly bool ThrottleApplied = throttleApplied;

        /// <summary>
        /// The speed of the back wheel.
        /// </summary>
        public readonly byte BackWheelSpeed = backWheelSpeed;

        /// <summary>
        /// The collision strength (?).
        /// </summary>
        public readonly byte CollisionStrength = collisionStrength;
    }
}
