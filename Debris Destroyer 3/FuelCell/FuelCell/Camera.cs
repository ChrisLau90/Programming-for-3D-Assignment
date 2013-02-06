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
    class Camera
    {
        public Vector3 AvatarHeadOffset { get; set; }
        public Vector3 TargetOffset { get; set; }
        public Matrix ViewMatrix { get; set; }
        public Matrix ProjectionMatrix { get; set; }

        public Camera()
        {
            AvatarHeadOffset = new Vector3(0, 5, -15);
            TargetOffset = new Vector3(0, 5, 0);
            ViewMatrix = Matrix.Identity;
            ProjectionMatrix = Matrix.Identity;
        }

        public void Update(float avatarYaw, float avatarPitch, Vector3 position, float aspectRatio)
        {
            //ADDED AVATARYAW2 PARAMETER

            Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(avatarYaw, avatarPitch, 0f);

            Vector3 transformedheadOffset =
                Vector3.Transform(AvatarHeadOffset, rotationMatrix);

            Vector3 transformedReference =
                Vector3.Transform(TargetOffset, rotationMatrix);

            Vector3 cameraPosition = position + transformedheadOffset;
            Vector3 cameraTarget = position + transformedReference;

            //Calculate the camera's view and projection
            // matrices based on current values.
            ViewMatrix =
                Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
            ProjectionMatrix =
                Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(GameConstants.ViewAngle), aspectRatio,
                    GameConstants.NearClip, GameConstants.FarClip);
        }
    }
}
