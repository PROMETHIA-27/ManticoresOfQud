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
            
        }

        /// Must be overridden but I don't need it
        public override void InitFromSeed(string seed) {}

        /// Whether or not this window will be shown
        /// Enabled if genotype is manticore and you've chosen a subtype
        public override bool shouldBeEnabled()
        {
            var genotypeModule = this.builder.GetModule<QudGenotypeModule>();
            if (genotypeModule?.data?.Genotype == "Manticore")
            {
                var subtypeModule = this.builder.GetModule<QudSubtypeModule>();
                return subtypeModule?.data != null;
            }
            return false;
        }

        /// Allows choosing a specific window order by exposing the list of windows
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
            // if (this.data.ToughnessPenalty > 0)
            //     return "You have excessive limbs, and will incur a toughness penalty if you continue.";
            return null;
        }

        /// Event handling while booting. 
        /// QudGameBootModule seems to have a list of event names, not sure if more exist.
        /// element is potentially some associated data of the event.
        public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
        {
            if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT && this.data != null)
            {

            }
            return base.handleBootEvent(id, game, info, element);
        }

        /// UI events, including of *other* windows.
        /// Each window likely has its own event list.
        public override object handleUIEvent(string id, object element)
        {
            // if (id == QudAttributesModuleWindow.EID_GET_BASE_ATTRIBUTES)
            // {
            //     List<AttributeDataElement> source = element as List<AttributeDataElement>;
            //     foreach (var attr in source)
            //         if (attr.Attribute == "Toughness")
            //             attr.Bonus -= this.data.ToughnessPenalty;
            // }
            return base.handleUIEvent(id, element);
        }
    }
}

