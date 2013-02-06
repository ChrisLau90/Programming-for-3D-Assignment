using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/// REFERENCES:
/// [0] FuelCell game code http://msdn.microsoft.com/en-us/library/dd940288.aspx
/// date accessed 18th April 2012
namespace FuelCell
{
    class Ship : GameObject
    {
        public float ForwardDirection { get; set; }
        public float AimDirection { get; set; }
        public int MaxRange { get; set; }

        public Ship()
            : base()
        {
            ForwardDirection = 0.0f;
            AimDirection = 0.0f;
            MaxRange = GameConstants.MaxRange;

            Mouse.SetPosition(290, 240);
        }

        public void LoadContent(ContentManager content, string modelName)
        {
            Model = content.Load<Model>(modelName);
            BoundingSphere = CalculateBoundingSphere();

            BoundingSphere scaledSphere;
            scaledSphere = BoundingSphere;
            scaledSphere.Radius *=
                GameConstants.FuelCarrierBoundingSphereFactor;
            BoundingSphere =
                new BoundingSphere(scaledSphere.Center, scaledSphere.Radius);
        }

        internal void Reset()
        {
            Position = Vector3.Zero;
            ForwardDirection = 0f;
            AimDirection = 0f;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix worldMatrix = Matrix.Identity;
            Matrix rotationYMatrix = Matrix.CreateRotationY(ForwardDirection);
            Matrix rotationXMatrix = Matrix.CreateRotationX(AimDirection); //<<<<<<<<<<<<<<<<<<<
            Matrix translateMatrix = Matrix.CreateTranslation(Position);

            worldMatrix = rotationXMatrix * rotationYMatrix;
            worldMatrix *= translateMatrix;


            //translateMatrix
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World =
                        worldMatrix * transforms[mesh.ParentBone.Index];
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }

        public void Update(GamePadState gamepadState,
            KeyboardState keyboardState, Debris[] barriers)
        {
            Vector3 futurePosition = Position;
            float xTurnAmount = 0;
            float yTurnAmount = 0;

            if (Mouse.GetState().X != 290 || Mouse.GetState().Y != 240)

            {
                xTurnAmount = 290 - Mouse.GetState().X;

                yTurnAmount = Mouse.GetState().Y - 240;

                Mouse.SetPosition(290, 240);
            }

            ForwardDirection += xTurnAmount * GameConstants.TurnSpeed;
            Matrix orientationMatrix = Matrix.CreateRotationY(ForwardDirection);

            if ((AimDirection + (yTurnAmount * GameConstants.TurnSpeed) < 1.5 &&
                (AimDirection + (yTurnAmount * GameConstants.TurnSpeed) > -1.5)))
            {
                AimDirection += yTurnAmount * GameConstants.TurnSpeed;
            }

            

            Vector3 movement = Vector3.Zero;

            
            if (keyboardState.IsKeyDown(Keys.W))
            {
                movement.Z = 1;
            }
            else if (keyboardState.IsKeyDown(Keys.S))
            {
                movement.Z = -1;
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                movement.X = 1;
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                movement.X = -1;
            }

            if (keyboardState.IsKeyDown(Keys.LeftShift))
            {
                movement.Y = -1;
            }
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                movement.Y = 1;
            }
            

            Vector3 speed = Vector3.Transform(movement, orientationMatrix);

            speed *= GameConstants.Velocity;

            futurePosition = Position + speed;

            if (ValidateMovement(futurePosition, barriers))
            {
                Position = futurePosition;

                BoundingSphere updatedSphere;
                updatedSphere = BoundingSphere;

                updatedSphere.Center.X = Position.X;
                updatedSphere.Center.Y = Position.Y;
                updatedSphere.Center.Z = Position.Z;
                BoundingSphere = new BoundingSphere(updatedSphere.Center,
                    updatedSphere.Radius);
            }


        }

        private bool ValidateMovement(Vector3 futurePosition,
            Debris[] barriers)
        {
            BoundingSphere futureBoundingSphere = BoundingSphere;
            futureBoundingSphere.Center.X = futurePosition.X;
            futureBoundingSphere.Center.Y = futurePosition.Y;
            futureBoundingSphere.Center.Z = futurePosition.Z;

            //Don't allow off-terrain driving
            if ((Math.Abs(futurePosition.X) > MaxRange) ||
                (Math.Abs(futurePosition.Z) > MaxRange))
                return false;
            if (futurePosition.Y <= 0 || futurePosition.Y >= 150)
                return false;
            //Don't allow driving through a barrier
            if (CheckForDebrisCollision(futureBoundingSphere, barriers))
                return false;

            return true;
        }

        private bool CheckForDebrisCollision(
            BoundingSphere vehicleBoundingSphere, Debris[] barriers)
        {
            for (int curBarrier = 0; curBarrier < barriers.Length; curBarrier++)
            {
                if (vehicleBoundingSphere.Intersects(
                    barriers[curBarrier].BoundingSphere) && !barriers[curBarrier].Destroyed)
                    return true;
            }
            return false;
        }

        public bool CheckForFuelCollision(FuelCell[] fuelCells)
        {
            for (int i = 0; i < fuelCells.Length; i++)
            {
                if (BoundingSphere.Intersects(fuelCells[i].BoundingSphere) && !fuelCells[i].Retrieved)
                {
                    fuelCells[i].Retrieved = true;
                    return true;
                }
            }
            return false;
        }
    }
}