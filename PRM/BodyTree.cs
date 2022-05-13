using System.Collections.Immutable;

namespace PRM {
    /// <summary>
    /// A tree structure that represents the anatomy of a creachure
    /// </summary>
    struct BodyTree {
        /// <summary>
        /// An arena that stores all the body parts of this tree.
        /// </summary>
        readonly ImmutableArray<BodyPart> partArena;

        BodyTree(ImmutableArray<BodyPart> partArena) {
            this.partArena = partArena;
        }
        
        /// <summary>
        /// Construct a new BodyTree
        /// </summary>
        /// <returns>An empty BodyTree</returns>
        public static BodyTree New() {
            return new BodyTree(ImmutableArray<BodyPart>.Empty);
        }
    }

    /// <summary>
    /// A struct representing a body part in the anatomy of a creachure
    /// </summary>
    struct BodyPart {
        /// <summary>
        /// The name of this body part, e.g. "Tentacle" or "Right Hand"
        /// </summary>
        public readonly string name;

        /// <summary>
        /// The archetype of this body part, e.g. Hand, Body, Feet
        /// </summary>
        public readonly LimbArchetype archetype;

        /// <summary>
        /// The index of this body part in its tree's arena
        /// </summary>
        readonly int thisIdx;

        /// <summary>
        /// The index of the parent of this body part in it's tree's arena.
        /// 
        /// None if this part has no parent. The only part which should lack a parent is the body,
        /// which should be located at index 0 in the arena of the body.
        /// </summary>
        readonly Option<int> parent;

        /// <summary>
        /// A list of the children of this body part.
        /// 
        /// Each element is an index of the arena of the body.
        /// </summary>
        readonly ImmutableArray<int> children;

        BodyPart(string name, LimbArchetype archetype, int thisIdx, Option<int> parent, ImmutableArray<int> children) {
            this.name = name;
            this.archetype = archetype;
            this.thisIdx = thisIdx;
            this.parent = parent;
            this.children = children;
        }
    }
}