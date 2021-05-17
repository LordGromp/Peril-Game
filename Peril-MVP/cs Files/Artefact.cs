using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Peril_MVP
{
    class Artefact
    {
        #region Variables
        
        Color artefact_colour = Color.White;
        float Sync_of_Hover = -0.50f;
        float hovering_Height = 0.20f;
        float hovering_Rate = 1.5f;
        float hover;
        public int PointValue = 24;
        private Texture2D artefactTexture;
        private Vector2 originPoint;
        //private SoundEffect collectionSFX;
        private Vector2 default_Position;

        #endregion
          
        public Level Level
        {
            get { return level; }
        }
        Level level;


        public Vector2 artefact_Position
        {
            get
            {
                return default_Position + new Vector2(0.0f, hover);
            }
        }

        public Artefact(Level level, Vector2 position)
        {
            this.level = level;
            this.default_Position = position;

            LoadContent();
        }

        #region Functions

        void LoadContent()
        {

            artefactTexture = Level.Content.Load<Texture2D>("Sprites/Artifact Piece 2");
            originPoint = new Vector2(artefactTexture.Width / 2.0f, artefactTexture.Height / 2.0f);
            //collectionSFX = Level.Content.Load<SoundEffect>("Audio/SFX/Collectable Sound");
        }

        public void Update(GameTime gameTime)
        {
            double t = gameTime.TotalGameTime.TotalSeconds * hovering_Rate + artefact_Position.X * Sync_of_Hover;
            hover = (float)Math.Sin(t) * hovering_Height * artefactTexture.Height;

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(artefactTexture, artefact_Position, null, artefact_colour, 0.0f, originPoint, 1.0f, SpriteEffects.None, 0.0f);
        }

        #endregion

        public void OnCollected(Player obtained)
        {
            //collectionSFX.Play();
        }


        public Circle BoundingCircle
        {
            get
            {
                return new Circle(artefact_Position, Tile.Width / 3.0f);
            }
        }


    }
}
