using PRM;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.UI;
using XRL.UI.Framework;
using XRL.CharacterBuilds.UI;

namespace XRL.CharacterBuilds.Qud.UI {
    [UIView("CharacterCreation:PickLimbs", false, false, false, null, null, false, 0, NavCategory = "Menu", UICanvas = "Chargen/PickLimbs", UICanvasHost = 1)]
    /// <summary>
    /// The UI for the limbs module. Describes UI behavior.
    /// </summary>
    public class QudLimbsModuleWindow : EmbarkBuilderModuleWindowBase<QudLimbsModule>
    {
        /// <summary>
        /// A list of MenuOptions to be used in GetKeyMenuBar and HandleMenuOption
        /// </summary>
        static readonly ImmutableArray<MenuOption> menuBar = ImmutableArray.Create(
            new MenuOption() {
                Id = "AddLimb",
                Description = "Add Appendage"
            },
            new MenuOption() { 
                Id = "RemoveLimb",
                Description = "Remove Appendage"
            }
        );
        
        static readonly string[] limbChoices = LimbArchetype.Appendages.Select(arch => arch.name).ToArray();

        /// <summary>
        /// Important UI element, which contains the categories and elements of them
        /// </summary>
        public CategoryMenusScroller scroller => base.GetComponentInChildren<CategoryMenusScroller>();

        /// <summary>
        /// Provide menu bar at the bottom of the screen, with key bindings
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<MenuOption> GetKeyMenuBar() {
            return menuBar;
        }
        
        /// <summary>
        /// Respond to chosen menu options
        /// </summary>
        /// <param name="menuOption">Menu option to handle</param>
        public override async void HandleMenuOption(MenuOption menuOption) {
            switch (menuOption.Id) {
                case "AddLimb":
                    if (!this.module.data.selectedPart.IsSome()) {
                        Popup.Show("You must select a part to add one to it.");
                        break;
                    }
                    var choice = await Popup.AsyncShowOptionsList("Pick a limb type to add", limbChoices);
                    var archetype = LimbArchetype.Appendages[choice];
                    var name = await Popup.AskStringAsync("Pick a name for the limb", archetype.name, 100, 1);
                    this.module.AddLimb(this.module.data.selectedPart.Unwrap(), archetype, name);
                    this.module.data.selectedPart = new Option<BodyPart>();
                    this.module.builder.RefreshActiveWindow();
                    break;
                case "RemoveLimb":
                    if (!this.module.data.selectedPart.IsSome()) {
                        Popup.Show("You must select a part to remove it.");
                        break;
                    }
                    var part = this.module.data.selectedPart.Unwrap();
                    if (part.Equals(this.module.data.anatomyTree.Root)) {
                        Popup.Show("You cannot remove the body!");
                        break;
                    }
                    if (await Popup.ShowYesNoAsync($"&rAre you sure you want to remove {part.name}?") != DialogResult.Yes)
                        break;
                    this.module.RemoveLimb(this.module.data.selectedPart.Unwrap());
                    this.module.data.selectedPart = new Option<BodyPart>();
                    this.module.builder.RefreshActiveWindow();
                    break;
            }
        }

        /// <summary>
        /// Provide navigation context (description of a smaller part of a larger screen)
        /// </summary>
        /// <returns>The current nav context of this window</returns>
        public override NavigationContext GetNavigationContext() {
			return this.scroller.scrollContext;
		}

        /// <summary>
        /// Provide breadcrumb (The UI Icon at the top)
        /// </summary>
        /// <returns>A UIBreadcrumb which will be displayed at the top of the screen</returns>
        public override UIBreadcrumb GetBreadcrumb() {
			return new UIBreadcrumb {
				Id = base.GetType().FullName,
				Title = "Anatomy",
				IconPath = "UI/sw_limbs.png",
				IconDetailColor = ConsoleLib.Console.ColorUtility.ColorMap['G'],
				IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['y']
			};
		}

        /// <summary>
        /// Called to update the window
        /// </summary>
        /// <param name="descriptor">?</param>
        public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor) {
            var data = this.module.data;
            var category = this.module.data.menuData[0];

            // Update menu options
            foreach (var option in category.menuOptions)
                data.prefixOptionPool.Return(option);
            category.menuOptions.Clear();
            data.optionPartMap.Clear();

            data.anatomyTree.MapPartsPreorder((part, depth) => {
                var prefixOption = data.prefixOptionPool.Take();
                prefixOption.Description = 
                    new StringBuilder()
                    // Color the text yellow if this is the selected part
                    .Append(
                        data.selectedPart.IsSome() && part.Equals(data.selectedPart.Unwrap())
                        ? "&W"
                        : string.Empty
                    )
                    // Indent the part based on depth
                    .Append(' ', depth * 2)
                    // List archetype and name
                    .Append($"[{part.archetype.name}] {part.name}")
                    .ToString();                    
                prefixOption.LongDescription = $"A piece of you.";

                data.optionPartMap[prefixOption] = part;

                category.menuOptions.Add(prefixOption);
            });

            // Ensure selection is being tracked
            this.scroller.onSelected.RemoveAllListeners();
            this.scroller.onSelected.AddListener(
                element => {
                    data.selectedPart = new Option<BodyPart>(data.optionPartMap[element as PrefixMenuOption]);
                    this.module.builder.RefreshActiveWindow();
                }
            );

            this.scroller.BeforeShow(this.descriptor, data.menuData);

            base.BeforeShow(descriptor);
        }
    }
}