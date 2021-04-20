using System;
using Microsoft.Xna.Framework;

namespace Peril_MVP
{
    public static class Accelerometer
    {
        // We want to prevent the Accelerometer from being initialized twice.
        private static bool isInitialized = false;

        // Whether or not the accelerometer is active
        private static bool isActive = false;

        // Initializes the Accelerometer for the current game. This method can only be called once per game.
        public static void Initialize()
        {
            // Make sure we don't initialize the Accelerometer twice
            if (isInitialized)
            {
                throw new InvalidOperationException("Initialize can only be called once");
            }

            // remember that we are initialized
            isInitialized = true;
        }

        // Gets the current state of the accelerometer.
        public static AccelerometerState GetState()
        {
            // Make sure we've initialized the Accelerometer before we try to get the state
            if (!isInitialized)
            {
                throw new InvalidOperationException("You must Initialize before you can call GetState");
            }

            // Create a new value for our state
            Vector3 stateValue = new Vector3();

            return new AccelerometerState(stateValue, isActive); // A new AccelerometerState with the current state of the accelerometer.
        }
    }

    //An encapsulation of the accelerometer's current state.
    public struct AccelerometerState
    {
        // Gets the accelerometer's current value in G-force.
        public Vector3 Acceleration { get; private set; }

        // Gets whether or not the accelerometer is active and running.
        public bool IsActive { get; private set; }

        // Initializes a new AccelerometerState.
        public AccelerometerState(Vector3 acceleration, bool isActive)
            : this()
        {
            Acceleration = acceleration;
            IsActive = isActive;
        }

        // Returns a string containing the values of the Acceleration and IsActive properties.
        public override string ToString()
        {
            return string.Format("Acceleration: {0}, IsActive: {1}", Acceleration, IsActive); // A new string describing the state.
        }
    }
}
