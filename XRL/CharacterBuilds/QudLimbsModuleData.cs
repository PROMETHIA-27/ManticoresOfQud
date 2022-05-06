using System.Globalization;
using System.Security;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using XRL.World.Parts;
using XRL.World;
using XRL.World.Capabilities;
using static System.Math;

namespace XRL.CharacterBuilds 
{
    /// Data storage for the module
    public class QudLimbsModuleData : AbstractEmbarkBuilderModuleData
    {
        public int currentLimbPoints = XRL.CharacterBuilds.Qud.QudLimbsModule.BaseLimbPoints;
        public int ToughnessPenalty => Abs(Min(0, currentLimbPoints));
    }
}