using System.Collections.Immutable;
using System.Collections.Generic;

namespace PRM {
    /// <summary>
    /// A struct representing a body part in the anatomy of a creachure
    /// </summary>
    public struct BodyPart {
        /// <summary>
        /// The name of this body part, e.g. "Tentacle" or "Right Hand"
        /// </summary>
        public readonly string name;

        /// <summary>
        /// The archetype of this body part, e.g. Hand, Body, Feet
        /// </summary>
        public readonly LimbArchetype archetype;

        /// <summary>
        /// A unique identifier for this body part in the context of its parent tree
        /// </summary>
        readonly uint id;

        /// <summary>
        /// The index of this body part in its tree's arena.
        /// Parts are stored in the tree arena with a None thisIdx;
        /// when a part is passed outside of the tree, it then has the thisIdx set to a value.
        /// </summary>
        readonly Option<int> thisIdx;

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
        /// Internal constructor for use by BodyTree
        /// </summary>
        /// <param name="name">Name of this body part, e.g. "Tentacle"</param>
        /// <param name="archetype">Archetype of this body part</param>
        /// <param name="thisIdx">Index of this body part in the tree's arena</param>
        /// <param name="parent">Optional index of this part's parent in the arena</param>
        /// <param name="children">Indices of this part's children in the arena</param>
        BodyPart(string name, LimbArchetype archetype, uint id, Option<int> thisIdx, Option<int> parent, ImmutableArray<int> children) {
            this.name = name;
            this.archetype = archetype;
            this.id = id;
            this.thisIdx = thisIdx;
            this.parent = parent;
            this.children = children;
        }

        /// <summary>
        /// The number of child body parts this part has
        /// </summary>
        public int NumChildren => children.Length;

        /// <summary>
        /// Returns a hash of the part based on its ID.
        /// Within a given tree, each part should have a unique hash.
        /// </summary>
        /// <returns>The hash value of this part.</returns>
        public override int GetHashCode()
        {
            // Adapted from https://stackoverflow.com/questions/664014/what-integer-hash-function-are-good-that-accepts-an-integer-hash-key
            var x = this.id;
            x = ((x >> 16) ^ x) * 0x45d9f3b;
            x = ((x >> 16) ^ x) * 0x45d9f3b;
            x = (x >> 16) ^ x;
            return unchecked((int)x);
        }

        /// <summary>
        /// A tree structure that represents the anatomy of a creachure
        /// </summary>
        public struct Tree {
            /// <summary>
            /// An arena that stores all the body parts of this tree.
            /// Is assumed to always have at least one element, the root.
            /// Each index may or may not store a part; In the case that a position contains
            /// None, it is free to insert a part at.
            /// </summary>
            readonly ImmutableArray<Option<BodyPart>> partArena;

            /// <summary>
            /// The total number of Some parts in the arena.
            /// </summary>
            readonly int partCount;

            /// <summary>
            /// The next ID to give to a part.
            /// Updated whenever adding parts.
            /// </summary>
            readonly uint nextId;

            /// <summary>
            /// Internal tree constructor
            /// </summary>
            Tree(ImmutableArray<Option<BodyPart>> partArena, int partCount, uint nextId) {
                this.partArena = partArena;
                this.partCount = partCount;
                this.nextId = nextId;
            }
            
            /// Construct a new BodyPart.Tree
            /// </summary>
            /// <param name="rootName">Name of the root body part, e.g. "Carapace", "Jim", "Blumbon"</param>
            /// <param name="rootArchetype">Archetype of the root body part (assumed to be a body, but not enforced)</param>
            /// <returns>A tree with one element, the root, which is assumed, but not
            /// enforced, to be a body and the first element of the underlying part arena.</returns>
            public Tree(string rootName, LimbArchetype rootArchetype) {
                var root = new BodyPart(rootName, rootArchetype, 0, new Option<int>(), new Option<int>(), ImmutableArray<int>.Empty);

                this.partArena = ImmutableArray.Create(new Option<BodyPart>(root));
                this.partCount = 1;
                this.nextId = 1;
            }

            /// <summary>
            /// Get the root of the body tree, which is assumed to be a Body and the first element
            /// of the underlying part arena of the tree
            /// </summary>
            /// <returns></returns>
            public BodyPart Root() {
                var part = this.partArena[0];
                if (part.IsSome())
                    return part.Unwrap();
                else throw new System.InvalidOperationException("Body tree has no root element!");
            }

            /// <summary>
            /// Get the nth child of a given part, where childIdx is n.
            /// </summary>
            /// <value>The nth child of "part"</value>
            public BodyPart this[BodyPart part, int childIdx] {
                get {
                    var index = part.children[childIdx];
                    var arenaPart = this.partArena[index];

                    if (!arenaPart.IsSome())
                        throw new System.InvalidOperationException("Failed to acquire part, body tree is invalid!");

                    var p = arenaPart.Unwrap();

                    return new BodyPart(
                        p.name, 
                        p.archetype, 
                        p.id, 
                        new Option<int>(index), 
                        p.parent, 
                        p.children
                    );
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
                if (!parent.thisIdx.IsSome())
                    throw new System.InvalidOperationException("Passed in BodyPart lacks a .thisIdx!");

                var childArenaIdx = this.partArena.Length;
                var child = new BodyPart(childName, childArchetype, this.nextId, new Option<int>(), parent.thisIdx, ImmutableArray<int>.Empty);

                var childList = childIdx.IsSome() ? parent.children.Insert(childIdx.Unwrap(), childArenaIdx) : parent.children.Add(childArenaIdx);
                var newParent = new BodyPart(parent.name, parent.archetype, parent.id, new Option<int>(), parent.parent, childList);

                var arenaBuilder = this.partArena.ToBuilder();
                if (this.partCount < this.partArena.Length) 
                    // If there are nones, find one and use it instead of appending
                    for (int i = 1; i < this.partArena.Length; i++)
                        if (!this.partArena[i].IsSome()) {
                            arenaBuilder[i] = new Option<BodyPart>(child);
                            break;
                        }
                else 
                    // Otherwise, just append
                    arenaBuilder.Add(new Option<BodyPart>(child));
                // Update parent
                arenaBuilder[parent.thisIdx.Unwrap()] = new Option<BodyPart>(newParent);
                var arena = arenaBuilder.ToImmutable();

                return new Tree(arena, this.partCount + 1, this.nextId + 1);
            }

            /// <summary>
            /// Construct a new tree without the given part or any of its children.
            /// The removed part must not be the root.
            /// </summary>
            /// <param name="part">The part to remove.</param>
            /// <returns>A new tree, not containing the given part or any of its children.</returns>
            /// <exception cref="System.InvalidOperationException">Throws if the part is the root of the tree.</exception>
            public Tree WithoutPart(BodyPart part) {
                if (!part.thisIdx.IsSome())
                    throw new System.InvalidOperationException("Passed in BodyPart lacks a .thisIdx!");

                var builder = this.partArena.ToBuilder();

                if (part.parent.IsSome()) {
                    // Remove part from its parent
                    var parent = builder[part.parent.Unwrap()].Unwrap();
                    var newChildren = parent.children.Remove(part.thisIdx.Unwrap());
                    var newParent = new BodyPart(parent.name, parent.archetype, parent.id, parent.thisIdx, parent.parent, newChildren);
                    builder[part.parent.Unwrap()] = new Option<BodyPart>(newParent);
                } 
                else throw new System.InvalidOperationException($"Attempted to remove the root of a {nameof(PRM.BodyPart.Tree)}!");

                var stack = new Stack<int>();
                stack.Push(part.thisIdx.Unwrap());

                var removed = 0;

                while (stack.Count > 0) {
                    var currentIdx = stack.Pop();
                    var current = this.partArena[currentIdx].Unwrap();

                    foreach (var childIdx in current.children)
                        stack.Push(childIdx);

                    builder[currentIdx] = new Option<BodyPart>();

                    removed += 1;
                }

                var arena = builder.ToImmutable();
                return new Tree(arena, this.partCount - removed, this.nextId);
            }

            /// <summary>
            /// Call a closure on every part of the body tree, in preorder
            /// </summary>
            /// <param name="fn">The closure to call. First parameter is the part, second is the depth (root is 0).</param>
            public void MapPartsPreorder(System.Action<BodyPart, int> fn) {
                var stack = new Stack<(int, int)>();

                stack.Push((0, 0));

                while (stack.Count > 0) {
                    var (currIdx, depth) = stack.Pop();
                    var current = this.partArena[currIdx].Unwrap();

                    fn(current, depth);

                    for (int i = current.NumChildren - 1; i >= 0; i--) {
                        stack.Push((current.children[i], depth + 1));
                    }
                }
            }
        }
    }
}