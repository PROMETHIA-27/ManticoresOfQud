using System.Globalization;
using System.Security;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using XRL.World.Parts;
using XRL.World;
using XRL.World.Capabilities;
using PRM;
using static System.Math;

namespace XRL.CharacterBuilds 
{
    /// <summary>
    /// Data storage for the module
    /// </summary>
    public class QudLimbsModuleData : AbstractEmbarkBuilderModuleData
    {
        /// <summary>
        /// The number of points a new manticore gets to spend on limbs
        /// </summary>
        public const int BASE_LIMB_POINTS = 16;

        /// <summary>
        /// The number of limb points the current chargen has left
        /// </summary>
        public int CurrentLimbPoints = BASE_LIMB_POINTS;

        /// <summary>
        /// Possibly an escape hatch to spend more than 16 points. TODO: Decide if this is wanted
        /// </summary>
        public int ToughnessPenalty => Abs(Min(0, CurrentLimbPoints));

        /// <summary>
        /// A list of all limb archetypes with the Appendage tag. This excludes body and back,
        /// as well as some abstract parts.
        /// </summary>
        public List<LimbArchetype> AppendageArchetypes => new List<LimbArchetype>();
    }
}