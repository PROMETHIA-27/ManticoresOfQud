using System.Globalization;
using System.Security;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using XRL.World.Parts;
using XRL.World;
using XRL.World.Capabilities;
using Newtonsoft.Json;

namespace XRL.CharacterBuilds 
{
    public class QudLimbsModuleData : AbstractEmbarkBuilderModuleData
    {
        public SimpleBodyPart root = new SimpleBodyPart(Anatomies.GetBodyPartTypeOrFail("Body"));

        public List<SimpleBodyPart> queuedPartsWithRequirements = new List<SimpleBodyPart>();

        public HashSet<SimpleBodyPart> implyingParts = new HashSet<SimpleBodyPart>();

        public int currentLimbPoints = XRL.CharacterBuilds.Qud.QudLimbsModule.BaseLimbPoints;
        public int ToughnessPenalty => System.Math.Abs(System.Math.Min(0, currentLimbPoints));
    }

    [Serializable]
    public class SimpleBodyPart : IEquatable<SimpleBodyPart>
    {
        public SimpleBodyPart(BodyPartType partType)
        {
            var part = new BodyPart(partType, null);
            this.Type = part.Type;
            this.VariantType = part.VariantType;
            this.Description = part.Description;
            this.DescriptionPrefix = part.DescriptionPrefix;
            this.Name = part.Name;
            this.Abstract = part.Abstract;
            this.Plural = part.Plural;
            this.Parts = new List<SimpleBodyPart>();
        }

        public SimpleBodyPart(Anatomy anatomy)
        {
            var tempPart = new BodyPart(anatomy.BodyType, null);
            this.Type = anatomy.BodyType;
            this.VariantType = tempPart.VariantType;
            this.Description = tempPart.Description;
            this.DescriptionPrefix = tempPart.DescriptionPrefix;
            this.Name = tempPart.Name;
            this.Abstract = tempPart.Abstract;
            this.Plural = tempPart.Plural;
            this.RequiresLaterality = XRL.World.Capabilities.Laterality.ANY;
            this.Parts = new List<SimpleBodyPart>();
            if (anatomy.Parts != null)
                for (int i = 0; i < anatomy.Parts.Count; i++)
                    this.Parts.Add(new SimpleBodyPart(anatomy.Parts[i]));
        }

        public SimpleBodyPart(AnatomyPart part)
        {
            var tempPart = new BodyPart(part.Type, null);
            this.Type = tempPart.Type;
            this.VariantType = tempPart.VariantType;
            this.DefaultBehavior = part.DefaultBehavior;
            this.Description = tempPart.Description;
            this.DescriptionPrefix = tempPart.DescriptionPrefix;
            this.Name = tempPart.Name;
            this.Abstract = tempPart.Abstract;
            this.Plural = tempPart.Plural;
            this.SupportsDependent = part.SupportsDependent;
            this.DependsOn = part.DependsOn;
            this.RequiresType = part.RequiresType;
            if (string.IsNullOrEmpty(this.RequiresType) && !string.IsNullOrEmpty(this.PartType.RequiresType))
                this.RequiresType = this.PartType.RequiresType;
            this.Parts = new List<SimpleBodyPart>();
            for (int i = 0; i < part.Subparts.Count; i++)
                this.Parts.Add(new SimpleBodyPart(part.Subparts[i]));
            if (part.Laterality.HasValue)
                this.ChangeLaterality(part.Laterality.Value);
            if (!string.IsNullOrEmpty(part.RequiresType))
                this.RequiresLaterality = part.RequiresLaterality ?? (this.Laterality == 0 ? XRL.World.Capabilities.Laterality.ANY : this.Laterality);
        }

        public AnatomyPart ToAnatomyPart()
        {
            var partType = Anatomies.GetBodyPartType(this.VariantType ?? this.Type);
            var anatomyPart = new AnatomyPart(partType);
            anatomyPart.Laterality = this.Laterality;
            anatomyPart.RequiresLaterality = this.RequiresLaterality;
            anatomyPart.SupportsDependent = this.SupportsDependent;
            anatomyPart.DependsOn = this.DependsOn;
            anatomyPart.RequiresType = this.RequiresType;
            anatomyPart.RequiresType = partType.RequiresType;
            anatomyPart.DefaultBehavior = this.DefaultBehavior ?? partType.DefaultBehavior;
            anatomyPart.Category = partType.Category;
            anatomyPart.RequiresLaterality = partType.RequiresLaterality;
            anatomyPart.Mobility = partType.Mobility;
            anatomyPart.Integral = partType.Integral;
            anatomyPart.Mortal = partType.Mortal;
            anatomyPart.Abstract = partType.Abstract;
            anatomyPart.Extrinsic = partType.Extrinsic;
            anatomyPart.Plural = partType.Plural;
            anatomyPart.Mass = partType.Mass;
            anatomyPart.Contact = partType.Contact;
            anatomyPart.IgnorePosition = partType.IgnorePosition;
            for (int i = 0; i < this.Parts.Count; i++)
                anatomyPart.Subparts.Add(this.Parts[i].ToAnatomyPart());
            return anatomyPart;
        }

        public bool Equals(SimpleBodyPart other)
        {
            return this.Type == other.Type &&
                this.VariantType == other.VariantType &&
                this.Description == other.Description &&
                this.DescriptionPrefix == other.DescriptionPrefix &&
                this.Name == other.Name &&
                this.Parts == other.Parts;
        }

        public SimpleBodyPart FindParentOf(SimpleBodyPart part)
        {
            for (int i = 0; i < this.Parts.Count; i++)
            {
                if (this.Parts[i] == part)
                    return this;
                else
                {
                    var foundPart = this.Parts[i].FindParentOf(part);
                    if (foundPart != null)
                        return foundPart;
                }
            }
            return null;
        }

        public int CountParts(bool includeSelf)
        {
            int count = 0;
            foreach (var part in this.LoopPartsRecursive(includeSelf))
                count++;
            return count;
        }

        public int CountParts(string type, int laterality = XRL.World.Capabilities.Laterality.ANY)
        {
            int count = 0;
            foreach (var part in this.LoopPartsRecursive(false))
                if ((part.Type == type || part.VariantType == type) && (laterality == XRL.World.Capabilities.Laterality.ANY || XRL.World.Capabilities.Laterality.Match(part.Laterality, laterality)))
                    count++;
            return count;
        }

        public int CountPartsWithRequirement(string requiredType)
        {
            int count = 0;
            foreach (var part in this.LoopPartsRecursive(false))
                if (part.RequiresType == requiredType)
                    count++;
            return count;
        }

        public int CountPartsWithDependency(string dependency)
        {
            int count = 0;
            foreach (var part in this.LoopPartsRecursive(false))
                if (part.DependsOn == dependency)
                    count++;
            return count;
        }

        public IEnumerable<SimpleBodyPart> LoopPartsBottomUp(bool includeSelf)
        {
            if (includeSelf)
                yield return this;
            for (int i = 0; i < this.Parts.Count; i++)
            {
                foreach (var part in this.Parts[i].LoopPartsBottomUp(false))
                    yield return part;
                yield return this.Parts[i];
            }
        }

        public IEnumerable<SimpleBodyPart> LoopPartsTopDown(bool includeSelf)
        {
            if (includeSelf)
                yield return this;
            for (int i = 0; i < this.Parts.Count; i++)
                yield return this.Parts[i];
            for (int i = 0; i < this.Parts.Count; i++)
                foreach (var part in this.Parts[i].LoopPartsTopDown(false))
                    yield return part;
        }

        public IEnumerable<SimpleBodyPart> LoopPartsRecursive(bool includeSelf)
        {
            if (includeSelf)
                yield return this;
            for (int i = 0; i < this.Parts.Count; i++)
            {
                yield return this.Parts[i];
                foreach (var part in this.Parts[i].LoopPartsRecursive(false))
                    yield return part;
            }
        }

        public void ModNameDescRecursively(string nameMod, string descMod, int maxDepth)
        {
            if (maxDepth == 0)
                return;
            this.NameMod = nameMod;
            this.DescMod = descMod;
            for (int i = 0; i < this.Parts.Count; i++)
                this.Parts[i].ModNameDescRecursively(nameMod, descMod, Math.Max(-1, maxDepth - 1));
        }

        public int GetPartDepth(SimpleBodyPart part, int depth)
        {
            if (this == part)
                return depth;
            for (int i = 0; i < this.Parts.Count; i++)
            {
                var foundDepth = this.Parts[i].GetPartDepth(part, depth + 1);
                if (foundDepth != -1)
                    return foundDepth;
            }
            return -1;
        }

        public SimpleBodyPart ChangeLaterality(int newLaterality, bool recursive = false)
		{
			BodyPartType bodyPartType = Anatomies.GetBodyPartType(this.VariantType ?? this.Type);
			
            this.Laterality = XRL.CharacterBuilds.Qud.UI.QudLimbsModuleWindow.VerifyLaterality(this.Laterality, this.Laterality | newLaterality);
            this.LateralityMod = XRL.World.Capabilities.Laterality.LateralityAdjective(this.Laterality, true);
            this.RequiresLaterality = this.Laterality;
            
			if (recursive && this.Parts != null)
                for (int i = 0; i < this.Parts.Count; i++)
					this.Parts[i].ChangeLaterality(XRL.CharacterBuilds.Qud.UI.QudLimbsModuleWindow.VerifyLaterality(this.Parts[i].Laterality, this.Parts[i].Laterality | newLaterality), true);
			return this;
		}

        public BodyPartType PartType => Anatomies.GetBodyPartType(this.VariantType ?? this.Type);

        public string ImpliedBy => this.PartType.ImpliedBy;

        public int? ImpliedPer => this.PartType.ImpliedPer;

        public string Type;

		public string VariantType;

		public string Description;

        public string DescMod = "";

		public string DescriptionPrefix;

		public string Name;

        public string NameMod = "";

        public string LateralityMod = "";

        public bool Abstract;

        public bool Plural;

        public int Laterality;

        public int RequiresLaterality;

        public string RequiresType;

        public string SupportsDependent;

        public string DependsOn;

        public string DefaultBehavior;
		
		public List<SimpleBodyPart> Parts;
    }
}