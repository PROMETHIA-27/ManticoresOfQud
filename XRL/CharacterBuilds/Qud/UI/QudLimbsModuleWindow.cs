using XRL.UI;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Capabilities;
using Qud.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.UI.Framework;
using ConsoleLib.Console;
using XRL.CharacterBuilds.UI;
using UnityEngine;
using UnityEngine.Events;

namespace XRL.CharacterBuilds.Qud.UI
{
    [UIView("CharacterCreation:PickLimbs", false, false, false, null, null, false, 0, NavCategory = "Menu", UICanvas = "Chargen/PickLimbs", UICanvasHost = 1)]
    /// The UI for the limbs module. Describes UI behavior.
    public class QudLimbsModuleWindow : EmbarkBuilderModuleWindowBase<QudLimbsModule>
    {
        /// Important UI element
        public CategoryMenusScroller scroller => base.GetComponentInChildren<CategoryMenusScroller>();

        public QudLimbsModuleWindow()
        {
            
        }

        /// Provide menu bar at the bottom of the screen, with key bindings
        // public override IEnumerable<MenuOption> GetKeyMenuBar()
        // {
            
        // }
        
        /// Respond to chosen menu options
        public override void HandleMenuOption(MenuOption menuOption)
        {
            
        }

        /// Provide navigation context (description of a smaller part of a larger screen)
        public override NavigationContext GetNavigationContext()
		{
			return this.scroller.scrollContext;
		}

        /// Provide breadcrumb (The UI Icon at the top)
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

        /// Called to update the window
        public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
        {
            var categories = new List<CategoryMenuData>();

            categories[0] = new CategoryMenuData() {
                Title = "Limbs",
                Description = "A bunch of limbs.",
                menuOptions = new List<PrefixMenuOption>(),
            };
            
            foreach (var archetype in this.module.data.AppendageArchetypes) {
                var option = new PrefixMenuOption() {
                    Description = archetype.name,
                    LongDescription = archetype.isAppendage ? "This is an appendage!" : "This is not an appendage.",
                };
                categories[0].menuOptions.Add(option);
            }

            this.scroller.BeforeShow(this.descriptor, categories);

            base.BeforeShow(descriptor);
        }
    }
}