namespace PRM {
    /// <summary>
    /// Represents an archetypal limb type, such as "Hand", "Head", or "Face".
    /// Does not include specialized limbs, such as "Hardpoint".
    /// </summary>
    public struct LimbArchetype {
        public readonly string name;
        public readonly bool isAppendage;

        public LimbArchetype(string name, bool isAppendage) {
            this.name = name;
            this.isAppendage = isAppendage;
        }
    }
}