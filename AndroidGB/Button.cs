using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AndroidGB
{
    class Button
    {
        Texture2D _texture;
        Vector2 _position;
        public float Scale { get; set; } = 1;

        public bool Down { get; set; } = false;

        public Button(int x, int y, Texture2D texture)
        {
            _position = new Vector2(x, y);
            _texture = texture;
        }

        public void HandleInput(int mx, int my)
        {
            int width = (int)(_texture.Width/2 * Scale) + 50;
            int height = (int)(_texture.Height/2 * Scale) + 50;

            if(mx >= _position.X - width && mx <= _position.X + width &&
               my >= _position.Y - height && my <= _position.Y + height)
            {
                Down = true;
            }
        }

        public void Reset()
        {
            Down = false;
        }

        public void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position, null, Color.White, 0f, new Vector2(_texture.Width / 2, _texture.Height / 2), 2f, SpriteEffects.None, 0f);
        }
    }
}