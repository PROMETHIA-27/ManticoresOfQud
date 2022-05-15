using PRM;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Text;
using XRL.UI;
using XRL.UI.Framework;
using XRL.CharacterBuilds.UI;

namespace XRL.CharacterBuilds.Qud.UI
{
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
        public override void HandleMenuOption(MenuOption menuOption) {
            switch (menuOption.Id) {
                case "AddLimb":
                    var data = this.module.data;
                    var tree = data.anatomyTree;
                    data.anatomyTree = 
                        tree.WithChildPart(tree.Root, new Option<int>(), "NewPartLol", new LimbArchetype("Hand", true));
                    this.module.builder.RefreshActiveWindow();
                    break;
                case "RemoveLimb":
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
				Title = "Limbs",
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

            foreach (var option in category.menuOptions)
                data.prefixOptionPool.Return(option);
            category.menuOptions.Clear();

            data.anatomyTree.MapPartsPreorder((part, depth) => {
                var prefixOption = data.prefixOptionPool.Take();
                prefixOption.Description = $"[{part.archetype.name}] {part.name}";
                prefixOption.Prefix = 
                    new StringBuilder()
                    .Append(' ', depth * 2)
                    .ToString();
                prefixOption.LongDescription = $"A piece of you.";

                category.menuOptions.Add(prefixOption);
            });

            this.scroller.BeforeShow(this.descriptor, data.menuData);

            base.BeforeShow(descriptor);
        }
    }
}