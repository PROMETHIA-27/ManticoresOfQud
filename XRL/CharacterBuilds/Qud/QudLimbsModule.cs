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

        public override void InitFromSeed(string seed)
        {
        }

        public override void assembleWindowDescriptors(List<EmbarkBuilderModuleWindowDescriptor> windows)
        {
            var idx = windows.FindIndex(w => w.viewID == "Chargen/ChooseSubtypes");
            windows.Insert(idx + 1, this.windows["Chargen/ChooseLimbs"]);
        }

        public override string DataErrors()
        {
            return null;
        }

        public override string DataWarnings()
        {
            if (this.data.ToughnessPenalty > 0)
                return "You have excessive limbs, and will incur a toughness penalty if you continue.";
            return null;
        }

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

        public static void TraverseAnatomyPartsAndAddToSet(HashSet<AnatomyPart> set, AnatomyPart part)
        {
            set.Add(part);
            for (int i = 0; i < part.Subparts.Count; i++)
                TraverseAnatomyPartsAndAddToSet(set, part.Subparts[i]);
        }

        // public static void TraverseAnatomyAndApplyChanges(SimpleBodyPart part, BodyPart bodyPart)
        // {
        //     // bodyPart.Name = part.Name;
        //     // bodyPart.Description = part.Description;
        //     // bodyPart.DescriptionPrefix = part.DescriptionPrefix; 

        //     for (int i = 0; i < part.Parts.Count; i++)
        //         TraverseAnatomyAndApplyChanges(part.Parts[i], bodyPart.Parts[i]);
        // }

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

        public List<BodyPartType> Limbs = new List<BodyPartType>();
		public List<BodyPartType> ImpliedLimbs = new List<BodyPartType>();
        public const int BaseLimbPoints = 16;
    }
}

