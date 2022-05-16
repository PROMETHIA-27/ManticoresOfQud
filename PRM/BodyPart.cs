using System.Collections.Immutable;
using System.Collections.Generic;
using System;

namespace PRM {
    /// <summary>
    /// A struct representing a body part in the anatomy of a creachure
    /// </summary>
    public struct BodyPart : IEquatable<BodyPart> {
        /// <summary>
        /// The name of this body part, e.g. "Tentacle" or "Right Hand"
        /// </summary>
        public readonly string name;

        /// <summary>
        /// The archetype of this body part, e.g. Hand, Body, Feet
        /// </summary>
        public readonly LimbArchetype archetype;

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
        BodyPart(string name, LimbArchetype archetype, Option<int> thisIdx, Option<int> parent, ImmutableArray<int> children) {
            this.name = name;
            this.archetype = archetype;
            this.thisIdx = thisIdx;
            this.parent = parent;
            this.children = children;
        }

        /// <summary>
        /// The number of child body parts this part has
        /// </summary>
        public int NumChildren => children.Length;

        /// <summary>
        /// Copy the part but replace the thisIdx field
        /// </summary>
        /// <param name="thisIdx">The thisIdx value to place into the copy's field</param>
        /// <returns>A copied body part with a new thisIdx field</returns>
        BodyPart WithThisIdx(Option<int> thisIdx) => 
            new BodyPart(this.name, this.archetype, thisIdx, this.parent, this.children);

        /// <summary>
        /// Copy the part but replace the children field
        /// </summary>
        /// <param name="children">The children value to place into the copy's field</param>
        /// <returns>A copied body part with a new children field</returns>
        BodyPart WithChildren(ImmutableArray<int> children) => 
            new BodyPart(this.name, this.archetype, this.thisIdx, this.parent, children);

        public override string ToString()
        {
            return $"BodyPart {{ name: \"{this.name}\", archetype: {this.archetype}, NumChildren: {this.NumChildren} }}";
        }

        public bool Equals(BodyPart other)
        {
            return this.name == other.name &&
                   this.archetype.Equals(other.archetype) &&
                   this.thisIdx.Equals(other.thisIdx) && 
                   this.parent.Equals(other.parent) &&
                   this.children == other.children;
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
            /// Internal tree constructor
            /// </summary>
            Tree(ImmutableArray<Option<BodyPart>> partArena, int partCount) {
                this.partArena = partArena;
                this.partCount = partCount;
            }
            
            /// Construct a new BodyPart.Tree
            /// </summary>
            /// <param name="rootName">Name of the root body part, e.g. "Carapace", "Jim", "Blumbon"</param>
            /// <param name="rootArchetype">Archetype of the root body part (assumed to be a body, but not enforced)</param>
            /// <returns>A tree with one element, the root, which is assumed, but not
            /// enforced, to be a body and the first element of the underlying part arena.</returns>
            public Tree(string rootName, LimbArchetype rootArchetype) {
                var root = new BodyPart(rootName, rootArchetype, new Option<int>(), new Option<int>(), ImmutableArray<int>.Empty);

                this.partArena = ImmutableArray.Create(new Option<BodyPart>(root));
                this.partCount = 1;
            }

            /// <summary>
            /// Get the root of the body tree, which is assumed to be a Body and the first element
            /// of the underlying part arena of the tree
            /// </summary>
            /// <returns></returns>
            public BodyPart Root {
                get {
                    var part = this.partArena[0];
                    if (part.IsSome())
                        return part.Unwrap().WithThisIdx(new Option<int>(0));
                    else throw new System.InvalidOperationException("Body tree has no root element!");
                }
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

                    return arenaPart.Unwrap().WithThisIdx(new Option<int>(index));
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
                    throw new System.ArgumentException("Passed in BodyPart lacks a .thisIdx!");

                int childArenaIdx;
                if (this.partCount < this.partArena.Length) {
                    // If there are empty slots, find one and use it instead of appending
                    // This is inefficient but there will never be a significant number of elements
                    // so idc
                    for (int i = 1; i < this.partArena.Length; i++)
                        if (!this.partArena[i].IsSome()) {
                            childArenaIdx = i;
                            goto GotIndex;
                        }
                    throw new InvalidOperationException("Failed to acquire None, arena is invalid!");
                } else
                    childArenaIdx = this.partArena.Length;

                GotIndex:

                var child = new BodyPart(childName, childArchetype, new Option<int>(), parent.thisIdx, ImmutableArray<int>.Empty);

                var childList = childIdx.IsSome() 
                    ? parent.children.Insert(childIdx.Unwrap(), childArenaIdx) 
                    : parent.children.Add(childArenaIdx);
                var newParent = parent.WithThisIdx(new Option<int>()).WithChildren(childList);

                var arenaBuilder = this.partArena.ToBuilder();
                if (childArenaIdx == this.partArena.Length)
                    arenaBuilder.Add(new Option<BodyPart>(child));
                else
                    arenaBuilder[childArenaIdx] = new Option<BodyPart>(child);
                
                // Update parent
                arenaBuilder[parent.thisIdx.Unwrap()] = new Option<BodyPart>(newParent);
                var arena = arenaBuilder.ToImmutable();

                return new Tree(arena, this.partCount + 1);
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
                    throw new System.ArgumentException("Passed in BodyPart lacks a .thisIdx!");

                var builder = this.partArena.ToBuilder();

                if (part.parent.IsSome()) {
                    // Remove part from its parent
                    var parent = builder[part.parent.Unwrap()].Unwrap();
                    var newChildren = parent.children.Remove(part.thisIdx.Unwrap());
                    var newParent = parent.WithChildren(newChildren);
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
                return new Tree(arena, this.partCount - removed);
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
                    var current = this.partArena[currIdx].Unwrap().WithThisIdx(new Option<int>(currIdx));

                    fn(current, depth);

                    for (int i = current.NumChildren - 1; i >= 0; i--)
                        stack.Push((current.children[i], depth + 1));
                }
            }

            /// <summary>
            /// Iterate over the children of a part
            /// </summary>
            /// <param name="part">the part to iterate the children of</param>
            /// <returns>An IEnumerable which can be used in a foreach loop</returns>
            public IEnumerable<BodyPart> ChildrenOf(BodyPart part) {
                for (int i = 0; i < part.NumChildren; i++)
                    yield return this.partArena[part.children[i]].Unwrap();
            }

            /// <summary>
            /// Like ChildrenOf, but with the order reversed.
            /// </summary>
            /// <param name="part">The part to iterate the children of in reverse</param>
            /// <returns>An IEnumerable which can be used in a foreach loop</returns>
            public IEnumerable<BodyPart> ReverseChildrenOf(BodyPart part) {
                for (int i = part.NumChildren; i >= 0; i--)
                    yield return this.partArena[part.children[i]].Unwrap();
            }
        }
    }
}