using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace Peril_MVP
{
    class Level : IDisposable
    {
        #region Declarations
        // Physical structure of the level.
        private Tile[,] tiles;
        private Texture2D[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        private List<Artefact> artefacts = new List<Artefact>();
        private List<Enemy> enemies = new List<Enemy>();

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(69420); // Arbitrary, but constant seed

        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;
        #endregion

        #region Loading
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            timeRemaining = TimeSpan.FromMinutes(2.0);

            LoadTiles(fileStream);

            //// Load background layer textures. For now, all levels must
            //// use the same backgrounds and only use the left-most part of them.
            layers = new Texture2D[3];
            for (int i = 0; i < layers.Length; ++i)
            {
                // Choose a random segment if each background layer for level variety.
                int segmentIndex = levelIndex;
                layers[i] = Content.Load<Texture2D>("Backgrounds/Layer" + i + "_" + segmentIndex);
            }

            // Load sounds.

        }
        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                    {
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    }
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
            {
                throw new NotSupportedException("A level must have a starting point.");
            }
            if (exit == InvalidPosition)
            {
                throw new NotSupportedException("A level must have an exit.");
            }

        }
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Artefact
                case 'G':
                    return LoadArtefactTile(x, y);

                // Floating platform
                case '-':
                    return LoadTile("Floating Platform", TileCollision.Platform);

                // Various enemies
                case 'A':
                    return LoadEnemyTile(x, y, "EnemyA");
                case 'B':
                    return LoadEnemyTile(x, y, "EnemyB");

                // Platform block
                case '~':
                    return LoadVarietyTile("Platform Block", TileCollision.Platform);

                // Passable block
                case ':':
                    return LoadVarietyTile("Passable Block", TileCollision.Passable);

                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    return LoadVarietyTile("Impassable Block", TileCollision.Impassable);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }
        private Tile LoadVarietyTile(string baseName,TileCollision collision)
        {
            //int index = random.Next(variationCount);
            return LoadTile(baseName, collision);
        }
        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
            {
                throw new NotSupportedException("A level may only have one starting point.");
            }

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
            {
                throw new NotSupportedException("A level may only have one exit.");
            }

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }
        private Tile LoadEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }
        private Tile LoadArtefactTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            artefacts.Add(new Artefact(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }
        public void Dispose()
        {
            Content.Unload();
        }
        #endregion

        #region Bounds and collision
        // Gets the collision mode of the tile at a particular location.

        // This method handles tiles outside of the levels boundries by making it
        // impossible to escape past the left or right edges, but allowing things
        // to jump beyond the top of the level and fall off the bottom.
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        // Gets the bounding rectangle of a tile in world space.
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        // Width of level measured in tiles.
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        // Height of the level measured in tiles.
        public int Height
        {
            get { return tiles.GetLength(1); }
        }
        #endregion

        #region Update

        // Updates all objects in the world, performs collision between them,
        // and handles the time limit with scoring.

        public void Update(
            GameTime gameTime,
            KeyboardState keyboardState,
            GamePadState gamePadState,
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit)
            {
                //// Animate the time being converted into points.
                //int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                //seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                //timeRemaining -= TimeSpan.FromSeconds(seconds);
                //score += seconds * PointsPerSecond;
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                Player.Update(gameTime, keyboardState, gamePadState, accelState, orientation);
                UpdateArtefacts(gameTime);

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                {
                    OnPlayerKilled(null);
                }

                UpdateEnemies(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. 
                // They can only exit when they have collected all of the artefacts.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }

            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }


        // Animates each artefact and checks to allows the player to collect them.
        private void UpdateArtefacts(GameTime gameTime)
        {
            for (int i = 0; i < artefacts.Count; ++i)
            {
                Artefact artefact = artefacts[i];

                artefact.Update(gameTime);

                if (artefact.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    artefacts.RemoveAt(i--);
                    OnArtefactCollected(artefact, Player);
                }
            }
        }


        // Animates each enemy and allow them to kill the player.
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    OnPlayerKilled(enemy);
                }
            }
        }

        // Called when a artefact is collected.
        private void OnArtefactCollected(Artefact artefact, Player collectedBy)
        {
            score += artefact.PointValue;

            artefact.OnCollected(collectedBy);
        }

        // Called when the player is killed.

        // The enemy who killed the player. 
        // This is null if the player was not killed by an enemy, such as when a player falls into a hole.
        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy);
        }

        // Called when the player reaches the level's exit.
        private void OnExitReached()
        {
            Player.OnReachedExit();
            //exitReachedSound.Play();
            reachedExit = true;
        }

        // Restores the player to the starting point to try the level again.
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw
        // Draw everything in the level from background to foreground.
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i <= EntityLayer; ++i)
            {
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);
            }

            DrawTiles(spriteBatch);

            foreach (Artefact artefact in artefacts)
            {
                artefact.Draw(gameTime, spriteBatch);
            }

            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
            {
                enemy.Draw(gameTime, spriteBatch);
            }

            for (int i = EntityLayer + 1; i < layers.Length; ++i)
            {
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);
            }
        }

        // Draws each tile in the level.
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }
        #endregion
    }
}
