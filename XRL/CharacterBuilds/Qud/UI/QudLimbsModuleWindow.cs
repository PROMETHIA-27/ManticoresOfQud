using XRL.UI;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Capabilities;
using Qud.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XRL.UI.Framework;
using ConsoleLib.Console;
using XRL.CharacterBuilds.UI;
using UnityEngine;
using UnityEngine.Events;

namespace XRL.CharacterBuilds.Qud.UI
{
    [UIView("CharacterCreation:PickLimbs", false, false, false, null, null, false, 0, NavCategory = "Menu", UICanvas = "Chargen/PickLimbs", UICanvasHost = 1)]
    public class QudLimbsModuleWindow : EmbarkBuilderModuleWindowBase<QudLimbsModule>
    {
        public CategoryMenusScroller scroller => base.GetComponentInChildren<CategoryMenusScroller>();

        public QudLimbsModuleWindow()
        {
            var acceptButton = new QudMenuItem();
            acceptButton.command = "Accept";
            acceptButton.text = "Accept";
            var deleteButton = new QudMenuItem();
            deleteButton.command = "Delete";
            deleteButton.text = "Delete";
            var cancelButton = new QudMenuItem();
            cancelButton.command = "Cancel";
            cancelButton.text = "Cancel";
            this.AcceptDeleteCancelButton.Add(acceptButton);
            this.AcceptDeleteCancelButton.Add(deleteButton);
            this.AcceptDeleteCancelButton.Add(cancelButton);

            this.limbTiles.Add("Hand", "UI/hand3.png");
            this.limbTiles.Add("Hands", "UI/hands.png");
            this.limbTiles.Add("Body", "UI/body.png");
            this.limbTiles.Add("Arm", "UI/arm.png");
            this.limbTiles.Add("Back", "UI/back.png");
            this.limbTiles.Add("Face", "UI/face.png");
            this.limbTiles.Add("Feet", "UI/feet.png");
            this.limbTiles.Add("Head", "UI/head.png");
            this.limbTiles.Add("Missile Weapon", "UI/missile.png");
            this.limbTiles.Add("Thrown Weapon", "UI/thrown.png");
            this.limbTiles.Add("Floating Nearby", "UI/float.png");

            this.showPoints = new MenuOption()
            {
                Id = "ShowPoints",
                InputCommand = "",
                KeyDescription = null,
                Description = ""
            };
            this.pickLimb = new MenuOption()
            {
                Id = "PickLimb",
                InputCommand = "CmdChargenMutationVariant",
                KeyDescription = ControlManager.getCommandInputDescription("CmdChargenMutationVariant"),
                Description = "Choose Limb Type"
            };
            this.removeLimb = new MenuOption()
            {
                Id = "RemoveLimb",
                InputCommand = "",
                KeyDescription = null,
                Description = "Remove Limb"
            };
            this.renameLimb = new MenuOption()
            {
                Id = "RenameLimb",
                InputCommand = "",
                KeyDescription = null,
                Description = "Rename Limb"
            };
            this.moveLimbUp = new MenuOption()
            {
                Id = "MoveLimbUp",
                InputCommand = "",
                KeyDescription = null,
                Description = "Move Limb Up"
            };
            this.moveLimbDown = new MenuOption()
            {
                Id = "MoveLimbDown",
                InputCommand = "",
                KeyDescription = null,
                Description = "Move Limb Down"
            };
            this.chooseLaterality = new MenuOption()
            {
                Id = "ChooseLaterality",
                InputCommand = "",
                KeyDescription = null,
                Description = "Choose Laterality"
            };
            this.chooseSupport = new MenuOption()
            {
                Id = "ChooseSupport",
                InputCommand = "",
                KeyDescription = null,
                Description = "Choose Support"
            };
            this.chooseDependency = new MenuOption()
            {
                Id = "ChooseDependency",
                InputCommand = "",
                KeyDescription = null,
                Description = "Choose Dependency"
            };
        }

        public override IEnumerable<MenuOption> GetKeyMenuBar()
        {
            showPoints.Description = this.module.data.currentLimbPoints >= 0 ? $"{{{{y|Points Remaining: {this.module.data.currentLimbPoints}}}}}" : $"{{{{R|Points Remaining: {this.module.data.currentLimbPoints}}}}}";
            yield return this.showPoints;
            if (this.lastHighlightedButton != null)
            {
                yield return this.pickLimb;
                yield return this.removeLimb;
                yield return this.renameLimb;
                yield return this.moveLimbUp;
                yield return this.moveLimbDown;
                yield return this.chooseLaterality;
                yield return this.chooseSupport;
                yield return this.chooseDependency;
            }
            foreach (var option in base.GetKeyMenuBar())
                yield return option;
            yield break;
        }
        
        public override void HandleMenuOption(MenuOption menuOption)
        {
            if (menuOption.Id == "PickLimb")
                this.OptionSelectLimb(this.lastHighlightedButton);
            if (menuOption.Id == "RemoveLimb")
                this.OptionRemoveLimb(this.lastHighlightedButton);
            if (menuOption.Id == "RenameLimb")
                this.OptionRenameLimb(this.lastHighlightedButton);
            if (menuOption.Id == "MoveLimbUp")
                this.OptionMoveLimbUp(this.lastHighlightedButton);
            if (menuOption.Id == "MoveLimbDown")
                this.OptionMoveLimbDown(this.lastHighlightedButton);
            if (menuOption.Id == "ChooseLaterality")
                this.OptionChooseLaterality(this.lastHighlightedButton);
            if (menuOption.Id == "ChooseSupport")
                this.OptionChooseSupport(this.lastHighlightedButton);
            if (menuOption.Id == "ChooseDependency")
                this.OptionChooseDependency(this.lastHighlightedButton);
        }

        public override NavigationContext GetNavigationContext()
		{
			return this.scroller.scrollContext;
		}

        public override UIBreadcrumb GetBreadcrumb()
		{
			return new UIBreadcrumb
			{
				Id = base.GetType().FullName,
				Title = "Limbs",
				IconPath = "UI/sw_limbs.png",
				IconDetailColor = ConsoleLib.Console.ColorUtility.ColorMap['G'],
				IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['y']
			};
		}

        string loadedBodyType = null;
        public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
        {
            if (descriptor != null)
				this.windowDescriptor = descriptor;
			
			if (base.module.data == null)
				base.module.setData(new QudLimbsModuleData());

            var anatomyType = this.module.builder.GetModule<QudSubtypeModule>().data.info.BodyObject ??
                this.module.builder.GetModule<QudGenotypeModule>().data.info.BodyObject ??
                "Humanoid";
            var anatomyBody = XRL.World.GameObject.create(anatomyType);
            if (loadedBodyType == null || loadedBodyType != anatomyType)
            {
                loadedBodyType = anatomyType;
                base.module.setData(new QudLimbsModuleData());
                if (anatomyBody.GetPart<Body>() == null)
                    UnityEngine.Debug.LogError("Body part is null! Body parts: ");
                var anatomy = Anatomies.GetAnatomyOrFail(anatomyBody.GetPart<Body>().Anatomy);
                this.module.data.root = new SimpleBodyPart(anatomy);
                var partCount = this.module.data.root.CountParts(false);
                this.module.data.currentLimbPoints = QudLimbsModule.BaseLimbPoints - partCount;
                foreach (var part in this.module.data.root.LoopPartsRecursive(true))
                    if (!string.IsNullOrEmpty(part.SupportsDependent))
                        foreach (var dependent in this.module.data.root.LoopPartsRecursive(true).Where(p => p.DependsOn == part.SupportsDependent))
                            if (!string.IsNullOrEmpty(dependent.ImpliedBy))
                                this.module.data.implyingParts.Add(part);
                this.UpdatePartsWithRequirements();
            }

            this.UpdateMenuOptions();
            base.BeforeShow(descriptor);
        }

        public Dictionary<string, string> limbTiles = new Dictionary<string, string>();
        public void UpdateMenuOptions()
        {
            SimpleBodyPart currHighlightedLimb = null;
            if (this.lastHighlightedButton != null && this.partButtonsToParts.TryGetValue(this.lastHighlightedButton, out var buttonPart))
                currHighlightedLimb = buttonPart;
            this.categoryMenus.Clear();
            CategoryMenuData category = new CategoryMenuData();
            this.categoryMenus.Add(category);
            category.Title = "Limbs";
            category.menuOptions = new List<PrefixMenuOption>();
            partButtonsToParts.Clear();
            foreach (var part in this.module.data.root.LoopPartsRecursive(true))
            {
                StringBuilder prefix = new StringBuilder()
                    .Append('─', this.module.data.root.GetPartDepth(part, 0))
                    .Append((!string.IsNullOrEmpty(part.SupportsDependent) ? $"[S: {part.SupportsDependent}]" : ""))
                    .Append((!string.IsNullOrEmpty(part.DependsOn) ? $"[D: {part.DependsOn}]" : ""))
                    .Append((!string.IsNullOrEmpty(part.RequiresType) ? $"[R: {(part.RequiresLaterality != Laterality.ANY ? Laterality.LateralityAdjective(part.RequiresLaterality) + " " : "")}{part.RequiresType}]" : ""))
                    .Append((!string.IsNullOrEmpty(part.ImpliedBy) ? $"[I: {part.ImpliedBy}]" : ""));
                StringBuilder description = new StringBuilder()
                    .Append(string.IsNullOrEmpty(part.DescriptionPrefix) ? "" : part.DescriptionPrefix + ' ')
                    .Append(string.IsNullOrEmpty(part.LateralityMod) ? "" : part.LateralityMod + ' ')
                    .Append(string.IsNullOrEmpty(part.DescMod) ? "" : part.DescMod + '-')
                    .Append(part.Description);
                PrefixMenuOption menuOp = new PrefixMenuOption()
                {
                    Prefix = prefix.ToString(),
                    Description = description.ToString(),
                    LongDescription = part.Plural ? $"Some {part.Type.ToLower()}" : XRL.Language.Grammar.A(part.Type.ToLower(), true)
                };
                if (this.limbTiles.TryGetValue(part.Type, out var tile))
                {
                    menuOp.Renderable = new Renderable
                    {
                        Tile = tile,
                        ColorString = "&y",
                        DetailColor = 'K'
                    };
                }
                partButtonsToParts.Add(menuOp, part);
                category.menuOptions.Add(menuOp);
                if (part == currHighlightedLimb)
                    this.lastHighlightedButton = menuOp;
            }
            if (this.lastHighlightedButton == null)
                this.lastHighlightedButton = this.categoryMenus[0].menuOptions[0];
            this.scroller.BeforeShow(this.windowDescriptor, this.categoryMenus);
            this.scroller.onHighlight.AddListener(new UnityAction<FrameworkDataElement>(this.HighlightOption));
            this.GetOverlayWindow().UpdateMenuBars();
        }

        HashSet<SimpleBodyPart> blacklist = new HashSet<SimpleBodyPart>();
        public void UpdatePartsWithRequirements()
        {
            var additions = new List<SimpleBodyPart>();
            foreach (var part in this.module.ImpliedLimbs)
            {
                if (this.module.data.root.CountParts(part.Type) == 0)
                {
                    var possibleImpliers = this.module.data.root.LoopPartsRecursive(true)
                        .Where(p => (p.VariantType ?? p.Type) == part.ImpliedBy && string.IsNullOrEmpty(p.SupportsDependent))
                        .ToList();
                    if (possibleImpliers.Count >= part.ImpliedPer)
                    {
                        if (!part.ImpliedPer.HasValue)
                            UnityEngine.Debug.LogError("ERROR!: Manticores: Implied part has no per value");
                        List<SimpleBodyPart> impliers = possibleImpliers.GetRange(0, part.ImpliedPer ?? -1);
                        for (int i = 0; i < impliers.Count; i++)
                        {
                            impliers[i].SupportsDependent = part.Name;
                            this.module.data.implyingParts.Add(impliers[i]);
                        }
                        var newPart = new SimpleBodyPart(part);
                        newPart.DependsOn = part.Name;
                        additions.Add(newPart);
                        this.blacklist.Add(newPart);
                    }
                }
            }
            foreach (var addition in additions)
            {
                this.blacklist.Remove(addition);
                this.AddLimbToPart(this.module.data.root, addition);
            }

            List<SimpleBodyPart> removals = new List<SimpleBodyPart>();
            foreach (var part in this.module.data.root.LoopPartsRecursive(false))
                if (!string.IsNullOrEmpty(part.ImpliedBy) && !this.blacklist.Contains(part))
                    if (this.module.data.root.CountParts(part.ImpliedBy) < part.ImpliedPer)
                    {
                        removals.Add(part);
                        this.blacklist.Add(part);
                        foreach (var otherPart in this.module.data.root.LoopPartsRecursive(false))
                            if (otherPart.SupportsDependent == part.DependsOn)
                            {
                                otherPart.SupportsDependent = null;
                                this.module.data.implyingParts.Remove(otherPart);
                            }
                    }
            foreach (var removal in removals)
            {
                this.blacklist.Remove(removal);
                int refund = removal.CountParts(false);
                this.RemoveLimb(removal, false, true);
                this.module.data.currentLimbPoints += refund;
            }

            removals.Clear();
            foreach (var part in this.module.data.root.LoopPartsRecursive(false))
                if (!string.IsNullOrEmpty(part.RequiresType) && !this.blacklist.Contains(part))
                    if (this.module.data.root.CountParts(part.RequiresType, part.RequiresLaterality) == 0)
                    {
                        this.module.data.queuedPartsWithRequirements.Add(part);
                        this.blacklist.Add(part);
                        removals.Add(part);
                    }
            foreach (var removal in removals)
            {
                this.blacklist.Remove(removal);
                this.RemoveLimb(removal, false, true);
            }

            removals.Clear();
            for (int i = 0; i < this.module.data.queuedPartsWithRequirements.Count; i++)
            {
                var queued = this.module.data.queuedPartsWithRequirements[i];
                if (this.blacklist.Contains(queued))
                    continue;
                if (this.module.data.root.CountParts(queued.RequiresType, queued.RequiresLaterality) > 0)
                {
                    this.blacklist.Add(queued);
                    removals.Add(queued);
                    this.AddLimbToPart(this.module.data.root, queued);
                }
            }
            foreach (var removal in removals)
            {
                this.blacklist.Remove(removal);
                this.module.data.queuedPartsWithRequirements.Remove(removal);
            }

            this.UpdateMenuOptions();
        }

        public async void OptionSelectLimb(FrameworkDataElement elem)
        {
            var selected = elem as PrefixMenuOption;
            var part = this.partButtonsToParts[selected];
            if (part.Abstract)
            {
                await Popup.NewPopupMessageAsync("You cannot add parts to an abstract part!", PopupMessage.CancelButton, null, "{{W|Warning!}}");
                return;
            }
            int idx = await Popup.AsyncShowOptionsList("Choose a limb type to add", this.module.Limbs.Select(l => l.Type).ToArray(), null, 0, null, 60, false, true, 0, "", null, null, null, false, true, -1);
            int cost = AddLimbToPart(part, this.module.Limbs[idx]);
            if (this.module.Limbs[idx].Type == "Head" && this.module.data.root.CountParts("Head") == 0)
            {
                cost -= 1;
                UnityEngine.Debug.LogError($"Found head in first addition! Orig/Subtracted cost: {cost + 1}/{cost}");
            }
            this.module.data.currentLimbPoints -= cost;
            this.UpdateMenuOptions();
        }

        public int AddLimbToPart(SimpleBodyPart part, BodyPartType type)
        {
            var newPart = new SimpleBodyPart(type);
            return this.AddLimbToPart(part, newPart);
        }

        public int AddLimbToPart(SimpleBodyPart part, SimpleBodyPart newPart)
        {
            int totalParts = 1;
            var type = newPart.PartType;
            var typeChildList = Anatomies.FindUsualChildBodyPartTypes(type);
            if (typeChildList != null)
                foreach (var childPart in Anatomies.FindUsualChildBodyPartTypes(type))
                {
                    newPart.Parts.Add(new SimpleBodyPart(childPart));
                    totalParts++;
                }

            part.Parts.Add(newPart);

            if (newPart.Laterality == 0)
            {
                if (!string.IsNullOrEmpty(type.UsuallyOn) && type.UsuallyOn != part.Type)
                {
                    BodyPartType bodyPartType = Anatomies.GetBodyPartType(part.VariantType ?? part.Type);
                    newPart.ModNameDescRecursively(bodyPartType.Name.Replace(" ", "-"), bodyPartType.Description.Replace(" ", "-"), -1);
                }
                if (part.Laterality != 0)
                    newPart.ChangeLaterality(part.Laterality | newPart.Laterality, true);
            }

            this.UpdatePartsWithRequirements();
            return totalParts;
        }

        Dictionary<PrefixMenuOption, SimpleBodyPart> partButtonsToParts = new Dictionary<PrefixMenuOption, SimpleBodyPart>();
        public void HighlightOption(FrameworkDataElement elem)
        {
            var option = elem as PrefixMenuOption;
            if (option == null)
                return;
            this.lastHighlightedButton = option;
            this.GetOverlayWindow().UpdateMenuBars(this.GetOverlayWindow().currentWindowDescriptor);
        }

        public async void OptionRemoveLimb(FrameworkDataElement elem)
        {
            var option = elem as PrefixMenuOption;
            var part = this.partButtonsToParts[option];
            var refund = part.CountParts(true);
            if (part.Type == "Head" && this.module.data.root.CountParts("Head") == 1)
                refund -= 1;

            if (part.PartType.Integral ?? false)
            {
                await Popup.NewPopupMessageAsync("You can't remove an integral body part!", PopupMessage.CancelButton, null, "{{W|Warning!}}");
                return;
            }
            if (!string.IsNullOrEmpty(part.SupportsDependent) && !this.module.data.implyingParts.Contains(part))
            {
                await Popup.NewPopupMessageAsync("You can't remove a supporting part unless it supports an implied part!", PopupMessage.CancelButton, null, "{{W|Warning!}}");
                return;
            }
            if (!string.IsNullOrEmpty(part.RequiresType))
            {
                await Popup.NewPopupMessageAsync("You can't remove a type that has requirements!", PopupMessage.CancelButton, null, "{{W|Warning!}}");
                return;
            }
            if (!string.IsNullOrEmpty(part.ImpliedBy))
            {
                await Popup.NewPopupMessageAsync("You can't remove a part implied by other parts!", PopupMessage.CancelButton, null, "{{W|Warning!}}");
                return;
            }

            var result = await Popup.NewPopupMessageAsync($"Are you sure you want to remove the limb {part.Description} (Type: {part.Type})?", PopupMessage.YesNoButton, null, "{{W|Warning!}}");
            if (result.command == "Yes")
            {
                if (this.module.data.implyingParts.Contains(part))
                {
                    var newImplier = this.module.data.root.LoopPartsRecursive(false).Where(p => p.VariantType == part.VariantType && string.IsNullOrEmpty(p.SupportsDependent)).FirstOrDefault();
                    if (newImplier != null)
                    {
                        newImplier.SupportsDependent = part.SupportsDependent;
                        this.module.data.implyingParts.Add(newImplier);
                    }
                    this.module.data.implyingParts.Remove(part); 
                }
                this.RemoveLimb(part);
                this.module.data.currentLimbPoints += refund;
            }
        }

        public void RemoveLimb(SimpleBodyPart part, bool shouldRefund = true, bool bypassLimits = false)
        {
            var parentPart = this.module.data.root.FindParentOf(part);

            parentPart.Parts.Remove(part);

            this.UpdatePartsWithRequirements();
            this.UpdateMenuOptions();
        }

        public async void OptionRenameLimb(FrameworkDataElement elem)
        {
            var option = elem as PrefixMenuOption;
            var part = this.partButtonsToParts[option];
            var result = await Popup.NewPopupMessageAsync("Rename your limb:", PopupMessage.AcceptCancelButton, null, null, part.Name);
            if (result.command != "Cancel")
            {
                part.Description = result.text;
                part.Name = result.text.ToLower();
            }
            this.UpdateMenuOptions();
        }

        public void OptionMoveLimbUp(FrameworkDataElement elem)
        {
            var option = elem as PrefixMenuOption;
            var part = this.partButtonsToParts[option];
            this.MoveLimb(part, -1);
        }

        public void OptionMoveLimbDown(FrameworkDataElement elem)
        {
            var option = elem as PrefixMenuOption;
            var part = this.partButtonsToParts[option];
            this.MoveLimb(part, 1);
        }

        public void MoveLimb(SimpleBodyPart part, int offset)
        {
            if (offset == 0)
                return;
            var parentPart = this.module.data.root.FindParentOf(part);
            var oldPosition = parentPart.Parts.IndexOf(part);
            var targetPos = Math.Min(Math.Max(oldPosition + offset, 0), parentPart.Parts.Count);
            if (parentPart.Parts[targetPos].Abstract || parentPart.Parts[targetPos].ImpliedBy != null)
                while (targetPos != 0 && targetPos != parentPart.Parts.Count && parentPart.Parts[targetPos].Abstract || parentPart.Parts[targetPos].ImpliedBy != null)
                    targetPos += offset > 0 ? 1 : -1;
            parentPart.Parts.Remove(part);
            parentPart.Parts.Insert(targetPos, part);
            this.UpdateMenuOptions();
        }

        public async void OptionChooseLaterality(FrameworkDataElement elem)
        {
            var option = elem as PrefixMenuOption;
            var part = this.partButtonsToParts[option];

            if (part == this.module.data.root)
            {
                await Popup.NewPopupMessageAsync("You cannot change the laterality of your body!", PopupMessage.CancelButton, null, "{{W|Warning!}}");
                return;
            }
            // if (part.Abstract)
            // {
            //     await Popup.NewPopupMessageAsync("You cannot change the laterality of an abstract part!", PopupMessage.CancelButton, null, "{{W|Warning!}}");
            //     return;
            // }

            int[] lateralities = new int[11];
            for (int i = 0; i < 11; i++)
                lateralities[i] = (int)Math.Pow(2, i);
            
            string[] adjectives = new string[11];
            for (int i = 0; i < 11; i++)
                adjectives[i] = $"[{((part.Laterality & lateralities[i]) == lateralities[i] ? '■' : ' ')}] {Laterality.LateralityAdjective(lateralities[i], true)}";

            int chosenLaterality = await Popup.AsyncShowOptionsList("Choose Laterality", adjectives, null, 0, null, 60, false, true, 0, "", null, null, null, false, true, -1);
            if (chosenLaterality == -1)
                return;
            chosenLaterality = lateralities[chosenLaterality];

            part.Laterality ^= chosenLaterality;
            part.Laterality = VerifyLaterality(chosenLaterality, part.Laterality);

            var parent = this.module.data.root.FindParentOf(part);
            if (!string.IsNullOrEmpty(part.PartType.UsuallyOn) && part.PartType.UsuallyOn != parent.Type)
            {
                BodyPartType bodyPartType = Anatomies.GetBodyPartType(parent.VariantType ?? parent.Type);
                part.ModNameDescRecursively(bodyPartType.Name.Replace(" ", "-"), bodyPartType.Description.Replace(" ", "-"), 1);
            }
            part.ChangeLaterality(VerifyLaterality(part.Laterality, part.Laterality | parent.Laterality), true);

            this.UpdatePartsWithRequirements();
            this.UpdateMenuOptions();
        }

        public static int VerifyLaterality(int fallback, int laterality)
        {
			if ((laterality & 1) != 0 && (laterality & 2) != 0)
			{
				var value = fallback & 3;
                if (value == 1)
                    laterality &= -3;
                if (value == 2)
                    laterality &= -2;
			}
			if ((laterality & 4) != 0 && (laterality & 8) != 0)
			{
				var value = fallback & 12;
                if (value == 4)
                    laterality &= -9;
                if (value == 8)
                    laterality &= -5;
			}
			if ((laterality & 16) != 0 && (laterality & 64) != 0)
			{
				var value = fallback & 80;
                if (value == 16)
                    laterality &= -65;
                if (value == 64)
                    laterality &= -17;
			}
			if ((laterality & 128) != 0 && (laterality & 256) != 0)
			{
				var value = fallback & 384;
                if (value == 128)
                    laterality &= -257;
                if (value == 256)
                    laterality &= -129;
			}
            if ((laterality & 512) != 0 && (laterality & 1024) != 0)
			{
				var value = fallback & 1536;
                if (value == 512)
                    laterality &= -1025;
                if (value == 1024)
                    laterality &= -513;
			}
            return laterality;
        }

        public List<QudMenuItem> AcceptDeleteCancelButton = new List<QudMenuItem>();
        public async void OptionChooseSupport(FrameworkDataElement elem)
        {
            var option = elem as PrefixMenuOption;
            var part = this.partButtonsToParts[option];
            if (this.module.data.implyingParts.Contains(part))
            {
                await Popup.NewPopupMessageAsync("You cannot alter the support of an implying part!", PopupMessage.CancelButton, null, "{{W|Warning!}}");
                return;
            }
            var result = await Popup.NewPopupMessageAsync("Choose support name:", AcceptDeleteCancelButton, null, null, part.SupportsDependent ?? "");
            if (result.command == "Accept")
                part.SupportsDependent = result.text;
            else if (result.command == "Delete")
            {
                if (this.module.data.root.CountPartsWithDependency(part.SupportsDependent) != 0)
                {
                    await Popup.NewPopupMessageAsync("You must remove dependencies before removing supports!", PopupMessage.CancelButton, null, "{{W|Warning!}}");
                    return;
                }
                part.SupportsDependent = null;
            }
            this.UpdatePartsWithRequirements();
            this.UpdateMenuOptions();
        }

        public async void OptionChooseDependency(FrameworkDataElement elem)
        {
            var option = elem as PrefixMenuOption;
            var part = this.partButtonsToParts[option];
            if (!string.IsNullOrEmpty(part.ImpliedBy))
            {
                await Popup.NewPopupMessageAsync("You cannot alter the dependency of an implied part!", PopupMessage.CancelButton, null, "{{W|Warning!}}");
                return;
            }
            var result = await Popup.NewPopupMessageAsync("Choose dependency name:", AcceptDeleteCancelButton, null, null, part.DependsOn ?? "");
            if (result.command == "Accept")
            {
                if (this.module.data.root.CountParts(result.text) == 0)
                    {
                        await Popup.NewPopupMessageAsync("You must add supports before adding dependencies!", PopupMessage.CancelButton, null, "{{W|Warning!}}");
                        return;
                    }
                part.DependsOn = result.text;
            }
            else if (result.command == "Delete")
                part.DependsOn = null;

            this.UpdatePartsWithRequirements();
            this.UpdateMenuOptions();
        }

        public EmbarkBuilderModuleWindowDescriptor windowDescriptor;
        private List<CategoryMenuData> categoryMenus = new List<CategoryMenuData>();
        private MenuOption showPoints;
        private MenuOption pickLimb;
        private MenuOption removeLimb;
        private MenuOption renameLimb;
        private MenuOption moveLimbUp;
        private MenuOption moveLimbDown;
        private MenuOption chooseLaterality;
        private MenuOption chooseSupport;
        private MenuOption chooseDependency;
        private PrefixMenuOption lastHighlightedButton;
        private bool highlightedIsLimbButton;
    }
}