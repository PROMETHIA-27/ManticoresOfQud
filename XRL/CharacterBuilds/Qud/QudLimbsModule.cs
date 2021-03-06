using PRM;
using System.Collections.Generic;

namespace XRL.CharacterBuilds.Qud
{
    /// <summary>
    /// The logical backend of the chargen process module
    /// </summary>
    public class QudLimbsModule : EmbarkBuilderModule<QudLimbsModuleData>
    {
        /// <summary>
        /// Initialization of limbs module
        /// </summary>
        /// <param name="seed">Seed to build character from</param>
        public override void InitFromSeed(string seed) {}

        /// <summary>
        /// Whether or not this window will be shown.
        /// Enabled if genotype is manticore and you've chosen a subtype.
        /// </summary>
        /// <returns>True if should be enabled, false if not</returns>
        public override bool shouldBeEnabled() {
            var genotypeModule = this.builder.GetModule<QudGenotypeModule>();
            if (genotypeModule?.data?.Genotype == "Manticore") {
                var subtypeModule = this.builder.GetModule<QudSubtypeModule>();
                return subtypeModule?.data != null;
            }
            return false;
        }

        /// <summary>
        /// Allows choosing a specific window order by exposing the list of windows
        /// </summary>
        /// <param name="windows">Current list of ordered window descriptors</param>
        public override void assembleWindowDescriptors(List<EmbarkBuilderModuleWindowDescriptor> windows) {
            var idx = windows.FindIndex(w => w.viewID == "Chargen/ChooseSubtypes");
            windows.Insert(idx + 1, this.windows["Chargen/ChooseLimbs"]);
        }

        /// <summary>
        /// Prevents you from moving forward in the chargen process if something is wrong
        /// </summary>
        /// <returns>A string explaining the error, or null</returns>
        public override string DataErrors() {
            return null;
        }

        /// <summary>
        /// Warns you when moving forward in the chargen process if something is wrong
        /// </summary>
        /// <returns>A warning string or null</returns>
        public override string DataWarnings() {
            // if (this.data.ToughnessPenalty > 0)
            //     return "You have excessive limbs, and will incur a toughness penalty if you continue.";
            return null;
        }

        /// <summary>
        /// Event handling while booting. 
        /// QudGameBootModule seems to have a list of event names, not sure if more exist.
        /// </summary>
        /// <param name="id">ID of the event</param>
        /// <param name="game">Game data</param>
        /// <param name="info">Current embark info</param>
        /// <param name="element">Either null or associated data of the event</param>
        /// <returns>element, usually</returns>
        public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null) {
            if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT && this.enabled) {
                XRL.World.GameObject player = element as XRL.World.GameObject;

                var anatomy = ConstructAnatomy(this.data.anatomyTree);
                XRL.World.Anatomies.AnatomyTable["CustomManticore"] = anatomy;
                player.Body.Rebuild("CustomManticore");
            }
            return base.handleBootEvent(id, game, info, element);
        }

        /// <summary>
        /// UI events, including of *other* windows.
        /// Each window likely has its own event list.
        /// </summary>
        /// <param name="id">ID of the event</param>
        /// <param name="element">associated data</param>
        /// <returns>element, usually</returns>
        public override object handleUIEvent(string id, object element) {
            // if (id == QudAttributesModuleWindow.EID_GET_BASE_ATTRIBUTES)
            // {
            //     List<AttributeDataElement> source = element as List<AttributeDataElement>;
            //     foreach (var attr in source)
            //         if (attr.Attribute == "Toughness")
            //             attr.Bonus -= this.data.ToughnessPenalty;
            // }
            return base.handleUIEvent(id, element);
        }

        /// <summary>
        /// Set the default value for data, since it normally defaults to null and that's annoying
        /// </summary>
        public override AbstractEmbarkBuilderModuleData DefaultData => new QudLimbsModuleData();

        /// <summary>
        /// Add a limb as a child of the given part
        /// </summary>
        /// <param name="part">The parent part</param>
        public void AddLimb(BodyPart part, LimbArchetype archetype, string name) {
            var data = this.data;
            var tree = data.anatomyTree;
            data.anatomyTree = tree.WithChildPart(part, new Option<int>(), name, archetype);
        }

        /// <summary>
        /// Remove a limb from the body tree, as well as its children.
        /// </summary>
        /// <param name="part">The part to remove</param>
        public void RemoveLimb(BodyPart part) {
            this.data.anatomyTree = this.data.anatomyTree.WithoutPart(part);
        }

        /// <summary>
        /// Use a body tree to construct a new anatomy for the manticore
        /// </summary>
        /// <param name="tree">The tree to construct an anatomy from</param>
        /// <returns>A new Anatomy object</returns>
        static World.Anatomy ConstructAnatomy(BodyPart.Tree tree) {
            var anatomy = new World.Anatomy("CustomManticore");
            var stack = new Stack<(List<World.AnatomyPart>, BodyPart)>();
            
            foreach (var child in tree.ReverseChildrenOf(tree.Root)) {
                var anatomyPart = new World.AnatomyPart(World.Anatomies.GetBodyPartType(child.archetype.name));
                
                stack.Push((anatomy.Parts, child));
            }

            while (stack.Count > 0) {
                var (list, part) = stack.Pop();
                var anatomyPart = new World.AnatomyPart(World.Anatomies.GetBodyPartType(part.archetype.name));

                list.Add(anatomyPart);

                foreach (var child in tree.ReverseChildrenOf(part))
                    stack.Push((anatomyPart.Subparts, child));
            }

            return anatomy;
        }
    }
}

