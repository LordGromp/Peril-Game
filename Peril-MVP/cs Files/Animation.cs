using System;
using Microsoft.Xna.Framework.Graphics;

namespace Peril_MVP
{
    class Animation
    {
        #region Declarations
        // All frames in the animation arranged horizontally.
        public Texture2D Texture
        {
            get { return texture; }
        }
        Texture2D texture;

        // Duration of time to show each frame.
        public float FrameTime
        {
            get { return frameTime; }
        }
        float frameTime;

        // When the end of the animation is reached, should it
        // continue playing from the beginning?
        public bool IsLooping
        {
            get { return isLooping; }
        }
        bool isLooping;

        // Gets the number of frames in the animation.
        public int FrameCount
        {
            // Assume square frames.
            get { return Texture.Width / FrameHeight; }
        }

        // Gets the width of a frame in the animation.
        public int FrameWidth
        {
            // Assume square frames.
            get { return Texture.Height; }
        }


        // Gets the height of a frame in the animation.
        public int FrameHeight
        {
            get { return Texture.Height; }
        }
        #endregion

        //Constructs a new animation.
        public Animation(Texture2D texture, float frameTime, bool isLooping)
        {
            this.texture = texture;
            this.frameTime = frameTime;
            this.isLooping = isLooping;
        }
    }
}
