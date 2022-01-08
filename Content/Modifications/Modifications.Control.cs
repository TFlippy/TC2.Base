using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterControlModifications(ref List<Modification.Definition> definitions)
		{
			definitions.Add(Modification.Definition.New<Control.Data>
			(
				identifier: "control.random_activation",
				category: "Control",
				name: "Random Activation",
				description: "Item randomly activates on its own.",

				// This randomly causes a left click and a space bar (at the same time)
				// Working examples: guns, fuse explosives, drills, mounts (yes they will use whatever is on them), melee weapons, even medkits
				// Due to the wide variety of uses this has, this costs a large amount of materials
				// This doesn't aim, so using anything which uses aim direction requires additional setup

				apply_0: static (ref Modification.Context context, ref Control.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var random_activation = ref context.GetOrAddComponent<RandomActivation.Data>();
					random_activation.duration += 0.20f;
				},

				apply_1: static (ref Modification.Context context, ref Control.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("salt.motion", 10.00f)); // High cost

					if (!context.GetComponent<Melee.Data>().IsNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("salt.motion", 10.00f)); // Even higher cost on melee weapons
					}
				}
			));
			definitions.Add(Modification.Definition.New<Control.Data>
			(
				identifier: "control.mind_swap",
				category: "Control",
				name: "Mind Swap",
				description: "Swap your mind with this when holding it.",

				// This randomly causes a left click and a space bar (at the same time)
				// Working examples: guns, fuse explosives, drills, mounts (yes they will use whatever is on them), melee weapons, even medkits
				// Due to the wide variety of uses this has, this costs a large amount of materials
				// This doesn't aim, so using anything which uses aim direction requires additional setup

				apply_0: static (ref Modification.Context context, ref Control.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var mindswap = ref context.GetOrAddComponent<Mindswap.Data>();
					ref var organic = ref context.GetOrAddComponent<Organic.Data>();
					organic.tags |= Organic.Tags.Brain;
					ref var organicstate = ref context.GetOrAddComponent<Organic.State>();
					organicstate.consciousness_shared = 1.00f;
					ref var npc = ref context.GetOrAddComponent<NPC.Data>();
				},

				apply_1: static (ref Modification.Context context, ref Control.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("salt.motion", 10.00f)); // High cost

					if (!context.GetComponent<Melee.Data>().IsNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("salt.motion", 10.00f)); // Even higher cost on melee weapons
					}
				}
			));
			/*definitions.Add(Modification.Definition.New<Control.Data>
			(
				identifier: "control.auto_aim",
				category: "Control",
				name: "Auto Aim",
				description: "Tries to automaticly aim towards nearby other creatures.",

				// This randomly causes a left click and a space bar (at the same time)
				// Working examples: guns, fuse explosives, drills, mounts (yes they will use whatever is on them), melee weapons, even medkits
				// Due to the wide variety of uses this has, this costs a large amount of materials
				// This doesn't aim, so using anything which uses aim direction requires additional setup

				apply_0: static (ref Modification.Context context, ref Control.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var npc = ref context.GetOrAddComponent<NPC.Data>();
					
					npc.navigation_state = NPC.NavigationState.Searching;
					npc.find_distance_max = 10.00f;
					npc.lose_distance_max = 128.00f;
				},

				apply_1: static (ref Modification.Context context, ref Control.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("salt.motion", 10.00f)); // High cost

					if (!context.GetComponent<Melee.Data>().IsNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("salt.motion", 10.00f)); // Even higher cost on melee weapons
					}
				}
			));*/
		}
	}
}

