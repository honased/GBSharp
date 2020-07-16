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
    class DPAD
    {
        Texture2D _texture;
        Vector2 _position;
        public float Scale { get; set; } = 1;

        public float XAxis { get; private set; }
        public float YAxis { get; private set; }

        public DPAD(int x, int y, Texture2D texture)
        {
            _position = new Vector2(x, y);
            _texture = texture;
        }

        public void HandleInput(int mx, int my)
        {
            int width = (int)(_texture.Width/2 * Scale) + 125;
            int height = (int)(_texture.Height/2 * Scale) + 125;

            if(mx >= _position.X - width && mx <= _position.X + width &&
               my >= _position.Y - height && my <= _position.Y + height)
            {
                XAxis = (mx - _position.X) / width;
                YAxis = (my - _position.Y) / height;
            }
        }

        public void Reset()
        {
            XAxis = 0;
            YAxis = 0;
        }

        public void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position, null, Color.White, 0f, new Vector2(_texture.Width / 2, _texture.Height / 2), 2f, SpriteEffects.None, 0f);
        }
    }
}