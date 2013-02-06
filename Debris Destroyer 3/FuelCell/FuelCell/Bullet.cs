using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// REFERENCES:
/// [0] FuelCell game code http://msdn.microsoft.com/en-us/library/dd940288.aspx
/// date accessed 18th April 2012
/// </summary>
namespace FuelCell
{
    class Bullet : GameObject
    {
        public Ship ship;
        public float ForwardDirection { get; set; }
        public float AimDirection { get; set; }
        private float distance = 1;
        Vector3 startPosition;
        public int timer;
        
        public Bullet(ContentManager content, Ship fc)
        {
            ship = fc;

            ForwardDirection = ship.ForwardDirection;
            AimDirection = ship.AimDirection;

            startPosition = ship.Position;
            
            Model = content.Load<Model>("Models/bullet4");

            BoundingSphere = CalculateBoundingSphere();
            BoundingSphere scaledSphere;
            scaledSphere = BoundingSphere;
            scaledSphere.Radius *=
                GameConstants.FuelCarrierBoundingSphereFactor;
            BoundingSphere =
                new BoundingSphere(scaledSphere.Center, scaledSphere.Radius);

            timer = 0;
            
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix worldMatrix = Matrix.Identity;
            Matrix translateMatrix = Matrix.CreateTranslation(Position);

            worldMatrix = translateMatrix;

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

        public void Update(Debris[] barriers)
        {
            distance = distance * 1.2f;
            Vector3 direction = Vector3.Transform(new Vector3(0, 0, distance), Matrix.CreateRotationX(AimDirection));
            direction = Vector3.Transform(direction, Matrix.CreateRotationY(ForwardDirection));
            
            Position = direction + startPosition;

            timer++;

            BoundingSphere updatedSphere;
            updatedSphere = BoundingSphere;

            updatedSphere.Center.X = Position.X;
            updatedSphere.Center.Y = Position.Y;
            updatedSphere.Center.Z = Position.Z;
            BoundingSphere = new BoundingSphere(updatedSphere.Center,
                updatedSphere.Radius);
             
        }

        public bool CheckForCollision(BoundingSphere bulletBoundingSphere, Debris[] barriers)
        {
            for (int curBarrier = 0; curBarrier < barriers.Length; curBarrier++)
            {
                if (bulletBoundingSphere.Intersects(
                    barriers[curBarrier].BoundingSphere) && !barriers[curBarrier].Destroyed)
                {
                    barriers[curBarrier].Destroyed = true;
                    return true;
                }
            }
            return false;
        }
    }
}
