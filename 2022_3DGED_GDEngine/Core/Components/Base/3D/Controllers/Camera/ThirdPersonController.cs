using GD.Engine.Globals;
using Microsoft.Xna.Framework;
using System;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace GD.Engine
{
    public class ThirdPersonController : Component
    {
        private GameObject target;

        public ThirdPersonController()
        { }

        public override void Update(GameTime gameTime)
        {
            if (Application.Player != null)
                target = Application.Player;
            else
                throw new ArgumentNullException("Target not set! Do this in main");

            if (target != null)
            {
                //use target position + offset to generate new camera position
                var newPosition = target.Transform.Translation
                    + new Vector3(0, 3.5f, -15);

                var newRotation = target.Transform.Rotation
                    + new Vector3(0, 180, 0);


                //set new camera position
                transform.SetTranslation(newPosition);

                transform.SetRotation(newRotation);
            }

            //since parent Update does nothing then dont bother calling it
            //base.Update(gameTime);
        }
    }
}