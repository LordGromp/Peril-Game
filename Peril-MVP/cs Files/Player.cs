using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Peril_MVP
{
    class Player
    {
        #region Declarations
        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        //private Animation celebrateAnimation;
        //private Animation dieAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;


        //Audio

        //private Song running;
        //private SoundEffect leaping;
        //private SoundEffect landing;
       

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;

        // Physics state
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        // Constants for controlling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 1.5f;
        private const Buttons JumpButton = Buttons.A;

        // Gets whether or not the player's feet are on the ground.
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        /// Current user movement input.
        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        private Rectangle localBounds;
        #endregion

        #region Bounding Rectangle
        // Gets a rectangle which bounds this player in world space.
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }
        #endregion

        #region Constructor
        // Player Constructor
        public Player(Level level, Vector2 position)
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }
        #endregion

        #region Load Content
        // Loads the player sprite sheet and sounds.
        public void LoadContent()
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/maincharacter_1-export"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/maincharacter_1-export"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/maincharacter_1-export"), 0.1f, false);
            //dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);


            // Load All Audio

            //running = Level.Content.Load<Song>("Audio/SFX/Walking");
            //leaping = Level.Content.Load<SoundEffect>("Audio/SFX/Jumping");
            //landing = Level.Content.Load<SoundEffect>("Audio/SFX/Landing");

            // Calculate bounds within texture size.            
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameHeight * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }
        #endregion

        #region Player Reset
        // Resets the player
        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }
        #endregion

        #region Player Update
        // Handles input, performs physics, and animates the player sprite.
        // We pass in all of the input states so that our game is only polling the hardware
        // once per frame. We also pass the game's orientation because when using the accelerometer,
        // we need to reverse our motion when the orientation is in the LandscapeRight orientation.
        public void Update(
            GameTime gameTime,
            KeyboardState keyboardState,
            GamePadState gamePadState,
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            GetInput(keyboardState, gamePadState, accelState, orientation);

            ApplyPhysics(gameTime);

            if (IsAlive && IsOnGround)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    sprite.PlayAnimation(runAnimation);
                }
                else
                {
                    sprite.PlayAnimation(idleAnimation);
                }
            }

            // Clear input.
            movement = 0.0f;
            isJumping = false;
        }
        #endregion

        #region Player Input
        // Gets player horizontal movement and jump commands from input.
        private void GetInput(
            KeyboardState keyboardState,
            GamePadState gamePadState,
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
            {
                movement = 0.0f;
            }

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                {
                    movement = -movement;
                }
            }

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;

                //MediaPlayer.Play(running);
                //MediaPlayer.IsRepeating = true;
                
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
                //MediaPlayer.Play(running);
                //MediaPlayer.IsRepeating = true;
            }


            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W);
        }
        #endregion

        #region Player Physics
        // Updates the player's velocity and position based on input, gravity, etc.
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
            {
                velocity.X *= GroundDragFactor;
            }
            else
            {
                velocity.X *= AirDragFactor;
            }

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
            {
                velocity.X = 0;
            }

            if (Position.Y == previousPosition.Y)
            {
                velocity.Y = 0;
            }
        }
        #endregion

        #region Jumping
        // Calculates the Y velocity accounting for jumping and
        // animates accordingly.

        // During the accent of a jump, the Y velocity is completely
        // overridden by a power curve. During the decent, gravity takes
        // over. The jump velocity is controlled by the jumpTime field
        // which measures time into the accent of the current jump.
        // The player's current velocity along the Y axis.

        // A new Y velocity if beginning or continuing a jump.
        // Otherwise, the existing Y velocity.

        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
               

                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    //if (jumpTime == 0.0f)
                    //jumpSound.Play();

                    //leaping.Play();  //Plays Jumpging Sound

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump

                    jumpTime = 0.0f;
                    //landing.Play();  //Plays landing Sound when you reach the ground
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }
        #endregion

        #region Player Collisons
        // Detects and resolves all collisions between the player and his neighboring
        // tiles. When a collision is detected, the player is pushed away along one
        // axis to prevent overlapping. There is some special logic for the Y axis to
        // handle platforms which behave differently depending on direction of movement.
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }
        #endregion

        #region Player Killed
        // Called when the player has been killed.
        // The enemy who killed the player. This parameter is null if the player was
        // not killed by an enemy (fell into a hole).
        public void OnKilled(Enemy killedBy)
        {
            isAlive = false;

            //sprite.PlayAnimation(dieAnimation);
        }
        #endregion

        #region Exit Reached
        // Called when this player reaches the level's exit.
        public void OnReachedExit()
        {
            //sprite.PlayAnimation(celebrateAnimation);
        }
        #endregion

        #region Player Draw
        // Draws the animated player.
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the sprite to face the way we are moving.
            if (Velocity.X > 0)
            {
                flip = SpriteEffects.None;
            }
            else if (Velocity.X < 0)
            {
                flip = SpriteEffects.FlipHorizontally;
            }

            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }
        #endregion
    }
}
