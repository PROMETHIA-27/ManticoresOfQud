using System;
using System.Collections.Immutable;
using System.Linq;
using XRL.World;

namespace PRM {
    /// <summary>
    /// Represents an archetypal limb type, such as "Hand", "Head", or "Face".
    /// Does not include specialized limbs, such as "Hardpoint" or "Tentacle".
    /// </summary>
    public struct LimbArchetype : IEquatable<LimbArchetype> {
        /// <summary>
        /// The name of the archetype, such as "Head" or "Hand"
        /// </summary>
        public readonly string name;

        /// <summary>
        /// True if the part is an appendage (such as Hand, Head, Arm, or Feet)
        /// False if the part is not an appendage (such as Body, Back, or Missile Weapon)
        /// </summary>
        public readonly bool isAppendage;

        public LimbArchetype(string name, bool isAppendage) {
            this.name = name;
            this.isAppendage = isAppendage;
        }

        public override string ToString() {
            return $"LimbArchetype {{ name: \"{this.name}\", isAppendage: {this.isAppendage} }}";
        }

        public bool Equals(LimbArchetype other)
        {
            return this.name == other.name &&
                   this.isAppendage == other.isAppendage;
        }

        /// <summary>
        /// A list of all limb archetypes with the Appendage tag. This excludes body and back,
        /// as well as some abstract parts.
        /// </summary>
        public readonly static ImmutableArray<LimbArchetype> Appendages = 
            (from part in Anatomies.BodyPartTypeList
            where part.Appendage ?? false // where part has `Appendage="True"`
            where part.Type == part.FinalType // where part is not a variant
            select new LimbArchetype(part.Type, true))
            .ToImmutableArray();
    }
}