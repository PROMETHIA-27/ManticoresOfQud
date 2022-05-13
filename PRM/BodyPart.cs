using System.Collections.Immutable;
using System.Collections.Generic;

namespace PRM {
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

        /// <summary>
        /// Internal constructor for use by BodyTree. Likely not useful to a user. Please use
        /// BodyTree's methods instead.
        /// </summary>
        /// <param name="name">Name of this body part, e.g. "Tentacle"</param>
        /// <param name="archetype">Archetype of this body part</param>
        /// <param name="thisIdx">Index of this body part in the tree's arena</param>
        /// <param name="parent">Optional index of this part's parent in the arena</param>
        /// <param name="children">Indices of this part's children in the arena</param>
        public BodyPart(string name, LimbArchetype archetype, int thisIdx, Option<int> parent, ImmutableArray<int> children) {
            this.name = name;
            this.archetype = archetype;
            this.thisIdx = thisIdx;
            this.parent = parent;
            this.children = children;
        }

        /// <summary>
        /// A tree structure that represents the anatomy of a creachure
        /// </summary>
        struct Tree {
            /// <summary>
            /// An arena that stores all the body parts of this tree.
            /// Is assumed to always have at least one element, the root.
            /// </summary>
            readonly ImmutableArray<BodyPart> partArena;

            Tree(ImmutableArray<BodyPart> partArena) {
                this.partArena = partArena;
            }
            
            /// <summary>
            /// Construct a new BodyPart.Tree
            /// </summary>
            /// <returns>A tree with one element, the root, which is assumed, but not
            /// enforced, to be a body and the first element of the underlying part arena.</returns>
            public static Tree New(BodyPart root) {
                return new Tree(ImmutableArray.Create(root));
            }

            /// <summary>
            /// Get the root of the body tree, which is assumed to be a Body and the first element
            /// of the underlying part arena of the tree
            /// </summary>
            /// <returns></returns>
            public BodyPart Root() {
                return this.partArena[0];
            }

            /// <summary>
            /// Get the nth child of a given part, where childIdx is n.
            /// </summary>
            /// <value>The nth child of "part"</value>
            public BodyPart this[BodyPart part, int childIdx] {
                get {
                    return this.partArena[part.children[childIdx]];
                }
            }

            /// <summary>
            /// Construct a new Tree which contains a new BodyPart as a child of the given parent
            /// BodyPart.
            /// </summary>
            /// <param name="parent">The parent to add a BodyPart as a child of.</param>
            /// <param name="childIdx">The index to insert the child to on the parent.
            /// Some will insert at the given position, None will append to the end of the
            /// child list.</param>
            /// <param name="childName">The name of the new child.</param>
            /// <param name="childArchetype">The archetype of the new child.</param>
            /// <returns>A new tree containing the new child part</returns>
            public Tree WithChildPart(BodyPart parent, Option<int> childIdx, string childName, LimbArchetype childArchetype) {
                var child = new BodyPart(childName, childArchetype, this.partArena.Length, new Option<int>(parent.thisIdx), ImmutableArray<int>.Empty);

                var childList = childIdx.IsSome() ? parent.children.Insert(childIdx.Unwrap(), child.thisIdx) : parent.children.Add(child.thisIdx);
                var newParent = new BodyPart(parent.name, parent.archetype, parent.thisIdx, parent.parent, childList);

                var arenaBuilder = this.partArena.ToBuilder();
                arenaBuilder.Add(child);
                arenaBuilder[parent.thisIdx] = newParent;
                var arena = arenaBuilder.ToImmutable();

                return new Tree(arena);
            }

            /// <summary>
            /// Construct a new tree without the given part or any of its children.
            /// The removed part must not be the root.
            /// </summary>
            /// <param name="part">The part to remove.</param>
            /// <returns>A new tree, not containing the given part or any of its children.</returns>
            /// <exception cref="System.InvalidOperationException">Throws if the part is the root of the tree.</exception>
            public Tree WithoutPart(BodyPart part) {
                var builder = this.partArena.ToBuilder();

                if (part.parent.IsSome()) {
                    // Remove part from its parent
                    var parent = builder[part.parent.Unwrap()];
                    var newChildren = parent.children.Remove(part.thisIdx);
                    var newParent = new BodyPart(parent.name, parent.archetype, parent.thisIdx, parent.parent, newChildren);
                    builder[part.parent.Unwrap()] = newParent;
                } 
                else throw new System.InvalidOperationException($"Attempted to remove the root of a {nameof(PRM.BodyPart.Tree)}!");

                var stack = new Stack<int>();
                stack.Push(part.thisIdx);

                while (stack.Count > 0) {
                    var currentIdx = stack.Pop();
                    var current = this.partArena[currentIdx];

                    foreach (var childIdx in current.children)
                        stack.Push(childIdx);

                    builder.RemoveAt(currentIdx);
                }

                var arena = builder.ToImmutable();
                return new Tree(arena);
            }
        }
    }
}