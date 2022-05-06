using System.Diagnostics;
using XRL.World;
using XRL.World.Parts;
using XRL.Wish;
using XRL.CharacterBuilds.Qud.UI;
using System.Collections.Generic;
using System.Linq;

namespace XRL.CharacterBuilds.Qud
{
    [HasWishCommand]
    /// The logical backend of the chargen process module
    public class QudLimbsModule : EmbarkBuilderModule<QudLimbsModuleData>
    {
        public QudLimbsModule()
        {
            var partsDict = Anatomies.GetBodyPartTypeSelector(true, true, false, true, null, null, true);
            foreach (var part in partsDict.Keys)
                this.Limbs.Add(part);
			foreach (var part in Anatomies.BodyPartTypeList)
				if ((part.ImpliedBy ?? "") != "")
					this.ImpliedLimbs.Add(part);
        }

        /// Whether or not this window will be shown
        public override bool shouldBeEnabled()
        {
            var genotypeModule = this.builder.GetModule<QudGenotypeModule>();
            if (genotypeModule != null && genotypeModule.data != null && genotypeModule.data.Genotype == "Manticore")
            {
                var subtypeModule = this.builder.GetModule<QudSubtypeModule>();
                return subtypeModule != null && subtypeModule.data != null;
            }
            return false;
        }

        /// Eh? Gets a seed so probably good for randomizing some stuff. World seed maybe?
        public override void InitFromSeed(string seed)
        {
        }

        /// Allows choosing a specific window order be exposing the list of windows
        public override void assembleWindowDescriptors(List<EmbarkBuilderModuleWindowDescriptor> windows)
        {
            var idx = windows.FindIndex(w => w.viewID == "Chargen/ChooseSubtypes");
            windows.Insert(idx + 1, this.windows["Chargen/ChooseLimbs"]);
        }

        /// Prevents you from moving forward in the chargen process if something is wrong
        /// Returns a string explaining the error, or null
        public override string DataErrors()
        {
            return null;
        }

        /// Warns you when moving forward in the chargen process if something is wrong
        /// Returns a warning string or null
        public override string DataWarnings()
        {
            if (this.data.ToughnessPenalty > 0)
                return "You have excessive limbs, and will incur a toughness penalty if you continue.";
            return null;
        }

        /// Event handling while booting. 
        /// QudGameBootModule seems to have a list of event names, not sure if more exist.
        /// element is potentially some associated data of the event.
        public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
        {
            if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT && this.data != null)
            {
                GameObject player = element as GameObject;
                var newAnatomy = new Anatomy("Manticore");
                newAnatomy.BodyType = player.Body.GetBody().Type;

                foreach (var part in this.data.root.Parts)
                {
                    var anatomyPart = part.ToAnatomyPart();    
                    newAnatomy.Parts.Add(anatomyPart);
                }
                
                Anatomies.AnatomyTable.Add("Manticore", newAnatomy);
                Anatomies.AnatomyList.Add(newAnatomy);

                player.Body.Rebuild("Manticore");

                Anatomies.AnatomyTable.Remove("Manticore");
                Anatomies.AnatomyList.Remove(newAnatomy);

                // TraverseAnatomyAndApplyChanges(this.data.root, player.Body.GetBody());

                player.GetStat("Toughness").BaseValue -= this.data.ToughnessPenalty;
            }
            return base.handleBootEvent(id, game, info, element);
        }

        /// TODO: Remove this from this class. This is my own logic, and should be separated.
        /// I believe this collects all different part types into a hashset.
        public static void TraverseAnatomyPartsAndAddToSet(HashSet<AnatomyPart> set, AnatomyPart part)
        {
            set.Add(part);
            for (int i = 0; i < part.Subparts.Count; i++)
                TraverseAnatomyPartsAndAddToSet(set, part.Subparts[i]);
        }

        /// UI events, including of *other* windows.
        /// Each window likely has its own event list.
        public override object handleUIEvent(string id, object element)
        {
            if (id == QudAttributesModuleWindow.EID_GET_BASE_ATTRIBUTES)
            {
                List<AttributeDataElement> source = element as List<AttributeDataElement>;
                foreach (var attr in source)
                    if (attr.Attribute == "Toughness")
                        attr.Bonus -= this.data.ToughnessPenalty;
            }
            return base.handleUIEvent(id, element);
        }

        /// TODO: This is all my own logic, and is also data, should be moved somewhere else I think.
        public List<BodyPartType> Limbs = new List<BodyPartType>();
		public List<BodyPartType> ImpliedLimbs = new List<BodyPartType>();
        public const int BaseLimbPoints = 16;
    }
}

