﻿using GD.App;
using GD.Engine;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace GD.Engine
{
    /// <summary>
    /// Adds collidable 1st person controller to camera using keyboard and mouse input
    /// </summary>
    public class Player : FirstPersonController
    {
        #region Statics

        private static readonly float DEFAULT_JUMP_HEIGHT = 5;

        #endregion Statics

        #region Fields

        private CharacterCollider characterCollider;
        private Character characterBody;
        private float jumpHeight;

        //temp vars
        private Vector3 restrictedLook, restrictedRight;

        #endregion Fields

        #region Contructors

        public Player(GameObject gameObject,
            CharacterCollider characterCollider,
            float moveSpeed,
            float strafeSpeed, Vector2 rotationSpeed,
            float smoothFactor, bool isGrounded,
            float jumpHeight)
        : base(moveSpeed, strafeSpeed, rotationSpeed, smoothFactor, isGrounded)
        {
            this.jumpHeight = jumpHeight;
            //get the collider attached to the game object for this controller
            this.characterCollider = characterCollider;
            //get the body so that we can change its position when keys
            characterBody = characterCollider.Body as Character;
        }

        #endregion Contructors

        #region Actions - Update

        public override void Update(GameTime gameTime)
        {
            if (characterBody == null)
                throw new NullReferenceException("No body to move with this controller. You need to add the collider component before this controller!");

            HandleKeyboardInput(gameTime);
            HandleMouseInput(gameTime);
            HandleStrafe(gameTime);
            HandleJump(gameTime);
        }

        #endregion Actions - Update

        #region Actions - Input

        protected override void HandleKeyboardInput(GameTime gameTime)
        {


            if (Input.Keys.IsPressed(Keys.Up) && Application.CameraManager.ActiveCamera.transform.Translation.Z >= -0.5f)//&& Input.Keys.IsPressed(Keys.LeftControl))

            {
                restrictedLook = transform.World.Forward; //we use Up instead of Forward
                restrictedLook.Y = 0;
                characterBody.Velocity -= moveSpeed * restrictedLook * gameTime.ElapsedGameTime.Milliseconds;
            }
            else
            {
                characterBody.DesiredVelocity = Vector3.Zero;
            }
        }

        private void HandleStrafe(GameTime gameTime)
        {
            if (Input.Keys.IsPressed(Keys.Left) && Application.CameraManager.ActiveCameraTransform.Translation.Z >= -0.5f)
            {
                restrictedRight = transform.World.Right;
                restrictedRight.Y = 0;
                characterBody.Velocity += strafeSpeed * restrictedRight * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (Input.Keys.IsPressed(Keys.Right) && Application.CameraManager.ActiveCameraTransform.Translation.Z >= -0.5f)
            {
                restrictedRight = transform.World.Right;
                restrictedRight.Y = 0;
                characterBody.Velocity -= strafeSpeed * restrictedRight * gameTime.ElapsedGameTime.Milliseconds;
            }
            else
            {
                characterBody.DesiredVelocity = Vector3.Zero;
            }
        }

        private void HandleJump(GameTime gameTime)
        {
            if (Input.Keys.IsPressed(Keys.Space) && Application.CameraManager.ActiveCameraTransform.Translation.Z >= -1)
                characterBody.DoJump(jumpHeight);
        }

        #endregion Actions - Input
    }
}