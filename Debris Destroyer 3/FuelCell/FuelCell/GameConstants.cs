// Copyright (C) Microsoft Corporation. All rights reserved.

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
    class GameConstants
    {
        //camera constants
        public const float NearClip = 1.0f;
        public const float FarClip = 1000.0f;
        public const float ViewAngle = 45.0f;

        //ship constants
        public const float Velocity = 0.75f;
        public const float TurnSpeed = 0.01f;
        public const int MaxRange = 98;

        //bullet constants
        public const float BulletSpeed = 2.0f;

        //general
        public const int MaxRangeTerrain = 98;
        public const int NumBarriers = 30;
        public const int NumFuelCells = 15;
        public const int MinDistance = 10;
        public const int MaxDistance = 90;
        public const string StrContinue = "Press Enter to begin the next level";
        public const string StrPlayAgain = 
            "Press Enter to play again or Esc to quit";

        //bounding sphere scaling factors
        public const float FuelCarrierBoundingSphereFactor = .8f;
        public const float FuelCellBoundingSphereFactor = .5f;
        public const float BarrierBoundingSphereFactor = .7f;
    }
}
