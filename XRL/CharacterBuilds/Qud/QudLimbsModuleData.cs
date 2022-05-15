
using PRM;
using System.Collections.Generic;
using XRL.CharacterBuilds.UI;
using XRL.UI.Framework;

using static System.Math;

namespace XRL.CharacterBuilds 
{
    /// <summary>
    /// Data storage for the limbs module
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
        public int currentLimbPoints = BASE_LIMB_POINTS;

        /// <summary>
        /// Possibly an escape hatch to spend more than 16 points. TODO: Decide if this is wanted
        /// </summary>
        public int ToughnessPenalty => Abs(Min(0, this.currentLimbPoints));

        /// <summary>
        /// The anatomy tree of the current chargen.
        /// </summary>
        public BodyPart.Tree anatomyTree = new BodyPart.Tree("Body", new LimbArchetype("Body", false));

        /// <summary>
        /// A list of categories to be used in the UI.
        /// </summary>
        public List<CategoryMenuData> menuData = new List<CategoryMenuData>(); 

        /// <summary>
        /// Pool of PrefixMenuOptions, to avoid constant re-alloc.
        /// </summary>
        public Pool<PrefixMenuOption> prefixOptionPool = new Pool<PrefixMenuOption>(12);

        public QudLimbsModuleData() {
            this.menuData.Add(new CategoryMenuData() {
                Title = "Manticore Anatomy",
                Description = "Build-a-Creature",
                menuOptions = new List<PrefixMenuOption>()
            });
        }
    }
}