using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Peril_MVP
{
    enum TileCollision
    {
        // A passable tile is one which does not hinder player motion at all.
        Passable = 0,
        
        // An impassable tile is one which does not allow the player to move through
        // it at all. It is completely solid.
        Impassable = 1,

        // A platform tile is one which behaves like a passable tile except when the
        // player is above it. A player can jump up through a platform as well as move
        // past it to the left and right, but can not fall down through the top of it.
        Platform = 2,
    }

    // Stores the appearance and collision behavior of a tile.
    class Tile
    {
        public Texture2D Texture;
        public TileCollision Collision;

        public const int Width = 40;
        public const int Height = 32;

        public static readonly Vector2 Size = new Vector2(Width, Height);

        // Constructs a new tile.
        public Tile(Texture2D texture, TileCollision collision)
        {
            Texture = texture;
            Collision = collision;
        }
    }
}




